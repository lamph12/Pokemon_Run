#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1.X509
{
	/**
     * PKIX RFC-2459
     * 
     * The X.509 v2 CRL syntax is as follows.  For signature calculation,
     * the data that is to be signed is ASN.1 Der encoded.
     * <pre>
     *     CertificateList  ::=  Sequence  {
     *     tbsCertList          TbsCertList,
     *     signatureAlgorithm   AlgorithmIdentifier,
     *     signatureValue       BIT STRING  }
     * </pre>
     */
	public class CertificateList
        : Asn1Encodable
    {
        private CertificateList(
            Asn1Sequence seq)
        {
            if (seq.Count != 3)
                throw new ArgumentException("sequence wrong size for CertificateList", "seq");

            TbsCertList = TbsCertificateList.GetInstance(seq[0]);
            SignatureAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
            Signature = DerBitString.GetInstance(seq[2]);
        }

        public TbsCertificateList TbsCertList { get; }

        public AlgorithmIdentifier SignatureAlgorithm { get; }

        public DerBitString Signature { get; }

        public int Version => TbsCertList.Version;

        public X509Name Issuer => TbsCertList.Issuer;

        public Time ThisUpdate => TbsCertList.ThisUpdate;

        public Time NextUpdate => TbsCertList.NextUpdate;

        public static CertificateList GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static CertificateList GetInstance(
            object obj)
        {
            if (obj is CertificateList)
                return (CertificateList)obj;

            if (obj != null)
                return new CertificateList(Asn1Sequence.GetInstance(obj));

            return null;
        }

        public CrlEntry[] GetRevokedCertificates()
        {
            return TbsCertList.GetRevokedCertificates();
        }

        public IEnumerable GetRevokedCertificateEnumeration()
        {
            return TbsCertList.GetRevokedCertificateEnumeration();
        }

        public byte[] GetSignatureOctets()
        {
            return Signature.GetOctets();
        }

        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(TbsCertList, SignatureAlgorithm, Signature);
        }
    }
}

#endif