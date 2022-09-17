#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)

#if NETFX_CORE
    using FileStream = BestHTTP.PlatformSupport.IO.FileStream;
    using Directory = BestHTTP.PlatformSupport.IO.Directory;
    using File = BestHTTP.PlatformSupport.IO.File;

    using BestHTTP.PlatformSupport.IO;
#else
using System;
using System.Collections.Generic;
using BestHTTP.Extensions;
using FileStream = System.IO.FileStream;
using System.IO;
#endif

namespace BestHTTP.Caching
{
    /// <summary>
    ///     Holds all metadata that need for efficient caching, so we don't need to touch the disk to load headers.
    /// </summary>
    public class HTTPCacheFileInfo : IComparable<HTTPCacheFileInfo>
    {
        #region IComparable<HTTPCacheFileInfo>

        public int CompareTo(HTTPCacheFileInfo other)
        {
            return LastAccess.CompareTo(other.LastAccess);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The uri that this HTTPCacheFileInfo belongs to.
        /// </summary>
        internal Uri Uri { get; set; }

        /// <summary>
        ///     The last access time to this cache entity. The date is in UTC.
        /// </summary>
        internal DateTime LastAccess { get; set; }

        /// <summary>
        ///     The length of the cache entity's body.
        /// </summary>
        public int BodyLength { get; set; }

        /// <summary>
        ///     ETag of the entity.
        /// </summary>
        private string ETag { get; set; }

        /// <summary>
        ///     LastModified date of the entity.
        /// </summary>
        private string LastModified { get; set; }

        /// <summary>
        ///     When the cache will expire.
        /// </summary>
        private DateTime Expires { get; set; }

        /// <summary>
        ///     The age that came with the response
        /// </summary>
        private long Age { get; set; }

        /// <summary>
        ///     Maximum how long the entry should served from the cache without revalidation.
        /// </summary>
        private long MaxAge { get; set; }

        /// <summary>
        ///     The Date that came with the response.
        /// </summary>
        private DateTime Date { get; set; }

        /// <summary>
        ///     Indicates whether the entity must be revalidated with the server or can be serverd directly from the cache without
        ///     touching the server.
        /// </summary>
        private bool MustRevalidate { get; set; }

        /// <summary>
        ///     The date and time when the HTTPResponse received.
        /// </summary>
        private DateTime Received { get; set; }

        /// <summary>
        ///     Cached path.
        /// </summary>
        private string ConstructedPath { get; set; }

        /// <summary>
        ///     This is the index of the entity. Filenames are generated from this value.
        /// </summary>
        internal ulong MappedNameIDX { get; set; }

        #endregion

        #region Constructors

        internal HTTPCacheFileInfo(Uri uri)
            : this(uri, DateTime.UtcNow, -1)
        {
        }

        internal HTTPCacheFileInfo(Uri uri, DateTime lastAcces, int bodyLength)
        {
            Uri = uri;
            LastAccess = lastAcces;
            BodyLength = bodyLength;
            MaxAge = -1;

            MappedNameIDX = HTTPCacheService.GetNameIdx();
        }

        internal HTTPCacheFileInfo(Uri uri, BinaryReader reader, int version)
        {
            Uri = uri;
            LastAccess = DateTime.FromBinary(reader.ReadInt64());
            BodyLength = reader.ReadInt32();

            switch (version)
            {
                case 2:
                    MappedNameIDX = reader.ReadUInt64();
                    goto case 1;

                case 1:
                {
                    ETag = reader.ReadString();
                    LastModified = reader.ReadString();
                    Expires = DateTime.FromBinary(reader.ReadInt64());
                    Age = reader.ReadInt64();
                    MaxAge = reader.ReadInt64();
                    Date = DateTime.FromBinary(reader.ReadInt64());
                    MustRevalidate = reader.ReadBoolean();
                    Received = DateTime.FromBinary(reader.ReadInt64());
                    break;
                }
            }
        }

        #endregion

        #region Helper Functions

        internal void SaveTo(BinaryWriter writer)
        {
            writer.Write(LastAccess.ToBinary());
            writer.Write(BodyLength);
            writer.Write(MappedNameIDX);
            writer.Write(ETag);
            writer.Write(LastModified);
            writer.Write(Expires.ToBinary());
            writer.Write(Age);
            writer.Write(MaxAge);
            writer.Write(Date.ToBinary());
            writer.Write(MustRevalidate);
            writer.Write(Received.ToBinary());
        }

        public string GetPath()
        {
            if (ConstructedPath != null)
                return ConstructedPath;

            return ConstructedPath = Path.Combine(HTTPCacheService.CacheFolder, MappedNameIDX.ToString("X"));
        }

        public bool IsExists()
        {
            if (!HTTPCacheService.IsSupported)
                return false;

            return File.Exists(GetPath());
        }

        internal void Delete()
        {
            if (!HTTPCacheService.IsSupported)
                return;

            var path = GetPath();
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
            finally
            {
                Reset();
            }
        }

        private void Reset()
        {
            // MappedNameIDX will remain the same. When we re-save an entity, it will not reset the MappedNameIDX.
            BodyLength = -1;
            ETag = string.Empty;
            Expires = DateTime.FromBinary(0);
            LastModified = string.Empty;
            Age = 0;
            MaxAge = -1;
            Date = DateTime.FromBinary(0);
            MustRevalidate = false;
            Received = DateTime.FromBinary(0);
        }

        #endregion

        #region Caching

        private void SetUpCachingValues(HTTPResponse response)
        {
            response.CacheFileInfo = this;

            ETag = response.GetFirstHeaderValue("ETag").ToStrOrEmpty();
            Expires = response.GetFirstHeaderValue("Expires").ToDateTime(DateTime.FromBinary(0));
            LastModified = response.GetFirstHeaderValue("Last-Modified").ToStrOrEmpty();

            Age = response.GetFirstHeaderValue("Age").ToInt64();

            Date = response.GetFirstHeaderValue("Date").ToDateTime(DateTime.FromBinary(0));

            var cacheControl = response.GetFirstHeaderValue("cache-control");
            if (!string.IsNullOrEmpty(cacheControl))
            {
                var kvp = cacheControl.FindOption("max-age");
                if (kvp != null)
                {
                    // Some cache proxies will return float values
                    double maxAge;
                    if (double.TryParse(kvp[1], out maxAge))
                        MaxAge = (int)maxAge;
                }

                MustRevalidate = cacheControl.ToLower().Contains("must-revalidate");
            }

            Received = DateTime.UtcNow;
        }

        internal bool WillExpireInTheFuture()
        {
            if (!IsExists())
                return false;

            if (MustRevalidate)
                return false;

            // http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.2.4 :
            //  The max-age directive takes priority over Expires
            if (MaxAge != -1)
            {
                // Age calculation:
                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.2.3

                var apparent_age = Math.Max(0, (long)(Received - Date).TotalSeconds);
                var corrected_received_age = Math.Max(apparent_age, Age);
                var resident_time = (long)(DateTime.UtcNow - Date).TotalSeconds;
                var current_age = corrected_received_age + resident_time;

                return current_age < MaxAge;
            }

            return Expires > DateTime.UtcNow;
        }

        internal void SetUpRevalidationHeaders(HTTPRequest request)
        {
            if (!IsExists())
                return;

            // -If an entity tag has been provided by the origin server, MUST use that entity tag in any cache-conditional request (using If-Match or If-None-Match).
            // -If only a Last-Modified value has been provided by the origin server, SHOULD use that value in non-subrange cache-conditional requests (using If-Modified-Since).
            // -If both an entity tag and a Last-Modified value have been provided by the origin server, SHOULD use both validators in cache-conditional requests. This allows both HTTP/1.0 and HTTP/1.1 caches to respond appropriately.

            if (!string.IsNullOrEmpty(ETag))
                request.AddHeader("If-None-Match", ETag);

            if (!string.IsNullOrEmpty(LastModified))
                request.AddHeader("If-Modified-Since", LastModified);
        }

        public Stream GetBodyStream(out int length)
        {
            if (!IsExists())
            {
                length = 0;
                return null;
            }

            length = BodyLength;

            LastAccess = DateTime.UtcNow;

            var stream = new FileStream(GetPath(), FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(-length, SeekOrigin.End);

            return stream;
        }

        internal HTTPResponse ReadResponseTo(HTTPRequest request)
        {
            if (!IsExists())
                return null;

            LastAccess = DateTime.UtcNow;

            using (var stream = new FileStream(GetPath(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var response = new HTTPResponse(request, stream, request.UseStreaming, true);
                response.CacheFileInfo = this;
                response.Receive(BodyLength);
                return response;
            }
        }

        internal void Store(HTTPResponse response)
        {
            if (!HTTPCacheService.IsSupported)
                return;

            var path = GetPath();

            // Path name too long, we don't want to get exceptions
            if (path.Length > HTTPManager.MaxPathLength)
                return;

            if (File.Exists(path))
                Delete();

            using (var writer = new FileStream(path, FileMode.Create))
            {
                writer.WriteLine("HTTP/1.1 {0} {1}", response.StatusCode, response.Message);
                foreach (var kvp in response.Headers)
                    for (var i = 0; i < kvp.Value.Count; ++i)
                        writer.WriteLine("{0}: {1}", kvp.Key, kvp.Value[i]);

                writer.WriteLine();

                writer.Write(response.Data, 0, response.Data.Length);
            }

            BodyLength = response.Data.Length;
            LastAccess = DateTime.UtcNow;

            SetUpCachingValues(response);
        }

        internal Stream GetSaveStream(HTTPResponse response)
        {
            if (!HTTPCacheService.IsSupported)
                return null;

            LastAccess = DateTime.UtcNow;

            var path = GetPath();

            if (File.Exists(path))
                Delete();

            // Path name too long, we don't want to get exceptions
            if (path.Length > HTTPManager.MaxPathLength)
                return null;

            // First write out the headers
            using (var writer = new FileStream(path, FileMode.Create))
            {
                writer.WriteLine("HTTP/1.1 {0} {1}", response.StatusCode, response.Message);
                foreach (var kvp in response.Headers)
                    for (var i = 0; i < kvp.Value.Count; ++i)
                        writer.WriteLine("{0}: {1}", kvp.Key, kvp.Value[i]);

                writer.WriteLine();
            }

            // If caching is enabled and the response is from cache, and no content-length header set, then we set one to the response.
            if (response.IsFromCache && !response.Headers.ContainsKey("content-length"))
                response.Headers.Add("content-length", new List<string> { BodyLength.ToString() });

            SetUpCachingValues(response);

            // then create the stream with Append FileMode
            return new FileStream(GetPath(), FileMode.Append);
        }

        #endregion
    }
}

#endif