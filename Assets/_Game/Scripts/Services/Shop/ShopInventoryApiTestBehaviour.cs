using System;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Inventory;
using _Game.Scripts.NetworkModels.Shop;
using _Game.Scripts.Services.Inventory;
using _Game.Scripts.Services.Player;
using _Game.Scripts.Session;
using UnityEngine;

namespace _Game.Scripts.Services.Shop
{
    public class ShopInventoryApiTestBehaviour : MonoBehaviour
    {
        [Header("Test Purchase")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private int delayMilliseconds = 1500;

        [Tooltip("0 bırakılırsa shop listesindeki ilk item satın alınır.")]
        [SerializeField] private ulong targetShopItemId = 0;

        [SerializeField] private int quantity = 1;

        private ShopApiClient _shopApiClient;
        private InventoryApiClient _inventoryApiClient;
        private PlayerApiClient _playerApiClient;

        private async void Start()
        {
            if (!runOnStart)
                return;

            await Task.Delay(delayMilliseconds);

            if (!CurrentUser.IsLoggedIn)
            {
                Debug.LogError("ShopInventoryApiTest failed: CurrentUser is not logged in. Run AuthTest first.");
                return;
            }

            _shopApiClient = new ShopApiClient();
            _inventoryApiClient = new InventoryApiClient();
            _playerApiClient = new PlayerApiClient();

            try
            {
                await RunTestAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError($"ShopInventoryApiTest failed: {exception.Message}");
            }
        }

        private async Task RunTestAsync()
        {
            Debug.Log($"ShopInventoryApiTest started. CurrentUserId={CurrentUser.UserId}");

            ShopItemsResponse shopResponse = await _shopApiClient.GetShopItemsAsync();

            if (!shopResponse.success)
            {
                Debug.LogError($"Shop items request failed: {shopResponse.message}");
                return;
            }

            Debug.Log($"Shop Items Loaded → Count={shopResponse.items.Count}");

            foreach (var item in shopResponse.items)
            {
                Debug.Log(
                    $"Shop Item → Id={item.shopItemId}, Code={item.itemCode}, " +
                    $"Name={item.itemName}, Type={item.itemType}, " +
                    $"Price={item.priceSoftCurrency}, Max={item.maxQuantity}"
                );
            }

            InventoryResponse inventoryBefore = await _inventoryApiClient.GetInventoryAsync(CurrentUser.UserId);

            Debug.Log($"Inventory Before Purchase → Count={inventoryBefore.items.Count}");
            LogInventoryItems(inventoryBefore);

            ulong selectedItemId = ResolveTargetItemId(shopResponse);

            if (selectedItemId <= 0)
            {
                Debug.LogError("No valid shop item selected for purchase.");
                return;
            }

            PurchaseItemResponse purchaseResponse =
                await _shopApiClient.PurchaseItemAsync(CurrentUser.UserId, selectedItemId, quantity);

            Debug.Log(
                $"Purchase Response → success={purchaseResponse.success}, " +
                $"message={purchaseResponse.message}, " +
                $"shopItemId={purchaseResponse.shopItemId}, " +
                $"newBalance={purchaseResponse.newBalance}"
            );

            InventoryResponse inventoryAfter = await _inventoryApiClient.GetInventoryAsync(CurrentUser.UserId);

            Debug.Log($"Inventory After Purchase → Count={inventoryAfter.items.Count}");
            LogInventoryItems(inventoryAfter);

            var profileResponse = await _playerApiClient.GetProfileAsync(CurrentUser.UserId);

            if (profileResponse.success)
            {
                CurrentUser.SetProfile(profileResponse);

                Debug.Log(
                    $"Profile Refreshed → Currency={CurrentUser.SoftCurrency}, " +
                    $"TotalScore={CurrentUser.TotalScore}, " +
                    $"Matches={CurrentUser.TotalMatchesPlayed}"
                );
            }
            else
            {
                Debug.LogWarning($"Profile refresh failed: {profileResponse.message}");
            }
        }

        private ulong ResolveTargetItemId(ShopItemsResponse shopResponse)
        {
            if (targetShopItemId > 0)
                return targetShopItemId;

            if (shopResponse.items == null || shopResponse.items.Count == 0)
                return 0;

            return shopResponse.items[0].shopItemId;
        }

        private void LogInventoryItems(InventoryResponse inventory)
        {
            if (inventory.items == null || inventory.items.Count == 0)
            {
                Debug.Log("Inventory is empty.");
                return;
            }

            foreach (var item in inventory.items)
            {
                Debug.Log(
                    $"Inventory Item → InventoryId={item.inventoryId}, " +
                    $"ShopItemId={item.shopItemId}, Code={item.itemCode}, " +
                    $"Name={item.itemName}, Quantity={item.quantity}/{item.maxQuantity}"
                );
            }
        }
    }
}