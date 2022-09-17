namespace BestHTTP
{
    /// <summary>
    /// </summary>
    public sealed class HTTPRange
    {
        internal HTTPRange()
        {
            ContentLength = -1;
            IsValid = false;
        }

        internal HTTPRange(int contentLength)
        {
            ContentLength = contentLength;
            IsValid = false;
        }

        internal HTTPRange(int firstBytePosition, int lastBytePosition, int contentLength)
        {
            FirstBytePos = firstBytePosition;
            LastBytePos = lastBytePosition;
            ContentLength = contentLength;

            // A byte-content-range-spec with a byte-range-resp-spec whose last-byte-pos value is less than its first-byte-pos value, or whose instance-length value is less than or equal to its last-byte-pos value, is invalid.
            IsValid = FirstBytePos <= LastBytePos && ContentLength > LastBytePos;
        }

        /// <summary>
        ///     The first byte's position that the server sent.
        /// </summary>
        public int FirstBytePos { get; }

        /// <summary>
        ///     The last byte's position that the server sent.
        /// </summary>
        public int LastBytePos { get; }

        /// <summary>
        ///     Indicates the total length of the full entity-body on the server, -1 if this length is unknown or difficult to
        ///     determine.
        /// </summary>
        public int ContentLength { get; }

        /// <summary>
        /// </summary>
        public bool IsValid { get; }

        public override string ToString()
        {
            return string.Format("{0}-{1}/{2} (valid: {3})", FirstBytePos, LastBytePos, ContentLength, IsValid);
        }
    }
}