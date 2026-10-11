using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Shop;
using _Game.Scripts.Services.Api;
using UnityEngine;

namespace _Game.Scripts.Services.Shop
{
    public class ShopApiClient
    {
        private readonly ApiClient _apiClient;

        public ShopApiClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<ShopItemsResponse> GetShopItemsAsync()
        {
            string responseJson = await _apiClient.GetAsync("/api/Shop/items");
            return JsonUtility.FromJson<ShopItemsResponse>(responseJson);
        }

        public async Task<PurchaseItemResponse> PurchaseItemAsync(ulong userId, ulong shopItemId, int quantity)
        {
            var requestModel = new PurchaseItemRequest(userId, shopItemId, quantity);
            string json = JsonUtility.ToJson(requestModel);

            Debug.Log($"Purchase Request JSON: {json}");

            string responseJson = await _apiClient.PostJsonAsync("/api/Shop/purchase", json);

            return JsonUtility.FromJson<PurchaseItemResponse>(responseJson);
        }
    }
}