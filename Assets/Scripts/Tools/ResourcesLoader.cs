using System.Threading.Tasks;
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

        public static async Task<T> LoadAsync<T>(string path) where T : Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            while (!request.isDone)
                await Task.Yield();

            return request.asset as T;
        }

        public static T LoadAndInstantiate<T>(string path, Transform parent = null, bool worldCoordinates = false) where T : class
        {
            if (parent == null)
                parent = new GameObject($"[{typeof(T)}]").transform;

            var p = LoadPrefab(path).GetComponent<T>() as MonoBehaviour;
            var go = Object.Instantiate(p, parent, worldCoordinates);
            return go as T;
        }
    }
}
