#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    /// <summary>
    ///     A generic TLS 1.0-1.2 / SSLv3 block cipher. This can be used for AES or 3DES for example.
    /// </summary>
    public class TlsBlockCipher
        : TlsCipher
    {
        protected readonly TlsContext context;
        protected readonly IBlockCipher decryptCipher;

        protected readonly IBlockCipher encryptCipher;
        protected readonly bool encryptThenMac;
        protected readonly TlsMac mReadMac;

        protected readonly TlsMac mWriteMac;
        protected readonly byte[] randomData;
        protected readonly bool useExplicitIV;

        /// <exception cref="IOException"></exception>
        public TlsBlockCipher(TlsContext context, IBlockCipher clientWriteCipher, IBlockCipher serverWriteCipher,
            IDigest clientWriteDigest, IDigest serverWriteDigest, int cipherKeySize)
        {
            this.context = context;

            randomData = new byte[256];
            context.NonceRandomGenerator.NextBytes(randomData);

            useExplicitIV = TlsUtilities.IsTlsV11(context);
            encryptThenMac = context.SecurityParameters.encryptThenMac;

            var key_block_size = 2 * cipherKeySize + clientWriteDigest.GetDigestSize()
                                                   + serverWriteDigest.GetDigestSize();

            // From TLS 1.1 onwards, block ciphers don't need client_write_IV
            if (!useExplicitIV) key_block_size += clientWriteCipher.GetBlockSize() + serverWriteCipher.GetBlockSize();

            var key_block = TlsUtilities.CalculateKeyBlock(context, key_block_size);

            var offset = 0;

            var clientWriteMac = new TlsMac(context, clientWriteDigest, key_block, offset,
                clientWriteDigest.GetDigestSize());
            offset += clientWriteDigest.GetDigestSize();
            var serverWriteMac = new TlsMac(context, serverWriteDigest, key_block, offset,
                serverWriteDigest.GetDigestSize());
            offset += serverWriteDigest.GetDigestSize();

            var client_write_key = new KeyParameter(key_block, offset, cipherKeySize);
            offset += cipherKeySize;
            var server_write_key = new KeyParameter(key_block, offset, cipherKeySize);
            offset += cipherKeySize;

            byte[] client_write_IV, server_write_IV;
            if (useExplicitIV)
            {
                client_write_IV = new byte[clientWriteCipher.GetBlockSize()];
                server_write_IV = new byte[serverWriteCipher.GetBlockSize()];
            }
            else
            {
                client_write_IV = Arrays.CopyOfRange(key_block, offset, offset + clientWriteCipher.GetBlockSize());
                offset += clientWriteCipher.GetBlockSize();
                server_write_IV = Arrays.CopyOfRange(key_block, offset, offset + serverWriteCipher.GetBlockSize());
                offset += serverWriteCipher.GetBlockSize();
            }

            if (offset != key_block_size) throw new TlsFatalAlert(AlertDescription.internal_error);

            ICipherParameters encryptParams, decryptParams;
            if (context.IsServer)
            {
                mWriteMac = serverWriteMac;
                mReadMac = clientWriteMac;
                encryptCipher = serverWriteCipher;
                decryptCipher = clientWriteCipher;
                encryptParams = new ParametersWithIV(server_write_key, server_write_IV);
                decryptParams = new ParametersWithIV(client_write_key, client_write_IV);
            }
            else
            {
                mWriteMac = clientWriteMac;
                mReadMac = serverWriteMac;
                encryptCipher = clientWriteCipher;
                decryptCipher = serverWriteCipher;
                encryptParams = new ParametersWithIV(client_write_key, client_write_IV);
                decryptParams = new ParametersWithIV(server_write_key, server_write_IV);
            }

            encryptCipher.Init(true, encryptParams);
            decryptCipher.Init(false, decryptParams);
        }

        public virtual TlsMac WriteMac => mWriteMac;

        public virtual TlsMac ReadMac => mReadMac;

        public virtual int GetPlaintextLimit(int ciphertextLimit)
        {
            var blockSize = encryptCipher.GetBlockSize();
            var macSize = mWriteMac.Size;

            var plaintextLimit = ciphertextLimit;

            // An explicit IV consumes 1 block
            if (useExplicitIV) plaintextLimit -= blockSize;

            // Leave room for the MAC, and require block-alignment
            if (encryptThenMac)
            {
                plaintextLimit -= macSize;
                plaintextLimit -= plaintextLimit % blockSize;
            }
            else
            {
                plaintextLimit -= plaintextLimit % blockSize;
                plaintextLimit -= macSize;
            }

            // Minimum 1 byte of padding
            --plaintextLimit;

            return plaintextLimit;
        }

        public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
        {
            var blockSize = encryptCipher.GetBlockSize();
            var macSize = mWriteMac.Size;

            var version = context.ServerVersion;

            var enc_input_length = len;
            if (!encryptThenMac) enc_input_length += macSize;

            var padding_length = blockSize - 1 - enc_input_length % blockSize;

            // TODO[DTLS] Consider supporting in DTLS (without exceeding send limit though)
            if (!version.IsDtls && !version.IsSsl)
            {
                // Add a random number of extra blocks worth of padding
                var maxExtraPadBlocks = (255 - padding_length) / blockSize;
                var actualExtraPadBlocks = ChooseExtraPadBlocks(context.SecureRandom, maxExtraPadBlocks);
                padding_length += actualExtraPadBlocks * blockSize;
            }

            var totalSize = len + macSize + padding_length + 1;
            if (useExplicitIV) totalSize += blockSize;

            var outBuf = new byte[totalSize];
            var outOff = 0;

            if (useExplicitIV)
            {
                var explicitIV = new byte[blockSize];
                context.NonceRandomGenerator.NextBytes(explicitIV);

                encryptCipher.Init(true, new ParametersWithIV(null, explicitIV));

                Array.Copy(explicitIV, 0, outBuf, outOff, blockSize);
                outOff += blockSize;
            }

            var blocks_start = outOff;

            Array.Copy(plaintext, offset, outBuf, outOff, len);
            outOff += len;

            if (!encryptThenMac)
            {
                var mac = mWriteMac.CalculateMac(seqNo, type, plaintext, offset, len);
                Array.Copy(mac, 0, outBuf, outOff, mac.Length);
                outOff += mac.Length;
            }

            for (var i = 0; i <= padding_length; i++) outBuf[outOff++] = (byte)padding_length;

            for (var i = blocks_start; i < outOff; i += blockSize) encryptCipher.ProcessBlock(outBuf, i, outBuf, i);

            if (encryptThenMac)
            {
                var mac = mWriteMac.CalculateMac(seqNo, type, outBuf, 0, outOff);
                Array.Copy(mac, 0, outBuf, outOff, mac.Length);
                outOff += mac.Length;
            }

            //        assert outBuf.length == outOff;

            return outBuf;
        }

        /// <exception cref="IOException"></exception>
        public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
        {
            var blockSize = decryptCipher.GetBlockSize();
            var macSize = mReadMac.Size;

            var minLen = blockSize;
            if (encryptThenMac)
                minLen += macSize;
            else
                minLen = System.Math.Max(minLen, macSize + 1);

            if (useExplicitIV) minLen += blockSize;

            if (len < minLen)
                throw new TlsFatalAlert(AlertDescription.decode_error);

            var blocks_length = len;
            if (encryptThenMac) blocks_length -= macSize;

            if (blocks_length % blockSize != 0)
                throw new TlsFatalAlert(AlertDescription.decryption_failed);

            if (encryptThenMac)
            {
                var end = offset + len;
                var receivedMac = Arrays.CopyOfRange(ciphertext, end - macSize, end);
                var calculatedMac = mReadMac.CalculateMac(seqNo, type, ciphertext, offset, len - macSize);

                var badMacEtm = !Arrays.ConstantTimeAreEqual(calculatedMac, receivedMac);
                if (badMacEtm)
                    /*
                         * RFC 7366 3. The MAC SHALL be evaluated before any further processing such as
                         * decryption is performed, and if the MAC verification fails, then processing SHALL
                         * terminate immediately. For TLS, a fatal bad_record_mac MUST be generated [2]. For
                         * DTLS, the record MUST be discarded, and a fatal bad_record_mac MAY be generated
                         * [4]. This immediate response to a bad MAC eliminates any timing channels that may
                         * be available through the use of manipulated packet data.
                         */
                    throw new TlsFatalAlert(AlertDescription.bad_record_mac);
            }

            if (useExplicitIV)
            {
                decryptCipher.Init(false, new ParametersWithIV(null, ciphertext, offset, blockSize));

                offset += blockSize;
                blocks_length -= blockSize;
            }

            for (var i = 0; i < blocks_length; i += blockSize)
                decryptCipher.ProcessBlock(ciphertext, offset + i, ciphertext, offset + i);

            // If there's anything wrong with the padding, this will return zero
            var totalPad = CheckPaddingConstantTime(ciphertext, offset, blocks_length, blockSize,
                encryptThenMac ? 0 : macSize);
            var badMac = totalPad == 0;

            var dec_output_length = blocks_length - totalPad;

            if (!encryptThenMac)
            {
                dec_output_length -= macSize;
                var macInputLen = dec_output_length;
                var macOff = offset + macInputLen;
                var receivedMac = Arrays.CopyOfRange(ciphertext, macOff, macOff + macSize);
                var calculatedMac = mReadMac.CalculateMacConstantTime(seqNo, type, ciphertext, offset,
                    macInputLen,
                    blocks_length - macSize, randomData);

                badMac |= !Arrays.ConstantTimeAreEqual(calculatedMac, receivedMac);
            }

            if (badMac)
                throw new TlsFatalAlert(AlertDescription.bad_record_mac);

            return Arrays.CopyOfRange(ciphertext, offset, offset + dec_output_length);
        }

        protected virtual int CheckPaddingConstantTime(byte[] buf, int off, int len, int blockSize,
            int macSize)
        {
            var end = off + len;
            var lastByte = buf[end - 1];
            var padlen = lastByte & 0xff;
            var totalPad = padlen + 1;

            var dummyIndex = 0;
            byte padDiff = 0;

            if ((TlsUtilities.IsSsl(context) && totalPad > blockSize) || macSize + totalPad > len)
            {
                totalPad = 0;
            }
            else
            {
                var padPos = end - totalPad;
                do
                {
                    padDiff |= (byte)(buf[padPos++] ^ lastByte);
                } while (padPos < end);

                dummyIndex = totalPad;

                if (padDiff != 0) totalPad = 0;
            }

            // Run some extra dummy checks so the number of checks is always constant
            {
                var dummyPad = randomData;
                while (dummyIndex < 256) padDiff |= (byte)(dummyPad[dummyIndex++] ^ lastByte);
                // Ensure the above loop is not eliminated
                dummyPad[0] ^= padDiff;
            }

            return totalPad;
        }

        protected virtual int ChooseExtraPadBlocks(SecureRandom r, int max)
        {
            // return r.NextInt(max + 1);

            var x = r.NextInt();
            var n = LowestBitSet(x);
            return System.Math.Min(n, max);
        }

        protected virtual int LowestBitSet(int x)
        {
            if (x == 0)
                return 32;

            var ux = (uint)x;
            var n = 0;
            while ((ux & 1U) == 0)
            {
                ++n;
                ux >>= 1;
            }

            return n;
        }
    }
}

#endif