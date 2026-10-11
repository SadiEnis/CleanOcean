using _Game.Scripts.Gameplay.Player;
using System.Collections.Generic;
using _Game.Scripts.UI.HUD;
using _Game.Scripts.Gameplay.Match;
using Unity.Netcode;
using UnityEngine;
using System;

namespace _Game.Scripts.Gameplay.Rescue
{
    public class RescueTarget : NetworkBehaviour
    {
        [Header("Rescue Settings")] [SerializeField]
        private int _requiredPlayerCount = 2;

        [SerializeField] private float _requiredDuration = 5f;
        [SerializeField] private float _progressDecaySpeed = 1.5f;
        [SerializeField] private int _rescueScoreReward = 50;
        [SerializeField] private float _rescueFoodDuration = 3f;

        [Header("References")] [SerializeField]
        private Collider2D _rescueTrigger;

        [SerializeField] private GameObject _visual;

        public float RequiredDuration => IsRescueFoodApplied.Value ? _rescueFoodDuration : _requiredDuration;
        public int RequiredPlayerCount => _requiredPlayerCount;

        public NetworkVariable<float> Progress = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> IsCompleted = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> PlayerCountInZone = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> IsRescueFoodApplied = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private readonly HashSet<ulong> _playersInZone = new();

        private void Awake()
        {
            if (_rescueTrigger == null)
                _rescueTrigger = GetComponent<Collider2D>();

            if (_rescueTrigger != null)
                _rescueTrigger.isTrigger = true;
        }

        public override void OnNetworkSpawn()
        {
            Progress.OnValueChanged += OnProgressChanged;
            IsCompleted.OnValueChanged += OnCompletedChanged;
            PlayerCountInZone.OnValueChanged += OnPlayerCountChanged;
            IsRescueFoodApplied.OnValueChanged += OnRescueFoodAppliedChanged;

            ApplyCompletedVisual(IsCompleted.Value);

            Debug.Log(
                $"RescueTarget OnNetworkSpawn. " +
                $"IsServer={IsServer}, IsClient={IsClient}, IsSpawned={IsSpawned}"
            );
        }

        public override void OnNetworkDespawn()
        {
            Progress.OnValueChanged -= OnProgressChanged;
            IsCompleted.OnValueChanged -= OnCompletedChanged;
            PlayerCountInZone.OnValueChanged -= OnPlayerCountChanged;
            IsRescueFoodApplied.OnValueChanged -= OnRescueFoodAppliedChanged;
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (IsCompleted.Value)
                return;

            if (GameManager.Instance != null && !GameManager.Instance.IsGameActive.Value)
                return;

            bool hasEnoughPlayers = _playersInZone.Count >= _requiredPlayerCount;

            if (hasEnoughPlayers)
            {
                Progress.Value += Time.deltaTime;

                if (Progress.Value >= RequiredDuration)
                {
                    CompleteRescue();
                }
            }
            else
            {
                if (Progress.Value > 0f)
                {
                    Progress.Value -= Time.deltaTime * _progressDecaySpeed;
                    Progress.Value = Mathf.Max(0f, Progress.Value);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer)
                return;

            if (IsCompleted.Value)
                return;

            if (!TryGetPlayerNetworkObject(other, out NetworkObject playerNetworkObject))
                return;

            ulong clientId = playerNetworkObject.OwnerClientId;

            if (_playersInZone.Add(clientId))
            {
                PlayerCountInZone.Value = _playersInZone.Count;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsServer)
                return;

            if (!TryGetPlayerNetworkObject(other, out NetworkObject playerNetworkObject))
                return;

            ulong clientId = playerNetworkObject.OwnerClientId;

            if (_playersInZone.Remove(clientId))
            {
                PlayerCountInZone.Value = _playersInZone.Count;
            }
        }

        private bool TryGetPlayerNetworkObject(Collider2D other, out NetworkObject playerNetworkObject)
        {
            playerNetworkObject = other.GetComponentInParent<NetworkObject>();

            if (playerNetworkObject == null)
                return false;

            PlayerHealth playerHealth = playerNetworkObject.GetComponentInChildren<PlayerHealth>();

            if (playerHealth == null)
                return false;

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UseRescueFoodServerRpc(ServerRpcParams rpcParams = default)
        {
            if (IsCompleted.Value)
                return;

            if (IsRescueFoodApplied.Value)
            {
                Debug.LogWarning("RESCUE_FOOD use failed. Already applied to this RescueTarget.");
                return;
            }

            ulong senderClientId = rpcParams.Receive.SenderClientId;

            NetworkObject playerObject = FindPlayerByClientId(senderClientId);

            if (playerObject == null)
            {
                Debug.LogWarning($"RESCUE_FOOD use failed. Player not found. ClientId={senderClientId}");
                return;
            }

            if (!_playersInZone.Contains(senderClientId))
            {
                Debug.LogWarning($"RESCUE_FOOD use failed. Player is not in rescue zone. ClientId={senderClientId}");
                return;
            }

            if (!playerObject.TryGetComponent(out PlayerRuntimeInventory runtimeInventory))
            {
                Debug.LogWarning("RESCUE_FOOD use failed. PlayerRuntimeInventory not found.");
                return;
            }

            bool consumed = runtimeInventory.TryConsumeRescueFoodOnServer();

            if (!consumed)
                return;

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { senderClientId }
                }
            };

            ShowRescueFoodUsedClientRpc(clientRpcParams);

            IsRescueFoodApplied.Value = true;

            if (Progress.Value >= RequiredDuration)
            {
                CompleteRescue();
            }

            Debug.Log(
                $"RESCUE_FOOD applied to RescueTarget. " +
                $"ClientId={senderClientId}, NewDuration={RequiredDuration}"
            );
        }

