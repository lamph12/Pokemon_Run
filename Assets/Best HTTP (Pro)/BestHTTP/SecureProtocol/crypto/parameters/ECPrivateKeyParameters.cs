#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ECPrivateKeyParameters
        : ECKeyParameters
    {
        public ECPrivateKeyParameters(
            BigInteger d,
            ECDomainParameters parameters)
            : this("EC", d, parameters)
        {
        }

        [Obsolete("Use version with explicit 'algorithm' parameter")]
        public ECPrivateKeyParameters(
            BigInteger d,
            DerObjectIdentifier publicKeyParamSet)
            : base("ECGOST3410", true, publicKeyParamSet)
        {
            if (d == null)
                throw new ArgumentNullException("d");

            this.D = d;
        }

        public ECPrivateKeyParameters(
            string algorithm,
            BigInteger d,
            ECDomainParameters parameters)
            : base(algorithm, true, parameters)
        {
            if (d == null)
                throw new ArgumentNullException("d");

            this.D = d;
        }

        public ECPrivateKeyParameters(
            string algorithm,
            BigInteger d,
            DerObjectIdentifier publicKeyParamSet)
            : base(algorithm, true, publicKeyParamSet)
        {
            if (d == null)
                throw new ArgumentNullException("d");

            this.D = d;
        }

        public BigInteger D { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as ECPrivateKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            ECPrivateKeyParameters other)
        {
            return D.Equals(other.D) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return D.GetHashCode() ^ base.GetHashCode();
        }
    }
}

#endif