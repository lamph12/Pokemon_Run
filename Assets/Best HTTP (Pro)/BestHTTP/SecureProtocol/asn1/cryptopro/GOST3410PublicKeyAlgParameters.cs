#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.CryptoPro
{
    public class Gost3410PublicKeyAlgParameters
        : Asn1Encodable
    {
        public Gost3410PublicKeyAlgParameters(
            DerObjectIdentifier publicKeyParamSet,
            DerObjectIdentifier digestParamSet)
            : this(publicKeyParamSet, digestParamSet, null)
        {
        }

        public Gost3410PublicKeyAlgParameters(
            DerObjectIdentifier publicKeyParamSet,
            DerObjectIdentifier digestParamSet,
            DerObjectIdentifier encryptionParamSet)
        {
            if (publicKeyParamSet == null)
                throw new ArgumentNullException("publicKeyParamSet");
            if (digestParamSet == null)
                throw new ArgumentNullException("digestParamSet");

            PublicKeyParamSet = publicKeyParamSet;
            DigestParamSet = digestParamSet;
            EncryptionParamSet = encryptionParamSet;
        }

        public Gost3410PublicKeyAlgParameters(
            Asn1Sequence seq)
        {
            PublicKeyParamSet = (DerObjectIdentifier)seq[0];
            DigestParamSet = (DerObjectIdentifier)seq[1];

            if (seq.Count > 2) EncryptionParamSet = (DerObjectIdentifier)seq[2];
        }

        public DerObjectIdentifier PublicKeyParamSet { get; }

        public DerObjectIdentifier DigestParamSet { get; }

        public DerObjectIdentifier EncryptionParamSet { get; }

        public static Gost3410PublicKeyAlgParameters GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static Gost3410PublicKeyAlgParameters GetInstance(
            object obj)
        {
            if (obj == null || obj is Gost3410PublicKeyAlgParameters) return (Gost3410PublicKeyAlgParameters)obj;

            if (obj is Asn1Sequence) return new Gost3410PublicKeyAlgParameters((Asn1Sequence)obj);

            throw new ArgumentException("Invalid GOST3410Parameter: " + Platform.GetTypeName(obj));
        }

        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector(
                PublicKeyParamSet, DigestParamSet);

            if (EncryptionParamSet != null) v.Add(EncryptionParamSet);

            return new DerSequence(v);
        }
    }
}

#endif