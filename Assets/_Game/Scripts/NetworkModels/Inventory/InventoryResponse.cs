using System;
using System.Collections.Generic;

namespace _Game.Scripts.NetworkModels.Inventory
{
    [Serializable]
    public class InventoryResponse
    {
        public bool success;
        public string message;

        public ulong userId;

        public List<InventoryItemResponse> items;
    }
}