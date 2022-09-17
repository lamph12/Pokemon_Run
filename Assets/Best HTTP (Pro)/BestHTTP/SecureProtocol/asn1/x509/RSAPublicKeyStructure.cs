#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
    public class RsaPublicKeyStructure
        : Asn1Encodable
    {
        public RsaPublicKeyStructure(
            BigInteger modulus,
            BigInteger publicExponent)
        {
            if (modulus == null)
                throw new ArgumentNullException("modulus");
            if (publicExponent == null)
                throw new ArgumentNullException("publicExponent");
            if (modulus.SignValue <= 0)
                throw new ArgumentException("Not a valid RSA modulus", "modulus");
            if (publicExponent.SignValue <= 0)
                throw new ArgumentException("Not a valid RSA public exponent", "publicExponent");

            Modulus = modulus;
            PublicExponent = publicExponent;
        }

        private RsaPublicKeyStructure(
            Asn1Sequence seq)
        {
            if (seq.Count != 2)
                throw new ArgumentException("Bad sequence size: " + seq.Count);

            // Note: we are accepting technically incorrect (i.e. negative) values here
            Modulus = DerInteger.GetInstance(seq[0]).PositiveValue;
            PublicExponent = DerInteger.GetInstance(seq[1]).PositiveValue;
        }

        public BigInteger Modulus { get; }

        public BigInteger PublicExponent { get; }

        public static RsaPublicKeyStructure GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static RsaPublicKeyStructure GetInstance(
            object obj)
        {
            if (obj == null || obj is RsaPublicKeyStructure) return (RsaPublicKeyStructure)obj;

            if (obj is Asn1Sequence) return new RsaPublicKeyStructure((Asn1Sequence)obj);

            throw new ArgumentException("Invalid RsaPublicKeyStructure: " + Platform.GetTypeName(obj));
        }

        /**
         * This outputs the key in Pkcs1v2 format.
         * <pre>
         *     RSAPublicKey ::= Sequence {
         *     modulus Integer, -- n
         *     publicExponent Integer, -- e
         *     }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(
                new DerInteger(Modulus),
                new DerInteger(PublicExponent));
        }
    }
}

#endif