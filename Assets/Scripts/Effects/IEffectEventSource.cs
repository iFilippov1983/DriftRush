using System;

namespace RaceManager.Effects
{
    public interface IEffectEventSource
    {
        public Action<EffectData> EffectEvent { get; }
    }
}
