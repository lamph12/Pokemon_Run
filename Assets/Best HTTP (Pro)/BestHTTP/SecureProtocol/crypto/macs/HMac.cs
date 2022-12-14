#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Macs
{
	/**
     * HMAC implementation based on RFC2104
     * 
     * H(K XOR opad, H(K XOR ipad, text))
     */
	public class HMac
        : IMac
    {
        private const byte IPAD = 0x36;
        private const byte OPAD = 0x5C;
        private readonly int blockLength;

        private readonly IDigest digest;
        private readonly int digestSize;

        private readonly byte[] inputPad;
        private readonly byte[] outputBuf;
        private IMemoable ipadState;
        private IMemoable opadState;

        public HMac(IDigest digest)
        {
            this.digest = digest;
            digestSize = digest.GetDigestSize();
            blockLength = digest.GetByteLength();
            inputPad = new byte[blockLength];
            outputBuf = new byte[blockLength + digestSize];
        }

        public virtual string AlgorithmName => digest.AlgorithmName + "/HMAC";

        public virtual void Init(ICipherParameters parameters)
        {
            digest.Reset();

            var key = ((KeyParameter)parameters).GetKey();
            var keyLength = key.Length;

            if (keyLength > blockLength)
            {
                digest.BlockUpdate(key, 0, keyLength);
                digest.DoFinal(inputPad, 0);

                keyLength = digestSize;
            }
            else
            {
                Array.Copy(key, 0, inputPad, 0, keyLength);
            }

            Array.Clear(inputPad, keyLength, blockLength - keyLength);
            Array.Copy(inputPad, 0, outputBuf, 0, blockLength);

            XorPad(inputPad, blockLength, IPAD);
            XorPad(outputBuf, blockLength, OPAD);

            if (digest is IMemoable)
            {
                opadState = ((IMemoable)digest).Copy();

                ((IDigest)opadState).BlockUpdate(outputBuf, 0, blockLength);
            }

            digest.BlockUpdate(inputPad, 0, inputPad.Length);

            if (digest is IMemoable) ipadState = ((IMemoable)digest).Copy();
        }

        public virtual int GetMacSize()
        {
            return digestSize;
        }

        public virtual void Update(byte input)
        {
            digest.Update(input);
        }

        public virtual void BlockUpdate(byte[] input, int inOff, int len)
        {
            digest.BlockUpdate(input, inOff, len);
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            digest.DoFinal(outputBuf, blockLength);

            if (opadState != null)
            {
                ((IMemoable)digest).Reset(opadState);
                digest.BlockUpdate(outputBuf, blockLength, digest.GetDigestSize());
            }
            else
            {
                digest.BlockUpdate(outputBuf, 0, outputBuf.Length);
            }

            var len = digest.DoFinal(output, outOff);

            Array.Clear(outputBuf, blockLength, digestSize);

            if (ipadState != null)
                ((IMemoable)digest).Reset(ipadState);
            else
                digest.BlockUpdate(inputPad, 0, inputPad.Length);

            return len;
        }

        /**
        * Reset the mac generator.
        */
        public virtual void Reset()
        {
            // Reset underlying digest
            digest.Reset();

            // Initialise the digest
            digest.BlockUpdate(inputPad, 0, inputPad.Length);
        }

        public virtual IDigest GetUnderlyingDigest()
        {
            return digest;
        }

        private static void XorPad(byte[] pad, int len, byte n)
        {
            for (var i = 0; i < len; ++i) pad[i] ^= n;
        }
    }
}

#endif