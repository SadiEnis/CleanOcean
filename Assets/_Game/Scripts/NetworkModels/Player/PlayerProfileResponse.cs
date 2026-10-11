using System;

namespace _Game.Scripts.NetworkModels.Player
{
    [Serializable]
    public class PlayerProfileResponse
    {
        public bool success;
        public string message;

        public ulong userId;
        public string username;
        public string displayName;

        public int softCurrency;
        public int totalScore;

        public int totalMatchesPlayed;
        public int totalMatchesWon;

        public int totalTrashRecycled;

        public int totalRevivesDone;
        public int totalTimesFainted;

        public int totalPlaytimeSeconds;
    }
}