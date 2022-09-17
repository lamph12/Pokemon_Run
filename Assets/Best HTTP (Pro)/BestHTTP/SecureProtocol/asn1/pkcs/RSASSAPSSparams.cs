#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Pkcs
{
    public class RsassaPssParameters
        : Asn1Encodable
    {
        public static readonly AlgorithmIdentifier DefaultHashAlgorithm =
            new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);

        public static readonly AlgorithmIdentifier DefaultMaskGenFunction =
            new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, DefaultHashAlgorithm);

        public static readonly DerInteger DefaultSaltLength = new DerInteger(20);
        public static readonly DerInteger DefaultTrailerField = new DerInteger(1);

        /**
		 * The default version
		 */
        public RsassaPssParameters()
        {
            HashAlgorithm = DefaultHashAlgorithm;
            MaskGenAlgorithm = DefaultMaskGenFunction;
            SaltLength = DefaultSaltLength;
            TrailerField = DefaultTrailerField;
        }

        public RsassaPssParameters(
            AlgorithmIdentifier hashAlgorithm,
            AlgorithmIdentifier maskGenAlgorithm,
            DerInteger saltLength,
            DerInteger trailerField)
        {
            HashAlgorithm = hashAlgorithm;
            MaskGenAlgorithm = maskGenAlgorithm;
            SaltLength = saltLength;
            TrailerField = trailerField;
        }

        public RsassaPssParameters(
            Asn1Sequence seq)
        {
            HashAlgorithm = DefaultHashAlgorithm;
            MaskGenAlgorithm = DefaultMaskGenFunction;
            SaltLength = DefaultSaltLength;
            TrailerField = DefaultTrailerField;

            for (var i = 0; i != seq.Count; i++)
            {
                var o = (Asn1TaggedObject)seq[i];

                switch (o.TagNo)
                {
                    case 0:
                        HashAlgorithm = AlgorithmIdentifier.GetInstance(o, true);
                        break;
                    case 1:
                        MaskGenAlgorithm = AlgorithmIdentifier.GetInstance(o, true);
                        break;
                    case 2:
                        SaltLength = DerInteger.GetInstance(o, true);
                        break;
                    case 3:
                        TrailerField = DerInteger.GetInstance(o, true);
                        break;
                    default:
                        throw new ArgumentException("unknown tag");
                }
            }
        }

        public AlgorithmIdentifier HashAlgorithm { get; }

        public AlgorithmIdentifier MaskGenAlgorithm { get; }

        public DerInteger SaltLength { get; }

        public DerInteger TrailerField { get; }

        public static RsassaPssParameters GetInstance(
            object obj)
        {
            if (obj == null || obj is RsassaPssParameters) return (RsassaPssParameters)obj;

            if (obj is Asn1Sequence) return new RsassaPssParameters((Asn1Sequence)obj);

            throw new ArgumentException("Unknown object in factory: " + Platform.GetTypeName(obj), "obj");
        }

        /**
         * <pre>
         *     RSASSA-PSS-params ::= SEQUENCE {
         *     hashAlgorithm      [0] OAEP-PSSDigestAlgorithms  DEFAULT sha1,
         *     maskGenAlgorithm   [1] PKCS1MGFAlgorithms  DEFAULT mgf1SHA1,
         *     saltLength         [2] INTEGER  DEFAULT 20,
         *     trailerField       [3] TrailerField  DEFAULT trailerFieldBC
         *     }
         *     OAEP-PSSDigestAlgorithms    ALGORITHM-IDENTIFIER ::= {
         *     { OID id-sha1 PARAMETERS NULL   }|
         *     { OID id-sha256 PARAMETERS NULL }|
         *     { OID id-sha384 PARAMETERS NULL }|
         *     { OID id-sha512 PARAMETERS NULL },
         *     ...  -- Allows for future expansion --
         *     }
         *     PKCS1MGFAlgorithms    ALGORITHM-IDENTIFIER ::= {
         *     { OID id-mgf1 PARAMETERS OAEP-PSSDigestAlgorithms },
         *     ...  -- Allows for future expansion --
         *     }
         *     TrailerField ::= INTEGER { trailerFieldBC(1) }
         * </pre>
         * @return the asn1 primitive representing the parameters.
         */
        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector();

            if (!HashAlgorithm.Equals(DefaultHashAlgorithm)) v.Add(new DerTaggedObject(true, 0, HashAlgorithm));

            if (!MaskGenAlgorithm.Equals(DefaultMaskGenFunction)) v.Add(new DerTaggedObject(true, 1, MaskGenAlgorithm));

            if (!SaltLength.Equals(DefaultSaltLength)) v.Add(new DerTaggedObject(true, 2, SaltLength));

            if (!TrailerField.Equals(DefaultTrailerField)) v.Add(new DerTaggedObject(true, 3, TrailerField));

            return new DerSequence(v);
        }
    }
}

#endif