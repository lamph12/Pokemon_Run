#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.Collections;

namespace Org.BouncyCastle.Asn1
{
    internal class LazyDerSet
        : DerSet
    {
        private byte[] encoded;

        internal LazyDerSet(
            byte[] encoded)
        {
            this.encoded = encoded;
        }

        public override Asn1Encodable this[int index]
        {
            get
            {
                Parse();

                return base[index];
            }
        }

        public override int Count
        {
            get
            {
                Parse();

                return base.Count;
            }
        }

        private void Parse()
        {
            lock (this)
            {
                if (encoded != null)
                {
                    Asn1InputStream e = new LazyAsn1InputStream(encoded);

                    Asn1Object o;
                    while ((o = e.ReadObject()) != null) AddObject(o);

                    encoded = null;
                }
            }
        }

        public override IEnumerator GetEnumerator()
        {
            Parse();

            return base.GetEnumerator();
        }

        internal override void Encode(
            DerOutputStream derOut)
        {
            lock (this)
            {
                if (encoded == null)
                    base.Encode(derOut);
                else
                    derOut.WriteEncoded(Asn1Tags.Set | Asn1Tags.Constructed, encoded);
            }
        }
    }
}

#endif