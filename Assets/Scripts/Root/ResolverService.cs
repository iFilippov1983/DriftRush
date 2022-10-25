using System.Linq;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace RaceManager.Root
{
    public class ResolverService : SerializedMonoBehaviour
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
    }
}

