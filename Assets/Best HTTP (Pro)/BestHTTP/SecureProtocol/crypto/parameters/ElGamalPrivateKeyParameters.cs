#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ElGamalPrivateKeyParameters
        : ElGamalKeyParameters
    {
        public ElGamalPrivateKeyParameters(
            BigInteger x,
            ElGamalParameters parameters)
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

            var other = obj as ElGamalPrivateKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            ElGamalPrivateKeyParameters other)
        {
            return other.X.Equals(X) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ base.GetHashCode();
        }
    }
}

#endif