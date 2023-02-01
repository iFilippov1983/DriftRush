using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace RaceManager.Root
{
    [Serializable]
    public class GameFlagsHandler : SerializedMonoBehaviour, ISaveable
    {
        [JsonProperty]
        [ShowInInspector, ReadOnly]
        private HashSet<GameFlagType> _flags = new HashSet<GameFlagType>();
        [JsonProperty]
        [ShowInInspector, ReadOnly]
        private Dictionary<GameFlagType, GameFlagsListener> _listeners = new Dictionary<GameFlagType, GameFlagsListener>();

        public bool HasFlag(GameFlagType key) => _flags.Contains(key);
        public bool IsLast(GameFlagType key) => _flags.ElementAtOrDefault(_flags.Count - 1) == key;

        public void Add(GameFlagType key)
        {
            if (!HasFlag(key))
                _flags.Add(key);

            if (_listeners.TryGetValue(key, out GameFlagsListener listener))
                listener.OnAdd.OnNext(key);
        }

        public void Remove(GameFlagType key)
        {
            if (HasFlag(key))
                _flags.Remove(key);

            if (_listeners.TryGetValue(key, out GameFlagsListener listener))
                listener.OnRemove.OnNext(key);
        }

        public IDisposable Subscribe(GameFlagType key, Action callback)
        {
            if (HasFlag(key))
            {
                callback?.Invoke();
                return null;
            }

            return OnEventAdd(key).Take(1).Subscribe(_ => callback?.Invoke());
        }

        public IDisposable OnEventRemoveOnce(GameFlagType key, Action callback) =>
            OnEventAdd(key)
            .Take(1)
            .Subscribe(_ => callback?.Invoke());

        public IObservable<GameFlagType> OnEventRemove(GameFlagType key) => GetOrAdd(key).OnRemove;

        public IObservable<GameFlagType> OnEventAdd(GameFlagType key) => GetOrAdd(key).OnAdd;

        private GameFlagsListener GetOrAdd(GameFlagType key)
        {
            GameFlagsListener listener;
            if (_listeners.TryGetValue(key, out listener))
                return listener;

            listener = new GameFlagsListener(key);
            _listeners.Add(key, listener);

            return listener;
        }

        public class SaveData
        {
            public HashSet<GameFlagType> flags;
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            _flags = ((SaveData)data).flags;
        }

        public object Save()
        {
            return new SaveData() { flags = _flags };
        }

        #region In Editor test

        [Title("TEST")]
        [ShowInInspector]
        private GameFlagType _flag;
        [Button, ButtonGroup()] private void Add() => Add(_flag);
        [Button, ButtonGroup()] private void Remove() => Remove(_flag);

        #endregion
    }
}
