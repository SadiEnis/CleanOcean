using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Player;
using _Game.Scripts.Services.Api;
using UnityEngine;

namespace _Game.Scripts.Services.Player
{
    public class PlayerApiClient
    {
        private readonly ApiClient _apiClient;

        public PlayerApiClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<PlayerProfileResponse> GetProfileAsync(ulong userId)
        {
            string responseJson = await _apiClient.GetAsync($"/api/Player/profile/{userId}");
            return JsonUtility.FromJson<PlayerProfileResponse>(responseJson);
        }
    }
}