using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Player = Unity.Services.Matchmaker.Models.Player;

public class LobbyInfoPlayerItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;

    public void SetPlayerName(string playerName)
    {
        _playerNameText.text = playerName;
    }
}
