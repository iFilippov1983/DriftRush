using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using RaceManager;
using RaceManager.Cars.Effects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using SaveData = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;
using Sirenix.Serialization;
using System.Threading.Tasks;
using System.Globalization;

namespace RaceManager.Infrastructure
{
    public class BootstrapInstaller : BaseInstaller
    {
        Action<SaveData> aotAction;

        public override void InstallBindings()
        {
            AotEnsureObjects();
            //TODO: Services installation
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

            AotHelper.EnsureList<int>();
            AotHelper.EnsureList<float>();
            AotHelper.EnsureList<string>();
            AotHelper.EnsureList<Action<SaveData>>();

            AotHelper.EnsureDictionary<string, JObject>();
        }

        private void HandleTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError(e.Exception);
        }
    }
}
