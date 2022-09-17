#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ParametersWithIV
        : ICipherParameters
    {
        private readonly byte[] iv;

        public ParametersWithIV(
            ICipherParameters parameters,
            byte[] iv)
            : this(parameters, iv, 0, iv.Length)
        {
        }

        public ParametersWithIV(
            ICipherParameters parameters,
            byte[] iv,
            int ivOff,
            int ivLen)
        {
            // NOTE: 'parameters' may be null to imply key re-use
            if (iv == null)
                throw new ArgumentNullException("iv");

            this.Parameters = parameters;
            this.iv = new byte[ivLen];
            Array.Copy(iv, ivOff, this.iv, 0, ivLen);
        }

        public ICipherParameters Parameters { get; }

        public byte[] GetIV()
        {
            return (byte[])iv.Clone();
        }
    }
}

#endif