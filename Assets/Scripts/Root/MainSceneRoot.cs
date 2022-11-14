using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Shed;
using RaceManager.UI;
using System;
using System.Threading.Tasks;
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

        [Inject]
        private void Construct(SaveManager saveManager, MainUI mainUI, PodiumView podium)
        {
            _menuCamerasHandler = Singleton<MenuCamerasHandler>.Instance;

            _saveManager = saveManager;
            _mainUI = mainUI;
            _podium = podium;
        }

        public void Run()
        {
            RegisterSavebles();
            LoadFromSave();
            InvokeInitializables();
            InitCameras();
        }

        private void InitCameras()
        {
            _menuCamerasHandler.LookAt(_podium.CarPlace);

            _mainUI.OnMainMenuActivityChange += _menuCamerasHandler.ToggleCamPriorities;
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

        private void LoadFromSave() => _saveManager.Load();

        private void Dispose()
        {
            var disposeables = Singleton<Resolver>.Instance.ResolveAll<IDisposable>();
            foreach (var d in disposeables)
                d.Dispose();

            _mainUI.OnMainMenuActivityChange -= _menuCamerasHandler.ToggleCamPriorities;
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}


