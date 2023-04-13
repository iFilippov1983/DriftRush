using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/CarsDepot", fileName = "CarsDepot", order = 1)]
    public class CarsDepot : ScriptableObject, ISaveable
    {
        public DriverType DriverType;
        public CarName CurrentCarName;
        [SerializeField]
        private List<CarProfile> CarProfiles;

        public IReadOnlyList<CarProfile> ProfilesList => CarProfiles;
        public CarProfile GetProfile(CarName carName) => CarProfiles.Find(p => p.CarName == carName);
        public CarProfile CurrentCarProfile => CarProfiles.Find(c => c.CarName == CurrentCarName);

        public void UpdateProfile(CarProfile newCP)
        {
            CarProfile currentCP = CarProfiles.Find(c => c.CarName == newCP.CarName);
            if(currentCP != null)
                CarProfiles.Remove(currentCP);
            CarProfiles.Add(newCP);
            CarProfiles.Sort((a, b) => a.CarCharacteristics.Rarity.CompareTo(b.CarCharacteristics.Rarity));
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object saveObject)
        {
            SaveData data = (SaveData)saveObject;
            DriverType = data.driverType;
            CurrentCarName = data.currentCarName;
            CarProfiles = data.carProfiles;
        }

        public object Save()
        {
            SaveData saveData = new SaveData
            {
                driverType = DriverType,
                currentCarName = CurrentCarName,
                carProfiles = CarProfiles
            };

            return saveData;
        }

        public class SaveData
        {
            public DriverType driverType;
            public CarName currentCarName;
            public List<CarProfile> carProfiles;
        }

        [Button]
        public void ResetCars(CarName defaultCarName = CarName.SuperBull)
        {
            foreach (var profile in CarProfiles)
            {
                foreach (var r in profile.RankingScheme.Ranks)
                {
                    r.IsGranted = false;
                    r.IsReached = false;
                    r.UpgradeCostCurrent = r.UpgradeCostBase;
                }

                if (profile.CarName == defaultCarName)
                {
                    profile.RankingScheme.Ranks[0].IsGranted = true;
                    profile.RankingScheme.Ranks[0].IsReached = true;
                }

                var c = profile.CarCharacteristics;
                c.CurrentFactorsProgress = Mathf.RoundToInt(c.FactorsMaxTotal * c.DefaultFactorsPercentage);
                c.CurrentAccelerationFactor = Mathf.RoundToInt(c.MaxAccelerationFactor * c.DefaultFactorsPercentage);
                c.CurrentDurabilityFactor = Mathf.RoundToInt(c.MaxDurabilityFactor * c.DefaultFactorsPercentage);
                c.CurrentHandlingFactor = Mathf.RoundToInt(c.MaxHandlingFactor * c.DefaultFactorsPercentage);
                c.CurrentSpeedFactor = Mathf.RoundToInt(c.MaxSpeedFactor * c.DefaultFactorsPercentage);
                c.CurrentFrictionFactor = Mathf.RoundToInt(c.MaxFrictionFactor * c.DefaultFactorsPercentage);
                c.FactorsMaxCurrent = 0;
            }

            Debug.Log($"All cars ranks IsReached and IsGranted properties are set to False (exept default car - {CarName.BlinkGoat}) | All CurrentFactors amount are set to Max/2");
        }
    }
}
