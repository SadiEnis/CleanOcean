namespace _Game.Scripts.Gameplay.Match
{
    public class PlayerMatchStats
    {
        public ulong ClientId { get; set; }
        public ulong DbUserId { get; set; }

        public int FinalScore { get; set; }
        public int TrashRecycledCount { get; set; }
        public int RevivesDone { get; set; }
        public int TimesFainted { get; set; }
        public int EarnedCurrency { get; set; }
    }
}