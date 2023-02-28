using RaceManager.Cars;

namespace RaceManager.Tools
{
    internal static class ResourcePath
    {
        public const string CarPrefabsFolder = "Prefabs/Cars/";
        public const string MaterialsPrefabsFolder = "Prefabs/Mat/";
        public const string LevelsPrefabsFolder = "Prefabs/Levels/";

        public const string MaterialsContainer = "SO/Containers/MaterialsContainer";
        public const string EffectsSettingsContainer = "SO/Containers/EffectsSettingsContainer";

        public const string GameProgressScheme = "SO/Schemes/GameProgressScheme";
        public const string CarDepotPlayer = "SO/Depots/CarsDepotPlayer";

        public const string WaypointPrefab = "Prefabs/Waypoint";
        public const string RaceLineSegmentPrefab = "Prefabs/RaceLineSegment";
        public const string DriverPrefab = "Prefabs/Driver";
        public const string LootboxPrefab = "Prefabs/Lootbox";

        public const string FXControllerPrefab = "Prefabs/Effects/FXControllerRace";

        public const string CollectionCardPrefab = "Prefabs/UI/CollectionCard";
        public const string ProgressStepPrefab = "Prefabs/UI/ProgressStep";
        public const string CarCardViewPrefab = "Prefabs/UI/CarCard";
        public const string ImageAnimationTrailPrefab = "Prefabs/UI/ImageTrail";
        public const string ExtraScoresIndicatorViewPrefab = "Prefabs/UI/ExtraScoresIndicator";

        private const string LootboxModelsFolder = "SO/LootboxModels/";
        private const string CommonLootboxName = "0-LootboxCommon";
        private const string UncommonLootboxName = "1-LootboxUncommon";
        private const string RareLootboxName = "2-LootboxRare";
        private const string EpicLootboxName = "3-LootboxEpic";
        private const string LegendaryLootboxName = "4-LootboxLegendary";

        public static string LootboxModelPath(Rarity rarity)
        {
            string path = rarity switch
            {
                Rarity.Common => string.Concat(LootboxModelsFolder, CommonLootboxName),
                Rarity.Uncommon => string.Concat(LootboxModelsFolder, UncommonLootboxName),
                Rarity.Rare => string.Concat(LootboxModelsFolder, RareLootboxName),
                Rarity.Epic => string.Concat(LootboxModelsFolder, EpicLootboxName),
                Rarity.Legendary => string.Concat(LootboxModelsFolder, LegendaryLootboxName),
                _ => throw new System.NotImplementedException(),
            };

            return path;
        }
    }
}