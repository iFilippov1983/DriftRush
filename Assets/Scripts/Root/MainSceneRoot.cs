using RaceManager.Cameras;
using RaceManager.Shed;
using RaceManager.UI;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    public class MainSceneRoot : MonoBehaviour
    {
        private SaveManager _saveManager;
        private MainUI _mainUI;
        private MenuCamerasHandler _menuCamerasHandler;
        private PodiumView _podium;
        private TutorialSteps _tutorial;
        private GameRemindHandler _remindHandler;

        [Inject]
        private void Construct
            (
            SaveManager saveManager, 
            MainUI mainUI, 
            PodiumView podium, 
            TutorialSteps tutorial, 
            GameRemindHandler remindHandler
            )
        {
            _menuCamerasHandler = Singleton<MenuCamerasHandler>.Instance;

            _saveManager = saveManager;
            _mainUI = mainUI;
            _podium = podium;
            _tutorial = tutorial;
            _remindHandler = remindHandler;

            InitCameras();
        }

        public void Run()
        {
            RegisterSavebles();
            LoadFromSave();
            InvokeInitializables();
            RunTutorial();
            InvokeLateInitializables();
            RunReminders();
        }

        private void InitCameras()
        {
            _menuCamerasHandler.LookAt(_podium.CarPlace);

            _mainUI.OnStatusChange.Subscribe(s => _menuCamerasHandler.ToggleCamPriorities(s)).AddTo(this);
        }

        private void RegisterSavebles()
        {
            try
            {
                var saveables = Singleton<Resolver>.Instance.ResolveAll<ISaveable>();
                foreach (var s in saveables)
                    _saveManager.RegisterSavable(s);
            }
            catch (Exception e)
            {
                $"[Saveables] Need to fix: {e}".Error();
            }
        }

        private void InvokeInitializables()
        {
            try
            {
                var initializables = Singleton<Resolver>.Instance.ResolveAll<IInitializable>();
                foreach (var i in initializables)
                    i.Initialize();
            }
            catch (Exception e)
            {
                $"[Initializables] Need to fix: {e}".Error();
            }
        }

        private void InvokeLateInitializables()
        {
            try
            {
                var initializables = Singleton<Resolver>.Instance.ResolveAll<ILateInitializable>();
                foreach (var i in initializables)
                    i.LateInitialize();
            }
            catch (Exception e)
            {
                $"[Late Initializables] Need to fix: {e}".Error();
            }
        }

        private void LoadFromSave() => _saveManager.Load();

        private void RunTutorial() => _tutorial.RunStep();

        private void RunReminders() => _remindHandler.RunRemindersSequence();

        private void Dispose()
        {
            var disposables = Singleton<Resolver>.Instance.ResolveAll<IDisposable>();
            foreach (var d in disposables)
                d.Dispose();
        }

        private void OnApplicationQuit()
        {
            _saveManager.Save();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}


