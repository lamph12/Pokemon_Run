#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Tls
{
    public abstract class AbstractTlsCredentials
        : TlsCredentials
    {
        public abstract Certificate Certificate { get; }
    }
}

#endif