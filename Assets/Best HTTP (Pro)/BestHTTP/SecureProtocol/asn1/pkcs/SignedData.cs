#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;

namespace Org.BouncyCastle.Asn1.Pkcs
{
    /**
     * a Pkcs#7 signed data object.
     */
    public class SignedData
        : Asn1Encodable
    {
        public SignedData(
            DerInteger _version,
            Asn1Set _digestAlgorithms,
            ContentInfo _contentInfo,
            Asn1Set _certificates,
            Asn1Set _crls,
            Asn1Set _signerInfos)
        {
            Version = _version;
            DigestAlgorithms = _digestAlgorithms;
            ContentInfo = _contentInfo;
            Certificates = _certificates;
            Crls = _crls;
            SignerInfos = _signerInfos;
        }

        private SignedData(
            Asn1Sequence seq)
        {
            var e = seq.GetEnumerator();

            e.MoveNext();
            Version = (DerInteger)e.Current;

            e.MoveNext();
            DigestAlgorithms = (Asn1Set)e.Current;

            e.MoveNext();
            ContentInfo = ContentInfo.GetInstance(e.Current);

            while (e.MoveNext())
            {
                var o = (Asn1Object)e.Current;

                //
                // an interesting feature of SignedData is that there appear to be varying implementations...
                // for the moment we ignore anything which doesn't fit.
                //
                if (o is DerTaggedObject)
                {
                    var tagged = (DerTaggedObject)o;

                    switch (tagged.TagNo)
                    {
                        case 0:
                            Certificates = Asn1Set.GetInstance(tagged, false);
                            break;
                        case 1:
                            Crls = Asn1Set.GetInstance(tagged, false);
                            break;
                        default:
                            throw new ArgumentException("unknown tag value " + tagged.TagNo);
                    }
                }
                else
                {
                    SignerInfos = (Asn1Set)o;
                }
            }
        }

        public DerInteger Version { get; }

        public Asn1Set DigestAlgorithms { get; }

        public ContentInfo ContentInfo { get; }

        public Asn1Set Certificates { get; }

        public Asn1Set Crls { get; }

        public Asn1Set SignerInfos { get; }

        public static SignedData GetInstance(object obj)
        {
            if (obj == null)
                return null;
            var existing = obj as SignedData;
            if (existing != null)
                return existing;
            return new SignedData(Asn1Sequence.GetInstance(obj));
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *     SignedData ::= Sequence {
         *     version Version,
         *     digestAlgorithms DigestAlgorithmIdentifiers,
         *     contentInfo ContentInfo,
         *     certificates
         *     [0] IMPLICIT ExtendedCertificatesAndCertificates
         *     OPTIONAL,
         *     crls
         *     [1] IMPLICIT CertificateRevocationLists OPTIONAL,
         *     signerInfos SignerInfos }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector(
                Version, DigestAlgorithms, ContentInfo);

            if (Certificates != null) v.Add(new DerTaggedObject(false, 0, Certificates));

            if (Crls != null) v.Add(new DerTaggedObject(false, 1, Crls));

            v.Add(SignerInfos);

            return new BerSequence(v);
        }
    }
}

#endif