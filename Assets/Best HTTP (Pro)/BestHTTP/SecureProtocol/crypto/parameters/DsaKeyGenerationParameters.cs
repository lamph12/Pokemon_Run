#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DsaKeyGenerationParameters
        : KeyGenerationParameters
    {
        public DsaKeyGenerationParameters(
            SecureRandom random,
            DsaParameters parameters)
            : base(random, parameters.P.BitLength - 1)
        {
            this.Parameters = parameters;
        }

        public DsaParameters Parameters { get; }
    }
}

#endif