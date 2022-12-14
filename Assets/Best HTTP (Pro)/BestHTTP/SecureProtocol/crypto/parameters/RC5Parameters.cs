#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RC5Parameters
        : KeyParameter
    {
        public RC5Parameters(
            byte[] key,
            int rounds)
            : base(key)
        {
            if (key.Length > 255)
                throw new ArgumentException("RC5 key length can be no greater than 255");

            this.Rounds = rounds;
        }

        public int Rounds { get; }
    }
}

#endif