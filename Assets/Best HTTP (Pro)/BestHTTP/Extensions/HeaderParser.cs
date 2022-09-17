using System;
using System.Collections.Generic;

namespace BestHTTP.Extensions
{
    /// <summary>
    ///     Will parse a comma-separeted header value
    /// </summary>
    public sealed class HeaderParser : KeyValuePairList
    {
        public HeaderParser(string headerStr)
        {
            Values = Parse(headerStr);
        }

        private List<HeaderValue> Parse(string headerStr)
        {
            var result = new List<HeaderValue>();

            var pos = 0;

            try
            {
                while (pos < headerStr.Length)
                {
                    var current = new HeaderValue();

                    current.Parse(headerStr, ref pos);

                    result.Add(current);
                }
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("HeaderParser - Parse", headerStr, ex);
            }

            return result;
        }
    }
}