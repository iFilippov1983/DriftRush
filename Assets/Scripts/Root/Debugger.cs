using RaceManager.Cars;
using RaceManager.Progress;
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
        [SerializeField] private static CarsDepot _playerCarDepot;
        [SerializeField] private static GameProgressScheme _gameProgressScheme;
        
        private bool IsRaceScene;
        private bool IsMenuScene;

        [ReadOnly]
        public SaveManager saveManager;
        [ReadOnly]
        public PlayerProfile playerProfile;
        public Profiler profiler;
        public LevelName nextLevelToPlay;

        [Inject]
        private void Construct(SaveManager saveManager, PlayerProfile playerProfile, Profiler profiler)
        { 
            this.saveManager = saveManager;
            this.playerProfile = playerProfile;
            this.profiler = profiler;
        }

        private void Awake()
        {
            IsRaceScene = SceneManager.GetActiveScene().name == Loader.Scene.RaceScene.ToString();
            IsMenuScene = SceneManager.GetActiveScene().name == Loader.Scene.MenuScene.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
                WinRace();
        }

        [Button]
        [ShowIf("IsRaceScene", true)]
        public void WinRace()
        {
            var drivers = FindObjectsOfType<Driver>();
            var list = new List<Driver>(drivers);
            var playerDriver = list.Find(d => d.DriverType == DriverType.Player);
            playerDriver.DriverProfile.PositionInRace = PositionInRace.First;
            playerDriver.DriverProfile.CarState.Value = CarState.Finished;
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void SetLevelPrefab()
        {
            var level = ResourcesLoader.LoadPrefab(nextLevelToPlay.ToString());

            if (level != null)
            {
                $"Next level to play: {nextLevelToPlay}".Log(Logger.ColorYellow);
                profiler.SetNextLevel(nextLevelToPlay);
                saveManager.Save();
                $"SAVE - {this}".Log();
            }
            else
            { 
                $"Prefab whith name '{nextLevelToPlay}' was not found!".Log(Logger.ColorRed);
            }
        }

        [Button]
        public void Save() => saveManager.Save();

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Clear All Data")]
#endif
        [Button]
        public static void ClearAllData()
        {
            if (_playerCarDepot == null)
                _playerCarDepot = ResourcesLoader.LoadObject<CarsDepot>(ResourcePath.CarDepotPlayer);
            _playerCarDepot.ResetCarsAccessibility();

            if(_gameProgressScheme == null)
                _gameProgressScheme = ResourcesLoader.LoadObject<GameProgressScheme>(ResourcePath.GameProgressScheme);
            _gameProgressScheme.ResetAllSteps();

            SaveManager.RemoveSave();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddCups(int amount)
        {
            profiler.AddCups(amount);
            saveManager.Save();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddGems(int amount)
        { 
            profiler.AddGems(amount);
            saveManager.Save();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddMoney(int amount)
        { 
            profiler.AddMoney(amount);
            saveManager.Save();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddCards(CarName carName, int amount)
        { 
            profiler.AddCarCards(carName, amount);
            saveManager.Save();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddWinsCount()
        { 
            profiler.CountVictory();
            saveManager.Save();
        }
    }
}
