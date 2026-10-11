using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : BaseUIComponent
{
    [SerializeField] private TMP_InputField _lobbyNameInputField;
    [SerializeField] private TMP_InputField _maxPlayerInputField;
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _backButton;

    string _lobbyName => _lobbyNameInputField.text;
    string _maxPlayer => _maxPlayerInputField.text;

    private void Awake()
    {
        _createLobbyButton.onClick.AddListener( () => LobbyService.Instance.CreateLobby(LobbyManager.Instance.playerName, _lobbyName, _maxPlayer, false));
        _backButton.onClick.AddListener(() =>
        {
            LobbyEventsManager.Instance.TriggerOnReturnToLobbiesList();
        });
    }
}