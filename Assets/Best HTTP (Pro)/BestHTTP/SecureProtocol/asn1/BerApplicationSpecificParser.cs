#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
namespace Org.BouncyCastle.Asn1
{
    public class BerApplicationSpecificParser
        : IAsn1ApplicationSpecificParser
    {
        private readonly Asn1StreamParser parser;
        private readonly int tag;

        internal BerApplicationSpecificParser(
            int tag,
            Asn1StreamParser parser)
        {
            this.tag = tag;
            this.parser = parser;
        }

        public IAsn1Convertible ReadObject()
        {
            return parser.ReadObject();
        }

        public Asn1Object ToAsn1Object()
        {
            return new BerApplicationSpecific(tag, parser.ReadVector());
        }
    }
}

#endif