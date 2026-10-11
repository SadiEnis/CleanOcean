using System;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Auth;
using _Game.Scripts.NetworkModels.Player;
using _Game.Scripts.Services.Auth;
using _Game.Scripts.Services.Player;
using _Game.Scripts.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Networking.Lobby.UI
{
    public class AuthUI : BaseUIComponent
    {
        [Header("Root Panels")] [SerializeField]
        private GameObject _loginPanel;

        [SerializeField] private GameObject _registerPanel;

        [Header("Login")] [SerializeField] private TMP_InputField _loginUsernameInput;
        [SerializeField] private TMP_InputField _loginPasswordInput;
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _openRegisterButton;

        [Header("Register")] [SerializeField] private TMP_InputField _registerUsernameInput;
        [SerializeField] private TMP_InputField _registerDisplayNameInput;
        [SerializeField] private TMP_InputField _registerPasswordInput;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _openLoginButton;

        [Header("Common")] [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _messageText;

        private AuthApiClient _authApiClient;
        private PlayerApiClient _playerApiClient;

        private void Awake()
        {
            _authApiClient = new AuthApiClient();
            _playerApiClient = new PlayerApiClient();

            _loginButton.onClick.AddListener(OnLoginClicked);
            _registerButton.onClick.AddListener(OnRegisterClicked);

            _openRegisterButton.onClick.AddListener(ShowRegisterPanel);
            _openLoginButton.onClick.AddListener(ShowLoginPanel);

            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            ShowLoginPanel();
            SetMessage(string.Empty);
        }

        private void ShowLoginPanel()
        {
            _loginPanel.SetActive(true);
            _registerPanel.SetActive(false);
            SetMessage(string.Empty);
        }

        private void ShowRegisterPanel()
        {
            _loginPanel.SetActive(false);
            _registerPanel.SetActive(true);
            SetMessage(string.Empty);
        }

        private async void OnLoginClicked()
        {
            string username = _loginUsernameInput.text.Trim();
            string password = _loginPasswordInput.text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                SetMessage("Username and password are required.");
                return;
            }

            SetInteractable(false);
            SetMessage("Logging in...");

            try
            {
                AuthResponse response = await _authApiClient.LoginAsync(username, password);

                if (!response.success)
                {
                    SetMessage(response.message);
                    return;
                }

                await CompleteAuthFlow(response);
            }
            catch (Exception exception)
            {
                SetMessage("Login failed.");
                Debug.LogError($"Login failed: {exception.Message}");
            }
            finally
            {
                SetInteractable(true);
            }
        }

        private async void OnRegisterClicked()
        {
            string username = _registerUsernameInput.text.Trim();
            string displayName = _registerDisplayNameInput.text.Trim();
            string password = _registerPasswordInput.text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(password))
            {
                SetMessage("Username, display name and password are required.");
                return;
            }

            SetInteractable(false);
            SetMessage("Registering...");

            try
            {
                AuthResponse response = await _authApiClient.RegisterAsync(username, password, displayName);

                if (!response.success)
                {
                    SetMessage(response.message);
                    return;
                }

                await CompleteAuthFlow(response);
            }
            catch (Exception exception)
            {
                SetMessage("Register failed.");
                Debug.LogError($"Register failed: {exception.Message}");
            }
            finally
            {
                SetInteractable(true);
            }
        }

        private async Task CompleteAuthFlow(AuthResponse authResponse)
        {
            CurrentUser.Set(authResponse.userId, authResponse.username, authResponse.displayName);

            PlayerProfileResponse profileResponse = await _playerApiClient.GetProfileAsync(CurrentUser.UserId);

            if (profileResponse.success)
            {
                CurrentUser.SetProfile(profileResponse);
            }
            else
            {
                Debug.LogWarning($"Profile could not be loaded: {profileResponse.message}");
            }

            LobbyEventsManager.Instance.TriggerOnNameChanged(new LobbyEventsManager.OnNameChangedArgs
            {
                playerName = CurrentUser.DisplayName
            });

            Hide();
        }

        private void OnBackClicked()
        {
            Hide();
            LobbyEventsManager.Instance.TriggerOnReturnToMainMenu();
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
                _messageText.text = message;
        }

        private void SetInteractable(bool value)
        {
            _loginButton.interactable = value;
            _registerButton.interactable = value;
            _openRegisterButton.interactable = value;
            _openLoginButton.interactable = value;
            _backButton.interactable = value;
        }
    }
}