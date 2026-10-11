using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameResultsUI : NetworkBehaviour
{
    [SerializeField] private GameObject _resultsPanel;
    [SerializeField] private Transform _resultsContainer;
    [SerializeField] private GameObject _scoreItemPrefab;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _returnToLobbyButton;

    private void Start()
    {
        _resultsPanel.SetActive(false);
        GameEventsManager.Instance.OnGameEnded += GameResults_OnGameEnded;

        _playAgainButton.onClick.AddListener(HandleRestartGame);
        _returnToLobbyButton.onClick.AddListener(HandleReturnToLobby);

        if (NetworkManager.Singleton.IsClient)
            _playAgainButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEventsManager.Instance.OnGameEnded -= GameResults_OnGameEnded;
    }

    private void GameResults_OnGameEnded(object sender, EventArgs e)
    {
        ShowResultsClientRpc();
    }

    private void HandleRestartGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    private void HandleReturnToLobby()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    [ClientRpc]
    private void ShowResultsClientRpc()
    {
        _resultsPanel.SetActive(true);

        var sortedScores = GetSortedPlayerScores();
        PopulateScoreBoard(sortedScores);
        if (!IsHost)
            _playAgainButton.gameObject.SetActive(false);
        else
            _playAgainButton.gameObject.SetActive(true);
    }

    private List<(string name, int score)> GetSortedPlayerScores()
    {
        var scoreList = new List<(string name, int score)>();
        var players = FindObjectsByType<PlayerPrefab>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            string pName = player.PlayerName.Value.ToString();
            int score = player.Score.Value;
            scoreList.Add((pName, score));
        }

        return scoreList.OrderByDescending(x => x.score).ToList();
    }

    private void PopulateScoreBoard(List<(string name, int score)> playerScores)
    {
        foreach (Transform child in _resultsContainer)
            Destroy(child.gameObject);
        foreach (var data in playerScores)
        {
            PlayerScoreItem item = Instantiate(_scoreItemPrefab, _resultsContainer).GetComponent<PlayerScoreItem>();
            item.SetScoreData(data.name, data.score);
        }
    }
}