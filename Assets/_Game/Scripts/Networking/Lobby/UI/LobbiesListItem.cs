using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbiesListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyPlayersText;

    private Lobby _lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            LobbyService.Instance.JoinLobbyById(_lobby.Id, LobbyManager.Instance.playerName);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        
        _lobbyNameText.text = _lobby.Name;
        _lobbyPlayersText.text = _lobby.Players.Count + " / " + _lobby.MaxPlayers;
    }
}
