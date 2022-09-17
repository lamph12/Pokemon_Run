#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RsaKeyParameters
        : AsymmetricKeyParameter
    {
        public RsaKeyParameters(
            bool isPrivate,
            BigInteger modulus,
            BigInteger exponent)
            : base(isPrivate)
        {
            if (modulus == null)
                throw new ArgumentNullException("modulus");
            if (exponent == null)
                throw new ArgumentNullException("exponent");
            if (modulus.SignValue <= 0)
                throw new ArgumentException("Not a valid RSA modulus", "modulus");
            if (exponent.SignValue <= 0)
                throw new ArgumentException("Not a valid RSA exponent", "exponent");

            this.Modulus = modulus;
            this.Exponent = exponent;
        }

        public BigInteger Modulus { get; }

        public BigInteger Exponent { get; }

        public override bool Equals(
            object obj)
        {
            var kp = obj as RsaKeyParameters;

            if (kp == null) return false;

            return kp.IsPrivate == IsPrivate
                   && kp.Modulus.Equals(Modulus)
                   && kp.Exponent.Equals(Exponent);
        }

        public override int GetHashCode()
        {
            return Modulus.GetHashCode() ^ Exponent.GetHashCode() ^ IsPrivate.GetHashCode();
        }
    }
}

#endif