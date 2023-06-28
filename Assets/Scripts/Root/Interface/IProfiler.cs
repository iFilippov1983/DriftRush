using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;
using System.Collections.Generic;

namespace RaceManager.Root
{
    public interface IProfiler
    {
        public int MoneyCached { get; }
        public int GemsCached { get; }
        public int CupsCached { get; }
        public int MoneyCost { get; }
        public int GemsCost { get; }
        public float IncomeFactor { get; }
        public int VictoriesCycleCounter { get; }
        public int CardsAmount { get; }
        public CarName CarName { get; }
        public LevelName NextLevelToLoad { get; }
        public PositionInRace LastInRacePosition { get; }
        public Lootbox LootboxToAdd { get; }
        public List<Lootbox> Lootboxes { get; }
        public void SetLootboxList(List<Lootbox> lootboxes);
    }
}

