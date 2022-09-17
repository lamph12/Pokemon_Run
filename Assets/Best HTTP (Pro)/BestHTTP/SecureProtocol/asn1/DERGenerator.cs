#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
    public abstract class DerGenerator
        : Asn1Generator
    {
        private readonly bool _isExplicit;
        private readonly bool _tagged;
        private readonly int _tagNo;

        protected DerGenerator(
            Stream outStream)
            : base(outStream)
        {
        }

        protected DerGenerator(
            Stream outStream,
            int tagNo,
            bool isExplicit)
            : base(outStream)
        {
            _tagged = true;
            _isExplicit = isExplicit;
            _tagNo = tagNo;
        }

        private static void WriteLength(
            Stream outStr,
            int length)
        {
            if (length > 127)
            {
                var size = 1;
                var val = length;

                while ((val >>= 8) != 0) size++;

                outStr.WriteByte((byte)(size | 0x80));

                for (var i = (size - 1) * 8; i >= 0; i -= 8) outStr.WriteByte((byte)(length >> i));
            }
            else
            {
                outStr.WriteByte((byte)length);
            }
        }

        internal static void WriteDerEncoded(
            Stream outStream,
            int tag,
            byte[] bytes)
        {
            outStream.WriteByte((byte)tag);
            WriteLength(outStream, bytes.Length);
            outStream.Write(bytes, 0, bytes.Length);
        }

        internal void WriteDerEncoded(
            int tag,
            byte[] bytes)
        {
            if (_tagged)
            {
                var tagNum = _tagNo | Asn1Tags.Tagged;

                if (_isExplicit)
                {
                    var newTag = _tagNo | Asn1Tags.Constructed | Asn1Tags.Tagged;
                    var bOut = new MemoryStream();
                    WriteDerEncoded(bOut, tag, bytes);
                    WriteDerEncoded(Out, newTag, bOut.ToArray());
                }
                else
                {
                    if ((tag & Asn1Tags.Constructed) != 0) tagNum |= Asn1Tags.Constructed;

                    WriteDerEncoded(Out, tagNum, bytes);
                }
            }
            else
            {
                WriteDerEncoded(Out, tag, bytes);
            }
        }

        internal static void WriteDerEncoded(
            Stream outStr,
            int tag,
            Stream inStr)
        {
            WriteDerEncoded(outStr, tag, Streams.ReadAll(inStr));
        }
    }
}

#endif