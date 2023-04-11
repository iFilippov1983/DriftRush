using RaceManager.Root;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using System.Threading.Tasks;
using System.Globalization;
using SaveData = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;
using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;

namespace RaceManager.Infrastructure
{
    public class BootstrapInstaller : BaseInstaller
    {
        Action<SaveData> aotAction;

        public override void InstallBindings()
        {
            AotEnsureObjects();
        }

        public override void Start()
        {
            base.Start();

            TaskScheduler.UnobservedTaskException += HandleTaskException;
            Application.targetFrameRate = 60;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Loader.Load(Loader.Scene.MenuScene);
        }

        private void AotEnsureObjects()
        {
            AotHelper.Ensure(() => aotAction.Invoke(null));

            AotHelper.EnsureType<SaveData>();
            AotHelper.EnsureType<JObject>();
            AotHelper.EnsureType<SaveManager>();
            AotHelper.EnsureType<SaveManager.SaveAction>();
            AotHelper.EnsureType<SaveManager.LoadAction>();

            AotHelper.EnsureList<int>();
            AotHelper.EnsureList<float>();
            AotHelper.EnsureList<string>();

            AotHelper.EnsureList<Action<SaveData>>();
            AotHelper.EnsureList<CarProfile>();
            AotHelper.EnsureList<Lootbox>();
            AotHelper.EnsureList<TutorialSteps.TutorialStep>();

            AotHelper.EnsureList<GameFlagType>();
            AotHelper.EnsureList<LevelName>();

            AotHelper.EnsureDictionary<string, JObject>();
            AotHelper.EnsureDictionary<int, ProgressStep>();
            AotHelper.EnsureDictionary<GameUnitType, GameProgressScheme.ExchangeRateData>();
        }

        private void HandleTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError(e.Exception);
        }

        private void OnDestroy()
        {
            TaskScheduler.UnobservedTaskException -= HandleTaskException;
        }
    }
}
