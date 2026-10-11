using System;
using System.Collections.Generic;

namespace _Game.Scripts.NetworkModels.Shop
{[Serializable]
    public class ShopItemsResponse
    {
        public bool success;
        public string message;

        public List<ShopItemResponse> items;
    }
}