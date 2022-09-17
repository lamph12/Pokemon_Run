#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public sealed class SessionParameters
    {
        private readonly byte[] mEncodedServerExtensions;

        private SessionParameters(int cipherSuite, byte compressionAlgorithm, byte[] masterSecret,
            Certificate peerCertificate, byte[] pskIdentity, byte[] srpIdentity, byte[] encodedServerExtensions)
        {
            CipherSuite = cipherSuite;
            CompressionAlgorithm = compressionAlgorithm;
            MasterSecret = Arrays.Clone(masterSecret);
            PeerCertificate = peerCertificate;
            PskIdentity = Arrays.Clone(pskIdentity);
            SrpIdentity = Arrays.Clone(srpIdentity);
            mEncodedServerExtensions = encodedServerExtensions;
        }

        public int CipherSuite { get; }

        public byte CompressionAlgorithm { get; }

        public byte[] MasterSecret { get; }

        public Certificate PeerCertificate { get; }

        public byte[] PskIdentity { get; }

        public byte[] SrpIdentity { get; }

        public void Clear()
        {
            if (MasterSecret != null) Arrays.Fill(MasterSecret, 0);
        }

        public SessionParameters Copy()
        {
            return new SessionParameters(CipherSuite, CompressionAlgorithm, MasterSecret, PeerCertificate,
                PskIdentity, SrpIdentity, mEncodedServerExtensions);
        }

        public IDictionary ReadServerExtensions()
        {
            if (mEncodedServerExtensions == null)
                return null;

            var buf = new MemoryStream(mEncodedServerExtensions, false);
            return TlsProtocol.ReadExtensions(buf);
        }

        public sealed class Builder
        {
            private int mCipherSuite = -1;
            private short mCompressionAlgorithm = -1;
            private byte[] mEncodedServerExtensions;
            private byte[] mMasterSecret;
            private Certificate mPeerCertificate;
            private byte[] mPskIdentity;
            private byte[] mSrpIdentity;

            public SessionParameters Build()
            {
                Validate(mCipherSuite >= 0, "cipherSuite");
                Validate(mCompressionAlgorithm >= 0, "compressionAlgorithm");
                Validate(mMasterSecret != null, "masterSecret");
                return new SessionParameters(mCipherSuite, (byte)mCompressionAlgorithm, mMasterSecret, mPeerCertificate,
                    mPskIdentity, mSrpIdentity, mEncodedServerExtensions);
            }

            public Builder SetCipherSuite(int cipherSuite)
            {
                mCipherSuite = cipherSuite;
                return this;
            }

            public Builder SetCompressionAlgorithm(byte compressionAlgorithm)
            {
                mCompressionAlgorithm = compressionAlgorithm;
                return this;
            }

            public Builder SetMasterSecret(byte[] masterSecret)
            {
                mMasterSecret = masterSecret;
                return this;
            }

            public Builder SetPeerCertificate(Certificate peerCertificate)
            {
                mPeerCertificate = peerCertificate;
                return this;
            }

            public Builder SetPskIdentity(byte[] pskIdentity)
            {
                mPskIdentity = pskIdentity;
                return this;
            }

            public Builder SetSrpIdentity(byte[] srpIdentity)
            {
                mSrpIdentity = srpIdentity;
                return this;
            }

            public Builder SetServerExtensions(IDictionary serverExtensions)
            {
                if (serverExtensions == null)
                {
                    mEncodedServerExtensions = null;
                }
                else
                {
                    var buf = new MemoryStream();
                    TlsProtocol.WriteExtensions(buf, serverExtensions);
                    mEncodedServerExtensions = buf.ToArray();
                }

                return this;
            }

            private void Validate(bool condition, string parameter)
            {
                if (!condition)
                    throw new InvalidOperationException("Required session parameter '" + parameter +
                                                        "' not configured");
            }
        }
    }
}

#endif