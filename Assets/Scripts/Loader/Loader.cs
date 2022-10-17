using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaceManager
{
    public static class Loader
    {
        public enum Scene
        { 
            Loader = 0,
            MenuScene = 1,
            RaceScene = 2,
        }

        private static AsyncOperation _loadingAsyncOperation;
        private static Action<MonoBehaviour> OnLoaderCallback;

        public static void Load(Scene scene)
        {
            OnLoaderCallback = (mb) =>
            {
                mb.StartCoroutine(LoadSceneAsync(scene));
            };

            SceneManager.LoadScene(Scene.Loader.ToString());
        }

        private static IEnumerator LoadSceneAsync(Scene scene)
        {
            yield return null;
            _loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());
            while (!_loadingAsyncOperation.isDone)
                yield return null;
        }

        public static float GetLoadingProgress()
        {
            if (_loadingAsyncOperation != null)
                return _loadingAsyncOperation.progress;
            else
                return 1f;
        }

        public static void InvokeLoadUsing(MonoBehaviour monoBehaviour)
        {
            OnLoaderCallback?.Invoke(monoBehaviour);
            OnLoaderCallback = null;
        }
    }
}
