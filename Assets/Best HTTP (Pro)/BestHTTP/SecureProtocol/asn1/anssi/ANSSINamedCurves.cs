#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.Collections;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.Anssi
{
    public class AnssiNamedCurves
    {
        private static readonly IDictionary objIds = Platform.CreateHashtable();
        private static readonly IDictionary curves = Platform.CreateHashtable();
        private static readonly IDictionary names = Platform.CreateHashtable();

        static AnssiNamedCurves()
        {
            DefineCurve("FRP256v1", AnssiObjectIdentifiers.FRP256v1, Frp256v1Holder.Instance);
        }

        /**
         * returns an enumeration containing the name strings for curves
         * contained in this structure.
         */
        public static IEnumerable Names => new EnumerableProxy(names.Values);

        private static ECCurve ConfigureCurve(ECCurve curve)
        {
            return curve;
        }

        private static BigInteger FromHex(string hex)
        {
            return new BigInteger(1, Hex.Decode(hex));
        }

        private static void DefineCurve(
            string name,
            DerObjectIdentifier oid,
            X9ECParametersHolder holder)
        {
            objIds.Add(Platform.ToUpperInvariant(name), oid);
            names.Add(oid, name);
            curves.Add(oid, holder);
        }

        public static X9ECParameters GetByName(
            string name)
        {
            var oid = GetOid(name);
            return oid == null ? null : GetByOid(oid);
        }

        /**
         * return the X9ECParameters object for the named curve represented by
         * the passed in object identifier. Null if the curve isn't present.
         * 
         * @param oid an object identifier representing a named curve, if present.
         */
        public static X9ECParameters GetByOid(
            DerObjectIdentifier oid)
        {
            var holder = (X9ECParametersHolder)curves[oid];
            return holder == null ? null : holder.Parameters;
        }

        /**
         * return the object identifier signified by the passed in name. Null
         * if there is no object identifier associated with name.
         * 
         * @return the object identifier associated with name, if present.
         */
        public static DerObjectIdentifier GetOid(
            string name)
        {
            return (DerObjectIdentifier)objIds[Platform.ToUpperInvariant(name)];
        }

        /**
         * return the named curve name represented by the given object identifier.
         */
        public static string GetName(
            DerObjectIdentifier oid)
        {
            return (string)names[oid];
        }

        /*
         * FRP256v1
         */
        internal class Frp256v1Holder
            : X9ECParametersHolder
        {
            internal static readonly X9ECParametersHolder Instance = new Frp256v1Holder();

            private Frp256v1Holder()
            {
            }

            protected override X9ECParameters CreateParameters()
            {
                var p = FromHex("F1FD178C0B3AD58F10126DE8CE42435B3961ADBCABC8CA6DE8FCF353D86E9C03");
                var a = FromHex("F1FD178C0B3AD58F10126DE8CE42435B3961ADBCABC8CA6DE8FCF353D86E9C00");
                var b = FromHex("EE353FCA5428A9300D4ABA754A44C00FDFEC0C9AE4B1A1803075ED967B7BB73F");
                byte[] S = null;
                var n = FromHex("F1FD178C0B3AD58F10126DE8CE42435B53DC67E140D2BF941FFDD459C6D655E1");
                var h = BigInteger.One;

                var curve = ConfigureCurve(new FpCurve(p, a, b, n, h));
                var G = new X9ECPoint(curve, Hex.Decode("04"
                                                        + "B6B3D4C356C139EB31183D4749D423958C27D2DCAF98B70164C97A2DD98F5CFF"
                                                        + "6142E0F7C8B204911F9271F0F3ECEF8C2701C307E8E4C9E183115A1554062CFB"));

                return new X9ECParameters(curve, G, n, h, S);
            }
        }
    }
}

#endif