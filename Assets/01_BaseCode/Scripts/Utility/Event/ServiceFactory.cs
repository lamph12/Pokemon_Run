using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple, *single-threaded*, service locator appropriate for use with Unity.
/// </summary>

namespace Frictionless
{
    public class ServiceFactory
    {
        private readonly Dictionary<Type, object> singletonInstances = new Dictionary<Type, object>();

        private readonly Dictionary<Type, Type> singletons = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> transients = new Dictionary<Type, Type>();

        static ServiceFactory()
        {
            Instance = new ServiceFactory();
        }

        protected ServiceFactory()
        {
        }

        public static ServiceFactory Instance { get; }

        public bool IsEmpty => singletons.Count == 0 && transients.Count == 0;

        public void HandleNewSceneLoaded()
        {
            var multis = new List<IMultiSceneSingleton>();
            foreach (var pair in singletonInstances)
            {
                var multi = pair.Value as IMultiSceneSingleton;
                if (multi != null)
                    multis.Add(multi);
            }

            foreach (var multi in multis)
            {
                var behavior = multi as MonoBehaviour;
                if (behavior != null)
                    behavior.StartCoroutine(multi.HandleNewSceneLoaded());
            }
        }

        public void Reset()
        {
            var survivorRegisteredTypes = new List<Type>();
            var survivors = new List<object>();
            foreach (var pair in singletonInstances)
                if (pair.Value is IMultiSceneSingleton)
                {
                    survivors.Add(pair.Value);
                    survivorRegisteredTypes.Add(pair.Key);
                }

            singletons.Clear();
            transients.Clear();
            singletonInstances.Clear();

            for (var i = 0; i < survivors.Count; i++)
            {
                singletonInstances[survivorRegisteredTypes[i]] = survivors[i];
                singletons[survivorRegisteredTypes[i]] = survivors[i].GetType();
            }
        }

        public void RegisterSingleton<TConcrete>()
        {
            singletons[typeof(TConcrete)] = typeof(TConcrete);
        }

        public void RegisterSingleton<TAbstract, TConcrete>()
        {
            singletons[typeof(TAbstract)] = typeof(TConcrete);
        }

        public void RegisterSingleton<TConcrete>(TConcrete instance)
        {
            singletons[typeof(TConcrete)] = typeof(TConcrete);
            singletonInstances[typeof(TConcrete)] = instance;
        }

        public void RegisterTransient<TAbstract, TConcrete>()
        {
            transients[typeof(TAbstract)] = typeof(TConcrete);
        }

        public T Resolve<T>() where T : class
        {
            return Resolve<T>(false);
        }

        public T Resolve<T>(bool onlyExisting) where T : class
        {
            var result = default(T);
            Type concreteType = null;
            if (singletons.TryGetValue(typeof(T), out concreteType))
            {
                object r = null;
                if (!singletonInstances.TryGetValue(typeof(T), out r) && !onlyExisting)
                {
#if NETFX_CORE
					if (concreteType.GetTypeInfo().IsSubclassOf(typeof(MonoBehaviour)))
#else
                    if (concreteType.IsSubclassOf(typeof(MonoBehaviour)))
#endif
                    {
                        var singletonGameObject = new GameObject();
                        r = singletonGameObject.AddComponent(concreteType);
                        singletonGameObject.name = typeof(T) + " (singleton)";
                    }
                    else
                    {
                        r = Activator.CreateInstance(concreteType);
                    }

                    singletonInstances[typeof(T)] = r;

                    var multi = r as IMultiSceneSingleton;
                    if (multi != null)
                        multi.HandleNewSceneLoaded();
                }

                result = (T)r;
            }
            else if (transients.TryGetValue(typeof(T), out concreteType))
            {
                var r = Activator.CreateInstance(concreteType);
                result = (T)r;
            }

            return result;
        }
    }
}