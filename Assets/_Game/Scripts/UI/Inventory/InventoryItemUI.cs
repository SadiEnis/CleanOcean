using _Game.Scripts.NetworkModels.Inventory;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Inventory
{
    public class InventoryItemUI : MonoBehaviour
    {
        [Header("Texts")] [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemTypeText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private TextMeshProUGUI _statusText;

        public void Initialize(InventoryItemResponse item)
        {
            _itemNameText.text = item.itemName;
            _itemTypeText.text = $"Type: {item.itemType}";
            _quantityText.text = $"Quantity: {item.quantity}/{item.maxQuantity}";
            _statusText.text = GetStatusText(item);
        }

        private string GetStatusText(InventoryItemResponse item)
        {
            return item.itemType switch
            {
                "consumable" => "Consumable item",
                "passive" => "Passive owned",
                "equipment" => "Equipment available",
                _ => "Owned"
            };
        }
    }
}
