using _Game.Scripts.NetworkModels.Player;

namespace _Game.Scripts.Session
{
    public static class CurrentUser
    {
        public static ulong UserId { get; private set; }
        public static string Username { get; private set; }
        public static string DisplayName { get; private set; }

        public static bool IsLoggedIn { get; private set; }

        public static int SoftCurrency { get; private set; }
        public static int TotalScore { get; private set; }
        public static int TotalMatchesPlayed { get; private set; }
        public static int TotalMatchesWon { get; private set; }
        public static int TotalTrashRecycled { get; private set; }
        public static int TotalRevivesDone { get; private set; }
        public static int TotalTimesFainted { get; private set; }
        public static int TotalPlaytimeSeconds { get; private set; }

        public static void Set(ulong userId, string username, string displayName)
        {
            UserId = userId;
            Username = username;
            DisplayName = displayName;
            IsLoggedIn = true;
        }

        public static void SetProfile(PlayerProfileResponse profile)
        {
            UserId = profile.userId;
            Username = profile.username;
            DisplayName = profile.displayName;

            SoftCurrency = profile.softCurrency;
            TotalScore = profile.totalScore;
            TotalMatchesPlayed = profile.totalMatchesPlayed;
            TotalMatchesWon = profile.totalMatchesWon;
            TotalTrashRecycled = profile.totalTrashRecycled;
            TotalRevivesDone = profile.totalRevivesDone;
            TotalTimesFainted = profile.totalTimesFainted;
            TotalPlaytimeSeconds = profile.totalPlaytimeSeconds;

            IsLoggedIn = true;
        }

        public static void Clear()
        {
            UserId = 0;
            Username = string.Empty;
            DisplayName = string.Empty;
            IsLoggedIn = false;

            SoftCurrency = 0;
            TotalScore = 0;
            TotalMatchesPlayed = 0;
            TotalMatchesWon = 0;
            TotalTrashRecycled = 0;
            TotalRevivesDone = 0;
            TotalTimesFainted = 0;
            TotalPlaytimeSeconds = 0;
        }
    }
}