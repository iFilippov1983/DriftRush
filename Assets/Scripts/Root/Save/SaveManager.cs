using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using SaveData = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace RaceManager.Root
{
    [Serializable]
    public class SaveManager : IDisposable
    {
        private readonly List<Type> _registeredTypes = new List<Type>();
        private readonly List<Action<SaveData>> _saveActions = new List<Action<SaveData>>();
        private readonly List<Action<SaveData>> _loadActions = new List<Action<SaveData>>();
        private readonly string _savePath;

        private IDisposable _savesDisposable;

        [ShowInInspector] 
        public Dictionary<string, string> _lastSave = new Dictionary<string, string>();

        public static string SavePath => Path.Combine(Application.persistentDataPath, "save.data");

        public SaveManager()
        {
            _savePath = SavePath;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Clear Save Data")]
#endif
        public static void RemoveSave()
        {
            var path = SavePath;
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

            _saveActions.Add(
                d =>
                {
                    _lastSave.Add(typeString, JsonConvert.SerializeObject(savable.Save()));
                    d[typeString] = JObject.FromObject(savable.Save());
                });

            _loadActions.Add(
                d =>
                {
                    if (!d.ContainsKey(typeString))
                        return;
                    savable.Load(d[typeString].ToObject(savable.DataType()));
                }
            );
        }

        public void Save()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            SaveData data = new SaveData();

            _lastSave.Clear();
            foreach (Action<SaveData> saveAction in _saveActions)
            {
                saveAction(data);
            }

            string json = JsonConvert.SerializeObject(data, settings);

            File.WriteAllText(_savePath, json);

            $"Data SAVED: {_savePath}".Log(ConsoleLog.Color.Green);
        }

        public void Load(bool ignoreMissing = true)
        {
            $"Load from: {_savePath}".Log(ConsoleLog.Color.Blue);

            if (!File.Exists(_savePath))
            {
                if (ignoreMissing)
                {
                    return;
                }
                else
                {
                    throw new FileNotFoundException($"Missing save file at {_savePath}", _savePath);
                }
            }

            string loadedJson = File.ReadAllText(_savePath);
            var data = JsonConvert.DeserializeObject<SaveData>(loadedJson);

            _lastSave = new Dictionary<string, string>();

            foreach (var kv in data)
                _lastSave.Add(kv.Key, kv.Value.ToString());

            foreach (Action<SaveData> loadAction in _loadActions)
            {
                loadAction(data);
            }
        }

        public void Dispose()
        {
            _savesDisposable.Dispose();
        }
    }
}

