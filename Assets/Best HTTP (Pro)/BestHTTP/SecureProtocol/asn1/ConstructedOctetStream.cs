#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.IO;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
    internal class ConstructedOctetStream
        : BaseInputStream
    {
        private readonly Asn1StreamParser _parser;
        private Stream _currentStream;

        private bool _first = true;

        internal ConstructedOctetStream(
            Asn1StreamParser parser)
        {
            _parser = parser;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_currentStream == null)
            {
                if (!_first)
                    return 0;

                var s = (Asn1OctetStringParser)_parser.ReadObject();

                if (s == null)
                    return 0;

                _first = false;
                _currentStream = s.GetOctetStream();
            }

            var totalRead = 0;

            for (;;)
            {
                var numRead = _currentStream.Read(buffer, offset + totalRead, count - totalRead);

                if (numRead > 0)
                {
                    totalRead += numRead;

                    if (totalRead == count)
                        return totalRead;
                }
                else
                {
                    var aos = (Asn1OctetStringParser)_parser.ReadObject();

                    if (aos == null)
                    {
                        _currentStream = null;
                        return totalRead;
                    }

                    _currentStream = aos.GetOctetStream();
                }
            }
        }

        public override int ReadByte()
        {
            if (_currentStream == null)
            {
                if (!_first)
                    return 0;

                var s = (Asn1OctetStringParser)_parser.ReadObject();

                if (s == null)
                    return 0;

                _first = false;
                _currentStream = s.GetOctetStream();
            }

            for (;;)
            {
                var b = _currentStream.ReadByte();

                if (b >= 0) return b;

                var aos = (Asn1OctetStringParser)_parser.ReadObject();

                if (aos == null)
                {
                    _currentStream = null;
                    return -1;
                }

                _currentStream = aos.GetOctetStream();
            }
        }
    }
}

#endif