using System;
using UniRx;

namespace RaceManager.Root
{
    [Serializable]
    public class GameFlagsListener
    {
        public GameFlagType Key;
        public Subject<GameFlagType> OnAdd = new Subject<GameFlagType>();
        public Subject<GameFlagType> OnRemove = new Subject<GameFlagType>();

        public GameFlagsListener(GameFlagType key)
        {
            Key = key;
        }
    }
}
