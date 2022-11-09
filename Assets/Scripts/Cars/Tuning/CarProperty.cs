using System;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public struct CarSpeed : ICarProperty
    {
        public float MinSpeed;
        public float MaxSpeed;
        public CarProfile Profile;

        public CharacteristicType Type => CharacteristicType.Speed;

        //public CarSpeed(CarProfile carProfile)
        //{
        //    _profile = carProfile;
        //}
        

        public void Apply(IModifier modifier)
        {
            modifier.Modify(this);
        }
    }
}
