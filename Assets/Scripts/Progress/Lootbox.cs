using RaceManager.Cars;
using RaceManager.Tools;
using System;

namespace RaceManager.Progress
{
    [Serializable]
    public class Lootbox
    {
        private LootboxModel _lootboxModel;

        public float CurrentTimeToOpen;

        public Lootbox(Rarity rarity)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
        }

        public LootboxModel LootboxModel => _lootboxModel;
        public float InitialTimeToOpen => _lootboxModel.TimeToOpen;
        public bool IsOpen => CurrentTimeToOpen == 0;
    }
}
