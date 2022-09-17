#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers
{
    /**
     * A deterministic K calculator based on the algorithm in section 3.2 of RFC 6979.
     */
    public class HMacDsaKCalculator
        : IDsaKCalculator
    {
        private readonly HMac hMac;
        private readonly byte[] K;
        private readonly byte[] V;

        private BigInteger n;

        /**
         * Base constructor.
         * 
         * @param digest digest to build the HMAC on.
         */
        public HMacDsaKCalculator(IDigest digest)
        {
            hMac = new HMac(digest);
            V = new byte[hMac.GetMacSize()];
            K = new byte[hMac.GetMacSize()];
        }

        public virtual bool IsDeterministic => true;

        public virtual void Init(BigInteger n, SecureRandom random)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        public void Init(BigInteger n, BigInteger d, byte[] message)
        {
            this.n = n;

            Arrays.Fill(V, 0x01);
            Arrays.Fill(K, 0);

            var x = new byte[(n.BitLength + 7) / 8];
            var dVal = BigIntegers.AsUnsignedByteArray(d);

            Array.Copy(dVal, 0, x, x.Length - dVal.Length, dVal.Length);

            var m = new byte[(n.BitLength + 7) / 8];

            var mInt = BitsToInt(message);

            if (mInt.CompareTo(n) >= 0) mInt = mInt.Subtract(n);

            var mVal = BigIntegers.AsUnsignedByteArray(mInt);

            Array.Copy(mVal, 0, m, m.Length - mVal.Length, mVal.Length);

            hMac.Init(new KeyParameter(K));

            hMac.BlockUpdate(V, 0, V.Length);
            hMac.Update(0x00);
            hMac.BlockUpdate(x, 0, x.Length);
            hMac.BlockUpdate(m, 0, m.Length);

            hMac.DoFinal(K, 0);

            hMac.Init(new KeyParameter(K));

            hMac.BlockUpdate(V, 0, V.Length);

            hMac.DoFinal(V, 0);

            hMac.BlockUpdate(V, 0, V.Length);
            hMac.Update(0x01);
            hMac.BlockUpdate(x, 0, x.Length);
            hMac.BlockUpdate(m, 0, m.Length);

            hMac.DoFinal(K, 0);

            hMac.Init(new KeyParameter(K));

            hMac.BlockUpdate(V, 0, V.Length);

            hMac.DoFinal(V, 0);
        }

        public virtual BigInteger NextK()
        {
            var t = new byte[(n.BitLength + 7) / 8];

            for (;;)
            {
                var tOff = 0;

                while (tOff < t.Length)
                {
                    hMac.BlockUpdate(V, 0, V.Length);

                    hMac.DoFinal(V, 0);

                    var len = System.Math.Min(t.Length - tOff, V.Length);
                    Array.Copy(V, 0, t, tOff, len);
                    tOff += len;
                }

                var k = BitsToInt(t);

                if (k.SignValue > 0 && k.CompareTo(n) < 0) return k;

                hMac.BlockUpdate(V, 0, V.Length);
                hMac.Update(0x00);

                hMac.DoFinal(K, 0);

                hMac.Init(new KeyParameter(K));

                hMac.BlockUpdate(V, 0, V.Length);

                hMac.DoFinal(V, 0);
            }
        }

        private BigInteger BitsToInt(byte[] t)
        {
            var v = new BigInteger(1, t);

            if (t.Length * 8 > n.BitLength) v = v.ShiftRight(t.Length * 8 - n.BitLength);

            return v;
        }
    }
}

#endif