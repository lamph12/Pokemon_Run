#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class Gost3410PrivateKeyParameters
        : Gost3410KeyParameters
    {
        public Gost3410PrivateKeyParameters(
            BigInteger x,
            Gost3410Parameters parameters)
            : base(true, parameters)
        {
            if (x.SignValue < 1 || x.BitLength > 256 || x.CompareTo(Parameters.Q) >= 0)
                throw new ArgumentException("Invalid x for GOST3410 private key", "x");

            this.X = x;
        }

        public Gost3410PrivateKeyParameters(
            BigInteger x,
            DerObjectIdentifier publicKeyParamSet)
            : base(true, publicKeyParamSet)
        {
            if (x.SignValue < 1 || x.BitLength > 256 || x.CompareTo(Parameters.Q) >= 0)
                throw new ArgumentException("Invalid x for GOST3410 private key", "x");

            this.X = x;
        }

        public BigInteger X { get; }
    }
}

#endif