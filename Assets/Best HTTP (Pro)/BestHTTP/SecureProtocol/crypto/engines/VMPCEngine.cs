#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Engines
{
    public class VmpcEngine
        : IStreamCipher
    {
        /*
        * variables to hold the state of the VMPC engine during encryption and
        * decryption
        */
        protected byte n;
        protected byte[] P;
        protected byte s;

        protected byte[] workingIV;
        protected byte[] workingKey;

        public virtual string AlgorithmName => "VMPC";

        /**
         * initialise a VMPC cipher.
         * 
         * @param forEncryption
         * whether or not we are for encryption.
         * @param params
         * the parameters required to set up the cipher.
         * @exception ArgumentException
         * if the params argument is inappropriate.
         */
        public virtual void Init(
            bool forEncryption,
            ICipherParameters parameters)
        {
            if (!(parameters is ParametersWithIV))
                throw new ArgumentException("VMPC Init parameters must include an IV");

            var ivParams = (ParametersWithIV)parameters;

            if (!(ivParams.Parameters is KeyParameter))
                throw new ArgumentException("VMPC Init parameters must include a key");

            var key = (KeyParameter)ivParams.Parameters;

            workingIV = ivParams.GetIV();

            if (workingIV == null || workingIV.Length < 1 || workingIV.Length > 768)
                throw new ArgumentException("VMPC requires 1 to 768 bytes of IV");

            workingKey = key.GetKey();

            InitKey(workingKey, workingIV);
        }

        public virtual void ProcessBytes(
            byte[] input,
            int inOff,
            int len,
            byte[] output,
            int outOff)
        {
            Check.DataLength(input, inOff, len, "input buffer too short");
            Check.OutputLength(output, outOff, len, "output buffer too short");

            for (var i = 0; i < len; i++)
            {
                s = P[(s + P[n & 0xff]) & 0xff];
                var z = P[(P[P[s & 0xff] & 0xff] + 1) & 0xff];
                // encryption
                var temp = P[n & 0xff];
                P[n & 0xff] = P[s & 0xff];
                P[s & 0xff] = temp;
                n = (byte)((n + 1) & 0xff);

                // xor
                output[i + outOff] = (byte)(input[i + inOff] ^ z);
            }
        }

        public virtual void Reset()
        {
            InitKey(workingKey, workingIV);
        }

        public virtual byte ReturnByte(
            byte input)
        {
            s = P[(s + P[n & 0xff]) & 0xff];
            var z = P[(P[P[s & 0xff] & 0xff] + 1) & 0xff];
            // encryption
            var temp = P[n & 0xff];
            P[n & 0xff] = P[s & 0xff];
            P[s & 0xff] = temp;
            n = (byte)((n + 1) & 0xff);

            // xor
            return (byte)(input ^ z);
        }

        protected virtual void InitKey(
            byte[] keyBytes,
            byte[] ivBytes)
        {
            s = 0;
            P = new byte[256];
            for (var i = 0; i < 256; i++) P[i] = (byte)i;

            for (var m = 0; m < 768; m++)
            {
                s = P[(s + P[m & 0xff] + keyBytes[m % keyBytes.Length]) & 0xff];
                var temp = P[m & 0xff];
                P[m & 0xff] = P[s & 0xff];
                P[s & 0xff] = temp;
            }

            for (var m = 0; m < 768; m++)
            {
                s = P[(s + P[m & 0xff] + ivBytes[m % ivBytes.Length]) & 0xff];
                var temp = P[m & 0xff];
                P[m & 0xff] = P[s & 0xff];
                P[s & 0xff] = temp;
            }

            n = 0;
        }
    }
}

#endif