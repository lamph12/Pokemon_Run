#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Engines
{
    /**
	* this does your basic ElGamal algorithm.
	*/
    public class ElGamalEngine
        : IAsymmetricBlockCipher
    {
        private int bitSize;
        private bool forEncryption;
        private ElGamalKeyParameters key;
        private SecureRandom random;

        public virtual string AlgorithmName => "ElGamal";

        /**
         * initialise the ElGamal engine.
         * 
         * @param forEncryption true if we are encrypting, false otherwise.
         * @param param the necessary ElGamal key parameters.
         */
        public virtual void Init(
            bool forEncryption,
            ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                var p = (ParametersWithRandom)parameters;

                key = (ElGamalKeyParameters)p.Parameters;
                random = p.Random;
            }
            else
            {
                key = (ElGamalKeyParameters)parameters;
                random = new SecureRandom();
            }

            this.forEncryption = forEncryption;
            bitSize = key.Parameters.P.BitLength;

            if (forEncryption)
            {
                if (!(key is ElGamalPublicKeyParameters))
                    throw new ArgumentException("ElGamalPublicKeyParameters are required for encryption.");
            }
            else
            {
                if (!(key is ElGamalPrivateKeyParameters))
                    throw new ArgumentException("ElGamalPrivateKeyParameters are required for decryption.");
            }
        }

        /**
         * Return the maximum size for an input block to this engine.
         * For ElGamal this is always one byte less than the size of P on
         * encryption, and twice the length as the size of P on decryption.
         * 
         * @return maximum size for an input block.
         */
        public virtual int GetInputBlockSize()
        {
            if (forEncryption) return (bitSize - 1) / 8;

            return 2 * ((bitSize + 7) / 8);
        }

        /**
         * Return the maximum size for an output block to this engine.
         * For ElGamal this is always one byte less than the size of P on
         * decryption, and twice the length as the size of P on encryption.
         * 
         * @return maximum size for an output block.
         */
        public virtual int GetOutputBlockSize()
        {
            if (forEncryption) return 2 * ((bitSize + 7) / 8);

            return (bitSize - 1) / 8;
        }

        /**
         * Process a single block using the basic ElGamal algorithm.
         * 
         * @param in the input array.
         * @param inOff the offset into the input buffer where the data starts.
         * @param length the length of the data to be processed.
         * @return the result of the ElGamal process.
         * @exception DataLengthException the input block is too large.
         */
        public virtual byte[] ProcessBlock(
            byte[] input,
            int inOff,
            int length)
        {
            if (key == null)
                throw new InvalidOperationException("ElGamal engine not initialised");

            var maxLength = forEncryption
                ? (bitSize - 1 + 7) / 8
                : GetInputBlockSize();

            if (length > maxLength)
                throw new DataLengthException("input too large for ElGamal cipher.\n");

            var p = key.Parameters.P;

            byte[] output;
            if (key is ElGamalPrivateKeyParameters) // decryption
            {
                var halfLength = length / 2;
                var gamma = new BigInteger(1, input, inOff, halfLength);
                var phi = new BigInteger(1, input, inOff + halfLength, halfLength);

                var priv = (ElGamalPrivateKeyParameters)key;

                // a shortcut, which generally relies on p being prime amongst other things.
                // if a problem with this shows up, check the p and g values!
                var m = gamma.ModPow(p.Subtract(BigInteger.One).Subtract(priv.X), p).Multiply(phi).Mod(p);

                output = m.ToByteArrayUnsigned();
            }
            else // encryption
            {
                var tmp = new BigInteger(1, input, inOff, length);

                if (tmp.BitLength >= p.BitLength)
                    throw new DataLengthException("input too large for ElGamal cipher.\n");


                var pub = (ElGamalPublicKeyParameters)key;

                var pSub2 = p.Subtract(BigInteger.Two);

                // TODO In theory, a series of 'k', 'g.ModPow(k, p)' and 'y.ModPow(k, p)' can be pre-calculated
                BigInteger k;
                do
                {
                    k = new BigInteger(p.BitLength, random);
                } while (k.SignValue == 0 || k.CompareTo(pSub2) > 0);

                var g = key.Parameters.G;
                var gamma = g.ModPow(k, p);
                var phi = tmp.Multiply(pub.Y.ModPow(k, p)).Mod(p);

                output = new byte[GetOutputBlockSize()];

                // TODO Add methods to allow writing BigInteger to existing byte array?
                var out1 = gamma.ToByteArrayUnsigned();
                var out2 = phi.ToByteArrayUnsigned();
                out1.CopyTo(output, output.Length / 2 - out1.Length);
                out2.CopyTo(output, output.Length - out2.Length);
            }

            return output;
        }
    }
}

#endif