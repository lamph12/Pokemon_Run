using System;
using System.Collections.Generic;

namespace BestHTTP.Authentication
{
    /// <summary>
    ///     Stores and manages already received digest infos.
    /// </summary>
    public static class DigestStore
    {
        private static readonly Dictionary<string, Digest> Digests = new Dictionary<string, Digest>();

        private static readonly object Locker = new object();

        /// <summary>
        ///     Array of algorithms that the plugin supports. It's in the order of priority(first has the highest priority).
        /// </summary>
        private static readonly string[] SupportedAlgorithms = { "digest", "basic" };

        public static Digest Get(Uri uri)
        {
            lock (Locker)
            {
                Digest digest = null;
                if (Digests.TryGetValue(uri.Host, out digest))
                    if (!digest.IsUriProtected(uri))
                        return null;
                return digest;
            }
        }

        /// <summary>
        ///     It will retrive or create a new Digest for the given Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Digest GetOrCreate(Uri uri)
        {
            lock (Locker)
            {
                Digest digest = null;
                if (!Digests.TryGetValue(uri.Host, out digest))
                    Digests.Add(uri.Host, digest = new Digest(uri));
                return digest;
            }
        }

        public static void Remove(Uri uri)
        {
            lock (Locker)
            {
                Digests.Remove(uri.Host);
            }
        }

        public static string FindBest(List<string> authHeaders)
        {
            if (authHeaders == null || authHeaders.Count == 0)
                return string.Empty;

            var headers = new List<string>(authHeaders.Count);
            for (var i = 0; i < authHeaders.Count; ++i)
                headers.Add(authHeaders[i].ToLower());

            for (var i = 0; i < SupportedAlgorithms.Length; ++i)
            {
                var idx = headers.FindIndex(header => header.StartsWith(SupportedAlgorithms[i]));
                if (idx != -1)
                    return authHeaders[idx];
            }

            return string.Empty;
        }
    }
}