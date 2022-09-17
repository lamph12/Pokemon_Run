using System;
using System.IO;
using BestHTTP.ServerSentEvents;
using BestHTTP.WebSocket;

namespace BestHTTP
{
    public enum SupportedProtocols
    {
        Unknown,
        HTTP,

#if !BESTHTTP_DISABLE_WEBSOCKET
        WebSocket,
#endif

#if !BESTHTTP_DISABLE_SERVERSENT_EVENTS
        ServerSentEvents
#endif
    }

    internal static class HTTPProtocolFactory
    {
        public static HTTPResponse Get(SupportedProtocols protocol, HTTPRequest request, Stream stream, bool isStreamed,
            bool isFromCache)
        {
            switch (protocol)
            {
#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)
                case SupportedProtocols.WebSocket:
                    return new WebSocketResponse(request, stream, isStreamed, isFromCache);
#endif

#if !BESTHTTP_DISABLE_SERVERSENT_EVENTS && (!UNITY_WEBGL || UNITY_EDITOR)
                case SupportedProtocols.ServerSentEvents:
                    return new EventSourceResponse(request, stream, isStreamed, isFromCache);
#endif
                default: return new HTTPResponse(request, stream, isStreamed, isFromCache);
            }
        }

        public static SupportedProtocols GetProtocolFromUri(Uri uri)
        {
            if (uri == null || uri.Scheme == null)
                throw new Exception("Malformed URI in GetProtocolFromUri");

            var scheme = uri.Scheme.ToLowerInvariant();
            switch (scheme)
            {
#if !BESTHTTP_DISABLE_WEBSOCKET
                case "ws":
                case "wss":
                    return SupportedProtocols.WebSocket;
#endif

                default:
                    return SupportedProtocols.HTTP;
            }
        }

        public static bool IsSecureProtocol(Uri uri)
        {
            if (uri == null || uri.Scheme == null)
                throw new Exception("Malformed URI in IsSecureProtocol");

            var scheme = uri.Scheme.ToLowerInvariant();
            switch (scheme)
            {
                // http
                case "https":

#if !BESTHTTP_DISABLE_WEBSOCKET
                // WebSocket
                case "wss":
#endif
                    return true;
            }

            return false;
        }
    }
}