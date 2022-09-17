#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RsaPrivateCrtKeyParameters
        : RsaKeyParameters
    {
        public RsaPrivateCrtKeyParameters(
            BigInteger modulus,
            BigInteger publicExponent,
            BigInteger privateExponent,
            BigInteger p,
            BigInteger q,
            BigInteger dP,
            BigInteger dQ,
            BigInteger qInv)
            : base(true, modulus, privateExponent)
        {
            ValidateValue(publicExponent, "publicExponent", "exponent");
            ValidateValue(p, "p", "P value");
            ValidateValue(q, "q", "Q value");
            ValidateValue(dP, "dP", "DP value");
            ValidateValue(dQ, "dQ", "DQ value");
            ValidateValue(qInv, "qInv", "InverseQ value");

            PublicExponent = publicExponent;
            this.P = p;
            this.Q = q;
            this.DP = dP;
            this.DQ = dQ;
            this.QInv = qInv;
        }

        public BigInteger PublicExponent { get; }

        public BigInteger P { get; }

        public BigInteger Q { get; }

        public BigInteger DP { get; }

        public BigInteger DQ { get; }

        public BigInteger QInv { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var kp = obj as RsaPrivateCrtKeyParameters;

            if (kp == null)
                return false;

            return kp.DP.Equals(DP)
                   && kp.DQ.Equals(DQ)
                   && kp.Exponent.Equals(Exponent)
                   && kp.Modulus.Equals(Modulus)
                   && kp.P.Equals(P)
                   && kp.Q.Equals(Q)
                   && kp.PublicExponent.Equals(PublicExponent)
                   && kp.QInv.Equals(QInv);
        }

        public override int GetHashCode()
        {
            return DP.GetHashCode() ^ DQ.GetHashCode() ^ Exponent.GetHashCode() ^ Modulus.GetHashCode()
                   ^ P.GetHashCode() ^ Q.GetHashCode() ^ PublicExponent.GetHashCode() ^ QInv.GetHashCode();
        }

        private static void ValidateValue(BigInteger x, string name, string desc)
        {
            if (x == null)
                throw new ArgumentNullException(name);
            if (x.SignValue <= 0)
                throw new ArgumentException("Not a valid RSA " + desc, name);
        }
    }
}

#endif