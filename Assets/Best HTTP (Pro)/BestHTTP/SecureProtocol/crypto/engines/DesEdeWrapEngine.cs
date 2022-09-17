#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
     * * Wrap keys according to
     * *
     * <a href="http://www.ietf.org/internet-drafts/draft-ietf-smime-key-wrap-01.txt">
     *     * draft-ietf-smime-key-wrap-01.txt
     * </a>
     * .
     * *
     * <p>
     *     * Note:
     *     *
     *     <ul>
     *         *
     *         <li>
     *             this is based on a draft, and as such is subject to change - don't use this class for anything requiring
     *             long term storage.
     *         </li>
     *         *
     *         <li>
     *             if you are using this to wrap triple-des keys you need to set the
     *             * parity bits on the key and, if it's a two-key triple-des key, pad it
     *             * yourself.
     *         </li>
     *         *
     *     </ul>
     *     *
     * </p>
     */
	public class DesEdeWrapEngine
        : IWrapper
    {
	    /**
         * Field IV2
         */
	    private static readonly byte[] IV2 =
        {
            0x4a, 0xdd, 0xa2,
            0x2c, 0x79, 0xe8,
            0x21, 0x05
        };

        private readonly byte[] digest = new byte[20];

        //
        // checksum digest
        //
        private readonly IDigest sha1 = new Sha1Digest();

        /**
         * Field engine
         */
        private CbcBlockCipher engine;

        /**
         * Field forWrapping
         */
        private bool forWrapping;

        /**
         * Field iv
         */
        private byte[] iv;

        /**
         * Field param
         */
        private KeyParameter param;

        /**
         * Field paramPlusIV
         */
        private ParametersWithIV paramPlusIV;

        /**
         * Method init
         * 
         * @param forWrapping
         * @param param
         */
        public virtual void Init(
            bool forWrapping,
            ICipherParameters parameters)
        {
            this.forWrapping = forWrapping;
            engine = new CbcBlockCipher(new DesEdeEngine());

            SecureRandom sr;
            if (parameters is ParametersWithRandom)
            {
                var pr = (ParametersWithRandom)parameters;
                parameters = pr.Parameters;
                sr = pr.Random;
            }
            else
            {
                sr = new SecureRandom();
            }

            if (parameters is KeyParameter)
            {
                param = (KeyParameter)parameters;
                if (this.forWrapping)
                {
                    // Hm, we have no IV but we want to wrap ?!?
                    // well, then we have to create our own IV.
                    iv = new byte[8];
                    sr.NextBytes(iv);

                    paramPlusIV = new ParametersWithIV(param, iv);
                }
            }
            else if (parameters is ParametersWithIV)
            {
                if (!forWrapping)
                    throw new ArgumentException("You should not supply an IV for unwrapping");

                paramPlusIV = (ParametersWithIV)parameters;
                iv = paramPlusIV.GetIV();
                param = (KeyParameter)paramPlusIV.Parameters;

                if (iv.Length != 8)
                    throw new ArgumentException("IV is not 8 octets", "parameters");
            }
        }

        /**
         * Method GetAlgorithmName
         * 
         * @return
         */
        public virtual string AlgorithmName => "DESede";

        /**
         * Method wrap
         * 
         * @param in
         * @param inOff
         * @param inLen
         * @return
         */
        public virtual byte[] Wrap(
            byte[] input,
            int inOff,
            int length)
        {
            if (!forWrapping) throw new InvalidOperationException("Not initialized for wrapping");

            var keyToBeWrapped = new byte[length];
            Array.Copy(input, inOff, keyToBeWrapped, 0, length);

            // Compute the CMS Key Checksum, (section 5.6.1), call this CKS.
            var CKS = CalculateCmsKeyChecksum(keyToBeWrapped);

            // Let WKCKS = WK || CKS where || is concatenation.
            var WKCKS = new byte[keyToBeWrapped.Length + CKS.Length];
            Array.Copy(keyToBeWrapped, 0, WKCKS, 0, keyToBeWrapped.Length);
            Array.Copy(CKS, 0, WKCKS, keyToBeWrapped.Length, CKS.Length);

            // Encrypt WKCKS in CBC mode using KEK as the key and IV as the
            // initialization vector. Call the results TEMP1.

            var blockSize = engine.GetBlockSize();

            if (WKCKS.Length % blockSize != 0)
                throw new InvalidOperationException("Not multiple of block length");

            engine.Init(true, paramPlusIV);

            var TEMP1 = new byte[WKCKS.Length];

            for (var currentBytePos = 0; currentBytePos != WKCKS.Length; currentBytePos += blockSize)
                engine.ProcessBlock(WKCKS, currentBytePos, TEMP1, currentBytePos);

            // Let TEMP2 = IV || TEMP1.
            var TEMP2 = new byte[iv.Length + TEMP1.Length];
            Array.Copy(iv, 0, TEMP2, 0, iv.Length);
            Array.Copy(TEMP1, 0, TEMP2, iv.Length, TEMP1.Length);

            // Reverse the order of the octets in TEMP2 and call the result TEMP3.
            var TEMP3 = reverse(TEMP2);

            // Encrypt TEMP3 in CBC mode using the KEK and an initialization vector
            // of 0x 4a dd a2 2c 79 e8 21 05. The resulting cipher text is the desired
            // result. It is 40 octets long if a 168 bit key is being wrapped.
            var param2 = new ParametersWithIV(param, IV2);
            engine.Init(true, param2);

            for (var currentBytePos = 0; currentBytePos != TEMP3.Length; currentBytePos += blockSize)
                engine.ProcessBlock(TEMP3, currentBytePos, TEMP3, currentBytePos);

            return TEMP3;
        }

        /**
         * Method unwrap
         * 
         * @param in
         * @param inOff
         * @param inLen
         * @return
         * @throws InvalidCipherTextException
         */
        public virtual byte[] Unwrap(
            byte[] input,
            int inOff,
            int length)
        {
            if (forWrapping) throw new InvalidOperationException("Not set for unwrapping");
            if (input == null) throw new InvalidCipherTextException("Null pointer as ciphertext");

            var blockSize = engine.GetBlockSize();

            if (length % blockSize != 0)
                throw new InvalidCipherTextException("Ciphertext not multiple of " + blockSize);

            /*
            // Check if the length of the cipher text is reasonable given the key
            // type. It must be 40 bytes for a 168 bit key and either 32, 40, or
            // 48 bytes for a 128, 192, or 256 bit key. If the length is not supported
            // or inconsistent with the algorithm for which the key is intended,
            // return error.
            //
            // we do not accept 168 bit keys. it has to be 192 bit.
            int lengthA = (estimatedKeyLengthInBit / 8) + 16;
            int lengthB = estimatedKeyLengthInBit % 8;
            if ((lengthA != keyToBeUnwrapped.Length) || (lengthB != 0)) {
                throw new XMLSecurityException("empty");
            }
            */

            // Decrypt the cipher text with TRIPLedeS in CBC mode using the KEK
            // and an initialization vector (IV) of 0x4adda22c79e82105. Call the output TEMP3.
            var param2 = new ParametersWithIV(param, IV2);
            engine.Init(false, param2);

            var TEMP3 = new byte[length];

            for (var currentBytePos = 0; currentBytePos != TEMP3.Length; currentBytePos += blockSize)
                engine.ProcessBlock(input, inOff + currentBytePos, TEMP3, currentBytePos);

            // Reverse the order of the octets in TEMP3 and call the result TEMP2.
            var TEMP2 = reverse(TEMP3);

            // Decompose TEMP2 into IV, the first 8 octets, and TEMP1, the remaining octets.
            iv = new byte[8];
            var TEMP1 = new byte[TEMP2.Length - 8];
            Array.Copy(TEMP2, 0, iv, 0, 8);
            Array.Copy(TEMP2, 8, TEMP1, 0, TEMP2.Length - 8);

            // Decrypt TEMP1 using TRIPLedeS in CBC mode using the KEK and the IV
            // found in the previous step. Call the result WKCKS.
            paramPlusIV = new ParametersWithIV(param, iv);
            engine.Init(false, paramPlusIV);

            var WKCKS = new byte[TEMP1.Length];

            for (var currentBytePos = 0; currentBytePos != WKCKS.Length; currentBytePos += blockSize)
                engine.ProcessBlock(TEMP1, currentBytePos, WKCKS, currentBytePos);

            // Decompose WKCKS. CKS is the last 8 octets and WK, the wrapped key, are
            // those octets before the CKS.
            var result = new byte[WKCKS.Length - 8];
            var CKStoBeVerified = new byte[8];
            Array.Copy(WKCKS, 0, result, 0, WKCKS.Length - 8);
            Array.Copy(WKCKS, WKCKS.Length - 8, CKStoBeVerified, 0, 8);

            // Calculate a CMS Key Checksum, (section 5.6.1), over the WK and compare
            // with the CKS extracted in the above step. If they are not equal, return error.
            if (!CheckCmsKeyChecksum(result, CKStoBeVerified))
                throw new InvalidCipherTextException(
                    "Checksum inside ciphertext is corrupted");

            // WK is the wrapped key, now extracted for use in data decryption.
            return result;
        }

        /**
         * Some key wrap algorithms make use of the Key Checksum defined
         * in CMS [CMS-Algorithms]. This is used to provide an integrity
         * check value for the key being wrapped. The algorithm is
         * 
         * - Compute the 20 octet SHA-1 hash on the key being wrapped.
         * - Use the first 8 octets of this hash as the checksum value.
         * 
         * @param key
         * @return
         * @throws Exception
         * @see http://www.w3.org/TR/xmlenc-core/#sec-CMSKeyChecksum
         */
        private byte[] CalculateCmsKeyChecksum(
            byte[] key)
        {
            sha1.BlockUpdate(key, 0, key.Length);
            sha1.DoFinal(digest, 0);

            var result = new byte[8];
            Array.Copy(digest, 0, result, 0, 8);
            return result;
        }

        /**
         * @param key
         * @param checksum
         * @return
         * @see http://www.w3.org/TR/xmlenc-core/#sec-CMSKeyChecksum
         */
        private bool CheckCmsKeyChecksum(
            byte[] key,
            byte[] checksum)
        {
            return Arrays.ConstantTimeAreEqual(CalculateCmsKeyChecksum(key), checksum);
        }

        private static byte[] reverse(byte[] bs)
        {
            var result = new byte[bs.Length];
            for (var i = 0; i < bs.Length; i++) result[i] = bs[bs.Length - (i + 1)];
            return result;
        }
    }
}

#endif