using System;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using _Game.Scripts.Services.Api;
using UnityEngine.Networking;

namespace _Game.Scripts.Services.Api
{
    public class ApiClient
    {
        public async Task<string> GetAsync(string endpoint)
        {
            string url = $"{ApiConfig.BaseUrl}{endpoint}";

            using UnityWebRequest request = UnityWebRequest.Get(url);

            await SendRequestAsync(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"GET request failed: {request.error} | URL: {url}");
            }

            return request.downloadHandler.text;
        }

        public async Task<string> PostJsonAsync(string endpoint, string json)
        {
            string url = $"{ApiConfig.BaseUrl}{endpoint}";

            using UnityWebRequest request = new UnityWebRequest(url, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await SendRequestAsync(request);

            string responseText = request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (!string.IsNullOrWhiteSpace(responseText))
                {
                    Debug.LogWarning(
                        $"POST request returned error status but has response body. " +
                        $"StatusCode={request.responseCode}, URL={url}, Response={responseText}"
                    );

                    return responseText;
                }

                throw new Exception($"POST request failed: {request.error} | URL: {url}");
            }

            return responseText;
        }

        private static Task SendRequestAsync(UnityWebRequest request)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                taskCompletionSource.SetResult(true);
            };

            return taskCompletionSource.Task;
        }
    }
}