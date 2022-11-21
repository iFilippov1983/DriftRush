using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Tools;
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
        private bool IsMenuScene;

        [ReadOnly]
        public SaveManager saveManager;
        [ReadOnly]
        public PlayerProfile playerProfile;
        public LevelName nextLevelToPlay;

        [Inject]
        private void Construct(SaveManager saveManager, PlayerProfile playerProfile)
        { 
            this.saveManager = saveManager;
            this.playerProfile = playerProfile;
        }

        private void Awake()
        {
            IsRaceScene = SceneManager.GetActiveScene().name == Loader.Scene.RaceScene.ToString();
            IsMenuScene = SceneManager.GetActiveScene().name == Loader.Scene.MenuScene.ToString();
        }

        [Button]
        [ShowIf("IsRaceScene", true)]
        public void FinishRace()
        {
            var drivers = FindObjectsOfType<Driver>();
            var list = new List<Driver>(drivers);
            var playerDriver = list.Find(d => d.DriverType == DriverType.Player);
            playerDriver.DriverProfile.CarState.Value = CarState.Finished;
        }

        [Button]
        public void SetLevelPrefab()
        {
            var level = ResourcesLoader.LoadPrefab(nextLevelToPlay.ToString());

            if (level != null)
            {
                $"Next level to play: {nextLevelToPlay}".Log(ConsoleLog.Color.Yellow);
                playerProfile.NextLevelPrefabToLoad = nextLevelToPlay;
                saveManager.Save();
            }
            else
            { 
                $"Prefab whith name '{nextLevelToPlay}' was not found!".Log(ConsoleLog.Color.Red);
            }
        }

        [Button]
        public void Save() => saveManager.Save();

        [Button]
        public void DeleteSave() => SaveManager.RemoveSave();

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void SetCupsAmount(int amount)
        {
            playerProfile.Currency.Cups = amount;
            saveManager.Save();
        }
    }
}
