#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto
{
    public abstract class AsymmetricKeyParameter
        : ICipherParameters
    {
        protected AsymmetricKeyParameter(
            bool privateKey)
        {
            this.IsPrivate = privateKey;
        }

        public bool IsPrivate { get; }

        public override bool Equals(
            object obj)
        {
            var other = obj as AsymmetricKeyParameter;

            if (other == null) return false;

            return Equals(other);
        }

        protected bool Equals(
            AsymmetricKeyParameter other)
        {
            return IsPrivate == other.IsPrivate;
        }

        public override int GetHashCode()
        {
            return IsPrivate.GetHashCode();
        }
    }
}

#endif