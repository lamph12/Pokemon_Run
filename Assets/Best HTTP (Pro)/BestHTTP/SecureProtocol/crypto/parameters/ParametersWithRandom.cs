#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ParametersWithRandom
        : ICipherParameters
    {
        public ParametersWithRandom(
            ICipherParameters parameters,
            SecureRandom random)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (random == null)
                throw new ArgumentNullException("random");

            this.Parameters = parameters;
            this.Random = random;
        }

        public ParametersWithRandom(
            ICipherParameters parameters)
            : this(parameters, new SecureRandom())
        {
        }

        public SecureRandom Random { get; }

        public ICipherParameters Parameters { get; }

        [Obsolete("Use Random property instead")]
        public SecureRandom GetRandom()
        {
            return Random;
        }
    }
}

#endif