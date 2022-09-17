#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ElGamalPublicKeyParameters
        : ElGamalKeyParameters
    {
        public ElGamalPublicKeyParameters(
            BigInteger y,
            ElGamalParameters parameters)
            : base(false, parameters)
        {
            if (y == null)
                throw new ArgumentNullException("y");

            this.Y = y;
        }

        public BigInteger Y { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as ElGamalPublicKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            ElGamalPublicKeyParameters other)
        {
            return Y.Equals(other.Y) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return Y.GetHashCode() ^ base.GetHashCode();
        }
    }
}

#endif