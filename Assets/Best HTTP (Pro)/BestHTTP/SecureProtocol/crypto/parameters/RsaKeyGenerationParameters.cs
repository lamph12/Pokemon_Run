#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RsaKeyGenerationParameters
        : KeyGenerationParameters
    {
        public RsaKeyGenerationParameters(
            BigInteger publicExponent,
            SecureRandom random,
            int strength,
            int certainty)
            : base(random, strength)
        {
            this.PublicExponent = publicExponent;
            this.Certainty = certainty;
        }

        public BigInteger PublicExponent { get; }

        public int Certainty { get; }

        public override bool Equals(
            object obj)
        {
            var other = obj as RsaKeyGenerationParameters;

            if (other == null) return false;

            return Certainty == other.Certainty
                   && PublicExponent.Equals(other.PublicExponent);
        }

        public override int GetHashCode()
        {
            return Certainty.GetHashCode() ^ PublicExponent.GetHashCode();
        }
    }
}

#endif