using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Matches;
using _Game.Scripts.Services.Api;
using UnityEngine;

namespace _Game.Scripts.Services.Matches
{
    public class MatchApiClient
    {
        private readonly ApiClient _apiClient;

        public MatchApiClient()
        {
            _apiClient = new ApiClient();
        }
        
        public async Task<CompleteMatchResponse> CompleteMatchAsync(CompleteMatchRequest request)
        {
            string json = JsonUtility.ToJson(request);

            Debug.Log($"Complete Match Request JSON: {json}");

            string responseJson = await _apiClient.PostJsonAsync("/api/Match/complete", json);

            return JsonUtility.FromJson<CompleteMatchResponse>(responseJson);
        }
    }
}