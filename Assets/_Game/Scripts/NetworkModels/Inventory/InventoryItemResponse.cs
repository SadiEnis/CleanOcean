using System;

namespace _Game.Scripts.NetworkModels.Inventory
{
    [Serializable]
    public class InventoryItemResponse
    {
        public ulong inventoryId;
        public ulong shopItemId;

        public string itemCode;
        public string itemName;
        public string itemType;

        public int quantity;
        public int maxQuantity;
    }
}