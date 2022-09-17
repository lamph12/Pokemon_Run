#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
	/**
     * <pre>
     *     IssuingDistributionPoint ::= SEQUENCE {
     *     distributionPoint          [0] DistributionPointName OPTIONAL,
     *     onlyContainsUserCerts      [1] BOOLEAN DEFAULT FALSE,
     *     onlyContainsCACerts        [2] BOOLEAN DEFAULT FALSE,
     *     onlySomeReasons            [3] ReasonFlags OPTIONAL,
     *     indirectCRL                [4] BOOLEAN DEFAULT FALSE,
     *     onlyContainsAttributeCerts [5] BOOLEAN DEFAULT FALSE }
     * </pre>
     */
	public class IssuingDistributionPoint
        : Asn1Encodable
    {
        private readonly Asn1Sequence seq;

        /**
         * Constructor from given details.
         * 
         * @param distributionPoint
         * May contain an URI as pointer to most current CRL.
         * @param onlyContainsUserCerts Covers revocation information for end certificates.
         * @param onlyContainsCACerts Covers revocation information for CA certificates.
         * 
         * @param onlySomeReasons
         * Which revocation reasons does this point cover.
         * @param indirectCRL
         * If
         * <code>true</code>
         * then the CRL contains revocation
         * information about certificates ssued by other CAs.
         * @param onlyContainsAttributeCerts Covers revocation information for attribute certificates.
         */
        public IssuingDistributionPoint(
            DistributionPointName distributionPoint,
            bool onlyContainsUserCerts,
            bool onlyContainsCACerts,
            ReasonFlags onlySomeReasons,
            bool indirectCRL,
            bool onlyContainsAttributeCerts)
        {
            DistributionPoint = distributionPoint;
            IsIndirectCrl = indirectCRL;
            OnlyContainsAttributeCerts = onlyContainsAttributeCerts;
            OnlyContainsCACerts = onlyContainsCACerts;
            OnlyContainsUserCerts = onlyContainsUserCerts;
            OnlySomeReasons = onlySomeReasons;

            var vec = new Asn1EncodableVector();
            if (distributionPoint != null)
                // CHOICE item so explicitly tagged
                vec.Add(new DerTaggedObject(true, 0, distributionPoint));
            if (onlyContainsUserCerts) vec.Add(new DerTaggedObject(false, 1, DerBoolean.True));
            if (onlyContainsCACerts) vec.Add(new DerTaggedObject(false, 2, DerBoolean.True));
            if (onlySomeReasons != null) vec.Add(new DerTaggedObject(false, 3, onlySomeReasons));
            if (indirectCRL) vec.Add(new DerTaggedObject(false, 4, DerBoolean.True));
            if (onlyContainsAttributeCerts) vec.Add(new DerTaggedObject(false, 5, DerBoolean.True));

            seq = new DerSequence(vec);
        }

        /**
         * Constructor from Asn1Sequence
         */
        private IssuingDistributionPoint(
            Asn1Sequence seq)
        {
            this.seq = seq;

            for (var i = 0; i != seq.Count; i++)
            {
                var o = Asn1TaggedObject.GetInstance(seq[i]);

                switch (o.TagNo)
                {
                    case 0:
                        // CHOICE so explicit
                        DistributionPoint = DistributionPointName.GetInstance(o, true);
                        break;
                    case 1:
                        OnlyContainsUserCerts = DerBoolean.GetInstance(o, false).IsTrue;
                        break;
                    case 2:
                        OnlyContainsCACerts = DerBoolean.GetInstance(o, false).IsTrue;
                        break;
                    case 3:
                        OnlySomeReasons = new ReasonFlags(DerBitString.GetInstance(o, false));
                        break;
                    case 4:
                        IsIndirectCrl = DerBoolean.GetInstance(o, false).IsTrue;
                        break;
                    case 5:
                        OnlyContainsAttributeCerts = DerBoolean.GetInstance(o, false).IsTrue;
                        break;
                    default:
                        throw new ArgumentException("unknown tag in IssuingDistributionPoint");
                }
            }
        }

        public bool OnlyContainsUserCerts { get; }

        public bool OnlyContainsCACerts { get; }

        public bool IsIndirectCrl { get; }

        public bool OnlyContainsAttributeCerts { get; }

        /**
		 * @return Returns the distributionPoint.
		 */
        public DistributionPointName DistributionPoint { get; }

        /**
		 * @return Returns the onlySomeReasons.
		 */
        public ReasonFlags OnlySomeReasons { get; }

        public static IssuingDistributionPoint GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static IssuingDistributionPoint GetInstance(
            object obj)
        {
            if (obj == null || obj is IssuingDistributionPoint) return (IssuingDistributionPoint)obj;

            if (obj is Asn1Sequence) return new IssuingDistributionPoint((Asn1Sequence)obj);

            throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
        }

        public override Asn1Object ToAsn1Object()
        {
            return seq;
        }

        public override string ToString()
        {
            var sep = Platform.NewLine;
            var buf = new StringBuilder();

            buf.Append("IssuingDistributionPoint: [");
            buf.Append(sep);
            if (DistributionPoint != null) appendObject(buf, sep, "distributionPoint", DistributionPoint.ToString());
            if (OnlyContainsUserCerts)
                appendObject(buf, sep, "onlyContainsUserCerts", OnlyContainsUserCerts.ToString());
            if (OnlyContainsCACerts) appendObject(buf, sep, "onlyContainsCACerts", OnlyContainsCACerts.ToString());
            if (OnlySomeReasons != null) appendObject(buf, sep, "onlySomeReasons", OnlySomeReasons.ToString());
            if (OnlyContainsAttributeCerts)
                appendObject(buf, sep, "onlyContainsAttributeCerts", OnlyContainsAttributeCerts.ToString());
            if (IsIndirectCrl) appendObject(buf, sep, "indirectCRL", IsIndirectCrl.ToString());
            buf.Append("]");
            buf.Append(sep);
            return buf.ToString();
        }

        private void appendObject(
            StringBuilder buf,
            string sep,
            string name,
            string val)
        {
            var indent = "    ";

            buf.Append(indent);
            buf.Append(name);
            buf.Append(":");
            buf.Append(sep);
            buf.Append(indent);
            buf.Append(indent);
            buf.Append(val);
            buf.Append(sep);
        }
    }
}

#endif