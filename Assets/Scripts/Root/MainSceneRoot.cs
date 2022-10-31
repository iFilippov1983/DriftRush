using RaceManager.Cars;
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
        private CarTunerVisual _carTunerVisual;
        private MainUI _mainUI;

        [Inject]
        private void Construct(SaveManager saveManager, CarTunerVisual carTunerVisual, MainUI mainUI)
        {
            _saveManager = saveManager;
            _carTunerVisual = carTunerVisual;
            _mainUI = mainUI;
        }

        public void Run()
        {
            RegisterSavebles();
            LoadFromSave();
        }

        private void InitializeTuners()
        {
            _mainUI.OnSpeedValueChange
                .Subscribe(_carTunerVisual.OnSpeedValueChanged)
                .AddTo(this);

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
                $"Need to fix: {e.Message}".Log(ConsoleLog.Color.Red);
            }
        }

        private void LoadFromSave() => _saveManager.Load();

        private void Dispose()
        {
            var disposeables = Singleton<Resolver>.Instance.ResolveAll<IDisposable>();
            foreach (var d in disposeables)
                d.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}


