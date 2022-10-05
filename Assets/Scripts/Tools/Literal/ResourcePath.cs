namespace RaceManager.Tools
{
    internal static class ResourcePath
    {
        public const string WaypointPrefab = "Prefabs/Waypoint";
        public const string DriverPrefab = "Prefabs/Driver";

        //public static string ItemConfigsSource(FeatureType.Item itemType)
        //{
        //    string path = itemType switch
        //    {
        //        FeatureType.Item.None => throw new System.Exception("Item not defined"),
        //        FeatureType.Item.Tire => string.Concat(ItemsConfigSources, FeatureType.Item.Tire.ToString()),
        //        FeatureType.Item.Suspension => string.Concat(ItemsConfigSources, FeatureType.Item.Suspension.ToString()),
        //        FeatureType.Item.Cannon => string.Concat(ItemsConfigSources, FeatureType.Item.Cannon.ToString()),
        //        _ => throw new System.NotImplementedException(),
        //    };

        //    return path;
        //}
    }
}
