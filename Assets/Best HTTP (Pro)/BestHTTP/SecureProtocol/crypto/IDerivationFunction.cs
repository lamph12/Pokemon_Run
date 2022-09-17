#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto
{
    /**
     * base interface for general purpose byte derivation functions.
     */
    public interface IDerivationFunction
    {
        /**
         * return the message digest used as the basis for the function
         */
        IDigest Digest { get; }

        void Init(IDerivationParameters parameters);

        int GenerateBytes(byte[] output, int outOff, int length);
        //throws DataLengthException, ArgumentException;
    }
}

#endif