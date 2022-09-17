#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Ocsp
{
    public class OcspResponse
        : Asn1Encodable
    {
        public OcspResponse(
            OcspResponseStatus responseStatus,
            ResponseBytes responseBytes)
        {
            if (responseStatus == null)
                throw new ArgumentNullException("responseStatus");

            this.ResponseStatus = responseStatus;
            this.ResponseBytes = responseBytes;
        }

        private OcspResponse(
            Asn1Sequence seq)
        {
            ResponseStatus = new OcspResponseStatus(
                DerEnumerated.GetInstance(seq[0]));

            if (seq.Count == 2)
                ResponseBytes = ResponseBytes.GetInstance(
                    (Asn1TaggedObject)seq[1], true);
        }

        public OcspResponseStatus ResponseStatus { get; }

        public ResponseBytes ResponseBytes { get; }

        public static OcspResponse GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static OcspResponse GetInstance(
            object obj)
        {
            if (obj == null || obj is OcspResponse) return (OcspResponse)obj;

            if (obj is Asn1Sequence) return new OcspResponse((Asn1Sequence)obj);

            throw new ArgumentException("unknown object in factory: " + Platform.GetTypeName(obj), "obj");
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *     OcspResponse ::= Sequence {
         *     responseStatus         OcspResponseStatus,
         *     responseBytes          [0] EXPLICIT ResponseBytes OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            var v = new Asn1EncodableVector(ResponseStatus);

            if (ResponseBytes != null) v.Add(new DerTaggedObject(true, 0, ResponseBytes));

            return new DerSequence(v);
        }
    }
}

#endif