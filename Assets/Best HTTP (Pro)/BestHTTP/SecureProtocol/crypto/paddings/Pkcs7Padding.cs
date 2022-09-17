#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Paddings
{
    /**
    * A padder that adds Pkcs7/Pkcs5 padding to a block.
    */
    public class Pkcs7Padding
        : IBlockCipherPadding
    {
        /**
         * Initialise the padder.
         * 
         * @param random - a SecureRandom if available.
         */
        public void Init(
            SecureRandom random)
        {
            // nothing to do.
        }

        /**
         * Return the name of the algorithm the cipher implements.
         * 
         * @return the name of the algorithm the cipher implements.
         */
        public string PaddingName => "PKCS7";

        /**
         * add the pad bytes to the passed in block, returning the
         * number of bytes added.
         */
        public int AddPadding(
            byte[] input,
            int inOff)
        {
            var code = (byte)(input.Length - inOff);

            while (inOff < input.Length)
            {
                input[inOff] = code;
                inOff++;
            }

            return code;
        }

        /**
        * return the number of pad bytes present in the block.
        */
        public int PadCount(
            byte[] input)
        {
            var countAsByte = input[input.Length - 1];
            int count = countAsByte;

            if (count < 1 || count > input.Length)
                throw new InvalidCipherTextException("pad block corrupted");

            for (var i = 2; i <= count; i++)
                if (input[input.Length - i] != countAsByte)
                    throw new InvalidCipherTextException("pad block corrupted");

            return count;
        }
    }
}

#endif