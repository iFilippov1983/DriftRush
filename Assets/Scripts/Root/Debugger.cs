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
        private static CarsDepot _opponentsCarDepot;
        private static GameProgressScheme _gameProgressScheme;
        private static GameSettingsContainer _settingsContainer;
        private static TutorialSteps _tutorial;
        private static OpponentsTuneScheme _opponentsTuneScheme;

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

        private static CarsDepot OpponentsCarDepot
        {
            get
            {
                if (_opponentsCarDepot == null)
                    _opponentsCarDepot = ResourcesLoader.LoadObject<CarsDepot>(ResourcePath.CarDepotOpponents);
                return _opponentsCarDepot;
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

        private static GameSettingsContainer SettingsContainer
        {
            get 
            {
                if (_settingsContainer == null)
                    _settingsContainer = ResourcesLoader.LoadObject<GameSettingsContainer>(ResourcePath.GameSettingsContainer);
                return _settingsContainer;
            }
        }

        private static OpponentsTuneScheme OpponentsTuneScheme
        {
            get
            {
                if (_opponentsTuneScheme == null)
                    _opponentsTuneScheme = ResourcesLoader.LoadObject<OpponentsTuneScheme>(ResourcePath.OpponentsTuneScheme);
                return _opponentsTuneScheme;
            }
        }

        private static TutorialSteps Tutorial
        {
            get
            { 
                if(_tutorial == null)
                    _tutorial = FindObjectOfType<TutorialSteps>();

                return _tutorial;
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
                FinishRace();

            HandleSoundtrackTest();
            HandleSfxTest();
        }
#endif


        [Button]
        [ShowIf("IsRaceScene", true)]
        public void FinishRace()
        {
            var drivers = FindObjectsOfType<Driver>();
            var list = new List<Driver>(drivers);
            var playerDriver = list.Find(d => d.DriverType == DriverType.Player);

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
                Debug.Log($"Next level to play => {nextLevelToPlay}");
                profiler.SetNextLevel(nextLevelToPlay);
                saveManager.Save();
                Debug.Log($"SAVE - {this}");
            }
            else
            { 
                Debug.LogError($"Prefab whith name '{nextLevelToPlay}' was not found!");
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
            OpponentsTuneScheme.ResetScheme();
            
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
                _mainUI.UpdateCurrencyAmountPanels(GameUnitType.Gems);
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddMoney(int amount)
        { 
            profiler.AddMoney(amount);
            saveManager.Save();

            if (_mainUI != null)
                _mainUI.UpdateCurrencyAmountPanels(GameUnitType.Money);
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
            profiler.CountVictoryCycle();
            saveManager.Save();
        }

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void ResetPlayerIap()
        {
            profiler.SetAdsOn();
            saveManager.Save();

            Debug.Log("GotSpecialOffer is set to => False");
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
