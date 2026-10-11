using System;
using UnityEngine;

namespace _Game.Scripts.Services.Api
{
    public class ApiTestBehaviour : MonoBehaviour
    {
        private async void Start()
        {
            var apiClient = new ApiClient();

            try
            {
                string response = await apiClient.GetAsync("/api/health");
                Debug.Log($"API Health Response: {response}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"API Health Request Failed: {exception.Message}");
            }
        }
    }
}