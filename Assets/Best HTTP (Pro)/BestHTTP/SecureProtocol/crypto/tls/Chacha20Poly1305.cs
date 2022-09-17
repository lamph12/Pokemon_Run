#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    /**
     * draft-ietf-tls-chacha20-poly1305-04
     */
    public class Chacha20Poly1305
        : TlsCipher
    {
        private static readonly byte[] Zeroes = new byte[15];

        protected readonly TlsContext context;

        protected readonly ChaCha7539Engine encryptCipher, decryptCipher;
        protected readonly byte[] encryptIV, decryptIV;

        /// <exception cref="IOException"></exception>
        public Chacha20Poly1305(TlsContext context)
        {
            if (!TlsUtilities.IsTlsV12(context))
                throw new TlsFatalAlert(AlertDescription.internal_error);

            this.context = context;

            var cipherKeySize = 32;
            // TODO SecurityParameters.fixed_iv_length
            var fixed_iv_length = 12;
            // TODO SecurityParameters.record_iv_length = 0

            var key_block_size = 2 * cipherKeySize + 2 * fixed_iv_length;

            var key_block = TlsUtilities.CalculateKeyBlock(context, key_block_size);

            var offset = 0;

            var client_write_key = new KeyParameter(key_block, offset, cipherKeySize);
            offset += cipherKeySize;
            var server_write_key = new KeyParameter(key_block, offset, cipherKeySize);
            offset += cipherKeySize;
            var client_write_IV = Arrays.CopyOfRange(key_block, offset, offset + fixed_iv_length);
            offset += fixed_iv_length;
            var server_write_IV = Arrays.CopyOfRange(key_block, offset, offset + fixed_iv_length);
            offset += fixed_iv_length;

            if (offset != key_block_size)
                throw new TlsFatalAlert(AlertDescription.internal_error);

            encryptCipher = new ChaCha7539Engine();
            decryptCipher = new ChaCha7539Engine();

            KeyParameter encryptKey, decryptKey;
            if (context.IsServer)
            {
                encryptKey = server_write_key;
                decryptKey = client_write_key;
                encryptIV = server_write_IV;
                decryptIV = client_write_IV;
            }
            else
            {
                encryptKey = client_write_key;
                decryptKey = server_write_key;
                encryptIV = client_write_IV;
                decryptIV = server_write_IV;
            }

            encryptCipher.Init(true, new ParametersWithIV(encryptKey, encryptIV));
            decryptCipher.Init(false, new ParametersWithIV(decryptKey, decryptIV));
        }

        public virtual int GetPlaintextLimit(int ciphertextLimit)
        {
            return ciphertextLimit - 16;
        }

        /// <exception cref="IOException"></exception>
        public virtual byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len)
        {
            var macKey = InitRecord(encryptCipher, true, seqNo, encryptIV);

            var output = new byte[len + 16];
            encryptCipher.ProcessBytes(plaintext, offset, len, output, 0);

            var additionalData = GetAdditionalData(seqNo, type, len);
            var mac = CalculateRecordMac(macKey, additionalData, output, 0, len);
            Array.Copy(mac, 0, output, len, mac.Length);

            return output;
        }

        /// <exception cref="IOException"></exception>
        public virtual byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len)
        {
            if (GetPlaintextLimit(len) < 0)
                throw new TlsFatalAlert(AlertDescription.decode_error);

            var macKey = InitRecord(decryptCipher, false, seqNo, decryptIV);

            var plaintextLength = len - 16;

            var additionalData = GetAdditionalData(seqNo, type, plaintextLength);
            var calculatedMac = CalculateRecordMac(macKey, additionalData, ciphertext, offset, plaintextLength);
            var receivedMac = Arrays.CopyOfRange(ciphertext, offset + plaintextLength, offset + len);

            if (!Arrays.ConstantTimeAreEqual(calculatedMac, receivedMac))
                throw new TlsFatalAlert(AlertDescription.bad_record_mac);

            var output = new byte[plaintextLength];
            decryptCipher.ProcessBytes(ciphertext, offset, plaintextLength, output, 0);
            return output;
        }

        protected virtual KeyParameter InitRecord(IStreamCipher cipher, bool forEncryption, long seqNo, byte[] iv)
        {
            var nonce = CalculateNonce(seqNo, iv);
            cipher.Init(forEncryption, new ParametersWithIV(null, nonce));
            return GenerateRecordMacKey(cipher);
        }

        protected virtual byte[] CalculateNonce(long seqNo, byte[] iv)
        {
            var nonce = new byte[12];
            TlsUtilities.WriteUint64(seqNo, nonce, 4);

            for (var i = 0; i < 12; ++i) nonce[i] ^= iv[i];

            return nonce;
        }

        protected virtual KeyParameter GenerateRecordMacKey(IStreamCipher cipher)
        {
            var firstBlock = new byte[64];
            cipher.ProcessBytes(firstBlock, 0, firstBlock.Length, firstBlock, 0);

            var macKey = new KeyParameter(firstBlock, 0, 32);
            Arrays.Fill(firstBlock, 0);
            return macKey;
        }

        protected virtual byte[] CalculateRecordMac(KeyParameter macKey, byte[] additionalData, byte[] buf, int off,
            int len)
        {
            IMac mac = new Poly1305();
            mac.Init(macKey);

            UpdateRecordMacText(mac, additionalData, 0, additionalData.Length);
            UpdateRecordMacText(mac, buf, off, len);
            UpdateRecordMacLength(mac, additionalData.Length);
            UpdateRecordMacLength(mac, len);

            return MacUtilities.DoFinal(mac);
        }

        protected virtual void UpdateRecordMacLength(IMac mac, int len)
        {
            var longLen = Pack.UInt64_To_LE((ulong)len);
            mac.BlockUpdate(longLen, 0, longLen.Length);
        }

        protected virtual void UpdateRecordMacText(IMac mac, byte[] buf, int off, int len)
        {
            mac.BlockUpdate(buf, off, len);

            var partial = len % 16;
            if (partial != 0) mac.BlockUpdate(Zeroes, 0, 16 - partial);
        }

        /// <exception cref="IOException"></exception>
        protected virtual byte[] GetAdditionalData(long seqNo, byte type, int len)
        {
            /*
             * additional_data = seq_num + TLSCompressed.type + TLSCompressed.version +
             * TLSCompressed.length
             */
            var additional_data = new byte[13];
            TlsUtilities.WriteUint64(seqNo, additional_data, 0);
            TlsUtilities.WriteUint8(type, additional_data, 8);
            TlsUtilities.WriteVersion(context.ServerVersion, additional_data, 9);
            TlsUtilities.WriteUint16(len, additional_data, 11);

            return additional_data;
        }
    }
}

#endif