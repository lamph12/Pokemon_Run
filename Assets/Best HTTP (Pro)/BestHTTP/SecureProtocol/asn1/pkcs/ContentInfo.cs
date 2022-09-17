#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
namespace Org.BouncyCastle.Asn1.Pkcs
{
    public class ContentInfo
        : Asn1Encodable
    {
        private ContentInfo(
            Asn1Sequence seq)
        {
            ContentType = (DerObjectIdentifier)seq[0];

            if (seq.Count > 1) Content = ((Asn1TaggedObject)seq[1]).GetObject();
        }

        public ContentInfo(
            DerObjectIdentifier contentType,
            Asn1Encodable content)
        {
            this.ContentType = contentType;
            this.Content = content;
        }

        public DerObjectIdentifier ContentType { get; }

        public Asn1Encodable Content { get; }

        public static ContentInfo GetInstance(object obj)
        {
            if (obj == null)
                return null;
            var existing = obj as ContentInfo;
            if (existing != null)
                return existing;
            return new ContentInfo(Asn1Sequence.GetInstance(obj));
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *     ContentInfo ::= Sequence {
         *     contentType ContentType,
         *     content
         *     [0] EXPLICIT ANY DEFINED BY contentType OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector(ContentType);

            if (Content != null) v.Add(new BerTaggedObject(0, Content));

            return new BerSequence(v);
        }
    }
}

#endif