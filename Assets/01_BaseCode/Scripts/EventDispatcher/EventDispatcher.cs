﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventDispatcher
{
    public class EventDispatcher : MonoBehaviour
    {
        #region Fields

        /// Store all "listener"
        private readonly Dictionary<EventID, Action<object>> _listeners = new Dictionary<EventID, Action<object>>();

        #endregion

        #region Singleton

        private static EventDispatcher s_instance;

        public static EventDispatcher Instance
        {
            get
            {
                // instance not exist, then create new one
                if (s_instance == null)
                {
                    // create new Gameobject, and add EventDispatcher component
                    var singletonObject = new GameObject();
                    s_instance = singletonObject.AddComponent<EventDispatcher>();
                    singletonObject.name = "Singleton - EventDispatcher";
                }

                return s_instance;
            }
            private set { }
        }

        public static bool HasInstance()
        {
            return s_instance != null;
        }

        private void Awake()
        {
            // check if there's another instance already exist in scene
            if (s_instance != null && s_instance.GetInstanceID() != GetInstanceID())
            {
                // Destroy this instances because already exist the singleton of EventsDispatcer
                Debug.Log("An instance of EventDispatcher already exist : " + s_instance.name +
                          " So destroy this instance : " + name);
                Destroy(gameObject);
            }
            else
            {
                // set instance
                s_instance = this;
            }
        }


        private void OnDestroy()
        {
            // reset this static var to null if it's the singleton instance
            if (s_instance == this)
            {
                ClearAllListener();
                s_instance = null;
            }
        }

        #endregion


        #region Add Listeners, Post events, Remove listener

        /// <summary>
        ///     Register to listen for eventID
        /// </summary>
        /// <param name="eventID">EventID that object want to listen</param>
        /// <param name="callback">Callback will be invoked when this eventID be raised</param>
        public void RegisterListener(EventID eventID, Action<object> callback)
        {
            // checking params
            Debug.Assert(callback != null, "AddListener, event " + eventID + "callback = null !!");
            Debug.Assert(eventID != EventID.NONE, "RegisterListener, event = None !!");

            // check if listener exist in distionary
            if (_listeners.ContainsKey(eventID))
            {
                // add callback to our collection
                _listeners[eventID] += callback;
            }
            else
            {
                // add new key-value pair
                _listeners.Add(eventID, null);
                _listeners[eventID] += callback;
            }
        }

        /// <summary>
        ///     Posts the event. This will notify all listener that register for this event
        /// </summary>
        /// <param name="eventID">EventID.</param>
        /// <param name="sender">Sender, in some case, the Listener will need to know who send this message.</param>
        /// <param name="param">Parameter. Can be anything (struct, class ...), Listener will make a cast to get the data</param>
        public void PostEvent(EventID eventID, object param = null)
        {
            if (!_listeners.ContainsKey(eventID))
            {
                Debug.Log("No listeners for this event : " + eventID);
                return;
            }

            // posting event
            var callbacks = _listeners[eventID];
            // if there's no listener remain, then do nothing
            if (callbacks != null)
            {
                callbacks(param);
            }
            else
            {
                Debug.Log("PostEvent " + eventID + "but no listener remain, Remove this key");
                _listeners.Remove(eventID);
            }
        }

        /// <summary>
        ///     Removes the listener. Use to Unregister listener
        /// </summary>
        /// <param name="eventID">EventID.</param>
        /// <param name="callback">Callback.</param>
        public void RemoveListener(EventID eventID, Action<object> callback)
        {
            // checking params
            Debug.Assert(callback != null, "RemoveListener, event " + eventID + "callback = null !!");
            Debug.Assert(eventID != EventID.NONE, "AddListener, event = None !!");

            if (_listeners.ContainsKey(eventID))
                _listeners[eventID] -= callback;
            else
                Debug.Log("RemoveListener, not found key : " + eventID);
        }

        /// <summary>
        ///     Clears all the listener.
        /// </summary>
        public void ClearAllListener()
        {
            _listeners.Clear();
        }

        #endregion
    }


    #region Extension class

    /// <summary>
    ///     Delare some "shortcut" for using EventDispatcher easier
    /// </summary>
    public static class EventDispatcherExtension
    {
        /// Use for registering with EventsManager
        public static void RegisterListener(this MonoBehaviour listener, EventID eventID, Action<object> callback)
        {
            EventDispatcher.Instance.RegisterListener(eventID, callback);
        }

        /// Post event with param
        public static void PostEvent(this MonoBehaviour listener, EventID eventID, object param)
        {
            EventDispatcher.Instance.PostEvent(eventID, param);
        }

        /// Post event with no param (param = null)
        public static void PostEvent(this MonoBehaviour sender, EventID eventID)
        {
            EventDispatcher.Instance.PostEvent(eventID);
        }

        public static void RemoveListener(this MonoBehaviour listener, EventID eventID, Action<object> callback)
        {
            EventDispatcher.Instance.RemoveListener(eventID, callback);
        }
    }

    #endregion
}