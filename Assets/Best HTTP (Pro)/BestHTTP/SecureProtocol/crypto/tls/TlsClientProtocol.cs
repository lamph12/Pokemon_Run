#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class TlsClientProtocol
        : TlsProtocol
    {
        protected TlsAuthentication mAuthentication;
        protected CertificateRequest mCertificateRequest;

        protected CertificateStatus mCertificateStatus;

        protected TlsKeyExchange mKeyExchange;

        protected byte[] mSelectedSessionID;
        protected TlsClient mTlsClient;
        internal TlsClientContextImpl mTlsClientContext;

        /**
         * Constructor for blocking mode.
         * @param stream The bi-directional stream of data to/from the server
         * @param secureRandom Random number generator for various cryptographic functions
         */
        public TlsClientProtocol(Stream stream, SecureRandom secureRandom)
            : base(stream, secureRandom)
        {
        }

        /**
         * Constructor for blocking mode.
         * @param input The stream of data from the server
         * @param output The stream of data to the server
         * @param secureRandom Random number generator for various cryptographic functions
         */
        public TlsClientProtocol(Stream input, Stream output, SecureRandom secureRandom)
            : base(input, output, secureRandom)
        {
        }

        /**
         * Constructor for non-blocking mode.
         * <br />
         * <br />
         * When data is received, use {@link #offerInput(java.nio.ByteBuffer)} to
         * provide the received ciphertext, then use
         * {@link #readInput(byte[], int, int)} to read the corresponding cleartext.
         * <br />
         * <br />
         * Similarly, when data needs to be sent, use
         * {@link #offerOutput(byte[], int, int)} to provide the cleartext, then use
         * {@link #readOutput(byte[], int, int)} to get the corresponding
         * ciphertext.
         * 
         * @param secureRandom
         * Random number generator for various cryptographic functions
         */
        public TlsClientProtocol(SecureRandom secureRandom)
            : base(secureRandom)
        {
        }

        protected override TlsContext Context => mTlsClientContext;

        internal override AbstractTlsContext ContextAdmin => mTlsClientContext;

        protected override TlsPeer Peer => mTlsClient;

        /**
         * Initiates a TLS handshake in the role of client.
         * <br />
         * <br />
         * In blocking mode, this will not return until the handshake is complete.
         * In non-blocking mode, use {@link TlsPeer#NotifyHandshakeComplete()} to
         * receive a callback when the handshake is complete.
         * 
         * @param tlsClient The {@link TlsClient} to use for the handshake.
         * @throws IOException If in blocking mode and handshake was not successful.
         */
        public virtual void Connect(TlsClient tlsClient)
        {
            if (tlsClient == null)
                throw new ArgumentNullException("tlsClient");
            if (mTlsClient != null)
                throw new InvalidOperationException("'Connect' can only be called once");

            mTlsClient = tlsClient;

            mSecurityParameters = new SecurityParameters();
            mSecurityParameters.entity = ConnectionEnd.client;

            mTlsClientContext = new TlsClientContextImpl(mSecureRandom, mSecurityParameters);

            mSecurityParameters.clientRandom = CreateRandomBlock(tlsClient.ShouldUseGmtUnixTime(),
                mTlsClientContext.NonceRandomGenerator);

            mTlsClient.Init(mTlsClientContext);
            mRecordStream.Init(mTlsClientContext);

            var sessionToResume = tlsClient.GetSessionToResume();
            if (sessionToResume != null && sessionToResume.IsResumable)
            {
                var sessionParameters = sessionToResume.ExportSessionParameters();
                if (sessionParameters != null)
                {
                    mTlsSession = sessionToResume;
                    mSessionParameters = sessionParameters;
                }
            }

            SendClientHelloMessage();
            mConnectionState = CS_CLIENT_HELLO;

            BlockForHandshake();
        }

        protected override void CleanupHandshake()
        {
            base.CleanupHandshake();

            mSelectedSessionID = null;
            mKeyExchange = null;
            mAuthentication = null;
            mCertificateStatus = null;
            mCertificateRequest = null;
        }

        protected override void HandleHandshakeMessage(byte type, byte[] data)
        {
            var buf = new MemoryStream(data, false);

            if (mResumedSession)
            {
                if (type != HandshakeType.finished || mConnectionState != CS_SERVER_HELLO)
                    throw new TlsFatalAlert(AlertDescription.unexpected_message);

                ProcessFinishedMessage(buf);
                mConnectionState = CS_SERVER_FINISHED;

                SendFinishedMessage();
                mConnectionState = CS_CLIENT_FINISHED;
                mConnectionState = CS_END;

                CompleteHandshake();
                return;
            }

            switch (type)
            {
                case HandshakeType.certificate:
                {
                    switch (mConnectionState)
                    {
                        case CS_SERVER_HELLO:
                        case CS_SERVER_SUPPLEMENTAL_DATA:
                        {
                            if (mConnectionState == CS_SERVER_HELLO) HandleSupplementalData(null);

                            // Parse the Certificate message and Send to cipher suite

                            mPeerCertificate = Certificate.Parse(buf);

                            AssertEmpty(buf);

                            // TODO[RFC 3546] Check whether empty certificates is possible, allowed, or excludes CertificateStatus
                            if (mPeerCertificate == null || mPeerCertificate.IsEmpty) mAllowCertificateStatus = false;

                            mKeyExchange.ProcessServerCertificate(mPeerCertificate);

                            mAuthentication = mTlsClient.GetAuthentication();
                            mAuthentication.NotifyServerCertificate(mPeerCertificate);

                            break;
                        }
                        default:
                            throw new TlsFatalAlert(AlertDescription.unexpected_message);
                    }

                    mConnectionState = CS_SERVER_CERTIFICATE;
                    break;
                }
                case HandshakeType.certificate_status:
                {
                    switch (mConnectionState)
                    {
                        case CS_SERVER_CERTIFICATE:
                        {
                            if (!mAllowCertificateStatus)
                                /*
                                     * RFC 3546 3.6. If a server returns a "CertificateStatus" message, then the
                                     * server MUST have included an extension of type "status_request" with empty
                                     * "extension_data" in the extended server hello..
                                     */
                                throw new TlsFatalAlert(AlertDescription.unexpected_message);

                            mCertificateStatus = CertificateStatus.Parse(buf);

                            AssertEmpty(buf);

                            // TODO[RFC 3546] Figure out how to provide this to the client/authentication.

                            mConnectionState = CS_CERTIFICATE_STATUS;
                            break;
                        }
                        default:
                            throw new TlsFatalAlert(AlertDescription.unexpected_message);
                    }

                    break;
                }
                case HandshakeType.finished:
                {
                    switch (mConnectionState)
                    {
                        case CS_CLIENT_FINISHED:
                        case CS_SERVER_SESSION_TICKET:
                        {
                            if (mConnectionState == CS_CLIENT_FINISHED && mExpectSessionTicket)
                                /*
                                     * RFC 5077 3.3. This message MUST be sent if the server included a
                                     * SessionTicket extension in the ServerHello.
                                     */
                                    throw new TlsFatalAlert(AlertDescription.unexpected_message);

                                ProcessFinishedMessage(buf);
                                mConnectionState = CS_SERVER_FINISHED;
                                mConnectionState = CS_END;

                                CompleteHandshake();
                                break;
                                }
                                default:
                                    throw new TlsFatalAlert(AlertDescription.unexpected_message);
                                }

                                break;
                                }
                                case HandshakeType.server_hello:
                                {
                                    switch (mConnectionState)
                                    {
                                        case CS_CLIENT_HELLO:
                                        {
                                            ReceiveServerHelloMessage(buf);
                                            mConnectionState = CS_SERVER_HELLO;

                                            mRecordStream.NotifyHelloComplete();

                                            ApplyMaxFragmentLengthExtension();

                                            if (mResumedSession)
                                            {
                                                mSecurityParameters.masterSecret =
                                                    Arrays.Clone(mSessionParameters.MasterSecret);
                                                mRecordStream.SetPendingConnectionState(
                                                    Peer.GetCompression(), Peer.GetCipher());

                                                SendChangeCipherSpecMessage();
                                            }
                                            else
                                            {
                                                InvalidateSession();

                                                if (mSelectedSessionID.Length > 0)
                                                    mTlsSession = new TlsSessionImpl(mSelectedSessionID,
                                                        null);
                                            }

                                            break;
                                        }
                                        default:
                                            throw new TlsFatalAlert(AlertDescription
                                                .unexpected_message);
                                    }

                                    break;
                                }
                                case HandshakeType.supplemental_data:
                                {
                                    switch (mConnectionState)
                                    {
                                        case CS_SERVER_HELLO:
                                        {
                                            HandleSupplementalData(ReadSupplementalDataMessage(buf));
                                            break;
                                        }
                                        default:
                                            throw new TlsFatalAlert(AlertDescription
                                                .unexpected_message);
                                    }

                                    break;
                                }
                                case HandshakeType.server_hello_done:
                                {
                                    switch (mConnectionState)
                                    {
                                        case CS_SERVER_HELLO:
                                        case CS_SERVER_SUPPLEMENTAL_DATA:
                                        case CS_SERVER_CERTIFICATE:
                                        case CS_CERTIFICATE_STATUS:
                                        case CS_SERVER_KEY_EXCHANGE:
                                        case CS_CERTIFICATE_REQUEST:
                                        {
                                            if (mConnectionState < CS_SERVER_SUPPLEMENTAL_DATA)
                                                HandleSupplementalData(null);

                                            if (mConnectionState < CS_SERVER_CERTIFICATE)
                                            {
                                                // There was no server certificate message; check it's OK
                                                mKeyExchange.SkipServerCredentials();
                                                mAuthentication = null;
                                            }

                                            if (mConnectionState < CS_SERVER_KEY_EXCHANGE)
                                                // There was no server key exchange message; check it's OK
                                                mKeyExchange.SkipServerKeyExchange();

                                            AssertEmpty(buf);

                                            mConnectionState = CS_SERVER_HELLO_DONE;

                                            mRecordStream.HandshakeHash.SealHashAlgorithms();

                                            var clientSupplementalData =
                                                mTlsClient.GetClientSupplementalData();
                                            if (clientSupplementalData != null)
                                                SendSupplementalDataMessage(clientSupplementalData);
                                            mConnectionState = CS_CLIENT_SUPPLEMENTAL_DATA;

                                            TlsCredentials clientCreds = null;
                                            if (mCertificateRequest == null)
                                            {
                                                mKeyExchange.SkipClientCredentials();
                                            }
                                            else
                                            {
                                                clientCreds =
                                                    mAuthentication.GetClientCredentials(Context,
                                                        mCertificateRequest);

                                                if (clientCreds == null)
                                                {
                                                    mKeyExchange.SkipClientCredentials();

                                                    /*
                                                     * RFC 5246 If no suitable certificate is available, the client MUST Send a
                                                     * certificate message containing no certificates.
                                                     *
                                                     * NOTE: In previous RFCs, this was SHOULD instead of MUST.
                                                     */
                                                    SendCertificateMessage(Certificate.EmptyChain);
                                                }
                                                else
                                                {
                                                    mKeyExchange.ProcessClientCredentials(clientCreds);

                                                    SendCertificateMessage(clientCreds.Certificate);
                                                }
                                            }

                                            mConnectionState = CS_CLIENT_CERTIFICATE;

                                            /*
                                             * Send the client key exchange message, depending on the key exchange we are using
                                             * in our CipherSuite.
                                             */
                                            SendClientKeyExchangeMessage();
                                            mConnectionState = CS_CLIENT_KEY_EXCHANGE;

                                            var prepareFinishHash = mRecordStream.PrepareToFinish();
                                            mSecurityParameters.sessionHash =
                                                GetCurrentPrfHash(Context, prepareFinishHash, null);

                                            EstablishMasterSecret(Context, mKeyExchange);
                                            mRecordStream.SetPendingConnectionState(
                                                Peer.GetCompression(), Peer.GetCipher());

                                            if (clientCreds != null &&
                                                clientCreds is TlsSignerCredentials)
                                            {
                                                var signerCredentials =
                                                    (TlsSignerCredentials)clientCreds;

                                                /*
                                                 * RFC 5246 4.7. digitally-signed element needs SignatureAndHashAlgorithm from TLS 1.2
                                                 */
                                                var signatureAndHashAlgorithm =
                                                    TlsUtilities.GetSignatureAndHashAlgorithm(
                                                        Context, signerCredentials);

                                                byte[] hash;
                                                if (signatureAndHashAlgorithm == null)
                                                    hash = mSecurityParameters.SessionHash;
                                                else
                                                    hash = prepareFinishHash.GetFinalHash(
                                                        signatureAndHashAlgorithm.Hash);

                                                var signature = signerCredentials
                                                    .GenerateCertificateSignature(hash);
                                                var certificateVerify =
                                                    new DigitallySigned(signatureAndHashAlgorithm,
                                                        signature);
                                                SendCertificateVerifyMessage(certificateVerify);

                                                mConnectionState = CS_CERTIFICATE_VERIFY;
                                            }

                                            SendChangeCipherSpecMessage();
                                            SendFinishedMessage();
                                            break;
                                        }
                                        default:
                                            throw new TlsFatalAlert(AlertDescription.handshake_failure);
                                    }

                                    mConnectionState = CS_CLIENT_FINISHED;
                                    break;
                                }
                                case HandshakeType.server_key_exchange:
                                {
                                    switch (mConnectionState)
                                    {
                                        case CS_SERVER_HELLO:
                                        case CS_SERVER_SUPPLEMENTAL_DATA:
                                        case CS_SERVER_CERTIFICATE:
                                        case CS_CERTIFICATE_STATUS:
                                        {
                                            if (mConnectionState < CS_SERVER_SUPPLEMENTAL_DATA)
                                                HandleSupplementalData(null);

                                            if (mConnectionState < CS_SERVER_CERTIFICATE)
                                            {
                                                // There was no server certificate message; check it's OK
                                                mKeyExchange.SkipServerCredentials();
                                                mAuthentication = null;
                                            }

                                            mKeyExchange.ProcessServerKeyExchange(buf);

                                            AssertEmpty(buf);
                                            break;
                                        }
                                        default:
                                            throw new TlsFatalAlert(AlertDescription
                                                .unexpected_message);
                                    }

                                    mConnectionState = CS_SERVER_KEY_EXCHANGE;
                                    break;
                                }
                                case HandshakeType.certificate_request:
                                {
                                    switch (mConnectionState)
                                    {
                                        case CS_SERVER_CERTIFICATE:
                                        case CS_CERTIFICATE_STATUS:
                                        case CS_SERVER_KEY_EXCHANGE:
                                        {
                                            if (mConnectionState != CS_SERVER_KEY_EXCHANGE)
                                                // There was no server key exchange message; check it's OK
                                                mKeyExchange.SkipServerKeyExchange();

                                            if (mAuthentication == null)
                                                /*
                                                     * RFC 2246 7.4.4. It is a fatal handshake_failure alert for an anonymous server
                                                     * to request client identification.
                                                     */
                                                    throw new TlsFatalAlert(AlertDescription
                                                        .handshake_failure);

                                                mCertificateRequest =
                                                    CertificateRequest.Parse(Context, buf);

                                                AssertEmpty(buf);

                                                mKeyExchange.ValidateCertificateRequest(
                                                    mCertificateRequest);

                                                /*
                                                 * TODO Give the client a chance to immediately select the CertificateVerify hash
                                                 * algorithm here to avoid tracking the other hash algorithms unnecessarily?
                                                 */
                                                TlsUtilities.TrackHashAlgorithms(
                                                    mRecordStream.HandshakeHash,
                                                    mCertificateRequest.SupportedSignatureAlgorithms);

                                                break;
                                                }
                                                default:
                                                    throw new TlsFatalAlert(AlertDescription
                                                        .unexpected_message);
                                                }

                                                mConnectionState = CS_CERTIFICATE_REQUEST;
                                                break;
                                                }
                                                case HandshakeType.session_ticket:
                                                {
                                                    switch (mConnectionState)
                                                    {
                                                        case CS_CLIENT_FINISHED:
                                                        {
                                                            if (!mExpectSessionTicket)
                                                                /*
                                                                     * RFC 5077 3.3. This message MUST NOT be sent if the server did not include a
                                                                     * SessionTicket extension in the ServerHello.
                                                                     */
                                                                    throw new TlsFatalAlert(
                                                                        AlertDescription
                                                                            .unexpected_message);

                                                                /*
                                                                 * RFC 5077 3.4. If the client receives a session ticket from the server, then it
                                                                 * discards any Session ID that was sent in the ServerHello.
                                                                 */
                                                                InvalidateSession();

                                                                ReceiveNewSessionTicketMessage(buf);
                                                                break;
                                                                }
                                                                default:
                                                                    throw new TlsFatalAlert(
                                                                        AlertDescription
                                                                            .unexpected_message);
                                                                }

                                                                mConnectionState =
                                                                    CS_SERVER_SESSION_TICKET;
                                                                break;
                                                                }
                                                                case HandshakeType.hello_request:
                                                                {
                                                                    AssertEmpty(buf);

                                                                    /*
                                                                     * RFC 2246 7.4.1.1 Hello request This message will be ignored by the client if the
                                                                     * client is currently negotiating a session. This message may be ignored by the client
                                                                     * if it does not wish to renegotiate a session, or the client may, if it wishes,
                                                                     * respond with a no_renegotiation alert.
                                                                     */
                                                                    if (mConnectionState == CS_END)
                                                                        RefuseRenegotiation();
                                                                    break;
                                                                }
                                                                case HandshakeType.client_hello:
                                                                case HandshakeType.client_key_exchange:
                                                                case HandshakeType.certificate_verify:
                                                                case HandshakeType.hello_verify_request:
                                                                default:
                                                                    throw new TlsFatalAlert(
                                                                        AlertDescription
                                                                            .unexpected_message);
                                                                }
                                                                }

                                                                protected virtual void
                                                                    HandleSupplementalData(
                                                                        IList serverSupplementalData)
                                                                {
                                                                    mTlsClient
                                                                        .ProcessServerSupplementalData(
                                                                            serverSupplementalData);
                                                                    mConnectionState =
                                                                        CS_SERVER_SUPPLEMENTAL_DATA;

                                                                    mKeyExchange =
                                                                        mTlsClient.GetKeyExchange();
                                                                    mKeyExchange.Init(Context);
                                                                }

                                                                protected virtual void
                                                                    ReceiveNewSessionTicketMessage(
                                                                        MemoryStream buf)
                                                                {
                                                                    var newSessionTicket =
                                                                        NewSessionTicket.Parse(buf);

                                                                    AssertEmpty(buf);

                                                                    mTlsClient.NotifyNewSessionTicket(
                                                                        newSessionTicket);
                                                                }

                                                                protected virtual void
                                                                    ReceiveServerHelloMessage(
                                                                        MemoryStream buf)
                                                                {
                                                                    {
                                                                        var server_version =
                                                                            TlsUtilities.ReadVersion(
                                                                                buf);
                                                                        if (server_version.IsDtls)
                                                                            throw new TlsFatalAlert(
                                                                                AlertDescription
                                                                                    .illegal_parameter);

                                                                        // Check that this matches what the server is Sending in the record layer
                                                                        if (!server_version.Equals(
                                                                                mRecordStream
                                                                                    .ReadVersion))
                                                                            throw new TlsFatalAlert(
                                                                                AlertDescription
                                                                                    .illegal_parameter);

                                                                        var client_version =
                                                                            Context.ClientVersion;
                                                                        if (!server_version
                                                                                .IsEqualOrEarlierVersionOf(
                                                                                    client_version))
                                                                            throw new TlsFatalAlert(
                                                                                AlertDescription
                                                                                    .illegal_parameter);

                                                                        mRecordStream.SetWriteVersion(
                                                                            server_version);
                                                                        ContextAdmin.SetServerVersion(
                                                                            server_version);
                                                                        mTlsClient.NotifyServerVersion(
                                                                            server_version);
                                                                    }

                                                                    /*
                                                                     * Read the server random
                                                                     */
                                                                    mSecurityParameters.serverRandom =
                                                                        TlsUtilities.ReadFully(32, buf);

                                                                    mSelectedSessionID =
                                                                        TlsUtilities.ReadOpaque8(buf);
                                                                    if (mSelectedSessionID.Length > 32)
                                                                        throw new TlsFatalAlert(
                                                                            AlertDescription
                                                                                .illegal_parameter);
                                                                    mTlsClient.NotifySessionID(
                                                                        mSelectedSessionID);
                                                                    mResumedSession =
                                                                        mSelectedSessionID.Length > 0 &&
                                                                        mTlsSession != null
                                                                        && Arrays.AreEqual(
                                                                            mSelectedSessionID,
                                                                            mTlsSession.SessionID);

                                                                    /*
                                                                     * Find out which CipherSuite the server has chosen and check that it was one of the offered
                                                                     * ones, and is a valid selection for the negotiated version.
                                                                     */
                                                                    var selectedCipherSuite =
                                                                        TlsUtilities.ReadUint16(buf);
                                                                    if (!Arrays.Contains(
                                                                            mOfferedCipherSuites,
                                                                            selectedCipherSuite)
                                                                        || selectedCipherSuite ==
                                                                        CipherSuite
                                                                            .TLS_NULL_WITH_NULL_NULL
                                                                        || CipherSuite.IsScsv(
                                                                            selectedCipherSuite)
                                                                        || !TlsUtilities
                                                                            .IsValidCipherSuiteForVersion(
                                                                                selectedCipherSuite,
                                                                                Context.ServerVersion))
                                                                        throw new TlsFatalAlert(
                                                                            AlertDescription
                                                                                .illegal_parameter);
                                                                    mTlsClient
                                                                        .NotifySelectedCipherSuite(
                                                                            selectedCipherSuite);

                                                                    /*
                                                                     * Find out which CompressionMethod the server has chosen and check that it was one of the
                                                                     * offered ones.
                                                                     */
                                                                    var selectedCompressionMethod =
                                                                        TlsUtilities.ReadUint8(buf);
                                                                    if (!Arrays.Contains(
                                                                            mOfferedCompressionMethods,
                                                                            selectedCompressionMethod))
                                                                        throw new TlsFatalAlert(
                                                                            AlertDescription
                                                                                .illegal_parameter);
                                                                    mTlsClient
                                                                        .NotifySelectedCompressionMethod(
                                                                            selectedCompressionMethod);

                                                                    /*
                                                                     * RFC3546 2.2 The extended server hello message format MAY be sent in place of the server
                                                                     * hello message when the client has requested extended functionality via the extended
                                                                     * client hello message specified in Section 2.1. ... Note that the extended server hello
                                                                     * message is only sent in response to an extended client hello message. This prevents the
                                                                     * possibility that the extended server hello message could "break" existing TLS 1.0
                                                                     * clients.
                                                                     */
                                                                    mServerExtensions =
                                                                        ReadExtensions(buf);

                                                                    /*
                                                                     * RFC 3546 2.2 Note that the extended server hello message is only sent in response to an
                                                                     * extended client hello message.
                                                                     *
                                                                     * However, see RFC 5746 exception below. We always include the SCSV, so an Extended Server
                                                                     * Hello is always allowed.
                                                                     */
                                                                    if (mServerExtensions != null)
                                                                        foreach (int extType in
                                                                         mServerExtensions.Keys)
                                                                        {
                                                                            /*
                                                                             * RFC 5746 3.6. Note that Sending a "renegotiation_info" extension in response to a
                                                                             * ClientHello containing only the SCSV is an explicit exception to the prohibition
                                                                             * in RFC 5246, Section 7.4.1.4, on the server Sending unsolicited extensions and is
                                                                             * only allowed because the client is signaling its willingness to receive the
                                                                             * extension via the TLS_EMPTY_RENEGOTIATION_INFO_SCSV SCSV.
                                                                             */
                                                                            if (extType == ExtensionType
                                                                                    .renegotiation_info)
                                                                                continue;

                                                                            /*
                                                                             * RFC 5246 7.4.1.4 An extension type MUST NOT appear in the ServerHello unless the
                                                                             * same extension type appeared in the corresponding ClientHello. If a client
                                                                             * receives an extension type in ServerHello that it did not request in the
                                                                             * associated ClientHello, it MUST abort the handshake with an unsupported_extension
                                                                             * fatal alert.
                                                                             */
                                                                            if (null == TlsUtilities
                                                                                    .GetExtensionData(
                                                                                        mClientExtensions,
                                                                                        extType))
                                                                                throw new TlsFatalAlert(
                                                                                    AlertDescription
                                                                                        .unsupported_extension);

                                                                            /*
                                                                             * RFC 3546 2.3. If [...] the older session is resumed, then the server MUST ignore
                                                                             * extensions appearing in the client hello, and Send a server hello containing no
                                                                             * extensions[.]
                                                                             */
                                                                            if (mResumedSession)
                                                                            {
                                                                                // TODO[compat-gnutls] GnuTLS test server Sends server extensions e.g. ec_point_formats
                                                                                // TODO[compat-openssl] OpenSSL test server Sends server extensions e.g. ec_point_formats
                                                                                // TODO[compat-polarssl] PolarSSL test server Sends server extensions e.g. ec_point_formats
                                                                                //                    throw new TlsFatalAlert(AlertDescription.illegal_parameter);
                                                                            }
                                                                        }

                                                                    /*
                                                                     * RFC 5746 3.4. Client Behavior: Initial Handshake
                                                                     */
                                                                    {
                                                                        /*
                                                                         * When a ServerHello is received, the client MUST check if it includes the
                                                                         * "renegotiation_info" extension:
                                                                         */
                                                                        var renegExtData =
                                                                            TlsUtilities
                                                                                .GetExtensionData(
                                                                                    mServerExtensions,
                                                                                    ExtensionType
                                                                                        .renegotiation_info);
                                                                        if (renegExtData != null)
                                                                        {
                                                                            /*
                                                                             * If the extension is present, set the secure_renegotiation flag to TRUE. The
                                                                             * client MUST then verify that the length of the "renegotiated_connection"
                                                                             * field is zero, and if it is not, MUST abort the handshake (by Sending a fatal
                                                                             * handshake_failure alert).
                                                                             */
                                                                            mSecureRenegotiation = true;

                                                                            if (!Arrays
                                                                                    .ConstantTimeAreEqual(
                                                                                        renegExtData,
                                                                                        CreateRenegotiationInfo(
                                                                                            TlsUtilities
                                                                                                .EmptyBytes)))
                                                                                throw new TlsFatalAlert(
                                                                                    AlertDescription
                                                                                        .handshake_failure);
                                                                        }
                                                                    }

                                                                    // TODO[compat-gnutls] GnuTLS test server fails to Send renegotiation_info extension when resuming
                                                                    mTlsClient
                                                                        .NotifySecureRenegotiation(
                                                                            mSecureRenegotiation);

                                                                    IDictionary
                                                                        sessionClientExtensions =
                                                                            mClientExtensions,
                                                                        sessionServerExtensions =
                                                                            mServerExtensions;
                                                                    if (mResumedSession)
                                                                    {
                                                                        if (selectedCipherSuite !=
                                                                            mSessionParameters
                                                                                .CipherSuite
                                                                            ||
                                                                            selectedCompressionMethod !=
                                                                            mSessionParameters
                                                                                .CompressionAlgorithm)
                                                                            throw new TlsFatalAlert(
                                                                                AlertDescription
                                                                                    .illegal_parameter);

                                                                        sessionClientExtensions = null;
                                                                        sessionServerExtensions =
                                                                            mSessionParameters
                                                                                .ReadServerExtensions();
                                                                    }

                                                                    mSecurityParameters.cipherSuite =
                                                                        selectedCipherSuite;
                                                                    mSecurityParameters
                                                                            .compressionAlgorithm =
                                                                        selectedCompressionMethod;

                                                                    if (sessionServerExtensions != null)
                                                                    {
                                                                        {
                                                                            /*
                                                                             * RFC 7366 3. If a server receives an encrypt-then-MAC request extension from a client
                                                                             * and then selects a stream or Authenticated Encryption with Associated Data (AEAD)
                                                                             * ciphersuite, it MUST NOT send an encrypt-then-MAC response extension back to the
                                                                             * client.
                                                                             */
                                                                            var
                                                                                serverSentEncryptThenMAC =
                                                                                    TlsExtensionsUtilities
                                                                                        .HasEncryptThenMacExtension(
                                                                                            sessionServerExtensions);
                                                                            if
                                                                                (serverSentEncryptThenMAC &&
                                                                                 !TlsUtilities
                                                                                     .IsBlockCipherSuite(
                                                                                         selectedCipherSuite))
                                                                                throw new TlsFatalAlert(
                                                                                    AlertDescription
                                                                                        .illegal_parameter);

                                                                            mSecurityParameters
                                                                                    .encryptThenMac =
                                                                                serverSentEncryptThenMAC;
                                                                        }

                                                                        mSecurityParameters
                                                                                .extendedMasterSecret =
                                                                            TlsExtensionsUtilities
                                                                                .HasExtendedMasterSecretExtension(
                                                                                    sessionServerExtensions);

                                                                        mSecurityParameters
                                                                                .maxFragmentLength =
                                                                            ProcessMaxFragmentLengthExtension(
                                                                                sessionClientExtensions,
                                                                                sessionServerExtensions,
                                                                                AlertDescription
                                                                                    .illegal_parameter);

                                                                        mSecurityParameters
                                                                                .truncatedHMac =
                                                                            TlsExtensionsUtilities
                                                                                .HasTruncatedHMacExtension(
                                                                                    sessionServerExtensions);

                                                                        /*
                                                                         * TODO It's surprising that there's no provision to allow a 'fresh' CertificateStatus to be sent in
                                                                         * a session resumption handshake.
                                                                         */
                                                                        mAllowCertificateStatus =
                                                                            !mResumedSession
                                                                            && TlsUtilities
                                                                                .HasExpectedEmptyExtensionData(
                                                                                    sessionServerExtensions,
                                                                                    ExtensionType
                                                                                        .status_request,
                                                                                    AlertDescription
                                                                                        .illegal_parameter);

                                                                        mExpectSessionTicket =
                                                                            !mResumedSession
                                                                            && TlsUtilities
                                                                                .HasExpectedEmptyExtensionData(
                                                                                    sessionServerExtensions,
                                                                                    ExtensionType
                                                                                        .session_ticket,
                                                                                    AlertDescription
                                                                                        .illegal_parameter);
                                                                    }

                                                                    /*
                                                                     * TODO[session-hash]
                                                                     *
                                                                     * draft-ietf-tls-session-hash-04 4. Clients and servers SHOULD NOT accept handshakes
                                                                     * that do not use the extended master secret [..]. (and see 5.2, 5.3)
                                                                     */

                                                                    if (sessionClientExtensions != null)
                                                                        mTlsClient
                                                                            .ProcessServerExtensions(
                                                                                sessionServerExtensions);

                                                                    mSecurityParameters.prfAlgorithm =
                                                                        GetPrfAlgorithm(Context,
                                                                            mSecurityParameters
                                                                                .CipherSuite);

                                                                    /*
                                                                     * RFC 5264 7.4.9. Any cipher suite which does not explicitly specify
                                                                     * verify_data_length has a verify_data_length equal to 12. This includes all
                                                                     * existing cipher suites.
                                                                     */
                                                                    mSecurityParameters
                                                                        .verifyDataLength = 12;
                                                                }

                                                                protected virtual void
                                                                    SendCertificateVerifyMessage(
                                                                        DigitallySigned
                                                                            certificateVerify)
                                                                {
                                                                    var message =
                                                                        new HandshakeMessage(
                                                                            HandshakeType
                                                                                .certificate_verify);

                                                                    certificateVerify.Encode(message);

                                                                    message.WriteToRecordStream(this);
                                                                }

                                                                protected virtual void
                                                                    SendClientHelloMessage()
                                                                {
                                                                    mRecordStream.SetWriteVersion(
                                                                        mTlsClient
                                                                            .ClientHelloRecordLayerVersion);

                                                                    var client_version =
                                                                        mTlsClient.ClientVersion;
                                                                    if (client_version.IsDtls)
                                                                        throw new TlsFatalAlert(
                                                                            AlertDescription
                                                                                .internal_error);

                                                                    ContextAdmin.SetClientVersion(
                                                                        client_version);

                                                                    /*
                                                                     * TODO RFC 5077 3.4. When presenting a ticket, the client MAY generate and include a
                                                                     * Session ID in the TLS ClientHello.
                                                                     */
                                                                    var session_id =
                                                                        TlsUtilities.EmptyBytes;
                                                                    if (mTlsSession != null)
                                                                    {
                                                                        session_id =
                                                                            mTlsSession.SessionID;
                                                                        if (session_id == null ||
                                                                            session_id.Length > 32)
                                                                            session_id =
                                                                                TlsUtilities.EmptyBytes;
                                                                    }

                                                                    var fallback =
                                                                        mTlsClient.IsFallback;

                                                                    mOfferedCipherSuites =
                                                                        mTlsClient.GetCipherSuites();

                                                                    mOfferedCompressionMethods =
                                                                        mTlsClient
                                                                            .GetCompressionMethods();

                                                                    if (session_id.Length > 0 &&
                                                                        mSessionParameters != null)
                                                                        if (!Arrays.Contains(
                                                                                mOfferedCipherSuites,
                                                                                mSessionParameters
                                                                                    .CipherSuite)
                                                                            || !Arrays.Contains(
                                                                                mOfferedCompressionMethods,
                                                                                mSessionParameters
                                                                                    .CompressionAlgorithm))
                                                                            session_id =
                                                                                TlsUtilities.EmptyBytes;

                                                                    mClientExtensions =
                                                                        mTlsClient
                                                                            .GetClientExtensions();

                                                                    var message =
                                                                        new HandshakeMessage(
                                                                            HandshakeType.client_hello);

                                                                    TlsUtilities.WriteVersion(
                                                                        client_version, message);

                                                                    message.Write(mSecurityParameters
                                                                        .ClientRandom);

                                                                    TlsUtilities.WriteOpaque8(
                                                                        session_id, message);

                                                                    // Cipher Suites (and SCSV)
                                                                    {
                                                                        /*
                                                                         * RFC 5746 3.4. The client MUST include either an empty "renegotiation_info" extension,
                                                                         * or the TLS_EMPTY_RENEGOTIATION_INFO_SCSV signaling cipher suite value in the
                                                                         * ClientHello. Including both is NOT RECOMMENDED.
                                                                         */
                                                                        var renegExtData =
                                                                            TlsUtilities
                                                                                .GetExtensionData(
                                                                                    mClientExtensions,
                                                                                    ExtensionType
                                                                                        .renegotiation_info);
                                                                        var noRenegExt =
                                                                            null == renegExtData;

                                                                        var noRenegScsv =
                                                                            !Arrays.Contains(
                                                                                mOfferedCipherSuites,
                                                                                CipherSuite
                                                                                    .TLS_EMPTY_RENEGOTIATION_INFO_SCSV);

                                                                        if (noRenegExt && noRenegScsv)
                                                                            // TODO Consider whether to default to a client extension instead
                                                                            //                this.mClientExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(this.mClientExtensions);
                                                                            //                this.mClientExtensions[ExtensionType.renegotiation_info] = CreateRenegotiationInfo(TlsUtilities.EmptyBytes);
                                                                            mOfferedCipherSuites =
                                                                                Arrays.Append(
                                                                                    mOfferedCipherSuites,
                                                                                    CipherSuite
                                                                                        .TLS_EMPTY_RENEGOTIATION_INFO_SCSV);

                                                                        /*
                                                                         * RFC 7507 4. If a client sends a ClientHello.client_version containing a lower value
                                                                         * than the latest (highest-valued) version supported by the client, it SHOULD include
                                                                         * the TLS_FALLBACK_SCSV cipher suite value in ClientHello.cipher_suites [..]. (The
                                                                         * client SHOULD put TLS_FALLBACK_SCSV after all cipher suites that it actually intends
                                                                         * to negotiate.)
                                                                         */
                                                                        if (fallback &&
                                                                            !Arrays.Contains(
                                                                                mOfferedCipherSuites,
                                                                                CipherSuite
                                                                                    .TLS_FALLBACK_SCSV))
                                                                            mOfferedCipherSuites =
                                                                                Arrays.Append(
                                                                                    mOfferedCipherSuites,
                                                                                    CipherSuite
                                                                                        .TLS_FALLBACK_SCSV);

                                                                        TlsUtilities
                                                                            .WriteUint16ArrayWithUint16Length(
                                                                                mOfferedCipherSuites,
                                                                                message);
                                                                    }

                                                                    TlsUtilities
                                                                        .WriteUint8ArrayWithUint8Length(
                                                                            mOfferedCompressionMethods,
                                                                            message);

                                                                    if (mClientExtensions != null)
                                                                        WriteExtensions(message,
                                                                            mClientExtensions);

                                                                    message.WriteToRecordStream(this);
                                                                }

                                                                protected virtual void
                                                                    SendClientKeyExchangeMessage()
                                                                {
                                                                    var message =
                                                                        new HandshakeMessage(
                                                                            HandshakeType
                                                                                .client_key_exchange);

                                                                    mKeyExchange
                                                                        .GenerateClientKeyExchange(
                                                                            message);

                                                                    message.WriteToRecordStream(this);
                                                                }
                                                                }
                                                                }

#endif