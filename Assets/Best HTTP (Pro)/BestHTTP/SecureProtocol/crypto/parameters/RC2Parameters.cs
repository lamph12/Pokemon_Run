#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RC2Parameters
        : KeyParameter
    {
        public RC2Parameters(
            byte[] key)
            : this(key, key.Length > 128 ? 1024 : key.Length * 8)
        {
        }

        public RC2Parameters(
            byte[] key,
            int keyOff,
            int keyLen)
            : this(key, keyOff, keyLen, keyLen > 128 ? 1024 : keyLen * 8)
        {
        }

        public RC2Parameters(
            byte[] key,
            int bits)
            : base(key)
        {
            this.EffectiveKeyBits = bits;
        }

        public RC2Parameters(
            byte[] key,
            int keyOff,
            int keyLen,
            int bits)
            : base(key, keyOff, keyLen)
        {
            this.EffectiveKeyBits = bits;
        }

        public int EffectiveKeyBits { get; }
    }
}

#endif