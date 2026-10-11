using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI player1Score;
    [SerializeField] private TextMeshProUGUI player2Score;
    [SerializeField] private TextMeshProUGUI player3Score;
    [SerializeField] private TextMeshProUGUI player4Score;
    [SerializeField] private TextMeshProUGUI player1Name;
    [SerializeField] private TextMeshProUGUI player2Name;
    [SerializeField] private TextMeshProUGUI player3Name;
    [SerializeField] private TextMeshProUGUI player4Name;
    [SerializeField] private TextMeshProUGUI timerText;

    private int lobbyPlayerCount = 0;

    private void Start()
    {
        GameEventsManager.Instance.OnScoreChanged += UIManager_OnScoreChanged;
        GameEventsManager.Instance.OnHealthChanged += UIManager_OnHealthChanged;
        GameEventsManager.Instance.OnPlayerDeath += UIManager_OnPlayerDeath;
        GameEventsManager.Instance.OnGameTimerUpdated += UIManager_OnGameTimerUpdated;

        player1Name.gameObject.SetActive(false);
        player2Name.gameObject.SetActive(false);
        player3Name.gameObject.SetActive(false);
        player4Name.gameObject.SetActive(false);
        player1Score.gameObject.SetActive(false);
        player2Score.gameObject.SetActive(false);
        player3Score.gameObject.SetActive(false);
        player4Score.gameObject.SetActive(false);
        
        lobbyPlayerCount = LobbyService.Instance.joinedLobby.Players.Count;

        if (lobbyPlayerCount > 0)
        {
            player1Score.gameObject.SetActive(true);
            player1Name.gameObject.SetActive(true);
            player1Name.text = LobbyService.Instance.joinedLobby.Players[0].Data["PlayerName"].Value;
        }
        if (lobbyPlayerCount > 1)
        {
            player2Score.gameObject.SetActive(true);
            player2Name.gameObject.SetActive(true);
            player2Name.text = LobbyService.Instance.joinedLobby.Players[1].Data["PlayerName"].Value;
        }
        if (lobbyPlayerCount > 2)
        {
            player3Score.gameObject.SetActive(true);
            player3Name.gameObject.SetActive(true);
            player3Name.text = LobbyService.Instance.joinedLobby.Players[2].Data["PlayerName"].Value;
        }
        if (lobbyPlayerCount > 3)
        {
            player4Score.gameObject.SetActive(true);
            player4Name.gameObject.SetActive(true);
            player4Name.text = LobbyService.Instance.joinedLobby.Players[3].Data["PlayerName"].Value;
        }

    }

    private void UIManager_OnGameTimerUpdated(object sender, GameEventsManager.OnGameTimerUpdatedEventsArgs e)
    {
        GameManager.Instance.UpdateTimerUI(timerText);
    }

    private void UIManager_OnPlayerDeath(object sender, GameEventsManager.OnPlayerDeathEventsArgs e)
    {
        Debug.Log($"Player{e.playerId} Death");
    }

    private void UIManager_OnHealthChanged(object sender, GameEventsManager.OnHealthChangedEventsArgs e)
    {
        Debug.Log($"Health changed. Player{e.playerId + 1}'s new health val: {e.newHealthScore}");
    }

    private void UIManager_OnScoreChanged(object sender, EventArgs e)
    {
        UpdateScoreTable();
    }

    private void UpdateScoreTable()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
        // NetworkManager hazır değilse ya da oyuncu yoksa işlem yapılmaz.

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null && playerObject.TryGetComponent(out PlayerPrefab player))
            {
                ulong playerId = client.ClientId;
                int score = player.Score.Value;

                switch (playerId)
                {
                    case 0:
                        player1Score.text = score.ToString();
                        break;
                    case 1:
                        player2Score.text = score.ToString();
                        break;
                    case 2:
                        player3Score.text = score.ToString();
                        break;
                    case 3:
                        player4Score.text = score.ToString();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}