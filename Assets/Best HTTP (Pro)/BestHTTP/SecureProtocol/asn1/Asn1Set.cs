#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
#if PORTABLE || NETFX_CORE
using System.Collections.Generic;
using System.Linq;
#endif

namespace Org.BouncyCastle.Asn1
{
    public abstract class Asn1Set
        : Asn1Object, IEnumerable
    {
        private readonly IList _set;

        /**
         * return an ASN1Set from the given object.
         * 
         * @param obj the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static Asn1Set GetInstance(
            object obj)
        {
            if (obj == null || obj is Asn1Set) return (Asn1Set)obj;

            if (obj is Asn1SetParser) return GetInstance(((Asn1SetParser)obj).ToAsn1Object());

            if (obj is byte[])
            {
                try
                {
                    return GetInstance(FromByteArray((byte[])obj));
                }
                catch (IOException e)
                {
                    throw new ArgumentException("failed to construct set from byte[]: " + e.Message);
                }
            }

            if (obj is Asn1Encodable)
            {
                var primitive = ((Asn1Encodable)obj).ToAsn1Object();

                if (primitive is Asn1Set) return (Asn1Set)primitive;
            }

            throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
        }

        /**
         * Return an ASN1 set from a tagged object. There is a special
         * case here, if an object appears to have been explicitly tagged on
         * reading but we were expecting it to be implicitly tagged in the
         * normal course of events it indicates that we lost the surrounding
         * set - so we need to add it back (this will happen if the tagged
         * object is a sequence that contains other sequences). If you are
         * dealing with implicitly tagged sets you really
         * <b>should</b>
         * be using this method.
         * 
         * @param obj the tagged object.
         * @param explicitly true if the object is meant to be explicitly tagged
         * false otherwise.
         * @exception ArgumentException if the tagged object cannot
         * be converted.
         */
        public static Asn1Set GetInstance(
            Asn1TaggedObject obj,
            bool explicitly)
        {
            var inner = obj.GetObject();

            if (explicitly)
            {
                if (!obj.IsExplicit())
                    throw new ArgumentException("object implicit - explicit expected.");

                return (Asn1Set)inner;
            }

            //
            // constructed object which appears to be explicitly tagged
            // and it's really implicit means we have to add the
            // surrounding sequence.
            //
            if (obj.IsExplicit()) return new DerSet(inner);

            if (inner is Asn1Set) return (Asn1Set)inner;

            //
            // in this case the parser returns a sequence, convert it
            // into a set.
            //
            if (inner is Asn1Sequence)
            {
                var v = new Asn1EncodableVector();
                var s = (Asn1Sequence)inner;

                foreach (Asn1Encodable ae in s) v.Add(ae);

                // TODO Should be able to construct set directly from sequence?
                return new DerSet(v, false);
            }

            throw new ArgumentException("Unknown object in GetInstance: " + Platform.GetTypeName(obj), "obj");
        }

        protected internal Asn1Set(
            int capacity)
        {
            _set = Platform.CreateArrayList(capacity);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        [Obsolete("Use GetEnumerator() instead")]
        public IEnumerator GetObjects()
        {
            return GetEnumerator();
        }

        /**
         * return the object at the set position indicated by index.
         * 
         * @param index the set number (starting at zero) of the object
         * @return the object at the set position indicated by index.
         */
        public virtual Asn1Encodable this[int index] => (Asn1Encodable)_set[index];

        [Obsolete("Use 'object[index]' syntax instead")]
        public Asn1Encodable GetObjectAt(
            int index)
        {
            return this[index];
        }

        [Obsolete("Use 'Count' property instead")]
        public int Size => Count;

        public virtual int Count => _set.Count;

        public virtual Asn1Encodable[] ToArray()
        {
            var values = new Asn1Encodable[Count];
            for (var i = 0; i < Count; ++i) values[i] = this[i];
            return values;
        }

        private class Asn1SetParserImpl
            : Asn1SetParser
        {
            private readonly int max;
            private readonly Asn1Set outer;
            private int index;

            public Asn1SetParserImpl(
                Asn1Set outer)
            {
                this.outer = outer;
                max = outer.Count;
            }

            public IAsn1Convertible ReadObject()
            {
                if (index == max)
                    return null;

                var obj = outer[index++];
                if (obj is Asn1Sequence)
                    return ((Asn1Sequence)obj).Parser;

                if (obj is Asn1Set)
                    return ((Asn1Set)obj).Parser;

                // NB: Asn1OctetString implements Asn1OctetStringParser directly
//				if (obj is Asn1OctetString)
//					return ((Asn1OctetString)obj).Parser;

                return obj;
            }

            public virtual Asn1Object ToAsn1Object()
            {
                return outer;
            }
        }

        public Asn1SetParser Parser => new Asn1SetParserImpl(this);

        protected override int Asn1GetHashCode()
        {
            var hc = Count;

            foreach (var o in this)
            {
                hc *= 17;
                if (o == null)
                    hc ^= DerNull.Instance.GetHashCode();
                else
                    hc ^= o.GetHashCode();
            }

            return hc;
        }

        protected override bool Asn1Equals(
            Asn1Object asn1Object)
        {
            var other = asn1Object as Asn1Set;

            if (other == null)
                return false;

            if (Count != other.Count) return false;

            var s1 = GetEnumerator();
            var s2 = other.GetEnumerator();

            while (s1.MoveNext() && s2.MoveNext())
            {
                var o1 = GetCurrent(s1).ToAsn1Object();
                var o2 = GetCurrent(s2).ToAsn1Object();

                if (!o1.Equals(o2))
                    return false;
            }

            return true;
        }

        private Asn1Encodable GetCurrent(IEnumerator e)
        {
            var encObj = (Asn1Encodable)e.Current;

            // unfortunately null was allowed as a substitute for DER null
            if (encObj == null)
                return DerNull.Instance;

            return encObj;
        }

        protected internal void Sort()
        {
            if (_set.Count < 2)
                return;

#if PORTABLE || NETFX_CORE
            var sorted = _set.Cast<Asn1Encodable>()
                             .Select(a => new { Item = a, Key = a.GetEncoded(Asn1Encodable.Der) })
                             .OrderBy(t => t.Key, new DerComparer())
                             .Select(t => t.Item)
                             .ToList();

            for (int i = 0; i < _set.Count; ++i)
            {
                _set[i] = sorted[i];
            }
#else
            var items = new Asn1Encodable[_set.Count];
            var keys = new byte[_set.Count][];

            for (var i = 0; i < _set.Count; ++i)
            {
                var item = (Asn1Encodable)_set[i];
                items[i] = item;
                keys[i] = item.GetEncoded(Der);
            }

            Array.Sort(keys, items, new DerComparer());

            for (var i = 0; i < _set.Count; ++i) _set[i] = items[i];
#endif
        }

        protected internal void AddObject(Asn1Encodable obj)
        {
            _set.Add(obj);
        }

        public override string ToString()
        {
            return CollectionUtilities.ToString(_set);
        }

#if PORTABLE || NETFX_CORE
        private class DerComparer
            : IComparer<byte[]>
        {
            public int Compare(byte[] x, byte[] y)
            {
                byte[] a = x, b = y;
#else
        private class DerComparer
            : IComparer
        {
            public int Compare(object x, object y)
            {
                byte[] a = (byte[])x, b = (byte[])y;
#endif
                var len = System.Math.Min(a.Length, b.Length);
                for (var i = 0; i != len; ++i)
                {
                    byte ai = a[i], bi = b[i];
                    if (ai != bi)
                        return ai < bi ? -1 : 1;
                }

                if (a.Length > b.Length)
                    return AllZeroesFrom(a, len) ? 0 : 1;
                if (a.Length < b.Length)
                    return AllZeroesFrom(b, len) ? 0 : -1;
                return 0;
            }

            private bool AllZeroesFrom(byte[] bs, int pos)
            {
                while (pos < bs.Length)
                    if (bs[pos++] != 0)
                        return false;
                return true;
            }
        }
    }
}

#endif