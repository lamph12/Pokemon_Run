#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class Gost3410ValidationParameters
    {
        public Gost3410ValidationParameters(
            int x0,
            int c)
        {
            X0 = x0;
            C = c;
        }

        public Gost3410ValidationParameters(
            long x0L,
            long cL)
        {
            X0L = x0L;
            CL = cL;
        }

        public int C { get; }

        public int X0 { get; }

        public long CL { get; }

        public long X0L { get; }

        public override bool Equals(
            object obj)
        {
            var other = obj as Gost3410ValidationParameters;

            return other != null
                   && other.C == C
                   && other.X0 == X0
                   && other.CL == CL
                   && other.X0L == X0L;
        }

        public override int GetHashCode()
        {
            return C.GetHashCode() ^ X0.GetHashCode() ^ CL.GetHashCode() ^ X0L.GetHashCode();
        }
    }
}

#endif