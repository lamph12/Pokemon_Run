#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DsaPrivateKeyParameters
        : DsaKeyParameters
    {
        public DsaPrivateKeyParameters(
            BigInteger x,
            DsaParameters parameters)
            : base(true, parameters)
        {
            if (x == null)
                throw new ArgumentNullException("x");

            this.X = x;
        }

        public BigInteger X { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DsaPrivateKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            DsaPrivateKeyParameters other)
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