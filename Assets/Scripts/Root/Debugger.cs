﻿using RaceManager.Cars;
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
        [ShowIf("IsMenuScene", true)]
        public void SetLevelPrefab()
        {
            var level = ResourcesLoader.LoadPrefab(nextLevelToPlay.ToString());

            if (level != null)
            {
                $"Next level to play: {nextLevelToPlay}".Log(Logger.ColorYellow);
                profiler.SetNextLevel(nextLevelToPlay);
                saveManager.Save();
            }
            else
            { 
                $"Prefab whith name '{nextLevelToPlay}' was not found!".Log(Logger.ColorRed);
            }
        }

        [Button]
        public void Save() => saveManager.Save();

        [Button]
        public void DeleteSave() => SaveManager.RemoveSave();

        [Button]
        [ShowIf("IsMenuScene", true)]
        public void AddCups(int amount)
        {
            profiler.AddCups(amount);
            saveManager.Save();
        }
    }
}
