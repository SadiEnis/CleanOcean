using System;

namespace _Game.Scripts.NetworkModels.Shop
{
    [Serializable]
    public class PurchaseItemResponse
    {
        public bool success;
        public string message;

        public long userId;
        public long shopItemId;

        public int purchasedQuantity;
        public int newBalance;
    }
}