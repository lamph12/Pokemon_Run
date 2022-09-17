#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public abstract class ECKeyParameters
        : AsymmetricKeyParameter
    {
        private static readonly string[] algorithms = { "EC", "ECDSA", "ECDH", "ECDHC", "ECGOST3410", "ECMQV" };

        protected ECKeyParameters(
            string algorithm,
            bool isPrivate,
            ECDomainParameters parameters)
            : base(isPrivate)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            this.AlgorithmName = VerifyAlgorithmName(algorithm);
            this.Parameters = parameters;
        }

        protected ECKeyParameters(
            string algorithm,
            bool isPrivate,
            DerObjectIdentifier publicKeyParamSet)
            : base(isPrivate)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            if (publicKeyParamSet == null)
                throw new ArgumentNullException("publicKeyParamSet");

            this.AlgorithmName = VerifyAlgorithmName(algorithm);
            Parameters = LookupParameters(publicKeyParamSet);
            this.PublicKeyParamSet = publicKeyParamSet;
        }

        public string AlgorithmName { get; }

        public ECDomainParameters Parameters { get; }

        public DerObjectIdentifier PublicKeyParamSet { get; }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            var other = obj as ECDomainParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            ECKeyParameters other)
        {
            return Parameters.Equals(other.Parameters) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return Parameters.GetHashCode() ^ base.GetHashCode();
        }

        internal ECKeyGenerationParameters CreateKeyGenerationParameters(
            SecureRandom random)
        {
            if (PublicKeyParamSet != null) return new ECKeyGenerationParameters(PublicKeyParamSet, random);

            return new ECKeyGenerationParameters(Parameters, random);
        }

        internal static string VerifyAlgorithmName(string algorithm)
        {
            var upper = Platform.ToUpperInvariant(algorithm);
            if (Array.IndexOf(algorithms, algorithm, 0, algorithms.Length) < 0)
                throw new ArgumentException("unrecognised algorithm: " + algorithm, "algorithm");
            return upper;
        }

        internal static ECDomainParameters LookupParameters(
            DerObjectIdentifier publicKeyParamSet)
        {
            if (publicKeyParamSet == null)
                throw new ArgumentNullException("publicKeyParamSet");

            var p = ECGost3410NamedCurves.GetByOid(publicKeyParamSet);

            if (p == null)
            {
                var x9 = ECKeyPairGenerator.FindECCurveByOid(publicKeyParamSet);

                if (x9 == null)
                    throw new ArgumentException("OID is not a valid public key parameter set", "publicKeyParamSet");

                p = new ECDomainParameters(x9.Curve, x9.G, x9.N, x9.H, x9.GetSeed());
            }

            return p;
        }
    }
}

#endif