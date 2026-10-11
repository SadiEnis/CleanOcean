using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPrefab : NetworkBehaviour
{
    [NonSerialized] public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [NonSerialized] public NetworkVariable<int> Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        Score.OnValueChanged += OnScoreValueChanged;

        if (IsOwner)
        {
            SetPlayerNameServerRpc(LobbyManager.Instance.playerName);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- KRİTİK DEĞİŞİKLİK BURADA ---
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isLobby = scene.name == "LobbyScene";

        // Görünürlük ve fizik ayarları
        GetComponent<PlayerMovement>().enabled = !isLobby;
        GetComponent<Rigidbody2D>().simulated = !isLobby;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = !isLobby;

        // EĞER OYUN SAHNESİNDEYSEK VE BU BENİM KARAKTERİMSE KAMERAYI BAĞLA
        if (!isLobby && IsOwner)
        {
            StartCoroutine(AttachCamera());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string name)
    {
        PlayerName.Value = name;
    }

    private IEnumerator AttachCamera()
    {
        // Kamera hemen sahnede oluşmamış olabilir, bulana kadar bekle
        while (CameraController.Instance == null)
            yield return null;

        CameraController.Instance.SetTarget(transform);
    }

    private void Start()
    {
        // Sadece sahibi olan oyuncuda bu nesne sahneler arası taşınsın
        if (IsOwner)
            DontDestroyOnLoad(gameObject);

        GameEventsManager.Instance.OnScoreIncrease += PlayerPrefab_OnScoreIncrease;
        GameEventsManager.Instance.OnScoreDecrease += PlayerPrefab_OnScoreDecrease;
        GameEventsManager.Instance.OnGameStarted += PlayerPrefab_OnGameStarted;
    }

    private void PlayerPrefab_OnGameStarted(object sender, EventArgs e)
    {
        // Burada kamera bağlamayı kaldırdık, OnSceneLoaded hallediyor.
        
        // DİKKAT: Score bir NetworkVariable ve yazma izni sadece Server'da.
        // Client burayı çalıştırırsa hata verir. Kontrol ekliyoruz:
        if (IsServer)
        {
            Score.Value = 0;
        }
    }

    private void OnScoreValueChanged(int previousValue, int newValue)
    {
        GameEventsManager.Instance.TriggerOnScoreChanged(); 
    }

    private void PlayerPrefab_OnScoreIncrease(object sender, GameEventsManager.OnScoreEventsArgs e)
    {
        if (!IsServer) return;
        if (OwnerClientId == e.playerId)
        {
            Score.Value += e.score;
            Debug.Log($"Player{OwnerClientId}'s score: " + e.score);
        }
    }

    private void PlayerPrefab_OnScoreDecrease(object sender, GameEventsManager.OnScoreEventsArgs e)
    {
        if (!IsServer) return;
        if (OwnerClientId == e.playerId)
        {
            Score.Value -= e.score;
            Debug.Log($"Player{OwnerClientId}'s score: " + e.score);
        }
    }
}