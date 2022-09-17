#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ElGamalKeyParameters
        : AsymmetricKeyParameter
    {
        protected ElGamalKeyParameters(
            bool isPrivate,
            ElGamalParameters parameters)
            : base(isPrivate)
        {
            // TODO Should we allow 'parameters' to be null?
            this.Parameters = parameters;
        }

        public ElGamalParameters Parameters { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as ElGamalKeyParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            ElGamalKeyParameters other)
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