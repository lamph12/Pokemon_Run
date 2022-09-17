#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Generators
{
	/**
     * a basic Diffie-Hellman key pair generator.
     * 
     * This generates keys consistent for use with the basic algorithm for
     * Diffie-Hellman.
     */
	public class DHBasicKeyPairGenerator
        : IAsymmetricCipherKeyPairGenerator
    {
        private DHKeyGenerationParameters param;

        public virtual void Init(
            KeyGenerationParameters parameters)
        {
            param = (DHKeyGenerationParameters)parameters;
        }

        public virtual AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var helper = DHKeyGeneratorHelper.Instance;
            var dhp = param.Parameters;

            var x = helper.CalculatePrivate(dhp, param.Random);
            var y = helper.CalculatePublic(dhp, x);

            return new AsymmetricCipherKeyPair(
                new DHPublicKeyParameters(y, dhp),
                new DHPrivateKeyParameters(x, dhp));
        }
    }
}

#endif