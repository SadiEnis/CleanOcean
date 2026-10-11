using System;
using System.Collections.Generic;
using _Game.Scripts.NetworkModels.Matches;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Match
{
    public class MatchActionLogTracker : MonoSingleton<MatchActionLogTracker>
    {
        private readonly List<PlayerActionLogRequest> _logs = new();

        public IReadOnlyList<PlayerActionLogRequest> Logs => _logs;

        public void Clear()
        {
            _logs.Clear();
        }

        public void RecordRescueCompleted(ulong userId, int scoreReward, Vector2 position)
        {
            if (userId <= 0)
            {
                Debug.LogWarning("MatchActionLogTracker: Rescue completed log skipped. Invalid userId.");
                return;
            }

            _logs.Add(new PlayerActionLogRequest(
                userId: userId,
                actionType: "rescue_completed",
                trashTypeId: 0,
                targetUserId: 0,
                itemCode: string.Empty,
                value: scoreReward,
                posX: position.x,
                posY: position.y,
                createdAt: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
            ));
        }
        
        public void RecordPlayerFainted(ulong userId, Vector2 position)
        {
            if (userId <= 0)
            {
                Debug.LogWarning("MatchActionLogTracker: Player fainted log skipped. Invalid userId.");
                return;
            }

            _logs.Add(new PlayerActionLogRequest(
                userId: userId,
                actionType: "player_fainted",
                trashTypeId: 0,
                targetUserId: 0,
                itemCode: string.Empty,
                value: 1,
                posX: position.x,
                posY: position.y,
                createdAt: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
            ));
        }
        
        public void RecordPlayerRevived(ulong reviverUserId, ulong revivedUserId, int rewardValue, Vector2 position)
        {
            if (reviverUserId <= 0)
            {
                Debug.LogWarning("MatchActionLogTracker: Player revived log skipped. Invalid reviverUserId.");
                return;
            }

            if (revivedUserId <= 0)
            {
                Debug.LogWarning("MatchActionLogTracker: Player revived log skipped. Invalid revivedUserId.");
                return;
            }

            _logs.Add(new PlayerActionLogRequest(
                userId: reviverUserId,
                actionType: "revive_player",
                trashTypeId: 0,
                targetUserId: revivedUserId,
                itemCode: string.Empty,
                value: rewardValue,
                posX: position.x,
                posY: position.y,
                createdAt: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
            ));
        }

        public List<PlayerActionLogRequest> CreateSnapshot()
        {
            return new List<PlayerActionLogRequest>(_logs);
        }
    }
}