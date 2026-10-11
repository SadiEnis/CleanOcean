using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Inventory;
using _Game.Scripts.Services.Inventory;
using _Game.Scripts.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Inventory
{
    public class InventoryPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Transform _inventoryItemContainer;
    [SerializeField] private InventoryItemUI _inventoryItemPrefab;
    [SerializeField] private TextMeshProUGUI _messageText;

    private readonly List<InventoryItemUI> _spawnedItems = new();

    private InventoryApiClient _inventoryApiClient;
    private bool _isLoading;

    private void Awake()
    {
        _inventoryApiClient = new InventoryApiClient();

        _refreshButton.onClick.AddListener(() =>
        {
            _ = LoadInventoryAsync();
        });
    }

    private void OnEnable()
    {
        _ = LoadInventoryAsync();
    }

    public async Task LoadInventoryAsync()
    {
        if (_isLoading)
            return;

        if (!CurrentUser.IsLoggedIn)
        {
            SetMessage("You must be logged in.");
            return;
        }

        _isLoading = true;
        SetMessage("Loading inventory...");

        try
        {
            ClearItems();

            InventoryResponse response = await _inventoryApiClient.GetInventoryAsync(CurrentUser.UserId);

            if (!response.success)
            {
                SetMessage(response.message);
                return;
            }

            if (response.items == null || response.items.Count == 0)
            {
                SetMessage("Inventory is empty.");
                return;
            }

            foreach (InventoryItemResponse item in response.items)
            {
                InventoryItemUI itemUI = Instantiate(_inventoryItemPrefab, _inventoryItemContainer);
                itemUI.gameObject.SetActive(true);
                itemUI.Initialize(item);

                _spawnedItems.Add(itemUI);
            }

            SetMessage(string.Empty);
        }
        catch (Exception exception)
        {
            SetMessage("Failed to load inventory.");
            Debug.LogError($"Inventory load failed: {exception.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ClearItems()
    {
        foreach (InventoryItemUI item in _spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        _spawnedItems.Clear();
    }

    private void SetMessage(string message)
    {
        if (_messageText != null)
            _messageText.text = message;
    }
}
}
