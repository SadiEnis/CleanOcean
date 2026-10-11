using System;

namespace _Game.Scripts.NetworkModels.Matches
{
    [Serializable]
    public class PlayerActionLogRequest
    {
        public ulong userId;
        public string actionType;

        public ulong trashTypeId;
        public ulong targetUserId;

        public string itemCode;

        public int value;

        public float posX;
        public float posY;

        public string createdAt;

        public PlayerActionLogRequest(
            ulong userId,
            string actionType,
            ulong trashTypeId,
            ulong targetUserId,
            string itemCode,
            int value,
            float posX,
            float posY,
            string createdAt)
        {
            this.userId = userId;
            this.actionType = actionType;
            this.trashTypeId = trashTypeId;
            this.targetUserId = targetUserId;
            this.itemCode = itemCode ?? string.Empty;
            this.value = value;
            this.posX = posX;
            this.posY = posY;
            this.createdAt = createdAt;
        }
    }
}