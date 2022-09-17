#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto
{
    /**
	 * The base class for symmetric, or secret, cipher key generators.
	 */
    public class CipherKeyGenerator
    {
        protected internal SecureRandom random;
        protected internal int strength;
        private bool uninitialised = true;

        public CipherKeyGenerator()
        {
        }

        internal CipherKeyGenerator(
            int defaultStrength)
        {
            if (defaultStrength < 1)
                throw new ArgumentException("strength must be a positive value", "defaultStrength");

            DefaultStrength = defaultStrength;
        }

        public int DefaultStrength { get; }

        /**
         * initialise the key generator.
         * 
         * @param param the parameters to be used for key generation
         */
        public void Init(
            KeyGenerationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            uninitialised = false;

            engineInit(parameters);
        }

        protected virtual void engineInit(
            KeyGenerationParameters parameters)
        {
            random = parameters.Random;
            strength = (parameters.Strength + 7) / 8;
        }

        /**
         * Generate a secret key.
         * 
         * @return a byte array containing the key value.
         */
        public byte[] GenerateKey()
        {
            if (uninitialised)
            {
                if (DefaultStrength < 1)
                    throw new InvalidOperationException("Generator has not been initialised");

                uninitialised = false;

                engineInit(new KeyGenerationParameters(new SecureRandom(), DefaultStrength));
            }

            return engineGenerateKey();
        }

        protected virtual byte[] engineGenerateKey()
        {
            return SecureRandom.GetNextBytes(random, strength);
        }
    }
}

#endif