#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ElGamalKeyGenerationParameters
        : KeyGenerationParameters
    {
        public ElGamalKeyGenerationParameters(
            SecureRandom random,
            ElGamalParameters parameters)
            : base(random, GetStrength(parameters))
        {
            this.Parameters = parameters;
        }

        public ElGamalParameters Parameters { get; }

        internal static int GetStrength(
            ElGamalParameters parameters)
        {
            return parameters.L != 0 ? parameters.L : parameters.P.BitLength;
        }
    }
}

#endif