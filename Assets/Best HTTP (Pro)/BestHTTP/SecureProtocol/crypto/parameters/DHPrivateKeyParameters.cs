#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DHPrivateKeyParameters
        : DHKeyParameters
    {
        public DHPrivateKeyParameters(
            BigInteger x,
            DHParameters parameters)
            : base(true, parameters)
        {
            this.X = x;
        }

        public DHPrivateKeyParameters(
            BigInteger x,
            DHParameters parameters,
            DerObjectIdentifier algorithmOid)
            : base(true, parameters, algorithmOid)
        {
            this.X = x;
        }

        public BigInteger X { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DHPrivateKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            DHPrivateKeyParameters other)
        {
            return X.Equals(other.X) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ base.GetHashCode();
        }
    }
}

#endif