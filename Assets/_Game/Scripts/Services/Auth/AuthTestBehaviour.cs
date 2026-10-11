using System;
using _Game.Scripts.NetworkModels.Auth;
using _Game.Scripts.Session;
using _Game.Scripts.Services.Player;
using UnityEngine;

namespace _Game.Scripts.Services.Auth
{
    public class AuthTestBehaviour : MonoBehaviour
    {
        [Header("Test Credentials")]
        [SerializeField] private string username = "user_test_1";
        [SerializeField] private string password = "123456";
        [SerializeField] private string displayName = "Test User";

        [Header("Test Options")]
        [SerializeField] private bool registerOnStart;
        [SerializeField] private bool loginOnStart = true;

        private AuthApiClient _authApiClient;

        private async void Start()
        {
            _authApiClient = new AuthApiClient();

            try
            {
                if (registerOnStart)
                {
                    AuthResponse registerResponse = await _authApiClient.RegisterAsync(username, password, displayName);
                    Debug.Log($"Register Response: success={registerResponse.success}, message={registerResponse.message}, userId={registerResponse.userId}");
                }

                if (loginOnStart)
                {
                    AuthResponse loginResponse = await _authApiClient.LoginAsync(username, password);

                    Debug.Log($"Login Response: success={loginResponse.success}, message={loginResponse.message}, userId={loginResponse.userId}, username={loginResponse.username}, displayName={loginResponse.displayName}");

                    if (loginResponse.success)
                    {
                        CurrentUser.Set(loginResponse.userId, loginResponse.username, loginResponse.displayName);

                        Debug.Log($"CurrentUser Set → UserId={CurrentUser.UserId}, Username={CurrentUser.Username}, DisplayName={CurrentUser.DisplayName}");

                        var playerApiClient = new PlayerApiClient();
                        var profileResponse = await playerApiClient.GetProfileAsync(CurrentUser.UserId);

                        if (profileResponse.success)
                        {
                            CurrentUser.SetProfile(profileResponse);

                            Debug.Log(
                                $"Profile Loaded → Currency={CurrentUser.SoftCurrency}, " +
                                $"TotalScore={CurrentUser.TotalScore}, " +
                                $"Matches={CurrentUser.TotalMatchesPlayed}, " +
                                $"TrashRecycled={CurrentUser.TotalTrashRecycled}"
                            );
                        }
                        else
                        {
                            Debug.LogWarning($"Profile load failed: {profileResponse.message}");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Auth test failed: {exception.Message}");
            }
        }
    }
}