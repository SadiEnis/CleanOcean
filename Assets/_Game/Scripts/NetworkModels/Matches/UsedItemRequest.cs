using System;

namespace _Game.Scripts.NetworkModels.Matches
{
    [Serializable]
    public class UsedItemRequest
    {
        public ulong userId;
        public string itemCode;
        public int quantity;

        public UsedItemRequest(ulong userId, string itemCode, int quantity)
        {
            this.userId = userId;
            this.itemCode = itemCode;
            this.quantity = quantity;
        }
    }
}