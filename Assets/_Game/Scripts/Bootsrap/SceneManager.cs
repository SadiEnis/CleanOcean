using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneManager : NetworkSingleton<SceneManager>
{
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        GameEventsManager.Instance.OnGameStarted += SceneManager_OnGameStarted;
    }

    private void SceneManager_OnGameStarted(object sender, EventArgs e)
    {
        Debug.Log("OnGameStarted Scene Manager üzerinde abonelik aldı.");
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        if (!IsHost) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

            if (playerObject != null)
            {
                Transform spawnPoint = spawnPoints[clientId % (ulong)spawnPoints.Length];
                SpawnPlayerClientRpc(playerObject.OwnerClientId, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnPlayerClientRpc(ulong clientId, Vector3 position, Quaternion rotation)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        NetworkObject localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayer != null)
        {
            localPlayer.transform.position = position;
            localPlayer.transform.rotation = rotation;
        }
    }
}
