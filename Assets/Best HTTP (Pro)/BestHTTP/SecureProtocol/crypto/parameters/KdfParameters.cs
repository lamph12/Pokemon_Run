#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    /**
     * parameters for Key derivation functions for IEEE P1363a
     */
    public class KdfParameters : IDerivationParameters
    {
        private readonly byte[] iv;
        private readonly byte[] shared;

        public KdfParameters(
            byte[] shared,
            byte[] iv)
        {
            this.shared = shared;
            this.iv = iv;
        }

        public byte[] GetSharedSecret()
        {
            return shared;
        }

        public byte[] GetIV()
        {
            return iv;
        }
    }
}

#endif