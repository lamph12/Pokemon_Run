#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    /**
     * Der BMPString object.
     */
    public class DerBmpString
        : DerStringBase
    {
        private readonly string str;

        /**
         * basic constructor - byte encoded string.
         */
        public DerBmpString(
            byte[] str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            var cs = new char[str.Length / 2];

            for (var i = 0; i != cs.Length; i++) cs[i] = (char)((str[2 * i] << 8) | (str[2 * i + 1] & 0xff));

            this.str = new string(cs);
        }

        /**
         * basic constructor
         */
        public DerBmpString(
            string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            this.str = str;
        }

        /**
         * return a BMP string from the given object.
         * 
         * @param obj the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static DerBmpString GetInstance(
            object obj)
        {
            if (obj == null || obj is DerBmpString) return (DerBmpString)obj;

            throw new ArgumentException("illegal object in GetInstance: " + Platform.GetTypeName(obj));
        }

        /**
         * return a BMP string from a tagged object.
         * 
         * @param obj the tagged object holding the object we want
         * @param explicitly true if the object is meant to be explicitly
         * tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         * be converted.
         */
        public static DerBmpString GetInstance(
            Asn1TaggedObject obj,
            bool isExplicit)
        {
            var o = obj.GetObject();

            if (isExplicit || o is DerBmpString) return GetInstance(o);

            return new DerBmpString(Asn1OctetString.GetInstance(o).GetOctets());
        }

        public override string GetString()
        {
            return str;
        }

        protected override bool Asn1Equals(
            Asn1Object asn1Object)
        {
            var other = asn1Object as DerBmpString;

            if (other == null)
                return false;

            return str.Equals(other.str);
        }

        internal override void Encode(
            DerOutputStream derOut)
        {
            var c = str.ToCharArray();
            var b = new byte[c.Length * 2];

            for (var i = 0; i != c.Length; i++)
            {
                b[2 * i] = (byte)(c[i] >> 8);
                b[2 * i + 1] = (byte)c[i];
            }

            derOut.WriteEncoded(Asn1Tags.BmpString, b);
        }
    }
}

#endif