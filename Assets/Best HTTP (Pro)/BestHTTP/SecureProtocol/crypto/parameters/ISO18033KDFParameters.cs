#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    /**
	* parameters for Key derivation functions for ISO-18033
	*/
    public class Iso18033KdfParameters
        : IDerivationParameters
    {
        private readonly byte[] seed;

        public Iso18033KdfParameters(
            byte[] seed)
        {
            this.seed = seed;
        }

        public byte[] GetSeed()
        {
            return seed;
        }
    }
}

#endif