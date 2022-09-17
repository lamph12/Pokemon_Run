#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Crypto.Operators
{
    internal class X509Utilities
    {
        private static readonly Asn1Null derNull = DerNull.Instance;

        private static readonly IDictionary algorithms = Platform.CreateHashtable();
        private static readonly IDictionary exParams = Platform.CreateHashtable();
        private static readonly ISet noParams = new HashSet();

        static X509Utilities()
        {
            algorithms.Add("MD2WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD2WithRsaEncryption);
            algorithms.Add("MD2WITHRSA", PkcsObjectIdentifiers.MD2WithRsaEncryption);
            algorithms.Add("MD5WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD5WithRsaEncryption);
            algorithms.Add("MD5WITHRSA", PkcsObjectIdentifiers.MD5WithRsaEncryption);
            algorithms.Add("SHA1WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
            algorithms.Add("SHA1WITHRSA", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
            algorithms.Add("SHA224WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
            algorithms.Add("SHA224WITHRSA", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
            algorithms.Add("SHA256WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
            algorithms.Add("SHA256WITHRSA", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
            algorithms.Add("SHA384WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
            algorithms.Add("SHA384WITHRSA", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
            algorithms.Add("SHA512WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
            algorithms.Add("SHA512WITHRSA", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
            algorithms.Add("SHA1WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
            algorithms.Add("SHA224WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
            algorithms.Add("SHA256WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
            algorithms.Add("SHA384WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
            algorithms.Add("SHA512WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
            algorithms.Add("RIPEMD160WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
            algorithms.Add("RIPEMD160WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
            algorithms.Add("RIPEMD128WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
            algorithms.Add("RIPEMD128WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
            algorithms.Add("RIPEMD256WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
            algorithms.Add("RIPEMD256WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
            algorithms.Add("SHA1WITHDSA", X9ObjectIdentifiers.IdDsaWithSha1);
            algorithms.Add("DSAWITHSHA1", X9ObjectIdentifiers.IdDsaWithSha1);
            algorithms.Add("SHA224WITHDSA", NistObjectIdentifiers.DsaWithSha224);
            algorithms.Add("SHA256WITHDSA", NistObjectIdentifiers.DsaWithSha256);
            algorithms.Add("SHA384WITHDSA", NistObjectIdentifiers.DsaWithSha384);
            algorithms.Add("SHA512WITHDSA", NistObjectIdentifiers.DsaWithSha512);
            algorithms.Add("SHA1WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha1);
            algorithms.Add("ECDSAWITHSHA1", X9ObjectIdentifiers.ECDsaWithSha1);
            algorithms.Add("SHA224WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha224);
            algorithms.Add("SHA256WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha256);
            algorithms.Add("SHA384WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha384);
            algorithms.Add("SHA512WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha512);
            algorithms.Add("GOST3411WITHGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
            algorithms.Add("GOST3411WITHGOST3410-94", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
            algorithms.Add("GOST3411WITHECGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
            algorithms.Add("GOST3411WITHECGOST3410-2001", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
            algorithms.Add("GOST3411WITHGOST3410-2001", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);

            //
            // According to RFC 3279, the ASN.1 encoding SHALL (id-dsa-with-sha1) or MUST (ecdsa-with-SHA*) omit the parameters field.
            // The parameters field SHALL be NULL for RSA based signature algorithms.
            //
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha1);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha224);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha256);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha384);
            noParams.Add(X9ObjectIdentifiers.ECDsaWithSha512);
            noParams.Add(X9ObjectIdentifiers.IdDsaWithSha1);
            noParams.Add(NistObjectIdentifiers.DsaWithSha224);
            noParams.Add(NistObjectIdentifiers.DsaWithSha256);
            noParams.Add(NistObjectIdentifiers.DsaWithSha384);
            noParams.Add(NistObjectIdentifiers.DsaWithSha512);

            //
            // RFC 4491
            //
            noParams.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
            noParams.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);

            //
            // explicit params
            //
            var sha1AlgId = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);
            exParams.Add("SHA1WITHRSAANDMGF1", CreatePssParams(sha1AlgId, 20));

            var sha224AlgId = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha224, DerNull.Instance);
            exParams.Add("SHA224WITHRSAANDMGF1", CreatePssParams(sha224AlgId, 28));

            var sha256AlgId = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha256, DerNull.Instance);
            exParams.Add("SHA256WITHRSAANDMGF1", CreatePssParams(sha256AlgId, 32));

            var sha384AlgId = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha384, DerNull.Instance);
            exParams.Add("SHA384WITHRSAANDMGF1", CreatePssParams(sha384AlgId, 48));

            var sha512AlgId = new AlgorithmIdentifier(NistObjectIdentifiers.IdSha512, DerNull.Instance);
            exParams.Add("SHA512WITHRSAANDMGF1", CreatePssParams(sha512AlgId, 64));
        }

        /**
         * Return the digest algorithm using one of the standard JCA string
         * representations rather than the algorithm identifier (if possible).
         */
        private static string GetDigestAlgName(
            DerObjectIdentifier digestAlgOID)
        {
            if (PkcsObjectIdentifiers.MD5.Equals(digestAlgOID))
                return "MD5";
            if (OiwObjectIdentifiers.IdSha1.Equals(digestAlgOID))
                return "SHA1";
            if (NistObjectIdentifiers.IdSha224.Equals(digestAlgOID))
                return "SHA224";
            if (NistObjectIdentifiers.IdSha256.Equals(digestAlgOID))
                return "SHA256";
            if (NistObjectIdentifiers.IdSha384.Equals(digestAlgOID))
                return "SHA384";
            if (NistObjectIdentifiers.IdSha512.Equals(digestAlgOID))
                return "SHA512";
            if (TeleTrusTObjectIdentifiers.RipeMD128.Equals(digestAlgOID))
                return "RIPEMD128";
            if (TeleTrusTObjectIdentifiers.RipeMD160.Equals(digestAlgOID))
                return "RIPEMD160";
            if (TeleTrusTObjectIdentifiers.RipeMD256.Equals(digestAlgOID))
                return "RIPEMD256";
            if (CryptoProObjectIdentifiers.GostR3411.Equals(digestAlgOID))
                return "GOST3411";
            return digestAlgOID.Id;
        }

        internal static string GetSignatureName(AlgorithmIdentifier sigAlgId)
        {
            var parameters = sigAlgId.Parameters;

            if (parameters != null && !derNull.Equals(parameters))
            {
                if (sigAlgId.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss))
                {
                    var rsaParams = RsassaPssParameters.GetInstance(parameters);

                    return GetDigestAlgName(rsaParams.HashAlgorithm.Algorithm) + "withRSAandMGF1";
                }

                if (sigAlgId.Algorithm.Equals(X9ObjectIdentifiers.ECDsaWithSha2))
                {
                    var ecDsaParams = Asn1Sequence.GetInstance(parameters);

                    return GetDigestAlgName((DerObjectIdentifier)ecDsaParams[0]) + "withECDSA";
                }
            }

            return sigAlgId.Algorithm.Id;
        }

        private static RsassaPssParameters CreatePssParams(
            AlgorithmIdentifier hashAlgId,
            int saltSize)
        {
            return new RsassaPssParameters(
                hashAlgId,
                new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, hashAlgId),
                new DerInteger(saltSize),
                new DerInteger(1));
        }

        internal static DerObjectIdentifier GetAlgorithmOid(
            string algorithmName)
        {
            algorithmName = Platform.ToUpperInvariant(algorithmName);

            if (algorithms.Contains(algorithmName)) return (DerObjectIdentifier)algorithms[algorithmName];

            return new DerObjectIdentifier(algorithmName);
        }

        internal static AlgorithmIdentifier GetSigAlgID(
            DerObjectIdentifier sigOid,
            string algorithmName)
        {
            if (noParams.Contains(sigOid)) return new AlgorithmIdentifier(sigOid);

            algorithmName = Platform.ToUpperInvariant(algorithmName);

            if (exParams.Contains(algorithmName))
                return new AlgorithmIdentifier(sigOid, (Asn1Encodable)exParams[algorithmName]);

            return new AlgorithmIdentifier(sigOid, DerNull.Instance);
        }

        internal static IEnumerable GetAlgNames()
        {
            return new EnumerableProxy(algorithms.Keys);
        }
    }

    internal class SignerBucket
        : Stream
    {
        protected readonly ISigner signer;

        public SignerBucket(
            ISigner signer)
        {
            this.signer = signer;
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override bool CanSeek => false;

        public override long Length => 0;

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            throw new NotImplementedException();
        }

        public override int ReadByte()
        {
            throw new NotImplementedException();
        }

        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
            if (count > 0) signer.BlockUpdate(buffer, offset, count);
        }

        public override void WriteByte(
            byte b)
        {
            signer.Update(b);
        }

        public override void Flush()
        {
        }

        public override long Seek(
            long offset,
            SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(
            long length)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Calculator factory class for signature generation in ASN.1 based profiles that use an AlgorithmIdentifier to
    ///     preserve
    ///     signature algorithm details.
    /// </summary>
    public class Asn1SignatureFactory : ISignatureFactory
    {
        private readonly AlgorithmIdentifier algID;
        private readonly string algorithm;
        private readonly AsymmetricKeyParameter privateKey;
        private readonly SecureRandom random;

        /// <summary>
        ///     Base constructor.
        /// </summary>
        /// <param name="algorithm">The name of the signature algorithm to use.</param>
        /// <param name="privateKey">The private key to be used in the signing operation.</param>
        public Asn1SignatureFactory(string algorithm, AsymmetricKeyParameter privateKey) : this(algorithm, privateKey,
            null)
        {
        }

        /// <summary>
        ///     Constructor which also specifies a source of randomness to be used if one is required.
        /// </summary>
        /// <param name="algorithm">The name of the signature algorithm to use.</param>
        /// <param name="privateKey">The private key to be used in the signing operation.</param>
        /// <param name="random">The source of randomness to be used in signature calculation.</param>
        public Asn1SignatureFactory(string algorithm, AsymmetricKeyParameter privateKey, SecureRandom random)
        {
            var sigOid = X509Utilities.GetAlgorithmOid(algorithm);

            this.algorithm = algorithm;
            this.privateKey = privateKey;
            this.random = random;
            algID = X509Utilities.GetSigAlgID(sigOid, algorithm);
        }

        /// <summary>
        ///     Allows enumeration of the signature names supported by the verifier provider.
        /// </summary>
        public static IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

        public object AlgorithmDetails => algID;

        public IStreamCalculator CreateCalculator()
        {
            var sig = SignerUtilities.GetSigner(algorithm);

            if (random != null)
                sig.Init(true, new ParametersWithRandom(privateKey, random));
            else
                sig.Init(true, privateKey);

            return new SigCalculator(sig);
        }
    }

    internal class SigCalculator : IStreamCalculator
    {
        private readonly ISigner sig;

        internal SigCalculator(ISigner sig)
        {
            this.sig = sig;
            Stream = new SignerBucket(sig);
        }

        public Stream Stream { get; }

        public object GetResult()
        {
            return new SigResult(sig);
        }
    }

    internal class SigResult : IBlockResult
    {
        private readonly ISigner sig;

        internal SigResult(ISigner sig)
        {
            this.sig = sig;
        }

        public byte[] Collect()
        {
            return sig.GenerateSignature();
        }

        public int Collect(byte[] destination, int offset)
        {
            var signature = Collect();

            Array.Copy(signature, 0, destination, offset, signature.Length);

            return signature.Length;
        }
    }

    /// <summary>
    ///     Verifier class for signature verification in ASN.1 based profiles that use an AlgorithmIdentifier to preserve
    ///     signature algorithm details.
    /// </summary>
    public class Asn1VerifierFactory : IVerifierFactory
    {
        private readonly AlgorithmIdentifier algID;
        private readonly AsymmetricKeyParameter publicKey;

        /// <summary>
        ///     Base constructor.
        /// </summary>
        /// <param name="algorithm">The name of the signature algorithm to use.</param>
        /// <param name="publicKey">The public key to be used in the verification operation.</param>
        public Asn1VerifierFactory(string algorithm, AsymmetricKeyParameter publicKey)
        {
            var sigOid = X509Utilities.GetAlgorithmOid(algorithm);

            this.publicKey = publicKey;
            algID = X509Utilities.GetSigAlgID(sigOid, algorithm);
        }

        public Asn1VerifierFactory(AlgorithmIdentifier algorithm, AsymmetricKeyParameter publicKey)
        {
            this.publicKey = publicKey;
            algID = algorithm;
        }

        public object AlgorithmDetails => algID;

        public IStreamCalculator CreateCalculator()
        {
            var sig = SignerUtilities.GetSigner(X509Utilities.GetSignatureName(algID));

            sig.Init(false, publicKey);

            return new VerifierCalculator(sig);
        }
    }

    internal class VerifierCalculator : IStreamCalculator
    {
        private readonly ISigner sig;

        internal VerifierCalculator(ISigner sig)
        {
            this.sig = sig;
            Stream = new SignerBucket(sig);
        }

        public Stream Stream { get; }

        public object GetResult()
        {
            return new VerifierResult(sig);
        }
    }

    internal class VerifierResult : IVerifier
    {
        private readonly ISigner sig;

        internal VerifierResult(ISigner sig)
        {
            this.sig = sig;
        }

        public bool IsVerified(byte[] signature)
        {
            return sig.VerifySignature(signature);
        }

        public bool IsVerified(byte[] signature, int off, int length)
        {
            var sigBytes = new byte[length];

            Array.Copy(signature, 0, sigBytes, off, sigBytes.Length);

            return sig.VerifySignature(signature);
        }
    }

    /// <summary>
    ///     Provider class which supports dynamic creation of signature verifiers.
    /// </summary>
    public class Asn1VerifierFactoryProvider : IVerifierFactoryProvider
    {
        private readonly AsymmetricKeyParameter publicKey;

        /// <summary>
        ///     Base constructor - specify the public key to be used in verification.
        /// </summary>
        /// <param name="publicKey">The public key to be used in creating verifiers provided by this object.</param>
        public Asn1VerifierFactoryProvider(AsymmetricKeyParameter publicKey)
        {
            this.publicKey = publicKey;
        }

        /// <summary>
        ///     Allows enumeration of the signature names supported by the verifier provider.
        /// </summary>
        public IEnumerable SignatureAlgNames => X509Utilities.GetAlgNames();

        public IVerifierFactory CreateVerifierFactory(object algorithmDetails)
        {
            return new Asn1VerifierFactory((AlgorithmIdentifier)algorithmDetails, publicKey);
        }
    }
}

#endif