namespace RaceManager.Race
{
    public interface IRaceLevelBuilder
    {
        public void SetPrefab(string path);

        /// <summary>
        /// Pass Difficulty.Random to set random configuration
        /// </summary>
        /// <param name="configurationDif"></param>
        public void SetTrackConfigurations(Difficulty configurationDifficulty = Difficulty.Random);

        /// <summary>
        /// Pass int value less then 0 to set default opponents amount
        /// </summary>
        /// <param name="amount"></param>
        public void SetOpponents(int amount = 0);

        public void ActivateAccessoryObjects(bool activate);

        public IRaceLevel GetResult();
    }
}