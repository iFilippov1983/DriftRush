using RaceManager.Cars;
using RaceManager.Effects;
using RaceManager.Progress;
using RaceManager.Race;
using RaceManager.Tools;
using RaceManager.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace RaceManager.Root
{
    public class Debugger : MonoBehaviour
    {
        private static CarsDepot _playerCarDepot;
        private static GameProgressScheme _gameProgressScheme;
        private static EffectsSettingsContainer _settingsContainer;

        [SerializeField]
        private MainUI _mainUI;

        private bool IsRaceScene;
        private bool IsMenuScene;

        [ReadOnly]
        public SaveManager saveManager;
        [ReadOnly]
        public PlayerProfile playerProfile;
        public Profiler profiler;
        public LevelName nextLevelToPlay;

        private GameEffectsController FxController => Singleton<GameEffectsController>.Instance;

        private static CarsDepot PlayerCarDepot
        {
            get
            {
                if (_playerCarDepot == null)
                    _playerCarDepot = ResourcesLoader.LoadObject<CarsDepot>(ResourcePath.CarDepotPlayer);
                return _playerCarDepot;
            }
        }

        private static GameProgressScheme GameProgressScheme
        {
            get
            {
                if (_gameProgressScheme == null)
                    _gameProgressScheme = ResourcesLoader.LoadObject<GameProgressScheme>(ResourcePath.GameProgressScheme);
                return _gameProgressScheme;
            }
        }

        private static EffectsSettingsContainer SettingsContainer
        {
            get 
            {
                if (_settingsContainer == null)
                    _settingsContainer = ResourcesLoader.LoadObject<EffectsSettingsContainer>(ResourcePath.EffectsSettingsContainer);
                return _settingsContainer;
            }
        }

        [Inject]
        private void Construct
            (
            SaveManager saveManager, 
            PlayerProfile playerProfile, 
            Profiler profiler
            )
        { 
            this.saveManager = saveManager;
            this.playerProfile = playerProfile;
            this.profiler = profiler;
        }

#if UNITY_EDITOR

        private void Awake()
        {
            IsRaceScene = SceneManager.GetActiveScene().name == Loader.Scene.RaceScene.ToString();
            IsMenuScene = SceneManager.GetActiveScene().name == Loader.Scene.MenuScene.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
                WinRace();

            HandleSoundtrackTest();
            HandleSfxTest();
        }

#endif


        [Button]
        [ShowIf("IsRaceScene", true)]
        public void WinRace()
        {
            var drivers = FindObjectsOfType<Driver>();
            var list = new List<Driver>(drivers);
            var playerDriver = list.Find(d => d.DriverType == DriverType.Player);
            playerDriver.DriverProfile.PositionInRace = PositionInRace.First;
            playerDriver.DriverProfile.CarState.Value = CarState.Finished;

            saveManager.Save();
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
            PlayerCarDepot.ResetCars();
            GameProgressScheme.ResetAllSteps();
            SettingsContainer.ResetToDefault();

            SaveManager.RemoveSave();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddCups(int amount)
        {
            profiler.AddCups(amount);
            saveManager.Save();

            if (_mainUI != null)
            { 
                _mainUI.UpdateGameProgressPanel();
            }
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddGems(int amount)
        { 
            profiler.AddGems(amount);
            saveManager.Save();

            if(_mainUI != null)
                _mainUI.UpdateCurrencyAmountPanels();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddMoney(int amount)
        { 
            profiler.AddMoney(amount);
            saveManager.Save();

            if (_mainUI != null)
                _mainUI.UpdateCurrencyAmountPanels();
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

        #region Test Functions

        private void HandleSoundtrackTest()
        {
            if (Input.GetKeyUp(KeyCode.T))
            {
                FxController.PlayEffect(Effects.AudioType.MenuTrack_01, true);
            }

            if (Input.GetKeyUp(KeyCode.G))
            {
                FxController.StopAudio(Effects.AudioType.MenuTrack_01, true);
            }

            if (Input.GetKeyUp(KeyCode.B))
            {
                FxController.RestartAudio(Effects.AudioType.MenuTrack_01, true);
            }
        }

        private void HandleSfxTest()
        {
            if (Input.GetKeyUp(KeyCode.Y))
            {
                FxController.PlayEffect(Effects.AudioType.SFX_ButtonPressed);
            }

            if (Input.GetKeyUp(KeyCode.H))
            {
                FxController.StopAudio(Effects.AudioType.SFX_ButtonPressed);
            }

            if (Input.GetKeyUp(KeyCode.N))
            {
                FxController.RestartAudio(Effects.AudioType.SFX_ButtonPressed);
            }
        }

        #endregion
    }
}
