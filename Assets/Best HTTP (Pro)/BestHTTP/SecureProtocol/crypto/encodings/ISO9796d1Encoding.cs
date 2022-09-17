#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Encodings
{
	/**
     * ISO 9796-1 padding. Note in the light of recent results you should
     * only use this with RSA (rather than the "simpler" Rabin keys) and you
     * should never use it with anything other than a hash (ie. even if the
     * message is small don't sign the message, sign it's hash) or some "random"
     * value. See your favorite search engine for details.
     */
	public class ISO9796d1Encoding
        : IAsymmetricBlockCipher
    {
        private static readonly BigInteger Sixteen = BigInteger.ValueOf(16);
        private static readonly BigInteger Six = BigInteger.ValueOf(6);

        private static readonly byte[] shadows =
        {
            0xe, 0x3, 0x5, 0x8, 0x9, 0x4, 0x2, 0xf,
            0x0, 0xd, 0xb, 0x6, 0x7, 0xa, 0xc, 0x1
        };

        private static readonly byte[] inverse =
        {
            0x8, 0xf, 0x6, 0x1, 0x5, 0x2, 0xb, 0xc,
            0x3, 0x4, 0xd, 0xa, 0xe, 0x9, 0x0, 0x7
        };

        private readonly IAsymmetricBlockCipher engine;
        private int bitSize;
        private bool forEncryption;
        private BigInteger modulus;
        private int padBits;

        public ISO9796d1Encoding(
            IAsymmetricBlockCipher cipher)
        {
            engine = cipher;
        }

        public string AlgorithmName => engine.AlgorithmName + "/ISO9796-1Padding";

        public void Init(
            bool forEncryption,
            ICipherParameters parameters)
        {
            RsaKeyParameters kParam;
            if (parameters is ParametersWithRandom)
            {
                var rParam = (ParametersWithRandom)parameters;
                kParam = (RsaKeyParameters)rParam.Parameters;
            }
            else
            {
                kParam = (RsaKeyParameters)parameters;
            }

            engine.Init(forEncryption, parameters);

            modulus = kParam.Modulus;
            bitSize = modulus.BitLength;

            this.forEncryption = forEncryption;
        }

        /**
         * return the input block size. The largest message we can process
         * is (key_size_in_bits + 3)/16, which in our world comes to
         * key_size_in_bytes / 2.
         */
        public int GetInputBlockSize()
        {
            var baseBlockSize = engine.GetInputBlockSize();

            if (forEncryption)
                return (baseBlockSize + 1) / 2;
            return baseBlockSize;
        }

        /**
		* return the maximum possible size for the output.
		*/
        public int GetOutputBlockSize()
        {
            var baseBlockSize = engine.GetOutputBlockSize();

            if (forEncryption)
                return baseBlockSize;
            return (baseBlockSize + 1) / 2;
        }

        public byte[] ProcessBlock(
            byte[] input,
            int inOff,
            int length)
        {
            if (forEncryption)
                return EncodeBlock(input, inOff, length);
            return DecodeBlock(input, inOff, length);
        }

        public IAsymmetricBlockCipher GetUnderlyingCipher()
        {
            return engine;
        }

        /**
         * set the number of bits in the next message to be treated as
         * pad bits.
         */
        public void SetPadBits(
            int padBits)
        {
            if (padBits > 7) throw new ArgumentException("padBits > 7");

            this.padBits = padBits;
        }

        /**
		* retrieve the number of pad bits in the last decoded message.
		*/
        public int GetPadBits()
        {
            return padBits;
        }

        private byte[] EncodeBlock(
            byte[] input,
            int inOff,
            int inLen)
        {
            var block = new byte[(bitSize + 7) / 8];
            var r = padBits + 1;
            var z = inLen;
            var t = (bitSize + 13) / 16;

            for (var i = 0; i < t; i += z)
                if (i > t - z)
                    Array.Copy(input, inOff + inLen - (t - i),
                        block, block.Length - t, t - i);
                else
                    Array.Copy(input, inOff, block, block.Length - (i + z), z);

            for (var i = block.Length - 2 * t; i != block.Length; i += 2)
            {
                var val = block[block.Length - t + i / 2];

                block[i] = (byte)((shadows[(uint)(val & 0xff) >> 4] << 4)
                                  | shadows[val & 0x0f]);
                block[i + 1] = val;
            }

            block[block.Length - 2 * z] ^= (byte)r;
            block[block.Length - 1] = (byte)((block[block.Length - 1] << 4) | 0x06);

            var maxBit = 8 - (bitSize - 1) % 8;
            var offSet = 0;

            if (maxBit != 8)
            {
                block[0] &= (byte)(0xff >> maxBit);
                block[0] |= (byte)(0x80 >> maxBit);
            }
            else
            {
                block[0] = 0x00;
                block[1] |= 0x80;
                offSet = 1;
            }

            return engine.ProcessBlock(block, offSet, block.Length - offSet);
        }

        /**
		* @exception InvalidCipherTextException if the decrypted block is not a valid ISO 9796 bit string
		*/
        private byte[] DecodeBlock(
            byte[] input,
            int inOff,
            int inLen)
        {
            var block = engine.ProcessBlock(input, inOff, inLen);
            var r = 1;
            var t = (bitSize + 13) / 16;

            var iS = new BigInteger(1, block);
            BigInteger iR;
            if (iS.Mod(Sixteen).Equals(Six))
            {
                iR = iS;
            }
            else
            {
                iR = modulus.Subtract(iS);

                if (!iR.Mod(Sixteen).Equals(Six))
                    throw new InvalidCipherTextException(
                        "resulting integer iS or (modulus - iS) is not congruent to 6 mod 16");
            }

            block = iR.ToByteArrayUnsigned();

            if ((block[block.Length - 1] & 0x0f) != 0x6)
                throw new InvalidCipherTextException("invalid forcing byte in block");

            block[block.Length - 1] =
                (byte)(((ushort)(block[block.Length - 1] & 0xff) >> 4)
                       | (inverse[(block[block.Length - 2] & 0xff) >> 4] << 4));

            block[0] = (byte)((shadows[(uint)(block[1] & 0xff) >> 4] << 4)
                              | shadows[block[1] & 0x0f]);

            var boundaryFound = false;
            var boundary = 0;

            for (var i = block.Length - 1; i >= block.Length - 2 * t; i -= 2)
            {
                var val = (shadows[(uint)(block[i] & 0xff) >> 4] << 4)
                          | shadows[block[i] & 0x0f];

                if (((block[i - 1] ^ val) & 0xff) != 0)
                {
                    if (!boundaryFound)
                    {
                        boundaryFound = true;
                        r = (block[i - 1] ^ val) & 0xff;
                        boundary = i - 1;
                    }
                    else
                    {
                        throw new InvalidCipherTextException("invalid tsums in block");
                    }
                }
            }

            block[boundary] = 0;

            var nblock = new byte[(block.Length - boundary) / 2];

            for (var i = 0; i < nblock.Length; i++) nblock[i] = block[2 * i + boundary + 1];

            padBits = r - 1;

            return nblock;
        }
    }
}

#endif