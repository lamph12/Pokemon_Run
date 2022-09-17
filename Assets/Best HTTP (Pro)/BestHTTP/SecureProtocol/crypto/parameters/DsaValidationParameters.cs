#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DsaValidationParameters
    {
        private readonly int counter;
        private readonly byte[] seed;

        public DsaValidationParameters(byte[] seed, int counter)
            : this(seed, counter, -1)
        {
        }

        public DsaValidationParameters(
            byte[] seed,
            int counter,
            int usageIndex)
        {
            if (seed == null)
                throw new ArgumentNullException("seed");

            this.seed = (byte[])seed.Clone();
            this.counter = counter;
            this.UsageIndex = usageIndex;
        }

        public virtual int Counter => counter;

        public virtual int UsageIndex { get; }

        public virtual byte[] GetSeed()
        {
            return (byte[])seed.Clone();
        }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as DsaValidationParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected virtual bool Equals(
            DsaValidationParameters other)
        {
            return counter == other.counter
                   && Arrays.AreEqual(seed, other.seed);
        }

        public override int GetHashCode()
        {
            return counter.GetHashCode() ^ Arrays.GetHashCode(seed);
        }
    }
}

#endif