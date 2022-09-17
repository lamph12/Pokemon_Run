#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    /// <summary>An implementation of the TLS 1.0/1.1/1.2 record layer, allowing downgrade to SSLv3.</summary>
    internal class RecordStream
    {
        private const int DEFAULT_PLAINTEXT_LIMIT = 1 << 14;

        internal const int TLS_HEADER_SIZE = 5;
        internal const int TLS_HEADER_TYPE_OFFSET = 0;
        internal const int TLS_HEADER_VERSION_OFFSET = 1;
        internal const int TLS_HEADER_LENGTH_OFFSET = 3;
        private readonly MemoryStream mBuffer = new MemoryStream();

        private readonly TlsProtocol mHandler;

        private TlsHandshakeHash mHandshakeHash;
        private readonly Stream mInput;
        private readonly Stream mOutput;
        private TlsCipher mPendingCipher, mReadCipher, mWriteCipher;
        private TlsCompression mPendingCompression, mReadCompression, mWriteCompression;

        private int mPlaintextLimit, mCompressedLimit, mCiphertextLimit;
        private long mReadSeqNo, mWriteSeqNo;

        private ProtocolVersion mReadVersion, mWriteVersion;
        private bool mRestrictReadVersion = true;

        internal RecordStream(TlsProtocol handler, Stream input, Stream output)
        {
            mHandler = handler;
            mInput = input;
            mOutput = output;
            mReadCompression = new TlsNullCompression();
            mWriteCompression = mReadCompression;
        }

        internal virtual ProtocolVersion ReadVersion
        {
            get => mReadVersion;
            set => mReadVersion = value;
        }

        internal virtual TlsHandshakeHash HandshakeHash => mHandshakeHash;

        internal virtual void Init(TlsContext context)
        {
            mReadCipher = new TlsNullCipher(context);
            mWriteCipher = mReadCipher;
            mHandshakeHash = new DeferredHash();
            mHandshakeHash.Init(context);

            SetPlaintextLimit(DEFAULT_PLAINTEXT_LIMIT);
        }

        internal virtual int GetPlaintextLimit()
        {
            return mPlaintextLimit;
        }

        internal virtual void SetPlaintextLimit(int plaintextLimit)
        {
            mPlaintextLimit = plaintextLimit;
            mCompressedLimit = mPlaintextLimit + 1024;
            mCiphertextLimit = mCompressedLimit + 1024;
        }

        internal virtual void SetWriteVersion(ProtocolVersion writeVersion)
        {
            mWriteVersion = writeVersion;
        }

        /**
         * RFC 5246 E.1. "Earlier versions of the TLS specification were not fully clear on what the
         * record layer version number (TLSPlaintext.version) should contain when sending ClientHello
         * (i.e., before it is known which version of the protocol will be employed). Thus, TLS servers
         * compliant with this specification MUST accept any value {03,XX} as the record layer version
         * number for ClientHello."
         */
        internal virtual void SetRestrictReadVersion(bool enabled)
        {
            mRestrictReadVersion = enabled;
        }

        internal virtual void SetPendingConnectionState(TlsCompression tlsCompression, TlsCipher tlsCipher)
        {
            mPendingCompression = tlsCompression;
            mPendingCipher = tlsCipher;
        }

        internal virtual void SentWriteCipherSpec()
        {
            if (mPendingCompression == null || mPendingCipher == null)
                throw new TlsFatalAlert(AlertDescription.handshake_failure);

            mWriteCompression = mPendingCompression;
            mWriteCipher = mPendingCipher;
            mWriteSeqNo = 0;
        }

        internal virtual void ReceivedReadCipherSpec()
        {
            if (mPendingCompression == null || mPendingCipher == null)
                throw new TlsFatalAlert(AlertDescription.handshake_failure);

            mReadCompression = mPendingCompression;
            mReadCipher = mPendingCipher;
            mReadSeqNo = 0;
        }

        internal virtual void FinaliseHandshake()
        {
            if (mReadCompression != mPendingCompression || mWriteCompression != mPendingCompression
                                                        || mReadCipher != mPendingCipher ||
                                                        mWriteCipher != mPendingCipher)
                throw new TlsFatalAlert(AlertDescription.handshake_failure);
            mPendingCompression = null;
            mPendingCipher = null;
        }

        internal virtual bool ReadRecord()
        {
            var recordHeader = TlsUtilities.ReadAllOrNothing(TLS_HEADER_SIZE, mInput);
            if (recordHeader == null)
                return false;

            var type = TlsUtilities.ReadUint8(recordHeader, TLS_HEADER_TYPE_OFFSET);

            /*
             * RFC 5246 6. If a TLS implementation receives an unexpected record type, it MUST send an
             * unexpected_message alert.
             */
            CheckType(type, AlertDescription.unexpected_message);

            if (!mRestrictReadVersion)
            {
                var version = TlsUtilities.ReadVersionRaw(recordHeader, TLS_HEADER_VERSION_OFFSET);
                if ((version & 0xffffff00) != 0x0300)
                    throw new TlsFatalAlert(AlertDescription.illegal_parameter);
            }
            else
            {
                var version = TlsUtilities.ReadVersion(recordHeader, TLS_HEADER_VERSION_OFFSET);
                if (mReadVersion == null)
                    mReadVersion = version;
                else if (!version.Equals(mReadVersion)) throw new TlsFatalAlert(AlertDescription.illegal_parameter);
            }

            var length = TlsUtilities.ReadUint16(recordHeader, TLS_HEADER_LENGTH_OFFSET);
            var plaintext = DecodeAndVerify(type, mInput, length);
            mHandler.ProcessRecord(type, plaintext, 0, plaintext.Length);
            return true;
        }

        internal virtual byte[] DecodeAndVerify(byte type, Stream input, int len)
        {
            CheckLength(len, mCiphertextLimit, AlertDescription.record_overflow);

            var buf = TlsUtilities.ReadFully(len, input);
            var decoded = mReadCipher.DecodeCiphertext(mReadSeqNo++, type, buf, 0, buf.Length);

            CheckLength(decoded.Length, mCompressedLimit, AlertDescription.record_overflow);

            /*
             * TODO RFC5264 6.2.2. Implementation note: Decompression functions are responsible for
             * ensuring that messages cannot cause internal buffer overflows.
             */
            var cOut = mReadCompression.Decompress(mBuffer);
            if (cOut != mBuffer)
            {
                cOut.Write(decoded, 0, decoded.Length);
                cOut.Flush();
                decoded = GetBufferContents();
            }

            /*
             * RFC 5264 6.2.2. If the decompression function encounters a TLSCompressed.fragment that
             * would decompress to a length in excess of 2^14 bytes, it should report a fatal
             * decompression failure error.
             */
            CheckLength(decoded.Length, mPlaintextLimit, AlertDescription.decompression_failure);

            /*
             * RFC 5264 6.2.1 Implementations MUST NOT send zero-length fragments of Handshake, Alert,
             * or ChangeCipherSpec content types.
             */
            if (decoded.Length < 1 && type != ContentType.application_data)
                throw new TlsFatalAlert(AlertDescription.illegal_parameter);

            return decoded;
        }

        internal virtual void WriteRecord(byte type, byte[] plaintext, int plaintextOffset, int plaintextLength)
        {
            // Never send anything until a valid ClientHello has been received
            if (mWriteVersion == null)
                return;

            /*
             * RFC 5264 6. Implementations MUST NOT send record types not defined in this document
             * unless negotiated by some extension.
             */
            CheckType(type, AlertDescription.internal_error);

            /*
             * RFC 5264 6.2.1 The length should not exceed 2^14.
             */
            CheckLength(plaintextLength, mPlaintextLimit, AlertDescription.internal_error);

            /*
             * RFC 5264 6.2.1 Implementations MUST NOT send zero-length fragments of Handshake, Alert,
             * or ChangeCipherSpec content types.
             */
            if (plaintextLength < 1 && type != ContentType.application_data)
                throw new TlsFatalAlert(AlertDescription.internal_error);

            if (type == ContentType.handshake) UpdateHandshakeData(plaintext, plaintextOffset, plaintextLength);

            var cOut = mWriteCompression.Compress(mBuffer);

            byte[] ciphertext;
            if (cOut == mBuffer)
            {
                ciphertext =
                    mWriteCipher.EncodePlaintext(mWriteSeqNo++, type, plaintext, plaintextOffset, plaintextLength);
            }
            else
            {
                cOut.Write(plaintext, plaintextOffset, plaintextLength);
                cOut.Flush();
                var compressed = GetBufferContents();

                /*
                 * RFC5264 6.2.2. Compression must be lossless and may not increase the content length
                 * by more than 1024 bytes.
                 */
                CheckLength(compressed.Length, plaintextLength + 1024, AlertDescription.internal_error);

                ciphertext = mWriteCipher.EncodePlaintext(mWriteSeqNo++, type, compressed, 0, compressed.Length);
            }

            /*
             * RFC 5264 6.2.3. The length may not exceed 2^14 + 2048.
             */
            CheckLength(ciphertext.Length, mCiphertextLimit, AlertDescription.internal_error);

            var record = new byte[ciphertext.Length + TLS_HEADER_SIZE];
            TlsUtilities.WriteUint8(type, record, TLS_HEADER_TYPE_OFFSET);
            TlsUtilities.WriteVersion(mWriteVersion, record, TLS_HEADER_VERSION_OFFSET);
            TlsUtilities.WriteUint16(ciphertext.Length, record, TLS_HEADER_LENGTH_OFFSET);
            Array.Copy(ciphertext, 0, record, TLS_HEADER_SIZE, ciphertext.Length);
            mOutput.Write(record, 0, record.Length);
            mOutput.Flush();
        }

        internal virtual void NotifyHelloComplete()
        {
            mHandshakeHash = mHandshakeHash.NotifyPrfDetermined();
        }

        internal virtual TlsHandshakeHash PrepareToFinish()
        {
            var result = mHandshakeHash;
            mHandshakeHash = mHandshakeHash.StopTracking();
            return result;
        }

        internal virtual void UpdateHandshakeData(byte[] message, int offset, int len)
        {
            mHandshakeHash.BlockUpdate(message, offset, len);
        }

        internal virtual void SafeClose()
        {
            try
            {
                Platform.Dispose(mInput);
            }
            catch (IOException)
            {
            }

            try
            {
                Platform.Dispose(mOutput);
            }
            catch (IOException)
            {
            }
        }

        internal virtual void Flush()
        {
            mOutput.Flush();
        }

        private byte[] GetBufferContents()
        {
            var contents = mBuffer.ToArray();
            mBuffer.SetLength(0);
            return contents;
        }

        private static void CheckType(byte type, byte alertDescription)
        {
            switch (type)
            {
                case ContentType.application_data:
                case ContentType.alert:
                case ContentType.change_cipher_spec:
                case ContentType.handshake:
                case ContentType.heartbeat:
                    break;
                default:
                    throw new TlsFatalAlert(alertDescription);
            }
        }

        private static void CheckLength(int length, int limit, byte alertDescription)
        {
            if (length > limit)
                throw new TlsFatalAlert(alertDescription);
        }
    }
}

#endif