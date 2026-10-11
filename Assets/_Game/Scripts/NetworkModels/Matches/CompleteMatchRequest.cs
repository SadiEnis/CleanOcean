using System;
using System.Collections.Generic;

namespace _Game.Scripts.NetworkModels.Matches
{
    [Serializable]
    public class CompleteMatchRequest
    {
        public string matchCode;

        public string startedAt;
        public string endedAt;

        public int durationSeconds;

        public int totalTrashSpawned;
        public int totalTrashRecycled;

        public List<MatchPlayerResultRequest> players;
        public List<UsedItemRequest> usedItems;
        
        public List<PlayerActionLogRequest> actionLogs;

        public CompleteMatchRequest(
            string matchCode,
            string startedAt,
            string endedAt,
            int durationSeconds,
            int totalTrashSpawned,
            int totalTrashRecycled,
            List<MatchPlayerResultRequest> players,
            List<UsedItemRequest> usedItems = null,
            List<PlayerActionLogRequest> actionLogs = null)
        {
            this.matchCode = matchCode;
            this.startedAt = startedAt;
            this.endedAt = endedAt;
            this.durationSeconds = durationSeconds;
            this.totalTrashSpawned = totalTrashSpawned;
            this.totalTrashRecycled = totalTrashRecycled;
            this.players = players;
            this.usedItems = usedItems ?? new List<UsedItemRequest>();
            this.actionLogs = actionLogs ?? new List<PlayerActionLogRequest>();
        }
    }
}