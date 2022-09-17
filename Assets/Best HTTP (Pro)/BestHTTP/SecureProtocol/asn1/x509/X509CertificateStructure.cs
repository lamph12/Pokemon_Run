#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * an X509Certificate structure.
     * <pre>
     *     Certificate ::= Sequence {
     *     tbsCertificate          TbsCertificate,
     *     signatureAlgorithm      AlgorithmIdentifier,
     *     signature               BIT STRING
     *     }
     * </pre>
     */
    public class X509CertificateStructure
        : Asn1Encodable
    {
        public X509CertificateStructure(
            TbsCertificateStructure tbsCert,
            AlgorithmIdentifier sigAlgID,
            DerBitString sig)
        {
            if (tbsCert == null)
                throw new ArgumentNullException("tbsCert");
            if (sigAlgID == null)
                throw new ArgumentNullException("sigAlgID");
            if (sig == null)
                throw new ArgumentNullException("sig");

            this.TbsCertificate = tbsCert;
            this.SignatureAlgorithm = sigAlgID;
            this.Signature = sig;
        }

        private X509CertificateStructure(
            Asn1Sequence seq)
        {
            if (seq.Count != 3)
                throw new ArgumentException("sequence wrong size for a certificate", "seq");

            //
            // correct x509 certficate
            //
            TbsCertificate = TbsCertificateStructure.GetInstance(seq[0]);
            SignatureAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
            Signature = DerBitString.GetInstance(seq[2]);
        }

        public TbsCertificateStructure TbsCertificate { get; }

        public int Version => TbsCertificate.Version;

        public DerInteger SerialNumber => TbsCertificate.SerialNumber;

        public X509Name Issuer => TbsCertificate.Issuer;

        public Time StartDate => TbsCertificate.StartDate;

        public Time EndDate => TbsCertificate.EndDate;

        public X509Name Subject => TbsCertificate.Subject;

        public SubjectPublicKeyInfo SubjectPublicKeyInfo => TbsCertificate.SubjectPublicKeyInfo;

        public AlgorithmIdentifier SignatureAlgorithm { get; }

        public DerBitString Signature { get; }

        public static X509CertificateStructure GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static X509CertificateStructure GetInstance(
            object obj)
        {
            if (obj is X509CertificateStructure)
                return (X509CertificateStructure)obj;
            if (obj == null)
                return null;
            return new X509CertificateStructure(Asn1Sequence.GetInstance(obj));
        }

        public byte[] GetSignatureOctets()
        {
            return Signature.GetOctets();
        }

        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(TbsCertificate, SignatureAlgorithm, Signature);
        }
    }
}

#endif