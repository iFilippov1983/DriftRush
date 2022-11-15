using RaceManager.Race;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    public class RaceSceneRoot : MonoBehaviour
    {
        private SaveManager _saveManager;

        [Inject]
        private void Construct(SaveManager saveManager)
        {
            _saveManager = saveManager;

            EventsHub<RaceEvent>.Subscribe(RaceEvent.QUIT, HandleSceneQuit);
        }

        public void Run()
        {
            RegisterSavebles();
            LoadFromSave();
            InvokeInitializables();
        }

        private void HandleSceneQuit()
        {
            _saveManager.Save();
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
                $"[Saveables] Need to fix: {e}".Log(ConsoleLog.Color.Red);
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
                $"[Initializables] Need to fix: {e}".Log();
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

