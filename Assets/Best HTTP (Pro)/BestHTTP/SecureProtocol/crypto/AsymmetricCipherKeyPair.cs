#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto
{
    /**
     * a holding class for public/private parameter pairs.
     */
    public class AsymmetricCipherKeyPair
    {
        /**
         * basic constructor.
         * 
         * @param publicParam a public key parameters object.
         * @param privateParam the corresponding private key parameters.
         */
        public AsymmetricCipherKeyPair(
            AsymmetricKeyParameter publicParameter,
            AsymmetricKeyParameter privateParameter)
        {
            if (publicParameter.IsPrivate)
                throw new ArgumentException("Expected a public key", "publicParameter");
            if (!privateParameter.IsPrivate)
                throw new ArgumentException("Expected a private key", "privateParameter");

            this.Public = publicParameter;
            this.Private = privateParameter;
        }

        /**
         * return the public key parameters.
         * 
         * @return the public key parameters.
         */
        public AsymmetricKeyParameter Public { get; }

        /**
         * return the private key parameters.
         * 
         * @return the private key parameters.
         */
        public AsymmetricKeyParameter Private { get; }
    }
}

#endif