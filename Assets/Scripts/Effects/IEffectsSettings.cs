namespace RaceManager.Effects
{
    public interface IEffectsSettings
    { 
        public bool PlaySounds { get; }
        public bool PlayMusic { get; }
        public bool UseHaptics { get; }
    }
}
