#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ParametersWithSBox : ICipherParameters
    {
        private readonly byte[] sBox;

        public ParametersWithSBox(
            ICipherParameters parameters,
            byte[] sBox)
        {
            Parameters = parameters;
            this.sBox = sBox;
        }

        public ICipherParameters Parameters { get; }

        public byte[] GetSBox()
        {
            return sBox;
        }
    }
}

#endif