        [ClientRpc]
        private void ShowRescueFoodUsedClientRpc(ClientRpcParams clientRpcParams = default)
        {
            if (ItemFeedbackUI.Instance != null)
                ItemFeedbackUI.Instance.ShowMessage("RESCUE_FOOD used");
        }

        private void CompleteRescue()
        {
            if (IsCompleted.Value)
                return;

            Debug.Log($"RescueTarget: CompleteRescue called. PlayersInZone={_playersInZone.Count}");
            GiveRescueScoreToParticipants();

            Progress.Value = RequiredDuration;
            IsCompleted.Value = true;

            if (_rescueTrigger != null)
                _rescueTrigger.enabled = false;

            Debug.Log(
                $"RescueTarget: Rescue completed. " +
                $"Players={_playersInZone.Count}, Duration={RequiredDuration}"
            );
            GameEventsManager.Instance.TriggerOnRescueCompleted(this);
        }

        private void GiveRescueScoreToParticipants()
        {
            if (!IsServer)
                return;

            if (GameEventsManager.Instance == null)
            {
                Debug.LogWarning("RescueTarget: GameEventsManager not found. Score reward skipped.");
                return;
            }

            foreach (ulong clientId in _playersInZone)
            {
                GameEventsManager.Instance.TriggerOnScoreIncrease(clientId, _rescueScoreReward);

                ulong dbUserId = GetDbUserIdByClientId(clientId);

                if (dbUserId > 0 && MatchActionLogTracker.Instance != null)
                {
                    MatchActionLogTracker.Instance.RecordRescueCompleted(
                        dbUserId,
                        _rescueScoreReward,
                        transform.position
                    );
                }
            }
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

            Debug.LogWarning($"RescueTarget: DbUserId not found for ClientId={clientId}");
            return 0;
        }

        private void OnProgressChanged(float previousValue, float newValue)
        {
        }

        private void OnPlayerCountChanged(int previousValue, int newValue)
        {
        }

        private void OnCompletedChanged(bool previousValue, bool newValue)
        {
            ApplyCompletedVisual(newValue);

            if (newValue)
            {
                Debug.Log("RescueTarget: Completed state received.");
            }
        }

        private void ApplyCompletedVisual(bool completed)
        {
            if (_visual != null)
                _visual.SetActive(!completed);
        }

        private NetworkObject FindPlayerByClientId(ulong clientId)
        {
            var players = FindObjectsByType<PlayerRuntimeInventory>(FindObjectsSortMode.None);

            foreach (var playerInventory in players)
            {
                if (playerInventory.OwnerClientId == clientId)
                    return playerInventory.NetworkObject;
            }

            return null;
        }

        private void OnRescueFoodAppliedChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                Debug.Log($"RescueTarget: RESCUE_FOOD effect active. RequiredDuration={RequiredDuration}");
            }
        }
    }
}