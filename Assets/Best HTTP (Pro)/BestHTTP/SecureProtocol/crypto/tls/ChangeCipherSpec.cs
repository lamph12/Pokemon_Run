#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Tls
{
    public abstract class ChangeCipherSpec
    {
        public const byte change_cipher_spec = 1;
    }
}

#endif