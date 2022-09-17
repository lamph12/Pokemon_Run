#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.Text;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * class for breaking up an X500 Name into it's component tokens, ala
     * java.util.StringTokenizer. We need this class as some of the
     * lightweight Java environment don't support classes like
     * StringTokenizer.
     */
    public class X509NameTokenizer
    {
        private readonly StringBuilder buffer = new StringBuilder();
        private int index;
        private readonly char separator;
        private readonly string value;

        public X509NameTokenizer(
            string oid)
            : this(oid, ',')
        {
        }

        public X509NameTokenizer(
            string oid,
            char separator)
        {
            value = oid;
            index = -1;
            this.separator = separator;
        }

        public bool HasMoreTokens()
        {
            return index != value.Length;
        }

        public string NextToken()
        {
            if (index == value.Length) return null;

            var end = index + 1;
            var quoted = false;
            var escaped = false;

            buffer.Remove(0, buffer.Length);

            while (end != value.Length)
            {
                var c = value[end];

                if (c == '"')
                {
                    if (!escaped)
                    {
                        quoted = !quoted;
                    }
                    else
                    {
                        buffer.Append(c);
                        escaped = false;
                    }
                }
                else
                {
                    if (escaped || quoted)
                    {
                        if (c == '#' && buffer[buffer.Length - 1] == '=')
                            buffer.Append('\\');
                        else if (c == '+' && separator != '+') buffer.Append('\\');
                        buffer.Append(c);
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == separator)
                    {
                        break;
                    }
                    else
                    {
                        buffer.Append(c);
                    }
                }

                end++;
            }

            index = end;

            return buffer.ToString().Trim();
        }
    }
}

#endif