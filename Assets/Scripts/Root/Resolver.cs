using System.Linq;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace RaceManager.Root
{
    public class Resolver : SerializedMonoBehaviour
    {
        public Dictionary<Type, List<object>> map = new Dictionary<Type, List<object>>();

        public void Add<T>(T self) where T : class
        {
            var types = typeof(T).GetInterfaces();
            foreach (var type in types)
            {
                if (!map.TryGetValue(type, out var list))
                    map.Add(type, list = new List<object>());
                list.Add(self);
            }
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            map.TryGetValue(typeof(T), out var list);

            return list?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Returns first binded object with T type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class
        { 
            var targets = ResolveAll<IResolverServiceTarget<T>>();

            if (targets.Count() != 0)
            {
                var list = targets.ToList();
                var target = list.Find(t => t.Type == typeof(T));
                return target.Target;
            }
            else
                return null;
        }
    }
}

