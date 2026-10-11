using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbiesListUI : BaseUIComponent
{
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _refreshLobbiesButton;
    [SerializeField] private Button _searchLobbyButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_InputField _searchLobbyInputField;
    [SerializeField] private Transform _lobbyListContainer;
    [SerializeField] private Transform _lobbyListItemTemplate;

    private void Awake()
    {
        _refreshLobbiesButton.onClick.AddListener(RefreshLobbyList);
        _createLobbyButton.onClick.AddListener(() =>
        {
            LobbyUI.Instance.listLobbiesUI.Hide();
            LobbyUI.Instance.createLobbyUI.Show();
        });
        _searchLobbyButton.onClick.AddListener(() =>
        {
            LobbyService.Instance.JoinLobbyByCode(_searchLobbyInputField.text, LobbyManager.Instance.playerName);
        });
        _backButton.onClick.AddListener(() =>
        {
            LobbyEventsManager.Instance.TriggerOnReturnCreateProfile();
        });
    }

    private void OnEnable()
    {
        if (LobbyEventsManager.Instance != null)
        {
            LobbyEventsManager.Instance.OnLobbyListChanged += LobbyEventsManager_OnLobbyListChanged;
            LobbyEventsManager.Instance.OnLeftLobby += LobbyEventsManager_OnLeftLobby;
        }
    }

    private void OnDisable()
    {
        if (LobbyEventsManager.Instance != null)
        {
            LobbyEventsManager.Instance.OnLobbyListChanged -= LobbyEventsManager_OnLobbyListChanged;
            LobbyEventsManager.Instance.OnLeftLobby -= LobbyEventsManager_OnLeftLobby;
        }
    }

    private void LobbyEventsManager_OnLeftLobby(object sender, EventArgs e)
    {
        LobbyService.Instance.ListLobbies();
    }

    private void LobbyEventsManager_OnLobbyListChanged(object sender, LobbyEventsManager.OnLobbyListChangedEventsArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbiesList)
    {
        foreach (Transform child in _lobbyListContainer)
        {
            if (child == _lobbyListItemTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbiesList)
        {
            Transform lobbyItemTransform = Instantiate(_lobbyListItemTemplate, _lobbyListContainer);
            lobbyItemTransform.gameObject.SetActive(true);

            LobbiesListItem item = lobbyItemTransform.GetComponent<LobbiesListItem>();
            item.SetLobby(lobby);
        }
    }

    private void RefreshLobbyList()
    {
        LobbyService.Instance.ListLobbies();
    }
}