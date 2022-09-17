using System;
using System.Collections.Generic;

namespace BestHTTP.Extensions
{
    /// <summary>
    ///     Used in string parsers. Its Value is optional.
    /// </summary>
    public sealed class HeaderValue
    {
        #region Private Helper Functions

        private void ParseImplementation(string headerStr, ref int pos, bool isOptionIsAnOption)
        {
            var key = headerStr.Read(ref pos, ch => ch != ';' && ch != '=' && ch != ',');
            Key = key;

            var skippedChar = headerStr.Peek(pos - 1);
            var isValue = skippedChar == '=';
            var isOption = isOptionIsAnOption && skippedChar == ';';

            while ((skippedChar != null && isValue) || isOption)
            {
                if (isValue)
                {
                    var value = headerStr.ReadPossibleQuotedText(ref pos);
                    Value = value;
                }
                else if (isOption)
                {
                    var option = new HeaderValue();
                    option.ParseImplementation(headerStr, ref pos, false);

                    if (Options == null)
                        Options = new List<HeaderValue>();

                    Options.Add(option);
                }

                if (!isOptionIsAnOption)
                    return;

                skippedChar = headerStr.Peek(pos - 1);
                isValue = skippedChar == '=';
                isOption = isOptionIsAnOption && skippedChar == ';';
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
                return string.Concat(Key, '=', Value);
            return Key;
        }

        #endregion

        #region Public Properties

        public string Key { get; set; }
        public string Value { get; set; }
        public List<HeaderValue> Options { get; set; }

        public bool HasValue => !string.IsNullOrEmpty(Value);

        #endregion

        #region Constructors

        public HeaderValue()
        {
        }

        public HeaderValue(string key)
        {
            Key = key;
        }

        #endregion

        #region Public Helper Functions

        public void Parse(string headerStr, ref int pos)
        {
            ParseImplementation(headerStr, ref pos, true);
        }

        public bool TryGetOption(string key, out HeaderValue option)
        {
            option = null;

            if (Options == null || Options.Count == 0)
                return false;

            for (var i = 0; i < Options.Count; ++i)
                if (string.Equals(Options[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    option = Options[i];
                    return true;
                }

            return false;
        }

        #endregion
    }
}