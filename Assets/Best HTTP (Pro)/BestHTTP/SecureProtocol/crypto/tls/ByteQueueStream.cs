#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class ByteQueueStream
        : Stream
    {
        private readonly ByteQueue buffer;

        public ByteQueueStream()
        {
            buffer = new ByteQueue();
        }

        public virtual int Available => buffer.Available;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public virtual int Peek(byte[] buf)
        {
            var bytesToRead = System.Math.Min(buffer.Available, buf.Length);
            buffer.Read(buf, 0, bytesToRead, 0);
            return bytesToRead;
        }

        public virtual int Read(byte[] buf)
        {
            return Read(buf, 0, buf.Length);
        }

        public override int Read(byte[] buf, int off, int len)
        {
            var bytesToRead = System.Math.Min(buffer.Available, len);
            buffer.RemoveData(buf, off, bytesToRead, 0);
            return bytesToRead;
        }

        public override int ReadByte()
        {
            if (buffer.Available == 0)
                return -1;

            return buffer.RemoveData(1, 0)[0] & 0xFF;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public virtual int Skip(int n)
        {
            var bytesToSkip = System.Math.Min(buffer.Available, n);
            buffer.RemoveData(bytesToSkip);
            return bytesToSkip;
        }

        public virtual void Write(byte[] buf)
        {
            buffer.AddData(buf, 0, buf.Length);
        }

        public override void Write(byte[] buf, int off, int len)
        {
            buffer.AddData(buf, off, len);
        }

        public override void WriteByte(byte b)
        {
            buffer.AddData(new[] { b }, 0, 1);
        }
    }
}

#endif