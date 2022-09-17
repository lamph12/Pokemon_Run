#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections.Generic;

namespace BestHTTP.Caching
{
    internal sealed class HTTPCacheFileLock
    {
        private static readonly Dictionary<Uri, object> FileLocks = new Dictionary<Uri, object>();
        private static readonly object SyncRoot = new object();

        internal static object Acquire(Uri uri)
        {
            lock (SyncRoot)
            {
                object fileLock;
                if (!FileLocks.TryGetValue(uri, out fileLock))
                    FileLocks.Add(uri, fileLock = new object());

                return fileLock;
            }
        }

        internal static void Remove(Uri uri)
        {
            lock (SyncRoot)
            {
                if (FileLocks.ContainsKey(uri))
                    FileLocks.Remove(uri);
            }
        }

        internal static void Clear()
        {
            lock (SyncRoot)
            {
                FileLocks.Clear();
            }
        }
    }
}

#endif