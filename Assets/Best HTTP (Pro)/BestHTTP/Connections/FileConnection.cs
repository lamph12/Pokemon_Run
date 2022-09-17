#if NETFX_CORE
    using FileStream = BestHTTP.PlatformSupport.IO.FileStream;
    using FileMode = BestHTTP.PlatformSupport.IO.FileMode;
    using FileAccess = BestHTTP.PlatformSupport.IO.FileAccess;

    using Directory = BestHTTP.PlatformSupport.IO.Directory;
    using File = BestHTTP.PlatformSupport.IO.File;
#else
using FileStream = System.IO.FileStream;
using FileMode = System.IO.FileMode;
using FileAccess = System.IO.FileAccess;
#endif
using System;
using System.IO;
using BestHTTP.Extensions;

namespace BestHTTP
{
    public sealed class StreamList : Stream
    {
        private int CurrentIdx;
        private readonly Stream[] Streams;

        public StreamList(params Stream[] streams)
        {
            Streams = streams;
            CurrentIdx = 0;
        }

        public override bool CanRead
        {
            get
            {
                if (CurrentIdx >= Streams.Length)
                    return false;
                return Streams[CurrentIdx].CanRead;
            }
        }

        public override bool CanSeek => false;

        public override bool CanWrite
        {
            get
            {
                if (CurrentIdx >= Streams.Length)
                    return false;
                return Streams[CurrentIdx].CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (CurrentIdx >= Streams.Length)
                    return 0;

                long length = 0;
                for (var i = 0; i < Streams.Length; ++i)
                    length += Streams[i].Length;

                return length;
            }
        }

        public override long Position
        {
            get => throw new NotImplementedException("Position get");
            set => throw new NotImplementedException("Position set");
        }

        public override void Flush()
        {
            if (CurrentIdx >= Streams.Length)
                return;

            // We have to call the flush to all previous streams, as we may advanced the CurrentIdx
            for (var i = 0; i <= CurrentIdx; ++i)
                Streams[i].Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CurrentIdx >= Streams.Length)
                return -1;

            var readCount = Streams[CurrentIdx].Read(buffer, offset, count);

            while (readCount < count && CurrentIdx++ < Streams.Length)
                readCount += Streams[CurrentIdx].Read(buffer, offset + readCount, count - readCount);

            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CurrentIdx >= Streams.Length)
                return;

            Streams[CurrentIdx].Write(buffer, offset, count);
        }

        public void Write(string str)
        {
            var bytes = str.GetASCIIBytes();

            Write(bytes, 0, bytes.Length);
        }

        protected override void Dispose(bool disposing)
        {
            for (var i = 0; i < Streams.Length; ++i)
                try
                {
                    Streams[i].Dispose();
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("StreamList", "Dispose", ex);
                }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (CurrentIdx >= Streams.Length)
                return 0;

            return Streams[CurrentIdx].Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("SetLength");
        }
    }

    /*public static class AndroidFileHelper
    {
        // AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        // AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

        public static Stream GetAPKFileStream(string path)
        {
            UnityEngine.AndroidJavaClass up = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
            UnityEngine.AndroidJavaObject cActivity = up.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");

            UnityEngine.AndroidJavaObject assetManager = cActivity.GetStatic<UnityEngine.AndroidJavaObject>("getAssets");

            return new AndroidInputStream(assetManager.Call<UnityEngine.AndroidJavaObject>("open", path));
        }
    }

    public sealed class AndroidInputStream : Stream
    {
        private UnityEngine.AndroidJavaObject baseStream;

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public AndroidInputStream(UnityEngine.AndroidJavaObject inputStream)
        {
            this.baseStream = inputStream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.baseStream.Call<int>("read", buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }*/

    internal sealed class FileConnection : ConnectionBase
    {
        public FileConnection(string serverAddress)
            : base(serverAddress)
        {
        }

        internal override void Abort(HTTPConnectionStates newState)
        {
            State = newState;

            switch (State)
            {
                case HTTPConnectionStates.TimedOut:
                    TimedOutStart = DateTime.UtcNow;
                    break;
            }

            throw new NotImplementedException();
        }

        protected override void ThreadFunc(object param)
        {
            try
            {
                // Step 1 : create a stream with header information
                // Step 2 : create a stream from the file
                // Step 3 : create a StreamList
                // Step 4 : create a HTTPResponse object
                // Step 5 : call the Receive function of the response object

                using (var fs = new FileStream(CurrentRequest.CurrentUri.LocalPath, FileMode.Open, FileAccess.Read))
                    //using (Stream fs = AndroidFileHelper.GetAPKFileStream(this.CurrentRequest.CurrentUri.LocalPath))
                using (var stream = new StreamList(new MemoryStream(), fs))
                {
                    // This will write to the MemoryStream
                    stream.Write("HTTP/1.1 200 Ok\r\n");
                    stream.Write("Content-Type: application/octet-stream\r\n");
                    stream.Write("Content-Length: " + fs.Length + "\r\n");
                    stream.Write("\r\n");

                    stream.Seek(0, SeekOrigin.Begin);

                    CurrentRequest.Response =
                        new HTTPResponse(CurrentRequest, stream, CurrentRequest.UseStreaming, false);

                    if (!CurrentRequest.Response.Receive())
                        CurrentRequest.Response = null;
                }
            }
            catch (Exception ex)
            {
                if (CurrentRequest != null)
                {
                    // Something gone bad, Response must be null!
                    CurrentRequest.Response = null;

                    switch (State)
                    {
                        case HTTPConnectionStates.AbortRequested:
                            CurrentRequest.State = HTTPRequestStates.Aborted;
                            break;
                        case HTTPConnectionStates.TimedOut:
                            CurrentRequest.State = HTTPRequestStates.TimedOut;
                            break;
                        default:
                            CurrentRequest.Exception = ex;
                            CurrentRequest.State = HTTPRequestStates.Error;
                            break;
                    }
                }
            }
            finally
            {
                State = HTTPConnectionStates.Closed;
                if (CurrentRequest.State == HTTPRequestStates.Processing)
                {
                    if (CurrentRequest.Response != null)
                        CurrentRequest.State = HTTPRequestStates.Finished;
                    else
                        CurrentRequest.State = HTTPRequestStates.Error;
                }
            }
        }
    }
}