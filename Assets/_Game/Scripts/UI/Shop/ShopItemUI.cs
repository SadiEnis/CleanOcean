using System;
using _Game.Scripts.NetworkModels.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace _Game.Scripts.UI.Shop
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("Texts")] [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemTypeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _maxQuantityText;

        [Header("Button")] [SerializeField] private Button _purchaseButton;

        private ShopItemResponse _item;
        private Action<ShopItemResponse> _onPurchaseClicked;

        public void Initialize(ShopItemResponse item, Action<ShopItemResponse> onPurchaseClicked)
        {
            _item = item;
            _onPurchaseClicked = onPurchaseClicked;

            RefreshView();

            _purchaseButton.onClick.RemoveAllListeners();
            _purchaseButton.onClick.AddListener(() => { _onPurchaseClicked?.Invoke(_item); });
        }

        private void RefreshView()
        {
            _itemNameText.text = _item.itemName;
            _itemTypeText.text = $"Type: {_item.itemType}";
            _descriptionText.text = _item.itemDescription;
            _priceText.text = $"Price: {_item.priceSoftCurrency}";
            _maxQuantityText.text = $"Max: {_item.maxQuantity}";
        }

        public void SetPurchaseInteractable(bool value)
        {
            _purchaseButton.interactable = value;
        }
    }
}
