#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
    /// <summary> Cipher parameters with a fixed salt value associated with them.</summary>
    public class ParametersWithSalt : ICipherParameters
    {
        private readonly byte[] salt;

        public ParametersWithSalt(ICipherParameters parameters, byte[] salt) : this(parameters, salt, 0, salt.Length)
        {
        }

        public ParametersWithSalt(ICipherParameters parameters, byte[] salt, int saltOff, int saltLen)
        {
            this.salt = new byte[saltLen];
            Parameters = parameters;

            Array.Copy(salt, saltOff, this.salt, 0, saltLen);
        }

        public ICipherParameters Parameters { get; }

        public byte[] GetSalt()
        {
            return salt;
        }
    }
}

#endif