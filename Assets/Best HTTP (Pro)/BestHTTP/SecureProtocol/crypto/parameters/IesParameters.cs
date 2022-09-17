#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    /**
     * parameters for using an integrated cipher in stream mode.
     */
    public class IesParameters : ICipherParameters
    {
        private readonly byte[] derivation;
        private readonly byte[] encoding;

        /**
         * @param derivation the derivation parameter for the KDF function.
         * @param encoding the encoding parameter for the KDF function.
         * @param macKeySize the size of the MAC key (in bits).
         */
        public IesParameters(
            byte[] derivation,
            byte[] encoding,
            int macKeySize)
        {
            this.derivation = derivation;
            this.encoding = encoding;
            MacKeySize = macKeySize;
        }

        public int MacKeySize { get; }

        public byte[] GetDerivationV()
        {
            return derivation;
        }

        public byte[] GetEncodingV()
        {
            return encoding;
        }
    }
}

#endif