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
        }

        public async void Run()
        {
            await Task.Yield();
            RegisterSavebles();
        }

        private void RegisterSavebles()
        {
            try
            {
                var saveables = Singleton<ResolverService>.Instance.ResolveAll<ISaveable>();
                foreach (var s in saveables)
                    _saveManager.RegisterSavable(s);
            }
            catch (Exception e)
            {
                $"Need to fix: {e}".Log(ConsoleLog.Color.Red);
            }
        }
    }
}

