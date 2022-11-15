using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using SaveData = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace RaceManager.Root
{
    [Serializable]
    public class SaveManager
    {
        public const string FileName = "save.data";

        private readonly List<Type> _registeredTypes = new List<Type>();
        private readonly List<SaveAction> _saveActions;
        private readonly List<LoadAction> _loadActions;

        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Key", ValueLabel = "Data")]
        public Dictionary<string, string> _lastSave = new Dictionary<string, string>();

        public SaveManager()
        {
            _saveActions = new List<SaveAction>();
            _loadActions = new List<LoadAction>();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Clear Save Data")]
#endif
        public static void RemoveSave()
        {
            var path = Path.Combine(Application.persistentDataPath, FileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                $"File located in {path} - DELETED".Log(ConsoleLog.Color.Green);
                return;
            }
             
            $"No data to delete".Log(ConsoleLog.Color.Yellow);
        }

        public void RegisterSavable(ISaveable savable)
        {
            if (_registeredTypes.Contains(savable.GetType()))
            {
                throw new InvalidOperationException($"Data saver for {savable.GetType()} already exists");
            }

            _registeredTypes.Add(savable.GetType());

            string typeString = savable.GetType().Name;

            if (typeString is null)
            {
                throw new NullReferenceException("Type full name is somehow null");
            }

            SaveAction saveAction = new SaveAction(typeString, savable, _lastSave);
            _saveActions.Add(saveAction);

            LoadAction loadAction = new LoadAction(typeString, savable);
            _loadActions.Add(loadAction);
        }

        public void Save()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            SaveData data = new SaveData();

            _lastSave.Clear();
            foreach (SaveAction saveAction in _saveActions)
            {
                var pair = saveAction.Action.Invoke(data);
                if(!_lastSave.ContainsKey(pair.Item1))
                    _lastSave.Add(pair.Item1, pair.Item2);
            }

            string json = JsonConvert.SerializeObject(data, settings);
            string path = Path.Combine(Application.persistentDataPath, FileName);

            File.WriteAllText(path, json);
        }

        public void Load(bool ignoreMissing = true)
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            bool fileExists = File.Exists(path);

            $"Load from => {path};\n File exists => {fileExists}".Log(ConsoleLog.Color.Green);

            if (!fileExists)
            {
                if (ignoreMissing)
                {
                    return;
                }
                else
                {
                    throw new FileNotFoundException($"Missing save file at {path}", path);
                }
            }

            string loadedJson = File.ReadAllText(path);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(loadedJson);

            _lastSave = new Dictionary<string, string>();

            foreach (var kv in data)
                _lastSave.Add(kv.Key, kv.Value.ToString());

            foreach (LoadAction loadAction in _loadActions)
            {
                loadAction.Action.Invoke(data);
            }
        }

        /// <summary>
        /// Needed only for correct AOT serialization
        /// </summary>
        [Serializable]
        public class SaveAction
        {
            private readonly string _typeString;
            private readonly ISaveable _savable;
            private readonly Dictionary<string, string> _lastSave;

            private Action<SaveData> _action;

            public SaveAction()
            {
                throw new NotImplementedException("Open empty constructor for SaveAction is made just in purpose to solve AOT serialization problem! Don't use it."); 
            }

            public SaveAction(string typeString, ISaveable savable, Dictionary<string, string> lastSave)
            {
                _typeString = typeString;
                _savable = savable;
                _lastSave = lastSave;
            }

            public Func<SaveData, (string, string)> Action =>
                d =>
                {
                    if (!_lastSave.ContainsKey(_typeString))
                        _lastSave.Add(_typeString, JsonConvert.SerializeObject(_savable.Save()));
                    d[_typeString] = JObject.FromObject(_savable.Save());

                    return (_typeString, _lastSave[_typeString]);
                };
        }

        /// <summary>
        /// Needed only for correct AOT serialization
        /// </summary>
        [Serializable]
        public class LoadAction
        {
            private readonly string _typeString;
            private readonly ISaveable _savable;

            public LoadAction()
            {
                throw new NotImplementedException("Open empty constructor for LoadAction is made just in purpose to solve AOT serialization problem! Don't use it.");
            }

            public LoadAction(string typeString, ISaveable savable)
            {
                _typeString = typeString;
                _savable = savable;
            }

            public Action<SaveData> Action =>
                d =>
                {
                    if (!d.ContainsKey(_typeString))
                        return;
                    _savable.Load(d[_typeString].ToObject(_savable.DataType()));
                };
        }
    }
}

