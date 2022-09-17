#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public abstract class DsaKeyParameters
        : AsymmetricKeyParameter
    {
        protected DsaKeyParameters(
            bool isPrivate,
            DsaParameters parameters)
            : base(isPrivate)
        {
            // Note: parameters may be null
            this.Parameters = parameters;
        }

        public DsaParameters Parameters { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DsaKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            DsaKeyParameters other)
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