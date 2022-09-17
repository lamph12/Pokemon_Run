#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X9
{
    public class DHValidationParms
        : Asn1Encodable
    {
        public DHValidationParms(DerBitString seed, DerInteger pgenCounter)
        {
            if (seed == null)
                throw new ArgumentNullException("seed");
            if (pgenCounter == null)
                throw new ArgumentNullException("pgenCounter");

            this.Seed = seed;
            this.PgenCounter = pgenCounter;
        }

        private DHValidationParms(Asn1Sequence seq)
        {
            if (seq.Count != 2)
                throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

            Seed = DerBitString.GetInstance(seq[0]);
            PgenCounter = DerInteger.GetInstance(seq[1]);
        }

        public DerBitString Seed { get; }

        public DerInteger PgenCounter { get; }

        public static DHValidationParms GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        public static DHValidationParms GetInstance(object obj)
        {
            if (obj == null || obj is DHDomainParameters)
                return (DHValidationParms)obj;

            if (obj is Asn1Sequence)
                return new DHValidationParms((Asn1Sequence)obj);

            throw new ArgumentException("Invalid DHValidationParms: " + Platform.GetTypeName(obj), "obj");
        }

        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(Seed, PgenCounter);
        }
    }
}

#endif