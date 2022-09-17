#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DHValidationParameters
    {
        private readonly byte[] seed;

        public DHValidationParameters(
            byte[] seed,
            int counter)
        {
            if (seed == null)
                throw new ArgumentNullException("seed");

            this.seed = (byte[])seed.Clone();
            this.Counter = counter;
        }

        public int Counter { get; }

        public byte[] GetSeed()
        {
            return (byte[])seed.Clone();
        }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DHValidationParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            DHValidationParameters other)
        {
            return Counter == other.Counter
                   && Arrays.AreEqual(seed, other.seed);
        }

        public override int GetHashCode()
        {
            return Counter.GetHashCode() ^ Arrays.GetHashCode(seed);
        }
    }
}

#endif