using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frictionless
{
    public class MessageRouter : IMultiSceneSingleton
    {
        private readonly Dictionary<Type, List<MessageHandler>> handlers = new Dictionary<Type, List<MessageHandler>>();
        private bool isRaisingMessage;
        private readonly List<Delegate> pendingRemovals = new List<Delegate>();

        public IEnumerator HandleNewSceneLoaded()
        {
            yield return null;
        }

        public void AddHandler<T>(Action<T> handler)
        {
            List<MessageHandler> delegates = null;
            if (!handlers.TryGetValue(typeof(T), out delegates))
            {
                delegates = new List<MessageHandler>();
                handlers[typeof(T)] = delegates;
            }

            if (delegates.Find(x => x.Delegate == handler) == null)
                delegates.Add(new MessageHandler { Target = handler.Target, Delegate = handler });
        }

        public void RemoveHandler<T>(Action<T> handler)
        {
            List<MessageHandler> delegates = null;
            if (handlers.TryGetValue(typeof(T), out delegates))
            {
                var existingHandler = delegates.Find(x => x.Delegate == handler);
                if (existingHandler != null)
                {
                    if (isRaisingMessage)
                        pendingRemovals.Add(handler);
                    else
                        delegates.Remove(existingHandler);
                }
            }
        }

        public void Reset()
        {
            handlers.Clear();
        }

        public void RaiseMessage(object msg)
        {
            try
            {
                List<MessageHandler> delegates = null;
                if (handlers.TryGetValue(msg.GetType(), out delegates))
                {
                    isRaisingMessage = true;
                    try
                    {
                        foreach (var h in delegates)
                        {
#if NETFX_CORE
							h.Delegate.DynamicInvoke(msg);
#else
                            h.Delegate.Method.Invoke(h.Target, new[] { msg });
#endif
                        }
                    }
                    finally
                    {
                        isRaisingMessage = false;
                    }

                    foreach (var d in pendingRemovals)
                    {
                        var existingHandler = delegates.Find(x => x.Delegate == d);
                        if (existingHandler != null)
                            delegates.Remove(existingHandler);
                    }

                    pendingRemovals.Clear();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception while raising message " + msg + ": " + ex);
            }
        }

        public class MessageHandler
        {
            public object Target { get; set; }
            public Delegate Delegate { get; set; }
        }
    }
}