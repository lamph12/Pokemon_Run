#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Signers
{
    /**
     * The Digital Signature Algorithm - as described in "Handbook of Applied
     * Cryptography", pages 452 - 453.
     */
    public class DsaSigner
        : IDsa
    {
        protected readonly IDsaKCalculator kCalculator;

        protected DsaKeyParameters key;
        protected SecureRandom random;

        /**
         * Default configuration, random K values.
         */
        public DsaSigner()
        {
            kCalculator = new RandomDsaKCalculator();
        }

        /**
         * Configuration with an alternate, possibly deterministic calculator of K.
         * 
         * @param kCalculator a K value calculator.
         */
        public DsaSigner(IDsaKCalculator kCalculator)
        {
            this.kCalculator = kCalculator;
        }

        public virtual string AlgorithmName => "DSA";

        public virtual void Init(bool forSigning, ICipherParameters parameters)
        {
            SecureRandom providedRandom = null;

            if (forSigning)
            {
                if (parameters is ParametersWithRandom)
                {
                    var rParam = (ParametersWithRandom)parameters;

                    providedRandom = rParam.Random;
                    parameters = rParam.Parameters;
                }

                if (!(parameters is DsaPrivateKeyParameters))
                    throw new InvalidKeyException("DSA private key required for signing");

                key = (DsaPrivateKeyParameters)parameters;
            }
            else
            {
                if (!(parameters is DsaPublicKeyParameters))
                    throw new InvalidKeyException("DSA public key required for verification");

                key = (DsaPublicKeyParameters)parameters;
            }

            random = InitSecureRandom(forSigning && !kCalculator.IsDeterministic, providedRandom);
        }

        /**
         * Generate a signature for the given message using the key we were
         * initialised with. For conventional DSA the message should be a SHA-1
         * hash of the message of interest.
         * 
         * @param message the message that will be verified later.
         */
        public virtual BigInteger[] GenerateSignature(byte[] message)
        {
            var parameters = key.Parameters;
            var q = parameters.Q;
            var m = CalculateE(q, message);
            var x = ((DsaPrivateKeyParameters)key).X;

            if (kCalculator.IsDeterministic)
                kCalculator.Init(q, x, message);
            else
                kCalculator.Init(q, random);

            var k = kCalculator.NextK();

            var r = parameters.G.ModPow(k, parameters.P).Mod(q);

            k = k.ModInverse(q).Multiply(m.Add(x.Multiply(r)));

            var s = k.Mod(q);

            return new[] { r, s };
        }

        /**
         * return true if the value r and s represent a DSA signature for
         * the passed in message for standard DSA the message should be a
         * SHA-1 hash of the real message to be verified.
         */
        public virtual bool VerifySignature(byte[] message, BigInteger r, BigInteger s)
        {
            var parameters = key.Parameters;
            var q = parameters.Q;
            var m = CalculateE(q, message);

            if (r.SignValue <= 0 || q.CompareTo(r) <= 0) return false;

            if (s.SignValue <= 0 || q.CompareTo(s) <= 0) return false;

            var w = s.ModInverse(q);

            var u1 = m.Multiply(w).Mod(q);
            var u2 = r.Multiply(w).Mod(q);

            var p = parameters.P;
            u1 = parameters.G.ModPow(u1, p);
            u2 = ((DsaPublicKeyParameters)key).Y.ModPow(u2, p);

            var v = u1.Multiply(u2).Mod(p).Mod(q);

            return v.Equals(r);
        }

        protected virtual BigInteger CalculateE(BigInteger n, byte[] message)
        {
            var length = System.Math.Min(message.Length, n.BitLength / 8);

            return new BigInteger(1, message, 0, length);
        }

        protected virtual SecureRandom InitSecureRandom(bool needed, SecureRandom provided)
        {
            return !needed ? null : provided != null ? provided : new SecureRandom();
        }
    }
}

#endif