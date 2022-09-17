#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    /**
     * Buffers input until the hash algorithm is determined.
     */
    internal class DeferredHash
        : TlsHandshakeHash
    {
        protected const int BUFFERING_HASH_LIMIT = 4;

        private DigestInputBuffer mBuf;

        protected TlsContext mContext;
        private readonly IDictionary mHashes;
        private int mPrfHashAlgorithm;

        internal DeferredHash()
        {
            mBuf = new DigestInputBuffer();
            mHashes = Platform.CreateHashtable();
            mPrfHashAlgorithm = -1;
        }

        private DeferredHash(byte prfHashAlgorithm, IDigest prfHash)
        {
            mBuf = null;
            mHashes = Platform.CreateHashtable();
            mPrfHashAlgorithm = prfHashAlgorithm;
            mHashes[prfHashAlgorithm] = prfHash;
        }

        public virtual void Init(TlsContext context)
        {
            mContext = context;
        }

        public virtual TlsHandshakeHash NotifyPrfDetermined()
        {
            var prfAlgorithm = mContext.SecurityParameters.PrfAlgorithm;
            if (prfAlgorithm == PrfAlgorithm.tls_prf_legacy)
            {
                var legacyHash = new CombinedHash();
                legacyHash.Init(mContext);
                mBuf.UpdateDigest(legacyHash);
                return legacyHash.NotifyPrfDetermined();
            }

            mPrfHashAlgorithm = TlsUtilities.GetHashAlgorithmForPrfAlgorithm(prfAlgorithm);

            CheckTrackingHash((byte)mPrfHashAlgorithm);

            return this;
        }

        public virtual void TrackHashAlgorithm(byte hashAlgorithm)
        {
            if (mBuf == null)
                throw new InvalidOperationException("Too late to track more hash algorithms");

            CheckTrackingHash(hashAlgorithm);
        }

        public virtual void SealHashAlgorithms()
        {
            CheckStopBuffering();
        }

        public virtual TlsHandshakeHash StopTracking()
        {
            var prfHashAlgorithm = (byte)mPrfHashAlgorithm;
            var prfHash = TlsUtilities.CloneHash(prfHashAlgorithm, (IDigest)mHashes[prfHashAlgorithm]);
            if (mBuf != null) mBuf.UpdateDigest(prfHash);
            var result = new DeferredHash(prfHashAlgorithm, prfHash);
            result.Init(mContext);
            return result;
        }

        public virtual IDigest ForkPrfHash()
        {
            CheckStopBuffering();

            var prfHashAlgorithm = (byte)mPrfHashAlgorithm;
            if (mBuf != null)
            {
                var prfHash = TlsUtilities.CreateHash(prfHashAlgorithm);
                mBuf.UpdateDigest(prfHash);
                return prfHash;
            }

            return TlsUtilities.CloneHash(prfHashAlgorithm, (IDigest)mHashes[prfHashAlgorithm]);
        }

        public virtual byte[] GetFinalHash(byte hashAlgorithm)
        {
            var d = (IDigest)mHashes[hashAlgorithm];
            if (d == null)
                throw new InvalidOperationException("HashAlgorithm." + HashAlgorithm.GetText(hashAlgorithm) +
                                                    " is not being tracked");

            d = TlsUtilities.CloneHash(hashAlgorithm, d);
            if (mBuf != null) mBuf.UpdateDigest(d);

            return DigestUtilities.DoFinal(d);
        }

        public virtual string AlgorithmName =>
            throw new InvalidOperationException("Use Fork() to get a definite IDigest");

        public virtual int GetByteLength()
        {
            throw new InvalidOperationException("Use Fork() to get a definite IDigest");
        }

        public virtual int GetDigestSize()
        {
            throw new InvalidOperationException("Use Fork() to get a definite IDigest");
        }

        public virtual void Update(byte input)
        {
            if (mBuf != null)
            {
                mBuf.WriteByte(input);
                return;
            }

            foreach (IDigest hash in mHashes.Values) hash.Update(input);
        }

        public virtual void BlockUpdate(byte[] input, int inOff, int len)
        {
            if (mBuf != null)
            {
                mBuf.Write(input, inOff, len);
                return;
            }

            foreach (IDigest hash in mHashes.Values) hash.BlockUpdate(input, inOff, len);
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            throw new InvalidOperationException("Use Fork() to get a definite IDigest");
        }

        public virtual void Reset()
        {
            if (mBuf != null)
            {
                mBuf.SetLength(0);
                return;
            }

            foreach (IDigest hash in mHashes.Values) hash.Reset();
        }

        protected virtual void CheckStopBuffering()
        {
            if (mBuf != null && mHashes.Count <= BUFFERING_HASH_LIMIT)
            {
                foreach (IDigest hash in mHashes.Values) mBuf.UpdateDigest(hash);

                mBuf = null;
            }
        }

        protected virtual void CheckTrackingHash(byte hashAlgorithm)
        {
            if (!mHashes.Contains(hashAlgorithm))
            {
                var hash = TlsUtilities.CreateHash(hashAlgorithm);
                mHashes[hashAlgorithm] = hash;
            }
        }
    }
}

#endif