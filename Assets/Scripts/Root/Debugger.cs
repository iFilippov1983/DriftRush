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
            //var level = ResourcesLoader.LoadPrefab(string.Concat(ResourcePath.LevelsPrefabsFolder, nextLevelToPlay.ToString()));
            var level = ResourcesLoader.LoadPrefab(nextLevelToPlay.ToString());

            if (level != null)
            {
                $"Next level to play: {nextLevelToPlay}".Log(ConsoleLog.Color.Yellow);
                playerProfile.nextLevelPrefabToLoad = nextLevelToPlay;
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

    }
}
