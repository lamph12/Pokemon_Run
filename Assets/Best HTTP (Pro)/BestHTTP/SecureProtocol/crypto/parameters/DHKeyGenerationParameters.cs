#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DHKeyGenerationParameters
        : KeyGenerationParameters
    {
        public DHKeyGenerationParameters(
            SecureRandom random,
            DHParameters parameters)
            : base(random, GetStrength(parameters))
        {
            this.Parameters = parameters;
        }

        public DHParameters Parameters { get; }

        internal static int GetStrength(
            DHParameters parameters)
        {
            return parameters.L != 0 ? parameters.L : parameters.P.BitLength;
        }
    }
}

#endif