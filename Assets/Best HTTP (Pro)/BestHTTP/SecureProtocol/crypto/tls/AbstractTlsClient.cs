#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;

namespace Org.BouncyCastle.Crypto.Tls
{
    public abstract class AbstractTlsClient
        : AbstractTlsPeer, TlsClient
    {
        protected TlsCipherFactory mCipherFactory;
        protected byte[] mClientECPointFormats, mServerECPointFormats;

        protected TlsClientContext mContext;
        protected int[] mNamedCurves;

        protected int mSelectedCipherSuite;
        protected short mSelectedCompressionMethod;

        protected IList mSupportedSignatureAlgorithms;

        public AbstractTlsClient()
            : this(new DefaultTlsCipherFactory())
        {
        }

        public AbstractTlsClient(TlsCipherFactory cipherFactory)
        {
            mCipherFactory = cipherFactory;
        }

        public virtual ProtocolVersion MinimumVersion => ProtocolVersion.TLSv10;

        public List<string> HostNames { get; set; }

        public virtual void Init(TlsClientContext context)
        {
            mContext = context;
        }

        public virtual TlsSession GetSessionToResume()
        {
            return null;
        }

        public virtual ProtocolVersion ClientHelloRecordLayerVersion =>
            // "{03,00}"
            //return ProtocolVersion.SSLv3;
            // "the lowest version number supported by the client"
            //return MinimumVersion;
            // "the value of ClientHello.client_version"
            ClientVersion;

        public virtual ProtocolVersion ClientVersion => ProtocolVersion.TLSv12;

        public virtual bool IsFallback =>
            /*
             * RFC 7507 4. The TLS_FALLBACK_SCSV cipher suite value is meant for use by clients that
             * repeat a connection attempt with a downgraded protocol (perform a "fallback retry") in
             * order to work around interoperability problems with legacy servers.
             */
            false;

        public virtual IDictionary GetClientExtensions()
        {
            IDictionary clientExtensions = null;

            var clientVersion = mContext.ClientVersion;

            /*
             * RFC 5246 7.4.1.4.1. Note: this extension is not meaningful for TLS versions prior to 1.2.
             * Clients MUST NOT offer it if they are offering prior versions.
             */
            if (TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(clientVersion))
            {
                // TODO Provide a way for the user to specify the acceptable hash/signature algorithms.

                mSupportedSignatureAlgorithms = TlsUtilities.GetDefaultSupportedSignatureAlgorithms();

                clientExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(clientExtensions);

                TlsUtilities.AddSignatureAlgorithmsExtension(clientExtensions, mSupportedSignatureAlgorithms);
            }

            if (TlsEccUtilities.ContainsEccCipherSuites(GetCipherSuites()))
            {
                /*
                 * RFC 4492 5.1. A client that proposes ECC cipher suites in its ClientHello message
                 * appends these extensions (along with any others), enumerating the curves it supports
                 * and the point formats it can parse. Clients SHOULD send both the Supported Elliptic
                 * Curves Extension and the Supported Point Formats Extension.
                 */
                /*
                 * TODO Could just add all the curves since we support them all, but users may not want
                 * to use unnecessarily large fields. Need configuration options.
                 */
                mNamedCurves = new[] { NamedCurve.secp256r1, NamedCurve.secp384r1 };
                mClientECPointFormats = new[]
                {
                    ECPointFormat.uncompressed,
                    ECPointFormat.ansiX962_compressed_prime, ECPointFormat.ansiX962_compressed_char2
                };

                clientExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(clientExtensions);

                TlsEccUtilities.AddSupportedEllipticCurvesExtension(clientExtensions, mNamedCurves);
                TlsEccUtilities.AddSupportedPointFormatsExtension(clientExtensions, mClientECPointFormats);
            }

            if (HostNames != null && HostNames.Count > 0)
            {
                var list = new List<ServerName>(HostNames.Count);

                for (var i = 0; i < HostNames.Count; ++i)
                    list.Add(new ServerName(NameType.host_name, HostNames[i]));

                TlsExtensionsUtilities.AddServerNameExtension(clientExtensions, new ServerNameList(list));
            }

            return clientExtensions;
        }

        public virtual void NotifyServerVersion(ProtocolVersion serverVersion)
        {
            if (!MinimumVersion.IsEqualOrEarlierVersionOf(serverVersion))
                throw new TlsFatalAlert(AlertDescription.protocol_version);
        }

        public abstract int[] GetCipherSuites();

        public virtual byte[] GetCompressionMethods()
        {
            return new[] { CompressionMethod.cls_null };
        }

        public virtual void NotifySessionID(byte[] sessionID)
        {
            // Currently ignored
        }

        public virtual void NotifySelectedCipherSuite(int selectedCipherSuite)
        {
            mSelectedCipherSuite = selectedCipherSuite;
        }

        public virtual void NotifySelectedCompressionMethod(byte selectedCompressionMethod)
        {
            mSelectedCompressionMethod = selectedCompressionMethod;
        }

        public virtual void ProcessServerExtensions(IDictionary serverExtensions)
        {
            /*
             * TlsProtocol implementation validates that any server extensions received correspond to
             * client extensions sent. By default, we don't send any, and this method is not called.
             */
            if (serverExtensions != null)
            {
                /*
                 * RFC 5246 7.4.1.4.1. Servers MUST NOT send this extension.
                 */
                CheckForUnexpectedServerExtension(serverExtensions, ExtensionType.signature_algorithms);

                CheckForUnexpectedServerExtension(serverExtensions, ExtensionType.elliptic_curves);

                if (TlsEccUtilities.IsEccCipherSuite(mSelectedCipherSuite))
                    mServerECPointFormats = TlsEccUtilities.GetSupportedPointFormatsExtension(serverExtensions);
                else
                    CheckForUnexpectedServerExtension(serverExtensions, ExtensionType.ec_point_formats);

                /*
                 * RFC 7685 3. The server MUST NOT echo the extension.
                 */
                CheckForUnexpectedServerExtension(serverExtensions, ExtensionType.padding);
            }
        }

        public virtual void ProcessServerSupplementalData(IList serverSupplementalData)
        {
            if (serverSupplementalData != null)
                throw new TlsFatalAlert(AlertDescription.unexpected_message);
        }

        public abstract TlsKeyExchange GetKeyExchange();

        public abstract TlsAuthentication GetAuthentication();

        public virtual IList GetClientSupplementalData()
        {
            return null;
        }

        public override TlsCompression GetCompression()
        {
            switch (mSelectedCompressionMethod)
            {
                case CompressionMethod.cls_null:
                    return new TlsNullCompression();

                case CompressionMethod.DEFLATE:
                    return new TlsDeflateCompression();

                default:
                    /*
                     * Note: internal error here; the TlsProtocol implementation verifies that the
                     * server-selected compression method was in the list of client-offered compression
                     * methods, so if we now can't produce an implementation, we shouldn't have offered it!
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

        public virtual void NotifyNewSessionTicket(NewSessionTicket newSessionTicket)
        {
        }

        protected virtual bool AllowUnexpectedServerExtension(int extensionType, byte[] extensionData)
        {
            switch (extensionType)
            {
                case ExtensionType.elliptic_curves:
                    /*
                     * Exception added based on field reports that some servers do send this, although the
                     * Supported Elliptic Curves Extension is clearly intended to be client-only. If
                     * present, we still require that it is a valid EllipticCurveList.
                     */
                    TlsEccUtilities.ReadSupportedEllipticCurvesExtension(extensionData);
                    return true;
                default:
                    return false;
            }
        }

        protected virtual void CheckForUnexpectedServerExtension(IDictionary serverExtensions, int extensionType)
        {
            var extensionData = TlsUtilities.GetExtensionData(serverExtensions, extensionType);
            if (extensionData != null && !AllowUnexpectedServerExtension(extensionType, extensionData))
                throw new TlsFatalAlert(AlertDescription.illegal_parameter);
        }
    }
}

#endif