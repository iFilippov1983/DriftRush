using System;

namespace RaceManager.Effects
{
    public interface ISfxEventSource
    {
        public Action<AudioType> SfxEvent { get; }
    }
}
