#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public abstract class AbstractTlsServer
        : AbstractTlsPeer, TlsServer
    {
        protected TlsCipherFactory mCipherFactory;
        protected byte[] mClientECPointFormats, mServerECPointFormats;
        protected IDictionary mClientExtensions;

        protected ProtocolVersion mClientVersion;

        protected TlsServerContext mContext;
        protected bool mEccCipherSuitesOffered;

        protected bool mEncryptThenMacOffered;
        protected short mMaxFragmentLengthOffered;
        protected int[] mNamedCurves;
        protected int[] mOfferedCipherSuites;
        protected byte[] mOfferedCompressionMethods;
        protected int mSelectedCipherSuite;
        protected byte mSelectedCompressionMethod;
        protected IDictionary mServerExtensions;

        protected ProtocolVersion mServerVersion;
        protected IList mSupportedSignatureAlgorithms;
        protected bool mTruncatedHMacOffered;

        public AbstractTlsServer()
            : this(new DefaultTlsCipherFactory())
        {
        }

        public AbstractTlsServer(TlsCipherFactory cipherFactory)
        {
            mCipherFactory = cipherFactory;
        }

        protected virtual bool AllowEncryptThenMac => true;

        protected virtual bool AllowTruncatedHMac => false;

        protected virtual ProtocolVersion MaximumVersion => ProtocolVersion.TLSv11;

        protected virtual ProtocolVersion MinimumVersion => ProtocolVersion.TLSv10;

        public virtual void Init(TlsServerContext context)
        {
            mContext = context;
        }

        public virtual void NotifyClientVersion(ProtocolVersion clientVersion)
        {
            mClientVersion = clientVersion;
        }

        public virtual void NotifyFallback(bool isFallback)
        {
            /*
             * RFC 7507 3. If TLS_FALLBACK_SCSV appears in ClientHello.cipher_suites and the highest
             * protocol version supported by the server is higher than the version indicated in
             * ClientHello.client_version, the server MUST respond with a fatal inappropriate_fallback
             * alert [..].
             */
            if (isFallback && MaximumVersion.IsLaterVersionOf(mClientVersion))
                throw new TlsFatalAlert(AlertDescription.inappropriate_fallback);
        }

        public virtual void NotifyOfferedCipherSuites(int[] offeredCipherSuites)
        {
            mOfferedCipherSuites = offeredCipherSuites;
            mEccCipherSuitesOffered = TlsEccUtilities.ContainsEccCipherSuites(mOfferedCipherSuites);
        }

        public virtual void NotifyOfferedCompressionMethods(byte[] offeredCompressionMethods)
        {
            mOfferedCompressionMethods = offeredCompressionMethods;
        }

        public virtual void ProcessClientExtensions(IDictionary clientExtensions)
        {
            mClientExtensions = clientExtensions;

            if (clientExtensions != null)
            {
                mEncryptThenMacOffered = TlsExtensionsUtilities.HasEncryptThenMacExtension(clientExtensions);

                mMaxFragmentLengthOffered = TlsExtensionsUtilities.GetMaxFragmentLengthExtension(clientExtensions);
                if (mMaxFragmentLengthOffered >= 0 && !MaxFragmentLength.IsValid((byte)mMaxFragmentLengthOffered))
                    throw new TlsFatalAlert(AlertDescription.illegal_parameter);

                mTruncatedHMacOffered = TlsExtensionsUtilities.HasTruncatedHMacExtension(clientExtensions);

                mSupportedSignatureAlgorithms = TlsUtilities.GetSignatureAlgorithmsExtension(clientExtensions);
                if (mSupportedSignatureAlgorithms != null)
                    /*
                         * RFC 5246 7.4.1.4.1. Note: this extension is not meaningful for TLS versions prior
                         * to 1.2. Clients MUST NOT offer it if they are offering prior versions.
                         */
                    if (!TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(mClientVersion))
                        throw new TlsFatalAlert(AlertDescription.illegal_parameter);

                mNamedCurves = TlsEccUtilities.GetSupportedEllipticCurvesExtension(clientExtensions);
                mClientECPointFormats = TlsEccUtilities.GetSupportedPointFormatsExtension(clientExtensions);
            }

            /*
             * RFC 4429 4. The client MUST NOT include these extensions in the ClientHello message if it
             * does not propose any ECC cipher suites.
             * 
             * NOTE: This was overly strict as there may be ECC cipher suites that we don't recognize.
             * Also, draft-ietf-tls-negotiated-ff-dhe will be overloading the 'elliptic_curves'
             * extension to explicitly allow FFDHE (i.e. non-ECC) groups.
             */
            //if (!this.mEccCipherSuitesOffered && (this.mNamedCurves != null || this.mClientECPointFormats != null))
            //    throw new TlsFatalAlert(AlertDescription.illegal_parameter);
        }

        public virtual ProtocolVersion GetServerVersion()
        {
            if (MinimumVersion.IsEqualOrEarlierVersionOf(mClientVersion))
            {
                var maximumVersion = MaximumVersion;
                if (mClientVersion.IsEqualOrEarlierVersionOf(maximumVersion))
                    return mServerVersion = mClientVersion;
                if (mClientVersion.IsLaterVersionOf(maximumVersion)) return mServerVersion = maximumVersion;
            }

            throw new TlsFatalAlert(AlertDescription.protocol_version);
        }

        public virtual int GetSelectedCipherSuite()
        {
            /*
             * TODO RFC 5246 7.4.3. In order to negotiate correctly, the server MUST check any candidate
             * cipher suites against the "signature_algorithms" extension before selecting them. This is
             * somewhat inelegant but is a compromise designed to minimize changes to the original
             * cipher suite design.
             */

            /*
             * RFC 4429 5.1. A server that receives a ClientHello containing one or both of these
             * extensions MUST use the client's enumerated capabilities to guide its selection of an
             * appropriate cipher suite. One of the proposed ECC cipher suites must be negotiated only
             * if the server can successfully complete the handshake while using the curves and point
             * formats supported by the client [...].
             */
            var eccCipherSuitesEnabled = SupportsClientEccCapabilities(mNamedCurves, mClientECPointFormats);

            var cipherSuites = GetCipherSuites();
            for (var i = 0; i < cipherSuites.Length; ++i)
            {
                var cipherSuite = cipherSuites[i];

                if (Arrays.Contains(mOfferedCipherSuites, cipherSuite)
                    && (eccCipherSuitesEnabled || !TlsEccUtilities.IsEccCipherSuite(cipherSuite))
                    && TlsUtilities.IsValidCipherSuiteForVersion(cipherSuite, mServerVersion))
                    return mSelectedCipherSuite = cipherSuite;
            }

            throw new TlsFatalAlert(AlertDescription.handshake_failure);
        }

        public virtual byte GetSelectedCompressionMethod()
        {
            var compressionMethods = GetCompressionMethods();
            for (var i = 0; i < compressionMethods.Length; ++i)
                if (Arrays.Contains(mOfferedCompressionMethods, compressionMethods[i]))
                    return mSelectedCompressionMethod = compressionMethods[i];
            throw new TlsFatalAlert(AlertDescription.handshake_failure);
        }

        // IDictionary is (Int32 -> byte[])
        public virtual IDictionary GetServerExtensions()
        {
            if (mEncryptThenMacOffered && AllowEncryptThenMac)
                /*
                     * RFC 7366 3. If a server receives an encrypt-then-MAC request extension from a client
                     * and then selects a stream or Authenticated Encryption with Associated Data (AEAD)
                     * ciphersuite, it MUST NOT send an encrypt-then-MAC response extension back to the
                     * client.
                     */
                    if (TlsUtilities.IsBlockCipherSuite(mSelectedCipherSuite))
                        TlsExtensionsUtilities.AddEncryptThenMacExtension(CheckServerExtensions());

                if (mMaxFragmentLengthOffered >= 0
                    && TlsUtilities.IsValidUint8(mMaxFragmentLengthOffered)
                    && MaxFragmentLength.IsValid((byte)mMaxFragmentLengthOffered))
                    TlsExtensionsUtilities.AddMaxFragmentLengthExtension(CheckServerExtensions(),
                        (byte)mMaxFragmentLengthOffered);

                if (mTruncatedHMacOffered && AllowTruncatedHMac)
                    TlsExtensionsUtilities.AddTruncatedHMacExtension(CheckServerExtensions());

                if (mClientECPointFormats != null && TlsEccUtilities.IsEccCipherSuite(mSelectedCipherSuite))
                {
                    /*
                     * RFC 4492 5.2. A server that selects an ECC cipher suite in response to a ClientHello
                     * message including a Supported Point Formats Extension appends this extension (along
                     * with others) to its ServerHello message, enumerating the point formats it can parse.
                     */
                    mServerECPointFormats = new[]
                    {
                        ECPointFormat.uncompressed,
                        ECPointFormat.ansiX962_compressed_prime, ECPointFormat.ansiX962_compressed_char2
                    };

                    TlsEccUtilities.AddSupportedPointFormatsExtension(CheckServerExtensions(),
                        mServerECPointFormats);
                }

                return mServerExtensions;
                }

                public virtual IList GetServerSupplementalData()
                {
                    return null;
                }

                public abstract TlsCredentials GetCredentials();

                public virtual CertificateStatus GetCertificateStatus()
                {
                    return null;
                }

                public abstract TlsKeyExchange GetKeyExchange();

                public virtual CertificateRequest GetCertificateRequest()
                {
                    return null;
                }

                public virtual void ProcessClientSupplementalData(IList clientSupplementalData)
                {
                    if (clientSupplementalData != null)
                        throw new TlsFatalAlert(AlertDescription.unexpected_message);
                }

                public virtual void NotifyClientCertificate(Certificate clientCertificate)
                {
                    throw new TlsFatalAlert(AlertDescription.internal_error);
                }

                public override TlsCompression GetCompression()
                {
                    switch (mSelectedCompressionMethod)
                    {
                        case CompressionMethod.cls_null:
                            return new TlsNullCompression();

                        default:
                            /*
                             * Note: internal error here; we selected the compression method, so if we now can't
                             * produce an implementation, we shouldn't have chosen it!
                             */
                            throw new TlsFatalAlert(AlertDescription.internal_error);
                    }
                }

                public override TlsCipher GetCipher()
                {
                    var encryptionAlgorithm = TlsUtilities.GetEncryptionAlgorithm(mSelectedCipherSuite);
                    var macAlgorithm = TlsUtilities.GetMacAlgorithm(mSelectedCipherSuite);

                    return mCipherFactory.CreateCipher(mContext, encryptionAlgorithm, macAlgorithm);
                }

                public virtual NewSessionTicket GetNewSessionTicket()
                {
                    /*
                     * RFC 5077 3.3. If the server determines that it does not want to include a ticket after it
                     * has included the SessionTicket extension in the ServerHello, then it sends a zero-length
                     * ticket in the NewSessionTicket handshake message.
                     */
                    return new NewSessionTicket(0L, TlsUtilities.EmptyBytes);
                }

                protected virtual IDictionary CheckServerExtensions()
                {
                    return mServerExtensions =
                        TlsExtensionsUtilities.EnsureExtensionsInitialised(mServerExtensions);
                }

                protected abstract int[] GetCipherSuites();

                protected byte[] GetCompressionMethods()
                {
                    return new[] { CompressionMethod.cls_null };
                }

                protected virtual bool SupportsClientEccCapabilities(int[] namedCurves,
                    byte[] ecPointFormats)
                {
                    // NOTE: BC supports all the current set of point formats so we don't check them here

                    if (namedCurves == null)
                        /*
                             * RFC 4492 4. A client that proposes ECC cipher suites may choose not to include these
                             * extensions. In this case, the server is free to choose any one of the elliptic curves
                             * or point formats [...].
                             */
                            return TlsEccUtilities.HasAnySupportedNamedCurves();

                        for (var i = 0; i < namedCurves.Length; ++i)
                        {
                            var namedCurve = namedCurves[i];
                            if (NamedCurve.IsValid(namedCurve)
                                && (!NamedCurve.RefersToASpecificNamedCurve(namedCurve) ||
                                    TlsEccUtilities.IsSupportedNamedCurve(namedCurve)))
                                return true;
                        }

                        return false;
                        }
                        }
                        }

#endif