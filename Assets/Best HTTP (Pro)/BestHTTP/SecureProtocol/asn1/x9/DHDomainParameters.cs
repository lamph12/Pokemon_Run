#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X9
{
    public class DHDomainParameters
        : Asn1Encodable
    {
        public DHDomainParameters(DerInteger p, DerInteger g, DerInteger q, DerInteger j,
            DHValidationParms validationParms)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            if (g == null)
                throw new ArgumentNullException("g");
            if (q == null)
                throw new ArgumentNullException("q");

            this.P = p;
            this.G = g;
            this.Q = q;
            this.J = j;
            this.ValidationParms = validationParms;
        }

        private DHDomainParameters(Asn1Sequence seq)
        {
            if (seq.Count < 3 || seq.Count > 5)
                throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

            var e = seq.GetEnumerator();
            P = DerInteger.GetInstance(GetNext(e));
            G = DerInteger.GetInstance(GetNext(e));
            Q = DerInteger.GetInstance(GetNext(e));

            var next = GetNext(e);

            if (next != null && next is DerInteger)
            {
                J = DerInteger.GetInstance(next);
                next = GetNext(e);
            }

            if (next != null) ValidationParms = DHValidationParms.GetInstance(next.ToAsn1Object());
        }

        public DerInteger P { get; }

        public DerInteger G { get; }

        public DerInteger Q { get; }

        public DerInteger J { get; }

        public DHValidationParms ValidationParms { get; }

        public static DHDomainParameters GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        public static DHDomainParameters GetInstance(object obj)
        {
            if (obj == null || obj is DHDomainParameters)
                return (DHDomainParameters)obj;

            if (obj is Asn1Sequence)
                return new DHDomainParameters((Asn1Sequence)obj);

            throw new ArgumentException("Invalid DHDomainParameters: " + Platform.GetTypeName(obj), "obj");
        }

        private static Asn1Encodable GetNext(IEnumerator e)
        {
            return e.MoveNext() ? (Asn1Encodable)e.Current : null;
        }

        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector(P, G, Q);

            if (J != null) v.Add(J);

            if (ValidationParms != null) v.Add(ValidationParms);

            return new DerSequence(v);
        }
    }
}

#endif