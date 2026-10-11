using System;

namespace _Game.Scripts.NetworkModels.Leaderboard
{
    [Serializable]
    public class LeaderboardResponse
    {
        public bool success;
        public string message;

        public LeaderboardEntryResponse[] players;
    }
}