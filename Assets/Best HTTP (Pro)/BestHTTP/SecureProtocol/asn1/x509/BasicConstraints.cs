#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
    public class BasicConstraints
        : Asn1Encodable
    {
        private readonly DerBoolean cA;
        private readonly DerInteger pathLenConstraint;

        private BasicConstraints(
            Asn1Sequence seq)
        {
            if (seq.Count > 0)
            {
                if (seq[0] is DerBoolean)
                    cA = DerBoolean.GetInstance(seq[0]);
                else
                    pathLenConstraint = DerInteger.GetInstance(seq[0]);

                if (seq.Count > 1)
                {
                    if (cA == null)
                        throw new ArgumentException("wrong sequence in constructor", "seq");

                    pathLenConstraint = DerInteger.GetInstance(seq[1]);
                }
            }
        }

        public BasicConstraints(
            bool cA)
        {
            if (cA) this.cA = DerBoolean.True;
        }

        /**
         * create a cA=true object for the given path length constraint.
         * 
         * @param pathLenConstraint
         */
        public BasicConstraints(
            int pathLenConstraint)
        {
            cA = DerBoolean.True;
            this.pathLenConstraint = new DerInteger(pathLenConstraint);
        }

        public BigInteger PathLenConstraint => pathLenConstraint == null ? null : pathLenConstraint.Value;

        public static BasicConstraints GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static BasicConstraints GetInstance(
            object obj)
        {
            if (obj == null || obj is BasicConstraints) return (BasicConstraints)obj;

            if (obj is Asn1Sequence) return new BasicConstraints((Asn1Sequence)obj);

            if (obj is X509Extension) return GetInstance(X509Extension.ConvertValueToObject((X509Extension)obj));

            throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
        }

        public bool IsCA()
        {
            return cA != null && cA.IsTrue;
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *     BasicConstraints := Sequence {
         *     cA                  Boolean DEFAULT FALSE,
         *     pathLenConstraint   Integer (0..MAX) OPTIONAL
         *     }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector();

            if (cA != null) v.Add(cA);

            if (pathLenConstraint != null) // yes some people actually do this when cA is false...
                v.Add(pathLenConstraint);

            return new DerSequence(v);
        }

        public override string ToString()
        {
            if (pathLenConstraint == null) return "BasicConstraints: isCa(" + IsCA() + ")";

            return "BasicConstraints: isCa(" + IsCA() + "), pathLenConstraint = " + pathLenConstraint.Value;
        }
    }
}

#endif