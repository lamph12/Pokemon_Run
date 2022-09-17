using System;

namespace BestHTTP.Extensions
{
    public sealed class CircularBuffer<T>
    {
        private T[] buffer;
        private int endIdx;
        private int startIdx;

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }
        public int Count { get; private set; }

        public T this[int idx]
        {
            get
            {
                var realIdx = (startIdx + idx) % Capacity;

                return buffer[realIdx];
            }

            set
            {
                var realIdx = (startIdx + idx) % Capacity;

                buffer[realIdx] = value;
            }
        }

        public void Add(T element)
        {
            if (buffer == null)
                buffer = new T[Capacity];

            buffer[endIdx] = element;

            endIdx = (endIdx + 1) % Capacity;
            if (endIdx == startIdx)
                startIdx = (startIdx + 1) % Capacity;

            Count = Math.Min(Count + 1, Capacity);
        }

        public void Clear()
        {
            startIdx = endIdx = 0;
        }
    }
}