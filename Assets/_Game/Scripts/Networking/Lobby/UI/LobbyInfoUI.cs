using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyInfoUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyCodeText;
    [SerializeField] private Transform _lobbyPlayersContainer;
    [SerializeField] private Transform _lobbyPlayerItemTemplate;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _leaveLobbyButton;
    [SerializeField] private Button _deleteLobbyButton;

    private void Start()
    {
        LobbyEventsManager.Instance.OnJoinedLobby += LobbyInfoUI_OnJoinedLobby;
        LobbyEventsManager.Instance.OnUpdatedLobby += LobbyInfoUIOnUpdatedLobby;

        _leaveLobbyButton.onClick.AddListener(LobbyService.Instance.LeaveLobby);
        _deleteLobbyButton.onClick.AddListener(LobbyService.Instance.DeleteLobby);
        
        if (LobbyService.Instance != null && LobbyService.Instance.joinedLobby != null)
        {
            _lobbyNameText.text = LobbyService.Instance.joinedLobby.Name;
            _lobbyCodeText.text = LobbyService.Instance.joinedLobby.LobbyCode;
            UpdatePlayerList(LobbyService.Instance.joinedLobby);
        }

        RefreshHostControls();
    }
    private void OnDestroy()
    {
        if (LobbyEventsManager.Instance == null)
            return;

        LobbyEventsManager.Instance.OnJoinedLobby -= LobbyInfoUI_OnJoinedLobby;
        LobbyEventsManager.Instance.OnUpdatedLobby -= LobbyInfoUIOnUpdatedLobby;

        if (_startGameButton != null)
            _startGameButton.onClick.RemoveListener(OnStartGameClicked);

        if (_leaveLobbyButton != null && LobbyService.Instance != null)
            _leaveLobbyButton.onClick.RemoveListener(LobbyService.Instance.LeaveLobby);

        if (_deleteLobbyButton != null && LobbyService.Instance != null)
            _deleteLobbyButton.onClick.RemoveListener(LobbyService.Instance.DeleteLobby);
    }

    public override void OnNetworkSpawn()
    {
        RefreshHostControls();
    }

    private void Awake()
    {
        Debug.Log("[LobbyInfoUI] Awake");

        _startGameButton.onClick.RemoveListener(OnStartGameClicked);
        _startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void OnStartGameClicked()
    {
        Debug.Log("[LobbyInfoUI] Start button clicked.");

        bool canStartGame =
            LobbyService.Instance != null &&
            LobbyService.Instance.IsLocalPlayerLobbyHost() &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsHost;

        if (!canStartGame)
        {
            Debug.LogWarning("[LobbyInfoUI] Start blocked. Local player is not the active lobby/network host.");
            RefreshHostControls();
            return;
        }

        GameEventsManager.Instance.TriggerOnGameStarted();
        NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    private void LobbyInfoUIOnUpdatedLobby(object sender, LobbyEventsManager.LobbyEventsArgs e)
    {
        if (e.lobby != null)
            UpdatePlayerList(e.lobby);

        RefreshHostControls();
    }

    private void LobbyInfoUI_OnJoinedLobby(object sender, LobbyEventsManager.LobbyEventsArgs e)
    {
        if (e.lobby != null)
        {
            _lobbyNameText.text = e.lobby.Name;
            _lobbyCodeText.text = e.lobby.LobbyCode;

            UpdatePlayerList(e.lobby);
        }

        RefreshHostControls();
    }

    private void UpdatePlayerList(Lobby lobby)
    {
        foreach (Transform child in _lobbyPlayersContainer)
        {
            if (child == _lobbyPlayerItemTemplate) continue;

            Destroy(child.gameObject);
        }
        if (lobby.Players == null) return;
        foreach (Player player in lobby.Players)
        {
            Transform lobbyItemTransform = Instantiate(_lobbyPlayerItemTemplate, _lobbyPlayersContainer);
            lobbyItemTransform.gameObject.SetActive(true);

            LobbyInfoPlayerItem item = lobbyItemTransform.GetComponent<LobbyInfoPlayerItem>();

            string name = "unknown";
            if (player.Data != null && player.Data.ContainsKey("PlayerName"))
                name = player.Data["PlayerName"].Value;
            item.SetPlayerName(player.Data["PlayerName"].Value);
        }
    }
    
    private void RefreshHostControls()
    {
        bool isLobbyHost =
            LobbyService.Instance != null &&
            LobbyService.Instance.IsLocalPlayerLobbyHost();

        bool isNetworkHost =
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsHost;

        bool canStartGame = isLobbyHost && isNetworkHost;

        _startGameButton.gameObject.SetActive(isLobbyHost);
        _deleteLobbyButton.gameObject.SetActive(isLobbyHost);

        _startGameButton.interactable = canStartGame;

        Debug.Log(
            $"[LobbyInfoUI] RefreshHostControls | " +
            $"IsLobbyHost: {isLobbyHost}, " +
            $"IsNetworkHost: {isNetworkHost}, " +
            $"CanStartGame: {canStartGame}"
        );
    }

    public void Hide() => gameObject.SetActive(false);
    public void Show()
    {
        gameObject.SetActive(true);
        RefreshHostControls();
    }
}