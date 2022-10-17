using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RaceManager.Root
{
    public abstract class BaseController<T> : MonoBehaviour, IDisposable
    {
        private List<BaseController<T>> _baseControllers;
        private List<GameObject> _gameObjects;
        private bool _isDisposed;


        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            DisposeBaseControllers();
            DisposeGameObjects();

            OnDispose();
        }

        private void DisposeBaseControllers()
        {
            if (_baseControllers == null)
                return;

            foreach (BaseController<T> baseController in _baseControllers)
                baseController.Dispose();

            _baseControllers.Clear();
        }

        private void DisposeGameObjects()
        {
            if (_gameObjects == null)
                return;

            foreach (GameObject gameObject in _gameObjects)
                Object.Destroy(gameObject);

            _gameObjects.Clear();
        }

        public virtual void OnDispose() { }


        protected void AddController(BaseController<T> baseController)
        {
            _baseControllers ??= new List<BaseController<T>>();
            _baseControllers.Add(baseController);
        }

        protected void AddGameObject(GameObject gameObject)
        {
            _gameObjects ??= new List<GameObject>();
            _gameObjects.Add(gameObject);
        }
    }
}

