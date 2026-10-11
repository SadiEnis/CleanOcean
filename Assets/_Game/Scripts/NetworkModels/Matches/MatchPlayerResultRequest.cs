using System;

namespace _Game.Scripts.NetworkModels.Matches
{
    [Serializable]
    public class MatchPlayerResultRequest
    {
        public ulong userId;

        public int finalScore;
        public int trashRecycledCount;
        public int revivesDone;
        public int timesFainted;
        public int earnedCurrency;

        public MatchPlayerResultRequest(
            ulong userId,
            int finalScore,
            int trashRecycledCount,
            int revivesDone,
            int timesFainted,
            int earnedCurrency)
        {
            this.userId = userId;
            this.finalScore = finalScore;
            this.trashRecycledCount = trashRecycledCount;
            this.revivesDone = revivesDone;
            this.timesFainted = timesFainted;
            this.earnedCurrency = earnedCurrency;
        }
    }
}