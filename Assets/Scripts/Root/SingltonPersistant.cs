using UnityEngine;

namespace RaceManager.Root
{
    public abstract class SingletonPersistant<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<T>();

                {
                    GameObject go = new GameObject(string.Concat("[", typeof(T), "]"));
                    _instance = go.AddComponent<T>();
                }

                if (_instance == null) Debug.LogError("Singleton of type : " + typeof(T).Name + " not found on scene");
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                AwakeSingleton();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void AwakeSingleton() { }
    }
}

