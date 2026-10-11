using System;
using _Game.Scripts.Networking.Lobby.UI;
using _Game.Scripts.UI.MainHub;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoSingleton<LobbyUI>
{
    [SerializeField] private Transform _authUI;
    [SerializeField] private Transform _mainHubUI;
    [SerializeField] private Transform _listLobbiesUI;
    [SerializeField] private Transform _createLobbyUI;
    [SerializeField] private Transform _lobbyInfoUI;
	[SerializeField] private Transform _mainMenuUI;

    [NonSerialized] public AuthUI authUI;
    [NonSerialized] public MainHubUI mainHubUI;
    [NonSerialized] public LobbiesListUI listLobbiesUI;
    [NonSerialized] public CreateLobbyUI createLobbyUI;
    [NonSerialized] public LobbyInfoUI lobbyInfoUI;
	[NonSerialized] public MainMenuUIManager mainMenuUI;

    private void Awake()
    {
        authUI = _authUI.GetComponent<AuthUI>();
        mainHubUI = _mainHubUI.GetComponent<MainHubUI>();
        createLobbyUI = _createLobbyUI.GetComponent<CreateLobbyUI>();
        listLobbiesUI = _listLobbiesUI.GetComponent<LobbiesListUI>();
        lobbyInfoUI = _lobbyInfoUI.GetComponent<LobbyInfoUI>();
		mainMenuUI = _mainMenuUI.GetComponent<MainMenuUIManager>();
    }

    private void Start()
    {
        HideAllPanels();

        LobbyEventsManager.Instance.OnPressPlayButton += LobbyUI_OnPressPlayButton;
        LobbyEventsManager.Instance.OnCreatedLobby += LobbyUI_OnCreatedLobby;
        LobbyEventsManager.Instance.OnJoinedLobby += LobbyUI_OnJoinedLobby;
        LobbyEventsManager.Instance.OnLeftLobby += LobbyUI_OnLeftLobby;
        LobbyEventsManager.Instance.OnDeletedLobby += LobbyUI_OnLeftLobby;
        LobbyEventsManager.Instance.OnReturnToCreateProfile += LobbyUI_OnReturnToCreateProfile;
        LobbyEventsManager.Instance.OnReturnToLobbiesList += LobbyUI_OnReturnToLobbiesList;
		LobbyEventsManager.Instance.OnReturnToMainMenu += LobbyUI_OnReturnToMainMenu;

        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
        {
            ShowCurrentLobbyInfo();
        }
        else
        {
            LobbyEventsManager.Instance.TriggerOnReturnToMainMenu();
        }
    }

    private void OnDestroy()
    {
        if (LobbyEventsManager.Instance == null)
            return;

        LobbyEventsManager.Instance.OnPressPlayButton -= LobbyUI_OnPressPlayButton;
        LobbyEventsManager.Instance.OnCreatedLobby -= LobbyUI_OnCreatedLobby;
        LobbyEventsManager.Instance.OnJoinedLobby -= LobbyUI_OnJoinedLobby;
        LobbyEventsManager.Instance.OnLeftLobby -= LobbyUI_OnLeftLobby;
        LobbyEventsManager.Instance.OnDeletedLobby -= LobbyUI_OnLeftLobby;
        LobbyEventsManager.Instance.OnReturnToCreateProfile -= LobbyUI_OnReturnToCreateProfile;
        LobbyEventsManager.Instance.OnReturnToLobbiesList -= LobbyUI_OnReturnToLobbiesList;
		LobbyEventsManager.Instance.OnReturnToMainMenu -= LobbyUI_OnReturnToMainMenu;
    }
    
    private void HideAllPanels()
    {
        authUI.Hide();
        mainHubUI.Hide();
        createLobbyUI.Hide();
        listLobbiesUI.Hide();
        lobbyInfoUI.Hide();

        if (mainMenuUI != null)
            mainMenuUI.Hide();
    }

    public void ShowMainMenuState()
    {
        authUI.Hide();
        mainHubUI.Hide();
        createLobbyUI.Hide();
        listLobbiesUI.Hide();
        lobbyInfoUI.Hide();

        if (mainMenuUI != null)
            mainMenuUI.Show();
    }

private void ShowCurrentLobbyInfo()
{
    ShowLobbyInfoState();
}
    
    public void ShowLobbyInfoState()
    {
        if (mainMenuUI != null)
            mainMenuUI.Hide();

        authUI.Hide();

        mainHubUI.Show();

        createLobbyUI.Hide();
        listLobbiesUI.Hide();

        mainHubUI.RefreshUserInfo();

        lobbyInfoUI.Show();
    }

private void LobbyUI_OnReturnToMainMenu(object sender, EventArgs e)
{
    ShowMainMenuState();
}

private void LobbyUI_OnReturnToLobbiesList(object sender, EventArgs e)
{
    if (mainMenuUI != null)
        mainMenuUI.Hide();

    authUI.Hide();
    createLobbyUI.Hide();
    lobbyInfoUI.Hide();

    mainHubUI.Show();
    listLobbiesUI.Show();
}

    private void LobbyUI_OnReturnToCreateProfile(object sender, EventArgs e)
    {
        mainHubUI.Hide();
        listLobbiesUI.Hide();
        createLobbyUI.Hide();
        lobbyInfoUI.Hide();

        authUI.Show();
    }

    private void LobbyUI_OnPressPlayButton(object sender, EventArgs e)
    {
        if (mainMenuUI != null)
            mainMenuUI.Hide();

        mainHubUI.Hide();
        listLobbiesUI.Hide();
        createLobbyUI.Hide();
        lobbyInfoUI.Hide();

        authUI.Show();
    }

    private void LobbyUI_OnLeftLobby(object sender, EventArgs e)
    {
        ShowLobbiesListState();
    }

    private void LobbyUI_OnJoinedLobby(object sender, LobbyEventsManager.LobbyEventsArgs e)
    {
        Debug.Log("[LobbyUI] OnJoinedLobby -> ShowLobbyInfoState");
        ShowLobbyInfoState();
    }

    private void LobbyUI_OnCreatedLobby(object sender, EventArgs e)
    {
        Debug.Log("[LobbyUI] OnCreatedLobby -> ShowLobbyInfoState");
        ShowLobbyInfoState();
    }
    
    public void ShowLobbiesListState()
    {
        if (mainMenuUI != null)
            mainMenuUI.Hide();

        authUI.Hide();
        createLobbyUI.Hide();
        lobbyInfoUI.Hide();

        mainHubUI.Show();
        listLobbiesUI.Show();
    }
}