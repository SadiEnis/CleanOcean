using System;
using _Game.Scripts.NetworkModels.Inventory;
using _Game.Scripts.Services.Inventory;
using _Game.Scripts.Session;
using _Game.Scripts.Gameplay.Rescue;
using _Game.Scripts.UI.HUD;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Player
{
    public class PlayerRuntimeInventory : NetworkBehaviour
    {
        private const string GOOD_SWIMMER_CODE = "GOOD_SWIMMER";
        private const string ECO_BADGE_CODE = "ECO_BADGE";
        private const string MED_KIT_CODE = "MED_KIT";
        private const string RESCUE_FOOD_CODE = "RESCUE_FOOD";
		private const string OIL_CLEANER_CODE = "OIL_CLEANER";

        [SerializeField] private float _goodSwimmerSpeedMultiplier = 1.2f;

        [SerializeField] private float _oilCleanerProtectionDuration = 3f;
        
        public bool HasAvailableMedKit => AvailableMedKitCount.Value > 0;
        public bool HasAvailableRescueFood => AvailableRescueFoodCount.Value > 0;
        public bool HasAvailableOilCleaner => AvailableOilCleanerCount.Value > 0;
        
        private Coroutine _oilCleanerProtectionCoroutine;

        public NetworkVariable<bool> HasGoodSwimmer = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> HasEcoBadge = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> AvailableMedKitCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> UsedMedKitCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkVariable<int> AvailableRescueFoodCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> UsedRescueFoodCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        public NetworkVariable<int> AvailableOilCleanerCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkVariable<int> UsedOilCleanerCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        public NetworkVariable<bool> IsOilCleanerProtectionActive = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private InventoryApiClient _inventoryApiClient;
        private PlayerMovement _playerMovement;
        private PlayerHealth _playerHealth;
        
        private bool _isInsideOilHazard;

        private void Update()
        {
            if (!IsOwner)
                return;
        
            if (_playerHealth == null)
                return;
        
            if (_playerHealth.IsDead.Value)
            {
                if (HasAvailableMedKit && ItemFeedbackUI.Instance != null)
                {
                    ItemFeedbackUI.Instance.ShowHint("Press M to use MED_KIT");
                }

                if (Input.GetKeyDown(KeyCode.M))
                {
                    _playerHealth.UseMedKitSelfReviveServerRpc();
                }

                return;
            }
        
            
            if (HasAvailableRescueFood && FindNearestRescueTarget() != null && ItemFeedbackUI.Instance != null)
            {
                ItemFeedbackUI.Instance.ShowHint("Press R to use RESCUE_FOOD");
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryUseRescueFoodClientSide();
            }
        
            if (_isInsideOilHazard &&
                HasAvailableOilCleaner &&
                !IsOilCleanerProtectionActive.Value &&
                ItemFeedbackUI.Instance != null)
            {
                ItemFeedbackUI.Instance.ShowHint("Press O to use OIL_CLEANER");
            }
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                UseOilCleanerServerRpc();
            }
        }

        private void TryUseRescueFoodClientSide()
        {
            RescueTarget nearestTarget = FindNearestRescueTarget();

            if (nearestTarget == null)
            {
                Debug.LogWarning("RESCUE_FOOD use failed. No RescueTarget nearby.");
                return;
            }

            nearestTarget.UseRescueFoodServerRpc();
        }

        private RescueTarget FindNearestRescueTarget()
        {
            RescueTarget[] targets = FindObjectsByType<RescueTarget>(FindObjectsSortMode.None);

            RescueTarget nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target == null || target.IsCompleted.Value)
                    continue;

                float distance = Vector2.Distance(transform.position, target.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target;
                }
            }
            if (nearestDistance > 3f)
                return null;

            return nearest;
        }
        public override void OnNetworkSpawn()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _playerHealth = GetComponent<PlayerHealth>();

            if (IsOwner)
            {
                _inventoryApiClient = new InventoryApiClient();
                LoadInventoryAndSubmitAsync();
            }
        }
        
        public void SetInsideOilHazard(bool isInside)
        {
            if (!IsOwner)
                return;

            _isInsideOilHazard = isInside;
        }

        private async void LoadInventoryAndSubmitAsync()
        {
            if (!CurrentUser.IsLoggedIn)
            {
                Debug.LogWarning("PlayerRuntimeInventory: CurrentUser is not logged in.");
                return;
            }

            try
            {
                InventoryResponse inventoryResponse =
                    await _inventoryApiClient.GetInventoryAsync(CurrentUser.UserId);

                if (!inventoryResponse.success)
                {
                    Debug.LogWarning($"PlayerRuntimeInventory: Inventory load failed: {inventoryResponse.message}");
                    return;
                }

                bool hasGoodSwimmer = HasItem(inventoryResponse, GOOD_SWIMMER_CODE);
                bool hasEcoBadge = HasItem(inventoryResponse, ECO_BADGE_CODE);

                int medKitQuantity = GetItemQuantity(inventoryResponse, MED_KIT_CODE);
                int rescueFoodQuantity = GetItemQuantity(inventoryResponse, RESCUE_FOOD_CODE);
				int oilCleanerQuantity = GetItemQuantity(inventoryResponse, OIL_CLEANER_CODE);

                SubmitInventoryStateServerRpc(hasGoodSwimmer, hasEcoBadge, medKitQuantity, rescueFoodQuantity, oilCleanerQuantity);
            }
            catch (Exception exception)
            {
                Debug.LogError($"PlayerRuntimeInventory load failed: {exception.Message}");
            }
        }

        private static bool HasItem(InventoryResponse inventoryResponse, string itemCode)
        {
            if (inventoryResponse.items == null)
                return false;

            foreach (var item in inventoryResponse.items)
            {
                if (item.itemCode == itemCode && item.quantity > 0)
                    return true;
            }

            return false;
        }

        [ServerRpc]
        private void SubmitInventoryStateServerRpc(
            bool hasGoodSwimmer,
            bool hasEcoBadge,
            int medKitQuantity,
            int rescueFoodQuantity,
            int oilCleanerQuantity)
        {
            HasGoodSwimmer.Value = hasGoodSwimmer;
            HasEcoBadge.Value = hasEcoBadge;
        
            AvailableMedKitCount.Value = Mathf.Max(0, medKitQuantity);
            UsedMedKitCount.Value = 0;
        
            AvailableRescueFoodCount.Value = Mathf.Max(0, rescueFoodQuantity);
            UsedRescueFoodCount.Value = 0;
        
            AvailableOilCleanerCount.Value = Mathf.Max(0, oilCleanerQuantity);
            UsedOilCleanerCount.Value = 0;
            IsOilCleanerProtectionActive.Value = false;
        
            ApplyPassiveEffectsOnServer();
        
            ShowPassiveActivatedClientRpc(hasGoodSwimmer, hasEcoBadge);
        }
        
        [ClientRpc]
        private void ShowPassiveActivatedClientRpc(bool hasGoodSwimmer, bool hasEcoBadge)
        {
            if (!IsOwner)
                return;

            if (ItemFeedbackUI.Instance == null)
                return;

            if (hasGoodSwimmer)
                ItemFeedbackUI.Instance.ShowMessage("GOOD_SWIMMER activated");

            if (hasEcoBadge)
                ItemFeedbackUI.Instance.ShowMessage("ECO_BADGE activated");
        }

        private void ApplyPassiveEffectsOnServer()
        {
            if (!IsServer)
                return;

            if (_playerMovement == null)
                _playerMovement = GetComponent<PlayerMovement>();

            if (_playerMovement == null)
                return;

            float speedMultiplier = HasGoodSwimmer.Value ? _goodSwimmerSpeedMultiplier : 1f;

            _playerMovement.SetPassiveSpeedMultiplier(speedMultiplier);
        }

        [ServerRpc]
        private void UseOilCleanerServerRpc()
        {
            if (IsOilCleanerProtectionActive.Value)
            {
                Debug.LogWarning($"OIL_CLEANER use failed. Protection already active. OwnerClientId={OwnerClientId}");
                return;
            }
        
            bool consumed = TryConsumeOilCleanerOnServer();
        
            if (!consumed)
                return;
        
            if (_oilCleanerProtectionCoroutine != null)
                StopCoroutine(_oilCleanerProtectionCoroutine);
        
            _oilCleanerProtectionCoroutine = StartCoroutine(OilCleanerProtectionRoutine());
        
            ShowOilCleanerUsedClientRpc();
        }

        private System.Collections.IEnumerator OilCleanerProtectionRoutine()
        {
            IsOilCleanerProtectionActive.Value = true;
        
            if (_playerMovement != null)
                _playerMovement.SetHazardSpeedMultiplier(1f);
        
            Debug.Log($"OIL_CLEANER protection started. OwnerClientId={OwnerClientId}");
        
            yield return new WaitForSeconds(_oilCleanerProtectionDuration);
        
            IsOilCleanerProtectionActive.Value = false;
        
            Debug.Log($"OIL_CLEANER protection ended. OwnerClientId={OwnerClientId}");
        }

        [ClientRpc]
        private void ShowOilCleanerUsedClientRpc()
        {
            if (!IsOwner)
                return;
        
            if (ItemFeedbackUI.Instance != null)
                ItemFeedbackUI.Instance.ShowMessage("OIL_CLEANER used");
        }

        public bool TryConsumeMedKitOnServer()
        {
            if (!IsServer)
                return false;

            if (AvailableMedKitCount.Value <= 0)
            {
                Debug.LogWarning($"MED_KIT consume failed. OwnerClientId={OwnerClientId}, no MED_KIT available.");
                return false;
            }

            AvailableMedKitCount.Value--;
            UsedMedKitCount.Value++;
            return true;
        }

        public bool TryConsumeRescueFoodOnServer()
        {
            if (!IsServer)
                return false;

            if (AvailableRescueFoodCount.Value <= 0)
            {
                Debug.LogWarning(
                    $"RESCUE_FOOD consume failed. OwnerClientId={OwnerClientId}, no RESCUE_FOOD available."
                );
                return false;
            }

            AvailableRescueFoodCount.Value--;
            UsedRescueFoodCount.Value++;
            return true;
        }

        public bool TryConsumeOilCleanerOnServer()
        {
            if (!IsServer)
                return false;
        
            if (AvailableOilCleanerCount.Value <= 0)
            {
                Debug.LogWarning(
                    $"OIL_CLEANER consume failed. OwnerClientId={OwnerClientId}, no OIL_CLEANER available."
                );
                return false;
            }
        
            AvailableOilCleanerCount.Value--;
            UsedOilCleanerCount.Value++;
            return true;
        }

        private static int GetItemQuantity(InventoryResponse inventoryResponse, string itemCode)
        {
            if (inventoryResponse.items == null)
                return 0;

            foreach (var item in inventoryResponse.items)
            {
                if (item.itemCode == itemCode)
                    return item.quantity;
            }
            return 0;
        }
    }
}