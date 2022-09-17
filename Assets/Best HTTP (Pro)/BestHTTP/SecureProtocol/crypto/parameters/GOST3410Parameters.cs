#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class Gost3410Parameters
        : ICipherParameters
    {
        public Gost3410Parameters(
            BigInteger p,
            BigInteger q,
            BigInteger a)
            : this(p, q, a, null)
        {
        }

        public Gost3410Parameters(
            BigInteger p,
            BigInteger q,
            BigInteger a,
            Gost3410ValidationParameters validation)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            if (q == null)
                throw new ArgumentNullException("q");
            if (a == null)
                throw new ArgumentNullException("a");

            this.P = p;
            this.Q = q;
            this.A = a;
            this.ValidationParameters = validation;
        }

        public BigInteger P { get; }

        public BigInteger Q { get; }

        public BigInteger A { get; }

        public Gost3410ValidationParameters ValidationParameters { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as Gost3410Parameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            Gost3410Parameters other)
        {
            return P.Equals(other.P) && Q.Equals(other.Q) && A.Equals(other.A);
        }

        public override int GetHashCode()
        {
            return P.GetHashCode() ^ Q.GetHashCode() ^ A.GetHashCode();
        }
    }
}

#endif