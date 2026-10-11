using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Auth;
using _Game.Scripts.Services.Api;
using UnityEngine;

namespace _Game.Scripts.Services.Auth
{
    public class AuthApiClient
    {
        private readonly ApiClient _apiClient;

        public AuthApiClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<AuthResponse> RegisterAsync(string username, string password, string displayName)
        {
            var requestModel = new RegisterRequest(username, password, displayName);
            
            string json = JsonUtility.ToJson(requestModel);
            string responseJson  = await _apiClient.PostJsonAsync("/api/Auth/register", json);
            
            return JsonUtility.FromJson<AuthResponse>(responseJson);
        }
        
        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            var requestModel = new LoginRequest(username, password);
            
            string json = JsonUtility.ToJson(requestModel);
            string responseJson = await _apiClient.PostJsonAsync("/api/Auth/login", json);
            
            return JsonUtility.FromJson<AuthResponse>(responseJson);
        }
    }
    
}