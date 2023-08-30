namespace RaceManager.Race
{
    public struct TotalScoreData
    {
        public int Value;
        public int Timer;
        public bool ShowScores;
    }

    public struct DriftScoresData
    { 
        public int CurrentScoresValue;
        public int TotalScoresValue;
        
        public float ScoresFactorThisType;
        public float ScoresCountTime;

        public bool isDrifting;

        public void Reset()
        { 
            CurrentScoresValue = 0;
            TotalScoresValue = 0;

            ScoresFactorThisType = 0;
            ScoresCountTime = 0;

            isDrifting = false;
        }
    }

    public struct CollisionScoresData
    {
        public RaceScoresType ScoresType;

        public int CurrentScoresThisTypeValue;
        public int TotalScoresThisTypeValue;
    }
}