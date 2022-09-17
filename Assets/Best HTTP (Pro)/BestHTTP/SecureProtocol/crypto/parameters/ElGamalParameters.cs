#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ElGamalParameters
        : ICipherParameters
    {
        public ElGamalParameters(
            BigInteger p,
            BigInteger g)
            : this(p, g, 0)
        {
        }

        public ElGamalParameters(
            BigInteger p,
            BigInteger g,
            int l)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            if (g == null)
                throw new ArgumentNullException("g");

            this.P = p;
            this.G = g;
            this.L = l;
        }

        public BigInteger P { get; }

        /**
        * return the generator - g
        */
        public BigInteger G { get; }

        /**
		 * return private value limit - l
		 */
        public int L { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as ElGamalParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            ElGamalParameters other)
        {
            return P.Equals(other.P) && G.Equals(other.G) && L == other.L;
        }

        public override int GetHashCode()
        {
            return P.GetHashCode() ^ G.GetHashCode() ^ L;
        }
    }
}

#endif