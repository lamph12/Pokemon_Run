#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class SecurityParameters
    {
        internal int cipherSuite = -1;
        internal byte[] clientRandom = null;
        internal byte compressionAlgorithm = CompressionMethod.cls_null;
        internal bool encryptThenMac = false;
        internal int entity = -1;
        internal bool extendedMasterSecret = false;
        internal byte[] masterSecret;

        // TODO Keep these internal, since it's maybe not the ideal place for them
        internal short maxFragmentLength = -1;
        internal int prfAlgorithm = -1;
        internal byte[] pskIdentity = null;
        internal byte[] serverRandom = null;
        internal byte[] sessionHash = null;
        internal byte[] srpIdentity = null;
        internal bool truncatedHMac = false;
        internal int verifyDataLength = -1;

        /**
         * @return {@link ConnectionEnd}
         */
        public virtual int Entity => entity;

        /**
         * @return {@link CipherSuite}
         */
        public virtual int CipherSuite => cipherSuite;

        /**
         * @return {@link CompressionMethod}
         */
        public byte CompressionAlgorithm => compressionAlgorithm;

        /**
         * @return {@link PRFAlgorithm}
         */
        public virtual int PrfAlgorithm => prfAlgorithm;

        public virtual int VerifyDataLength => verifyDataLength;

        public virtual byte[] MasterSecret => masterSecret;

        public virtual byte[] ClientRandom => clientRandom;

        public virtual byte[] ServerRandom => serverRandom;

        public virtual byte[] SessionHash => sessionHash;

        public virtual byte[] PskIdentity => pskIdentity;

        public virtual byte[] SrpIdentity => srpIdentity;

        internal virtual void Clear()
        {
            if (masterSecret != null)
            {
                Arrays.Fill(masterSecret, 0);
                masterSecret = null;
            }
        }
    }
}

#endif