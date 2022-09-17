#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
    /**
     * RFC 5246 7.4.1.4.1
     */
    public class SignatureAndHashAlgorithm
    {
        protected readonly byte mHash;
        protected readonly byte mSignature;

        /**
         * @param hash      {@link HashAlgorithm}
         * @param signature {@link SignatureAlgorithm}
         */
        public SignatureAndHashAlgorithm(byte hash, byte signature)
        {
            if (!TlsUtilities.IsValidUint8(hash)) throw new ArgumentException("should be a uint8", "hash");
            if (!TlsUtilities.IsValidUint8(signature)) throw new ArgumentException("should be a uint8", "signature");
            if (signature == SignatureAlgorithm.anonymous)
                throw new ArgumentException("MUST NOT be \"anonymous\"", "signature");

            mHash = hash;
            mSignature = signature;
        }

        /**
         * @return {@link HashAlgorithm}
         */
        public virtual byte Hash => mHash;

        /**
         * @return {@link SignatureAlgorithm}
         */
        public virtual byte Signature => mSignature;

        public override bool Equals(object obj)
        {
            if (!(obj is SignatureAndHashAlgorithm)) return false;
            var other = (SignatureAndHashAlgorithm)obj;
            return other.Hash == Hash && other.Signature == Signature;
        }

        public override int GetHashCode()
        {
            return (Hash << 16) | Signature;
        }

        /**
         * Encode this {@link SignatureAndHashAlgorithm} to a {@link Stream}.
         * 
         * @param output the {@link Stream} to encode to.
         * @throws IOException
         */
        public virtual void Encode(Stream output)
        {
            TlsUtilities.WriteUint8(Hash, output);
            TlsUtilities.WriteUint8(Signature, output);
        }

        /**
         * Parse a {@link SignatureAndHashAlgorithm} from a {@link Stream}.
         * 
         * @param input the {@link Stream} to parse from.
         * @return a {@link SignatureAndHashAlgorithm} object.
         * @throws IOException
         */
        public static SignatureAndHashAlgorithm Parse(Stream input)
        {
            var hash = TlsUtilities.ReadUint8(input);
            var signature = TlsUtilities.ReadUint8(input);
            return new SignatureAndHashAlgorithm(hash, signature);
        }
    }
}

#endif