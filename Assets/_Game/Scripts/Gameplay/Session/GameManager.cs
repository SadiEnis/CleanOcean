using System;
using System.Collections.Generic;
using _Game.Scripts.Gameplay.Match;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] private float _gameDuration = 180f;

    private NetworkVariable<float> _timeRemaining = new NetworkVariable<float>();
    
    public DateTime MatchStartedAtUtc { get; private set; }
    public DateTime MatchEndedAtUtc { get; private set; }
    public int GameDurationSeconds => Mathf.RoundToInt(_gameDuration);

    [NonSerialized] public NetworkVariable<bool> IsGameActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }

        _timeRemaining.OnValueChanged += OnTimeValueChanged;
        GameEventsManager.Instance.TriggerOnGameTimerUpdated(_gameDuration);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        _timeRemaining.OnValueChanged -= OnTimeValueChanged;
    }

    private void OnTimeValueChanged(float previousValue, float newValue)
    {
        GameEventsManager.Instance.TriggerOnGameTimerUpdated(newValue);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;
        if (sceneName == "MainScene")
        {
            Debug.Log("Sahne yüklendi.");
            StartGame();
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (IsGameActive.Value)
        {
            _timeRemaining.Value -= Time.deltaTime;
            if (_timeRemaining.Value <= 0)
            {
                _timeRemaining.Value = 0;
                EndGame();
            }
        }
    }

    private void StartGame()
    {
        if (!IsServer) return;
        
        MatchStartedAtUtc = DateTime.UtcNow;
        
        if (MatchActionLogTracker.Instance != null)
        {
            MatchActionLogTracker.Instance.Clear();
        }
        
        var players = FindObjectsByType<PlayerPrefab>(FindObjectsSortMode.None);
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        int index = 0;
        foreach (var player in players)
        {
            // Eğer Spawn Point varsa sırayla dağıtır, yoksa (0,0,0)'a gönderir
            Vector3 targetPos = (spawnPoints.Length > 0)
                ? spawnPoints[index % spawnPoints.Length].transform.position
                : Vector3.zero;

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // CC varsa kapatıldı ki çakışma yapmasın

            player.transform.position = targetPos;

            if (cc != null) cc.enabled = true;
            index++;
        }
        _timeRemaining.Value = _gameDuration;
        IsGameActive.Value = true;
        GameEventsManager.Instance.TriggerOnGameStarted();
    }

    private void EndGame()
    {
        if (!IsServer) return;
        if (!IsGameActive.Value) return;
        
        MatchEndedAtUtc = DateTime.UtcNow;
        IsGameActive.Value = false;
        GameEventsManager.Instance.TriggerOnGameEnded();
    }

    public void UpdateTimerUI(TextMeshProUGUI timerText)
    {
        // TimeToDisplay negatif olmamalı her ihtimale karşı değerini min 0 olacak şekilde ayarladım.
        float timeToDisplay = Mathf.Max(0, _timeRemaining.Value);

        float minute = Mathf.FloorToInt(timeToDisplay / 60);
        float second = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minute, second);

        if (timeToDisplay <= 10f) timerText.color = Color.red;
        else timerText.color = Color.white;
    }
}