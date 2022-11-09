namespace RaceManager.Cars
{
    /// <summary>
    /// Visitable
    /// </summary>
    public interface ICarProperty
    { 
        CharacteristicType Type { get; }
        void Apply(IModifier modifier);
    }
}
