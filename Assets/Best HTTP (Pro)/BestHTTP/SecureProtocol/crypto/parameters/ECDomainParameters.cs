#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ECDomainParameters
    {
        internal ECCurve curve;
        internal ECPoint g;
        internal BigInteger h;
        internal BigInteger n;
        internal byte[] seed;

        public ECDomainParameters(
            ECCurve curve,
            ECPoint g,
            BigInteger n)
            : this(curve, g, n, BigInteger.One)
        {
        }

        public ECDomainParameters(
            ECCurve curve,
            ECPoint g,
            BigInteger n,
            BigInteger h)
            : this(curve, g, n, h, null)
        {
        }

        public ECDomainParameters(
            ECCurve curve,
            ECPoint g,
            BigInteger n,
            BigInteger h,
            byte[] seed)
        {
            if (curve == null)
                throw new ArgumentNullException("curve");
            if (g == null)
                throw new ArgumentNullException("g");
            if (n == null)
                throw new ArgumentNullException("n");
            if (h == null)
                throw new ArgumentNullException("h");

            this.curve = curve;
            this.g = g.Normalize();
            this.n = n;
            this.h = h;
            this.seed = Arrays.Clone(seed);
        }

        public ECCurve Curve => curve;

        public ECPoint G => g;

        public BigInteger N => n;

        public BigInteger H => h;

        public byte[] GetSeed()
        {
            return Arrays.Clone(seed);
        }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as ECDomainParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected virtual bool Equals(
            ECDomainParameters other)
        {
            return curve.Equals(other.curve)
                   && g.Equals(other.g)
                   && n.Equals(other.n)
                   && h.Equals(other.h);
        }

        public override int GetHashCode()
        {
            var hc = curve.GetHashCode();
            hc *= 37;
            hc ^= g.GetHashCode();
            hc *= 37;
            hc ^= n.GetHashCode();
            hc *= 37;
            hc ^= h.GetHashCode();
            return hc;
        }
    }
}

#endif