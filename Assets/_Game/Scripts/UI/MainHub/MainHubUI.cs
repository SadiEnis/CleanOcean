using System;
using System.Threading.Tasks;
using _Game.Scripts.NetworkModels.Player;
using _Game.Scripts.Services.Player;
using _Game.Scripts.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Authentication;

namespace _Game.Scripts.UI.MainHub
{
    public class MainHubUI : BaseUIComponent
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private Button _logoutButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _lobbyButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _settingsButton;

        [Header("Content Panels")]
        [SerializeField] private GameObject _lobbyContentPanel;
        [SerializeField] private GameObject _shopContentPanel;
        [SerializeField] private GameObject _inventoryContentPanel;
        [SerializeField] private GameObject _settingsContentPanel;

        private PlayerApiClient _playerApiClient;
        private bool _isRefreshingProfile;

        private void Awake()
        {
            _playerApiClient = new PlayerApiClient();

            _lobbyButton.onClick.AddListener(ShowLobbyPanel);
            _shopButton.onClick.AddListener(ShowShopPanel);
            _inventoryButton.onClick.AddListener(ShowInventoryPanel);
            _settingsButton.onClick.AddListener(ShowSettingsPanel);

            _logoutButton.onClick.AddListener(OnLogoutClicked);
        }

        public override void Show()
        {
            base.Show();

            ShowLobbyPanel();
            RefreshUserInfo();
        }

        public void RefreshTopBar()
        {
            if (_displayNameText != null)
                _displayNameText.text = CurrentUser.DisplayName;

            if (_currencyText != null)
                _currencyText.text = $"Para: {CurrentUser.SoftCurrency}";
        }

        public void RefreshUserInfo()
        {
            RefreshTopBar();
            _ = RefreshProfileAndTopBarAsync();
        }

        private async Task RefreshProfileAndTopBarAsync()
        {
            if (_isRefreshingProfile)
                return;

            if (!CurrentUser.IsLoggedIn || CurrentUser.UserId == 0)
                return;

            _isRefreshingProfile = true;

            try
            {
                PlayerProfileResponse profile =
                    await _playerApiClient.GetProfileAsync(CurrentUser.UserId);

                if (profile == null)
                {
                    Debug.LogWarning("[MainHubUI] Profile response is null.");
                    return;
                }

                if (!profile.success)
                {
                    Debug.LogWarning($"[MainHubUI] Profile refresh failed: {profile.message}");
                    return;
                }

                CurrentUser.SetProfile(profile);
                RefreshTopBar();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MainHubUI] Failed to refresh profile: {exception.Message}");
            }
            finally
            {
                _isRefreshingProfile = false;
            }
        }

        public void ShowLobbyPanel()
        {
            gameObject.SetActive(true);

            _shopContentPanel.SetActive(false);
            _inventoryContentPanel.SetActive(false);
            _settingsContentPanel.SetActive(false);
            _lobbyContentPanel.SetActive(true);

            if (HasActiveLobby())
            {
                LobbyUI.Instance.listLobbiesUI.Hide();
                LobbyUI.Instance.createLobbyUI.Hide();
                LobbyUI.Instance.lobbyInfoUI.Show();
            }
            else
            {
                LobbyUI.Instance.lobbyInfoUI.Hide();
                LobbyUI.Instance.createLobbyUI.Hide();
                LobbyUI.Instance.listLobbiesUI.Show();
            }
        }

        private void ShowShopPanel()
        {
            HideLobbySubPanels();
            SetActivePanel(_shopContentPanel);
        }

        private void ShowInventoryPanel()
        {
            HideLobbySubPanels();
            SetActivePanel(_inventoryContentPanel);
        }

        private void ShowSettingsPanel()
        {
            HideLobbySubPanels();
            SetActivePanel(_settingsContentPanel);
        }

        private void HideLobbySubPanels()
        {
            if (LobbyUI.Instance == null)
                return;

            LobbyUI.Instance.listLobbiesUI.Hide();
            LobbyUI.Instance.createLobbyUI.Hide();
            LobbyUI.Instance.lobbyInfoUI.Hide();
        }

        private void SetActivePanel(GameObject targetPanel)
        {
            _lobbyContentPanel.SetActive(false);
            _shopContentPanel.SetActive(false);
            _inventoryContentPanel.SetActive(false);
            _settingsContentPanel.SetActive(false);

            targetPanel.SetActive(true);
        }

        private async void OnLogoutClicked()
{
    _logoutButton.interactable = false;

    try
    {
        if (LobbyService.Instance != null)
        {
            await LobbyService.Instance.LeaveLobbyAsync(triggerUiEvents: false);
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
{
    NetworkManager.Singleton.Shutdown();

    while (NetworkManager.Singleton.IsListening)
    {
        await Task.Yield();
    }

    Debug.Log("[MainHubUI] NetworkManager shutdown completed.");
}

if (AuthenticationService.Instance.IsSignedIn)
{
    AuthenticationService.Instance.SignOut();
    Debug.Log("[MainHubUI] Unity Authentication signed out.");
}
        CurrentUser.Clear();

        Hide();

        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.ShowMainMenuState();
        }
        else
        {
            LobbyEventsManager.Instance.TriggerOnReturnToMainMenu();
        }
    }
    catch (Exception exception)
    {
        Debug.LogError($"[MainHubUI] Logout failed: {exception.Message}");
    }
    finally
    {
        _logoutButton.interactable = true;
    }
}

        private bool HasActiveLobby()
        {
            return LobbyService.Instance.joinedLobby != null;
        }
    }
}