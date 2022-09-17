#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    public class Asn1EncodableVector
        : IEnumerable
    {
        private readonly IList v = Platform.CreateArrayList();

//		public Asn1EncodableVector()
//		{
//		}

        public Asn1EncodableVector(
            params Asn1Encodable[] v)
        {
            Add(v);
        }

        public Asn1Encodable this[
            int index] =>
            (Asn1Encodable)v[index];

        [Obsolete("Use 'Count' property instead")]
        public int Size => v.Count;

        public int Count => v.Count;

        public IEnumerator GetEnumerator()
        {
            return v.GetEnumerator();
        }

        public static Asn1EncodableVector FromEnumerable(
            IEnumerable e)
        {
            var v = new Asn1EncodableVector();
            foreach (Asn1Encodable obj in e) v.Add(obj);
            return v;
        }

//		public void Add(
//			Asn1Encodable obj)
//		{
//			v.Add(obj);
//		}

        public void Add(
            params Asn1Encodable[] objs)
        {
            foreach (var obj in objs) v.Add(obj);
        }

        public void AddOptional(
            params Asn1Encodable[] objs)
        {
            if (objs != null)
                foreach (var obj in objs)
                    if (obj != null)
                        v.Add(obj);
        }

        [Obsolete("Use 'object[index]' syntax instead")]
        public Asn1Encodable Get(
            int index)
        {
            return this[index];
        }
    }
}

#endif