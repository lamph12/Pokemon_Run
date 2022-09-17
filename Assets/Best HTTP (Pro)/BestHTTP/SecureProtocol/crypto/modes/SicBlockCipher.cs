#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes
{
    /**
     * Implements the Segmented Integer Counter (SIC) mode on top of a simple
     * block cipher.
     */
    public class SicBlockCipher
        : IBlockCipher
    {
        private readonly int blockSize;
        private readonly IBlockCipher cipher;
        private readonly byte[] counter;
        private readonly byte[] counterOut;
        private byte[] IV;

        /**
         * Basic constructor.
         * 
         * @param c the block cipher to be used.
         */
        public SicBlockCipher(IBlockCipher cipher)
        {
            this.cipher = cipher;
            blockSize = cipher.GetBlockSize();
            counter = new byte[blockSize];
            counterOut = new byte[blockSize];
            IV = new byte[blockSize];
        }

        public virtual void Init(
            bool forEncryption, //ignored by this CTR mode
            ICipherParameters parameters)
        {
            var ivParam = parameters as ParametersWithIV;
            if (ivParam == null)
                throw new ArgumentException("CTR/SIC mode requires ParametersWithIV", "parameters");

            IV = Arrays.Clone(ivParam.GetIV());

            if (blockSize < IV.Length)
                throw new ArgumentException("CTR/SIC mode requires IV no greater than: " + blockSize + " bytes.");

            var maxCounterSize = System.Math.Min(8, blockSize / 2);
            if (blockSize - IV.Length > maxCounterSize)
                throw new ArgumentException("CTR/SIC mode requires IV of at least: " + (blockSize - maxCounterSize) +
                                            " bytes.");

            // if null it's an IV changed only.
            if (ivParam.Parameters != null) cipher.Init(true, ivParam.Parameters);

            Reset();
        }

        public virtual string AlgorithmName => cipher.AlgorithmName + "/SIC";

        public virtual bool IsPartialBlockOkay => true;

        public virtual int GetBlockSize()
        {
            return cipher.GetBlockSize();
        }

        public virtual int ProcessBlock(
            byte[] input,
            int inOff,
            byte[] output,
            int outOff)
        {
            cipher.ProcessBlock(counter, 0, counterOut, 0);

            //
            // XOR the counterOut with the plaintext producing the cipher text
            //
            for (var i = 0; i < counterOut.Length; i++) output[outOff + i] = (byte)(counterOut[i] ^ input[inOff + i]);

            // Increment the counter
            var j = counter.Length;
            while (--j >= 0 && ++counter[j] == 0)
            {
            }

            return counter.Length;
        }

        public virtual void Reset()
        {
            Arrays.Fill(counter, 0);
            Array.Copy(IV, 0, counter, 0, IV.Length);
            cipher.Reset();
        }

        /**
         * return the underlying block cipher that we are wrapping.
         * 
         * @return the underlying block cipher that we are wrapping.
         */
        public virtual IBlockCipher GetUnderlyingCipher()
        {
            return cipher;
        }
    }
}

#endif