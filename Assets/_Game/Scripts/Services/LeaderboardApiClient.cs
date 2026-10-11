using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Leaderboard;
using _Game.Scripts.Services.Api;
using UnityEngine;

namespace _Game.Scripts.Services.Leaderboard
{
    public class LeaderboardApiClient
    {
        private readonly ApiClient _apiClient;

        public LeaderboardApiClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<LeaderboardResponse> GetLeaderboardAsync(int limit = 10)
        {
            string responseJson = await _apiClient.GetAsync($"/api/leaderboard?limit={limit}");

            Debug.Log($"[LeaderboardApiClient] Response JSON: {responseJson}");

            return JsonUtility.FromJson<LeaderboardResponse>(responseJson);
        }
    }
}