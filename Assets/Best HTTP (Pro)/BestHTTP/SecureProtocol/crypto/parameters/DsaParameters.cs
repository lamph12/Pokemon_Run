#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DsaParameters
        : ICipherParameters
    {
        public DsaParameters(
            BigInteger p,
            BigInteger q,
            BigInteger g)
            : this(p, q, g, null)
        {
        }

        public DsaParameters(
            BigInteger p,
            BigInteger q,
            BigInteger g,
            DsaValidationParameters parameters)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            if (q == null)
                throw new ArgumentNullException("q");
            if (g == null)
                throw new ArgumentNullException("g");

            this.P = p;
            this.Q = q;
            this.G = g;
            ValidationParameters = parameters;
        }

        public BigInteger P { get; }

        public BigInteger Q { get; }

        public BigInteger G { get; }

        public DsaValidationParameters ValidationParameters { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DsaParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            DsaParameters other)
        {
            return P.Equals(other.P) && Q.Equals(other.Q) && G.Equals(other.G);
        }

        public override int GetHashCode()
        {
            return P.GetHashCode() ^ Q.GetHashCode() ^ G.GetHashCode();
        }
    }
}

#endif