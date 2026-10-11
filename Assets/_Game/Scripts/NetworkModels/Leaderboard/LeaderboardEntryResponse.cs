using System;

namespace _Game.Scripts.NetworkModels.Leaderboard
{
    [Serializable]
    public class LeaderboardEntryResponse
    {
        public int rank;
        public ulong userId;

        public string username;
        public string displayName;

        public int totalScore;
        public int totalMatchesPlayed;
        public int totalTrashRecycled;
    }
}