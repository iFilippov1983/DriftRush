using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;
using System.Collections.Generic;

namespace RaceManager.Root
{
    public interface IProfiler
    {
        public int Money { get; }
        public int Gems { get; }
        public int Cups { get; }
        public int MoneyCost { get; }
        public int GemsCost { get; }
        public float IncomeFactor { get; }
        public int VictoriesCounter { get; }
        public int CardsAmount { get; }
        public CarName CarName { get; }
        public LevelName LevelName { get; }
        public PositionInRace LastInRacePosition { get; }
        public Lootbox LootboxToAdd { get; }
        public List<Lootbox> Lootboxes { get; }
        public List<LevelName> AvailableLevels { get; }
        public void SetLootboxList(List<Lootbox> lootboxes);
        public void SetLevelsList(List<LevelName> levels);
    }
}

