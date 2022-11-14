using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using Sirenix.OdinInspector;
using UniRx;
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
        //private readonly List<Action<SaveData>> _saveActions;// = new List<Action<SaveData>>();
        //private readonly List<Action<SaveData>> _loadActions;// = new List<Action<SaveData>>();
        //private readonly string _savePath;

        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Key", ValueLabel = "Data")]
        public Dictionary<string, string> _lastSave = new Dictionary<string, string>();

        public SaveManager()
        {
            _saveActions = new List<SaveAction>();
            _loadActions = new List<LoadAction>();
            //HashSet<Action<SaveData>> actions = new HashSet<Action<SaveData>>();
            //_saveActions = new List<Action<SaveData>>(actions);
            //_loadActions = new List<Action<SaveData>>(actions);
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
            //_saveActions.Add(saveAction.Action);

            LoadAction loadAction = new LoadAction(typeString, savable);
            _loadActions.Add(loadAction);
            //_loadActions.Add(loadAction.Action);

            //_saveActions.Add(
            //    d =>
            //    {
            //        _lastSave.Add(typeString, JsonConvert.SerializeObject(savable.Save()));
            //        d[typeString] = JObject.FromObject(savable.Save());
            //    });
            //
            //_loadActions.Add(
            //    d =>
            //    {
            //        if (!d.ContainsKey(typeString))
            //            return;
            //        savable.Load(d[typeString].ToObject(savable.DataType()));
            //    }
            //);
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
                _lastSave.Add(pair.Item1, pair.Item2);
                //Debug.Log($"Data keys: {data.Keys};\nSaveAction: {saveAction._typeString};\nISaveable: {saveAction._savable.GetType()}");
            }
            //foreach (Action<SaveData> saveAction in _saveActions)
            //{
            //    saveAction(data);
            //}

            string json = JsonConvert.SerializeObject(data, settings);
            string path = Path.Combine(Application.persistentDataPath, FileName);

            File.WriteAllText(path, json);

            $"Data SAVED | SaveActions: {_saveActions.Count} | LoadActions: {_loadActions.Count} | Last save count: {_lastSave.Count}".Log(ConsoleLog.Color.Yellow);
            foreach (var kv in _lastSave)
                Debug.Log($"Key: {kv.Key}; Value: {kv.Value}".Colored(ConsoleLog.Color.Yellow));
        }

        public void Load(bool ignoreMissing = true)
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            bool fileExists = File.Exists(path);

            $"Load from => {path}; File exists => {fileExists}".Log(ConsoleLog.Color.Green);

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
            //foreach (Action<SaveData> loadAction in _loadActions)
            //{
            //    loadAction(data);
            //}

            $"Data LOADED | SaveActions: {_saveActions.Count} | LoadActions: {_loadActions.Count} | Last save count: {_lastSave.Count}".Log();
            foreach (var kv in _lastSave)
                Debug.Log($"Key: {kv.Key}; Value: {kv.Value}");
        }

        //public class ActionsList
        //{
        //    public List<Delegate> Actions;

        //    public ActionsList()
        //    {
        //        Actions = new List<Delegate>();
        //    }

        //    public void Add(Action<SaveData> action)
        //    {
        //        Delegate d = Delegate.CreateDelegate(typeof(SaveData), action.Target, action.Method);
        //        Actions.Add(d);
        //    }

        //    public void InvokeAll(SaveData data)
        //    {
        //        foreach (var action in Actions)
        //            action.DynamicInvoke(data);
        //    }
        //}


        /// <summary>
        /// Needed only for correct AOT serialization
        /// </summary>
        [Serializable]
        public class SaveAction
        {
            private readonly string _typeString;
            private readonly ISaveable _savable;
            private readonly Dictionary<string, string> _lastSave;

            //private Action<SaveData> _action;

            public SaveAction(string typeString, ISaveable savable, Dictionary<string, string> lastSave)
            {
                _typeString = typeString;
                _savable = savable;
                _lastSave = lastSave;

                //_action = d => Save(d);
            }

            public Func<SaveData, (string, string)> Action =>
                d =>
                {
                    if (!_lastSave.ContainsKey(_typeString))
                        _lastSave.Add(_typeString, JsonConvert.SerializeObject(_savable.Save()));
                    d[_typeString] = JObject.FromObject(_savable.Save());

                    Debug.Log($"Last save SAVE: {_lastSave.Count}".Colored(ConsoleLog.Color.Green));
                    return (_typeString, _lastSave[_typeString]);
                };

            //private void Save(SaveData d)
            //{
            //    if (_lastSave.ContainsKey(_typeString))
            //        return;
            //    _lastSave.Add(_typeString, JsonConvert.SerializeObject(_savable.Save()));
            //    d[_typeString] = JObject.FromObject(_savable.Save());
            //}
        }

        /// <summary>
        /// Needed only for correct AOT serialization
        /// </summary>
        [Serializable]
        public class LoadAction
        {
            private readonly string _typeString;
            private readonly ISaveable _savable;

            //private Action<SaveData> _action;

            public LoadAction(string typeString, ISaveable savable)
            {
                _typeString = typeString;
                _savable = savable;

                //_action = d => Load(d);
            }

            public Action<SaveData> Action =>
                d =>
                {
                    if (!d.ContainsKey(_typeString))
                        return;
                    _savable.Load(d[_typeString].ToObject(_savable.DataType()));

                    Debug.Log($"LOAD: {d.Count}".Colored(ConsoleLog.Color.Green));
                };

            //private void Load(SaveData d)
            //{
            //    if (!d.ContainsKey(_typeString))
            //        return;
            //    _savable.Load(d[_typeString].ToObject(_savable.DataType()));
            //}
        }
    }
}

