#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.IO;

namespace Org.BouncyCastle.Asn1
{
    public abstract class Asn1Generator
    {
        protected Asn1Generator(
            Stream outStream)
        {
            Out = outStream;
        }

        protected Stream Out { get; }

        public abstract void AddObject(Asn1Encodable obj);

        public abstract Stream GetRawOutputStream();

        public abstract void Close();
    }
}

#endif