#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
namespace Org.BouncyCastle.Asn1
{
    public interface IAsn1ApplicationSpecificParser
        : IAsn1Convertible
    {
        IAsn1Convertible ReadObject();
    }
}

#endif