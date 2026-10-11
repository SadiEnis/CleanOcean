using _Game.Scripts.Gameplay.Player;
using _Game.Scripts.UI.HUD;
using _Game.Scripts.Gameplay.Match;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            ResetPlayerStats();

        CurrentHealth.OnValueChanged += HandleHealthChanged;
        IsDead.OnValueChanged += HandleDeadChanged;
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            ResetPlayerStats();
        }
    }

    private void ResetPlayerStats()
    {
        CurrentHealth.Value = maxHealth;
        IsDead.Value = false;
    }


    private void HandleHealthChanged(int previousValue, int newValue)
    {
        GameEventsManager.Instance.TriggerOnHealthChanged(OwnerClientId, newValue);
    }

    private void HandleDeadChanged(bool previousValue, bool newValue)
    {
        if (!newValue)
            return;

        GameEventsManager.Instance.TriggerOnPlayerDeath(OwnerClientId);

        if (!IsServer)
            return;

        ulong dbUserId = GetDbUserId();

        if (dbUserId <= 0)
            return;

        if (MatchActionLogTracker.Instance != null)
        {
            MatchActionLogTracker.Instance.RecordPlayerFainted(
                dbUserId,
                transform.position
            );
        }
    }
    
    private ulong GetDbUserId()
    {
        if (TryGetComponent(out PlayerNetworkIdentity identity))
            return identity.dbUserId.Value;

        Debug.LogWarning($"PlayerHealth: PlayerNetworkIdentity not found. OwnerClientId={OwnerClientId}");
        return 0;
    }
    
    private ulong GetDbUserIdByClientId(ulong clientId)
    {
        PlayerNetworkIdentity[] identities = FindObjectsByType<PlayerNetworkIdentity>(
            FindObjectsSortMode.None
        );

        foreach (var identity in identities)
        {
            if (identity.OwnerClientId == clientId)
                return identity.dbUserId.Value;
        }

        Debug.LogWarning($"PlayerHealth: DbUserId not found for ClientId={clientId}");
        return 0;
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer || IsDead.Value) return;

        CurrentHealth.Value -= damageAmount;
        if (CurrentHealth.Value <= 0)
            IsDead.Value = true;
    }
    
    

    [ServerRpc(RequireOwnership = false)]
    public void ReviveServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsDead.Value) return;

        ulong reviverClientId = rpcParams.Receive.SenderClientId;
        if (reviverClientId == OwnerClientId) return;

        IsDead.Value = false;
        CurrentHealth.Value = maxHealth;

        GameEventsManager.Instance.TriggerOnPlayerRevived(OwnerClientId, reviverClientId);

        RecordReviveActionLog(reviverClientId);
    }
    
    [ServerRpc]
    public void UseMedKitSelfReviveServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsDead.Value)
            return;

        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if (senderClientId != OwnerClientId)
        {
            Debug.LogWarning(
                $"Self revive rejected. SenderClientId={senderClientId}, OwnerClientId={OwnerClientId}"
            );
            return;
        }

        if (!TryGetComponent(out PlayerRuntimeInventory runtimeInventory))
        {
            Debug.LogWarning("Self revive failed. PlayerRuntimeInventory not found.");
            return;
        }

        bool consumed = runtimeInventory.TryConsumeMedKitOnServer();

        if (!consumed)
            return;

        IsDead.Value = false;
        CurrentHealth.Value = maxHealth;

        ShowMedKitUsedClientRpc();
        Debug.Log($"Player self revived with MED_KIT. OwnerClientId={OwnerClientId}");
    }
    
    [ClientRpc]
    private void ShowMedKitUsedClientRpc()
    {
        if (!IsOwner)
            return;

        if (ItemFeedbackUI.Instance != null)
            ItemFeedbackUI.Instance.ShowMessage("MED_KIT used");
    }
    
    private void RecordReviveActionLog(ulong reviverClientId)
    {
        if (!IsServer)
            return;

        if (MatchActionLogTracker.Instance == null)
            return;

        ulong reviverUserId = GetDbUserIdByClientId(reviverClientId);
        ulong revivedUserId = GetDbUserId();

        if (reviverUserId <= 0 || revivedUserId <= 0)
            return;

        const int reviveRewardValue = 50;

        MatchActionLogTracker.Instance.RecordPlayerRevived(
            reviverUserId,
            revivedUserId,
            reviveRewardValue,
            transform.position
        );
    }
}