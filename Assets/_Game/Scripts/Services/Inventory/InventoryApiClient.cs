using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Inventory;
using _Game.Scripts.Services.Api;
using UnityEngine;

namespace _Game.Scripts.Services.Inventory
{
    public class InventoryApiClient
    {
        private readonly ApiClient _apiClient;

        public InventoryApiClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<InventoryResponse> GetInventoryAsync(ulong userId)
        {
            string responseJson = await _apiClient.GetAsync($"/api/Inventory/{userId}");

            Debug.Log($"Inventory Response JSON: {responseJson}");

            return JsonUtility.FromJson<InventoryResponse>(responseJson);
        }
    }
}