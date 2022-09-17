using System;
using System.Collections.Generic;

namespace BestHTTP.Extensions
{
    public interface IHeartbeat
    {
        void OnHeartbeatUpdate(TimeSpan dif);
    }

    /// <summary>
    ///     A manager class that can handle subscribing and unsubscribeing in the same update.
    /// </summary>
    public sealed class HeartbeatManager
    {
        private readonly List<IHeartbeat> Heartbeats = new List<IHeartbeat>();
        private DateTime LastUpdate = DateTime.MinValue;
        private IHeartbeat[] UpdateArray;

        public void Subscribe(IHeartbeat heartbeat)
        {
            lock (Heartbeats)
            {
                if (!Heartbeats.Contains(heartbeat))
                    Heartbeats.Add(heartbeat);
            }
        }

        public void Unsubscribe(IHeartbeat heartbeat)
        {
            lock (Heartbeats)
            {
                Heartbeats.Remove(heartbeat);
            }
        }

        public void Update()
        {
            if (LastUpdate == DateTime.MinValue)
            {
                LastUpdate = DateTime.UtcNow;
            }
            else
            {
                var dif = DateTime.UtcNow - LastUpdate;
                LastUpdate = DateTime.UtcNow;

                var count = 0;

                lock (Heartbeats)
                {
                    if (UpdateArray == null || UpdateArray.Length < Heartbeats.Count)
                        Array.Resize(ref UpdateArray, Heartbeats.Count);

                    Heartbeats.CopyTo(0, UpdateArray, 0, Heartbeats.Count);

                    count = Heartbeats.Count;
                }

                for (var i = 0; i < count; ++i)
                    try
                    {
                        UpdateArray[i].OnHeartbeatUpdate(dif);
                    }
                    catch
                    {
                    }
            }
        }
    }
}