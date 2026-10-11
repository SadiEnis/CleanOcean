using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerStateController : NetworkBehaviour
{
    [SerializeField] private RectTransform lobbyUI;
    
    // [SerializeField] private CharacterController characterController;
    [SerializeField] private SpriteRenderer playerModel;

    private void Start()
    {
        ActivateLobbyMode();
    }
    public override void OnNetworkSpawn()
    {
        GameEventsManager.Instance.OnGameStarted += StateController_OnGameStarted;
    }

    public override void OnNetworkDespawn()
    {
        if (GameEventsManager.Instance != null)
            GameEventsManager.Instance.OnGameStarted -= StateController_OnGameStarted;
    }

    private void StateController_OnGameStarted(object sender, EventArgs e)
    {
        ActivateGameMode();
    }

    public void ActivateLobbyMode()
    {
        if (lobbyUI != null) lobbyUI.gameObject.SetActive(true);
        
        // if (characterController != null) characterController.enabled = false;
        if (playerModel != null) playerModel.enabled = false;
    }

    private void ActivateGameMode()
    {
        if (!IsSpawned)
        {
            Debug.LogWarning("[PlayerStateController] ActivateGameMode skipped because NetworkObject is not spawned.");
            return;
        }

        if (!IsServer)
            return;

        ActivateGameModeRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ActivateGameModeRpc()
    {
        if (lobbyUI != null) lobbyUI.gameObject.SetActive(false);
        
        // if (characterController != null) characterController.enabled = true;
        if (playerModel != null) playerModel.enabled = true;
    }
}
