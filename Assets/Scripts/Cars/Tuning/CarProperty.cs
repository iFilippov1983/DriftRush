using System;

namespace RaceManager.Cars
{
    [Serializable]
    public struct CarSpeed : ICarProperty
    {
        public float MinSpeed;
        public float MaxSpeed;

        public CharacteristicType Type => CharacteristicType.Speed;

        public void Apply(IModifier modifier)
        {
            modifier.Modify(this);
        }
    }
}
