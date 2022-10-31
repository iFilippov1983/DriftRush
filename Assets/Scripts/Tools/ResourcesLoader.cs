using System;
using UnityEngine;
using Object = UnityEngine.Object;


namespace RaceManager.Tools
{
    internal static class ResourcesLoader
    {
        public static GameObject LoadPrefab(string path) =>
            Resources.Load<GameObject>(path);

        public static T LoadObject<T>(string path) where T : Object =>
            Resources.Load<T>(path);

        public static T LoadAndInstantiate<T>(string path, Transform parent = null, bool worldCoordinates = false) where T : MonoBehaviour
        {
            if (parent == null)
                parent = new GameObject($"[{typeof(T)}]").transform;

            var c = LoadPrefab(path).GetComponent<T>();
            return Object.Instantiate(c, parent, worldCoordinates);
        }
    }
}
