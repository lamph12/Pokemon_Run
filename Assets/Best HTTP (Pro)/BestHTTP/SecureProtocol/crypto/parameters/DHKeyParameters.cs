#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DHKeyParameters
        : AsymmetricKeyParameter
    {
        protected DHKeyParameters(
            bool isPrivate,
            DHParameters parameters)
            : this(isPrivate, parameters, PkcsObjectIdentifiers.DhKeyAgreement)
        {
        }

        protected DHKeyParameters(
            bool isPrivate,
            DHParameters parameters,
            DerObjectIdentifier algorithmOid)
            : base(isPrivate)
        {
            // TODO Should we allow parameters to be null?
            this.Parameters = parameters;
            this.AlgorithmOid = algorithmOid;
        }

        public DHParameters Parameters { get; }

        public DerObjectIdentifier AlgorithmOid { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DHKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            DHKeyParameters other)
        {
            return Equals(Parameters, other.Parameters)
                   && base.Equals(other);
        }

        public override int GetHashCode()
        {
            var hc = base.GetHashCode();

            if (Parameters != null) hc ^= Parameters.GetHashCode();

            return hc;
        }
    }
}

#endif