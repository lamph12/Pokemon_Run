#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class AeadParameters
        : ICipherParameters
    {
        private readonly byte[] associatedText;
        private readonly byte[] nonce;

        /**
         * Base constructor.
         * 
         * @param key key to be used by underlying cipher
         * @param macSize macSize in bits
         * @param nonce nonce to be used
         */
        public AeadParameters(KeyParameter key, int macSize, byte[] nonce)
            : this(key, macSize, nonce, null)
        {
        }

        /**
         * Base constructor.
         * 
         * @param key key to be used by underlying cipher
         * @param macSize macSize in bits
         * @param nonce nonce to be used
         * @param associatedText associated text, if any
         */
        public AeadParameters(
            KeyParameter key,
            int macSize,
            byte[] nonce,
            byte[] associatedText)
        {
            this.Key = key;
            this.nonce = nonce;
            this.MacSize = macSize;
            this.associatedText = associatedText;
        }

        public virtual KeyParameter Key { get; }

        public virtual int MacSize { get; }

        public virtual byte[] GetAssociatedText()
        {
            return associatedText;
        }

        public virtual byte[] GetNonce()
        {
            return nonce;
        }
    }
}

#endif