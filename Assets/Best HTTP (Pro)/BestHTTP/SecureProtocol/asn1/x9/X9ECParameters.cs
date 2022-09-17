#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.Field;

namespace Org.BouncyCastle.Asn1.X9
{
    /**
     * ASN.1 def for Elliptic-Curve ECParameters structure. See
     * X9.62, for further details.
     */
    public class X9ECParameters
        : Asn1Encodable
    {
        private readonly byte[] seed;

        public X9ECParameters(
            Asn1Sequence seq)
        {
            if (!(seq[0] is DerInteger)
                || !((DerInteger)seq[0]).Value.Equals(BigInteger.One))
                throw new ArgumentException("bad version in X9ECParameters");

            var x9c = new X9Curve(
                X9FieldID.GetInstance(seq[1]),
                Asn1Sequence.GetInstance(seq[2]));

            Curve = x9c.Curve;
            object p = seq[3];

            if (p is X9ECPoint)
                BaseEntry = (X9ECPoint)p;
            else
                BaseEntry = new X9ECPoint(Curve, (Asn1OctetString)p);

            N = ((DerInteger)seq[4]).Value;
            seed = x9c.GetSeed();

            if (seq.Count == 6) H = ((DerInteger)seq[5]).Value;
        }

        public X9ECParameters(
            ECCurve curve,
            ECPoint g,
            BigInteger n)
            : this(curve, g, n, null, null)
        {
        }

        public X9ECParameters(
            ECCurve curve,
            X9ECPoint g,
            BigInteger n,
            BigInteger h)
            : this(curve, g, n, h, null)
        {
        }

        public X9ECParameters(
            ECCurve curve,
            ECPoint g,
            BigInteger n,
            BigInteger h)
            : this(curve, g, n, h, null)
        {
        }

        public X9ECParameters(
            ECCurve curve,
            ECPoint g,
            BigInteger n,
            BigInteger h,
            byte[] seed)
            : this(curve, new X9ECPoint(g), n, h, seed)
        {
        }

        public X9ECParameters(
            ECCurve curve,
            X9ECPoint g,
            BigInteger n,
            BigInteger h,
            byte[] seed)
        {
            Curve = curve;
            BaseEntry = g;
            N = n;
            H = h;
            this.seed = seed;

            if (ECAlgorithms.IsFpCurve(curve))
            {
                FieldIDEntry = new X9FieldID(curve.Field.Characteristic);
            }
            else if (ECAlgorithms.IsF2mCurve(curve))
            {
                var field = (IPolynomialExtensionField)curve.Field;
                var exponents = field.MinimalPolynomial.GetExponentsPresent();
                if (exponents.Length == 3)
                    FieldIDEntry = new X9FieldID(exponents[2], exponents[1]);
                else if (exponents.Length == 5)
                    FieldIDEntry = new X9FieldID(exponents[4], exponents[1], exponents[2], exponents[3]);
                else
                    throw new ArgumentException("Only trinomial and pentomial curves are supported");
            }
            else
            {
                throw new ArgumentException("'curve' is of an unsupported type");
            }
        }

        public ECCurve Curve { get; }

        public ECPoint G => BaseEntry.Point;

        public BigInteger N { get; }

        public BigInteger H { get; }

        /**
         * Return the ASN.1 entry representing the Curve.
         * 
         * @return the X9Curve for the curve in these parameters.
         */
        public X9Curve CurveEntry => new X9Curve(Curve, seed);

        /**
         * Return the ASN.1 entry representing the FieldID.
         * 
         * @return the X9FieldID for the FieldID in these parameters.
         */
        public X9FieldID FieldIDEntry { get; }

        /**
         * Return the ASN.1 entry representing the base point G.
         * 
         * @return the X9ECPoint for the base point in these parameters.
         */
        public X9ECPoint BaseEntry { get; }

        public static X9ECParameters GetInstance(object obj)
        {
            if (obj is X9ECParameters) return (X9ECParameters)obj;

            if (obj != null) return new X9ECParameters(Asn1Sequence.GetInstance(obj));

            return null;
        }

        public byte[] GetSeed()
        {
            return seed;
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *     ECParameters ::= Sequence {
         *     version         Integer { ecpVer1(1) } (ecpVer1),
         *     fieldID         FieldID {{FieldTypes}},
         *     curve           X9Curve,
         *     base            X9ECPoint,
         *     order           Integer,
         *     cofactor        Integer OPTIONAL
         *     }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector(
                new DerInteger(BigInteger.One),
                FieldIDEntry,
                new X9Curve(Curve, seed),
                BaseEntry,
                new DerInteger(N));

            if (H != null) v.Add(new DerInteger(H));

            return new DerSequence(v);
        }
    }
}

#endif