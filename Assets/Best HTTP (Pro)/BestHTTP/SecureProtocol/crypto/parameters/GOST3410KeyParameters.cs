#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public abstract class Gost3410KeyParameters
        : AsymmetricKeyParameter
    {
        protected Gost3410KeyParameters(
            bool isPrivate,
            Gost3410Parameters parameters)
            : base(isPrivate)
        {
            this.Parameters = parameters;
        }

        protected Gost3410KeyParameters(
            bool isPrivate,
            DerObjectIdentifier publicKeyParamSet)
            : base(isPrivate)
        {
            Parameters = LookupParameters(publicKeyParamSet);
            this.PublicKeyParamSet = publicKeyParamSet;
        }

        public Gost3410Parameters Parameters { get; }

        public DerObjectIdentifier PublicKeyParamSet { get; }

        // TODO Implement Equals/GetHashCode

        private static Gost3410Parameters LookupParameters(
            DerObjectIdentifier publicKeyParamSet)
        {
            if (publicKeyParamSet == null)
                throw new ArgumentNullException("publicKeyParamSet");

            var p = Gost3410NamedParameters.GetByOid(publicKeyParamSet);

            if (p == null)
                throw new ArgumentException("OID is not a valid CryptoPro public key parameter set",
                    "publicKeyParamSet");

            return new Gost3410Parameters(p.P, p.Q, p.A);
        }
    }
}

#endif