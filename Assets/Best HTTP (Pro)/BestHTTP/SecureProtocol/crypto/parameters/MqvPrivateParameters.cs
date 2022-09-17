#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class MqvPrivateParameters
        : ICipherParameters
    {
        public MqvPrivateParameters(
            ECPrivateKeyParameters staticPrivateKey,
            ECPrivateKeyParameters ephemeralPrivateKey)
            : this(staticPrivateKey, ephemeralPrivateKey, null)
        {
        }

        public MqvPrivateParameters(
            ECPrivateKeyParameters staticPrivateKey,
            ECPrivateKeyParameters ephemeralPrivateKey,
            ECPublicKeyParameters ephemeralPublicKey)
        {
            if (staticPrivateKey == null)
                throw new ArgumentNullException("staticPrivateKey");
            if (ephemeralPrivateKey == null)
                throw new ArgumentNullException("ephemeralPrivateKey");

            var parameters = staticPrivateKey.Parameters;
            if (!parameters.Equals(ephemeralPrivateKey.Parameters))
                throw new ArgumentException("Static and ephemeral private keys have different domain parameters");

            if (ephemeralPublicKey == null)
                ephemeralPublicKey = new ECPublicKeyParameters(
                    parameters.G.Multiply(ephemeralPrivateKey.D),
                    parameters);
            else if (!parameters.Equals(ephemeralPublicKey.Parameters))
                throw new ArgumentException("Ephemeral public key has different domain parameters");

            this.StaticPrivateKey = staticPrivateKey;
            this.EphemeralPrivateKey = ephemeralPrivateKey;
            this.EphemeralPublicKey = ephemeralPublicKey;
        }

        public virtual ECPrivateKeyParameters StaticPrivateKey { get; }

        public virtual ECPrivateKeyParameters EphemeralPrivateKey { get; }

        public virtual ECPublicKeyParameters EphemeralPublicKey { get; }
    }
}

#endif