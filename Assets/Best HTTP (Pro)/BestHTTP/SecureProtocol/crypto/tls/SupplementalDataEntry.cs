#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Org.BouncyCastle.Crypto.Tls
{
    public class SupplementalDataEntry
    {
        protected readonly byte[] mData;
        protected readonly int mDataType;

        public SupplementalDataEntry(int dataType, byte[] data)
        {
            mDataType = dataType;
            mData = data;
        }

        public virtual int DataType => mDataType;

        public virtual byte[] Data => mData;
    }
}

#endif