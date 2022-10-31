using RaceManager.Cars;
using RaceManager.Race;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace RaceManager.Root
{
    public class Debugger : MonoBehaviour
    {
        private bool IsRaceScene;

        [ReadOnly]
        public SaveManager saveManager;
        [ReadOnly]
        public PlayerProfile playerProfile;

        [Inject]
        private void Construct(SaveManager saveManager, PlayerProfile playerProfile)
        { 
            this.saveManager = saveManager;
            this.playerProfile = playerProfile;
        }

        private void Awake()
        {
            IsRaceScene = SceneManager.GetActiveScene().name == Loader.Scene.RaceScene.ToString();
        }

        [Button]
        [ShowIf("IsRaceScene", true)]
        public void FinishRace()
        {
            var drivers = FindObjectsOfType<Driver>();
            var list = new List<Driver>(drivers);
            var playerDriver = list.Find(d => d.DriverType == DriverType.Player);
            playerDriver.StopRace();
        } 

        [Button]
        public void Load() => saveManager.Load();

        [Button]
        public void Save() => saveManager.Save();

        [Button]
        public void DeleteSave() => SaveManager.RemoveSave();

        [Button]
        public void PrintSave()
        {
            foreach (var pair in saveManager._lastSave)
            {
                $"Key: {pair.Key}".Log(ConsoleLog.Color.Red);
                $"Value: {pair.Value}".Log(ConsoleLog.Color.Yellow);
            }
        }
    }
}
