using System;

namespace _Game.Scripts.NetworkModels.Shop
{
    [Serializable]
    public class ShopItemResponse
    {
        public ulong shopItemId;

        public string itemCode;
        public string itemName;
        public string itemDescription;
        public string itemType;

        public int priceSoftCurrency;
        public int maxQuantity;

        public bool isActive;
    }
}