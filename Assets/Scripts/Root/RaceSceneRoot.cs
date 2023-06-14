using RaceManager.Race;
using System;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    public class RaceSceneRoot : MonoBehaviour
    {
        private SaveManager _saveManager;
        private TutorialSteps _tutorial;

        [Inject]
        private void Construct(SaveManager saveManager, TutorialSteps tutorial)
        {
            _saveManager = saveManager;
            _tutorial = tutorial;

            EventsHub<RaceEvent>.Subscribe(RaceEvent.QUIT, HandleSceneQuit);
        }

        public void Run()
        {
            RegisterSaveables();
            LoadFromSave();
            InvokeInitializables();
            RunTutorial();
        }

        private void HandleSceneQuit()
        {
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.QUIT, HandleSceneQuit);

            _saveManager.Save();

            Loader.Load(Loader.Scene.MenuScene);
        }

        private void RegisterSaveables()
        {
            try
            {
                var saveables = Singleton<Resolver>.Instance.ResolveAll<ISaveable>();
                foreach (var s in saveables)
                    _saveManager.RegisterSavable(s);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Saveables] Need to fix => {e}");
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
                Debug.LogError($"[Initializables] Need to fix => {e}");
            }
        }

        private void LoadFromSave() => _saveManager.Load();

        private void RunTutorial() => _tutorial.RunStep();

        private void Dispose()
        {
            var disposeables = Singleton<Resolver>.Instance.ResolveAll<IDisposable>();
            foreach (var d in disposeables)
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

