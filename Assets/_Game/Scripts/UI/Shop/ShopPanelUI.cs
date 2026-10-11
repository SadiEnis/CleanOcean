using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Shop;
using _Game.Scripts.Services.Player;
using _Game.Scripts.Services.Shop;
using _Game.Scripts.Session;
using _Game.Scripts.UI.MainHub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Shop
{
    public class ShopPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Transform _shopItemContainer;
    [SerializeField] private ShopItemUI _shopItemPrefab;
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Optional")]
    [SerializeField] private MainHubUI _mainHubUI;

    private readonly List<ShopItemUI> _spawnedItems = new();

    private ShopApiClient _shopApiClient;
    private PlayerApiClient _playerApiClient;

    private bool _isLoading;

    private void Awake()
    {
        _shopApiClient = new ShopApiClient();
        _playerApiClient = new PlayerApiClient();

        _refreshButton.onClick.AddListener(() =>
        {
            _ = LoadShopItemsAsync();
        });
    }

    private void OnEnable()
    {
        _ = LoadShopItemsAsync();
    }

    private async Task LoadShopItemsAsync()
    {
        if (_isLoading)
            return;

        _isLoading = true;
        SetMessage("Loading shop...");

        try
        {
            ClearItems();

            var response = await _shopApiClient.GetShopItemsAsync();

            if (!response.success)
            {
                SetMessage(response.message);
                return;
            }

            if (response.items == null || response.items.Count == 0)
            {
                SetMessage("No shop items found.");
                return;
            }

            foreach (ShopItemResponse item in response.items)
            {
                ShopItemUI itemUI = Instantiate(_shopItemPrefab, _shopItemContainer);
                itemUI.gameObject.SetActive(true);
                itemUI.Initialize(item, OnPurchaseClicked);

                _spawnedItems.Add(itemUI);
            }

            SetMessage(string.Empty);
        }
        catch (Exception exception)
        {
            SetMessage("Failed to load shop items.");
            Debug.LogError($"Shop load failed: {exception.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void OnPurchaseClicked(ShopItemResponse item)
    {
        if (!CurrentUser.IsLoggedIn)
        {
            SetMessage("You must be logged in.");
            return;
        }

        SetAllPurchaseButtons(false);
        SetMessage($"Purchasing {item.itemName}...");

        try
        {
            var purchaseResponse = await _shopApiClient.PurchaseItemAsync(
                CurrentUser.UserId,
                item.shopItemId,
                1
            );

            if (!purchaseResponse.success)
            {
                SetMessage(purchaseResponse.message);
                return;
            }

            SetMessage($"{item.itemName} purchased.");

            await RefreshProfileAsync();

            await LoadShopItemsAsync();
        }
        catch (Exception exception)
        {
            SetMessage("Purchase failed.");
            Debug.LogError($"Purchase failed: {exception.Message}");
        }
        finally
        {
            SetAllPurchaseButtons(true);
        }
    }

    private async Task RefreshProfileAsync()
    {
        var profileResponse = await _playerApiClient.GetProfileAsync(CurrentUser.UserId);

        if (!profileResponse.success)
        {
            Debug.LogWarning($"Profile refresh failed: {profileResponse.message}");
            return;
        }

        CurrentUser.SetProfile(profileResponse);

        if (_mainHubUI != null)
        {
            _mainHubUI.RefreshTopBar();
        }
    }

    private void ClearItems()
    {
        foreach (ShopItemUI item in _spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        _spawnedItems.Clear();
    }

    private void SetAllPurchaseButtons(bool value)
    {
        foreach (ShopItemUI item in _spawnedItems)
        {
            if (item != null)
                item.SetPurchaseInteractable(value);
        }
    }

    private void SetMessage(string message)
    {
        if (_messageText != null)
            _messageText.text = message;
    }
}
}