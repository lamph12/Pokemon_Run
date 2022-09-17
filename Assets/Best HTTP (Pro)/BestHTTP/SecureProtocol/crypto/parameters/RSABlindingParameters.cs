#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RsaBlindingParameters
        : ICipherParameters
    {
        public RsaBlindingParameters(
            RsaKeyParameters publicKey,
            BigInteger blindingFactor)
        {
            if (publicKey.IsPrivate)
                throw new ArgumentException("RSA parameters should be for a public key");

            this.PublicKey = publicKey;
            this.BlindingFactor = blindingFactor;
        }

        public RsaKeyParameters PublicKey { get; }

        public BigInteger BlindingFactor { get; }
    }
}

#endif