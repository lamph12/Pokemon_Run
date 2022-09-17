#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ECKeyGenerationParameters
        : KeyGenerationParameters
    {
        public ECKeyGenerationParameters(
            ECDomainParameters domainParameters,
            SecureRandom random)
            : base(random, domainParameters.N.BitLength)
        {
            DomainParameters = domainParameters;
        }

        public ECKeyGenerationParameters(
            DerObjectIdentifier publicKeyParamSet,
            SecureRandom random)
            : this(ECKeyParameters.LookupParameters(publicKeyParamSet), random)
        {
            this.PublicKeyParamSet = publicKeyParamSet;
        }

        public ECDomainParameters DomainParameters { get; }

        public DerObjectIdentifier PublicKeyParamSet { get; }
    }
}

#endif