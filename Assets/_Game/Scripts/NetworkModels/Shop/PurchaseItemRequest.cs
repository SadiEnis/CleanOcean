using System;

namespace _Game.Scripts.NetworkModels.Shop
{
    [Serializable]
    public class PurchaseItemRequest
    {
        public ulong userId;
        public ulong shopItemId;
        public int quantity;

        public PurchaseItemRequest(ulong userId, ulong shopItemId, int quantity)
        {
            this.userId = userId;
            this.shopItemId = shopItemId;
            this.quantity = quantity;
        }
    }
}