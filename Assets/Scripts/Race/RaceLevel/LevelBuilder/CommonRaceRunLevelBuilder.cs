using RaceManager.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class CommonRaceRunLevelBuilder : IRaceLevelBuilder
    {
        private IRaceLevel _raceLevel;

        public IRaceLevel GetResult() => _raceLevel;

        public void SetPrefab(string path)
        {
            _raceLevel = ResourcesLoader.LoadAndInstantiate<IRaceLevel>(path, new GameObject("Level").transform);
        }

        /// <summary>
        /// Pass Difficulty.Zero to set random configuration
        /// </summary>
        /// <param name="configurationDif"></param>
        public void SetTrackConfigurations(Difficulty configurationDif = Difficulty.Zero)
        {
            var configurations = _raceLevel.Configurations;

            foreach (var trackConfiguration in configurations)
                trackConfiguration.SetActive(false);

            TrackConfiguration c;

            if (configurationDif == Difficulty.Zero)
            {
                c = configurations[Random.Range(0, configurations.Count)];
            }
            else
            {
                List<TrackConfiguration> concreteConfigurations = new List<TrackConfiguration>();
                foreach (var trackConfiguration in configurations)
                { 
                    if(trackConfiguration.Difficulty == configurationDif)
                        concreteConfigurations.Add(trackConfiguration);
                }

                c = concreteConfigurations[Random.Range(0, concreteConfigurations.Count)];
            }

            foreach (var active in c.Actives)
                active.SetActive(true);

            foreach (var inactive in c.Inactives)
                inactive.SetActive(false);

            c.SetActive(true);
            _raceLevel.SetCurrentConfiguration(c);

            Debug.Log($"Race level configuration loaded => [{c.name}]");
        }

        /// <summary>
        /// Pass int value less then 0 to set default opponents amount
        /// </summary>
        /// <param name="amount"></param>
        public void SetOpponents(int amount = 0)
        {
            var startPoints = _raceLevel.StartPoints;

            int a = amount < 0 || amount > startPoints.Length
                ? _raceLevel.StartPoints.Length 
                : amount;

            for (int i = 0; i < startPoints.Length; i++)
            {
                if (startPoints[i].Type == Cars.DriverType.Player)
                {
                    startPoints[i].isAvailable = true;
                }
                else
                {
                    startPoints[i].isAvailable = i < a;
                }
            }
        }
    }
}