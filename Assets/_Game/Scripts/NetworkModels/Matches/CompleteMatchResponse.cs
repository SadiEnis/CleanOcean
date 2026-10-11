using System;

namespace _Game.Scripts.NetworkModels.Matches
{
    [Serializable]
    public class CompleteMatchResponse
    {
        public bool success;
        public string message;

        public long matchId;
        public string matchCode;
    }
}