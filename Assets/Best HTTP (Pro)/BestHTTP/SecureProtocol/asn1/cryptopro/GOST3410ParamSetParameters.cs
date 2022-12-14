#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.CryptoPro
{
    public class Gost3410ParamSetParameters
        : Asn1Encodable
    {
        private readonly DerInteger p, q, a;

        public Gost3410ParamSetParameters(
            int keySize,
            BigInteger p,
            BigInteger q,
            BigInteger a)
        {
            this.KeySize = keySize;
            this.p = new DerInteger(p);
            this.q = new DerInteger(q);
            this.a = new DerInteger(a);
        }

        private Gost3410ParamSetParameters(
            Asn1Sequence seq)
        {
            if (seq.Count != 4)
                throw new ArgumentException("Wrong number of elements in sequence", "seq");

            KeySize = DerInteger.GetInstance(seq[0]).Value.IntValue;
            p = DerInteger.GetInstance(seq[1]);
            q = DerInteger.GetInstance(seq[2]);
            a = DerInteger.GetInstance(seq[3]);
        }

        public int KeySize { get; }

        public BigInteger P => p.PositiveValue;

        public BigInteger Q => q.PositiveValue;

        public BigInteger A => a.PositiveValue;

        public static Gost3410ParamSetParameters GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static Gost3410ParamSetParameters GetInstance(
            object obj)
        {
            if (obj == null || obj is Gost3410ParamSetParameters) return (Gost3410ParamSetParameters)obj;

            if (obj is Asn1Sequence) return new Gost3410ParamSetParameters((Asn1Sequence)obj);

            throw new ArgumentException("Invalid GOST3410Parameter: " + Platform.GetTypeName(obj));
        }

        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(new DerInteger(KeySize), p, q, a);
        }
    }
}

#endif