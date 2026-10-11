using UnityEngine;
using _Game.Scripts.Gameplay.Player;
using Unity.Netcode;

public class OilSpill : BaseHazard
{
    [SerializeField, Range(0f, 1f)] private float viscosity = 0.5f;

    protected override void OnEnterHazard(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerMovement movement))
            return;

        if (HasOilCleanerProtection(other))
        {
            movement.SetHazardSpeedMultiplier(1f);
            return;
        }
        
        if (other.TryGetComponent(out PlayerRuntimeInventory runtimeInventory))
        {
            NotifyOilHazardStateClientRpc(runtimeInventory.OwnerClientId, true);
        }

        movement.SetHazardSpeedMultiplier(viscosity);
    }

    protected override void OnExitHazard(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMovement movement))
        {
            movement.SetHazardSpeedMultiplier(1f);
        }
        
        if (other.TryGetComponent(out PlayerRuntimeInventory runtimeInventory))
        {
            NotifyOilHazardStateClientRpc(runtimeInventory.OwnerClientId, false);
        }
    }
    
    [ClientRpc]
    private void NotifyOilHazardStateClientRpc(ulong targetClientId, bool isInside)
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        PlayerRuntimeInventory[] inventories = FindObjectsByType<PlayerRuntimeInventory>(FindObjectsSortMode.None);

        foreach (var inventory in inventories)
        {
            if (inventory.IsOwner)
            {
                inventory.SetInsideOilHazard(isInside);
                break;
            }
        }
    }

    protected override void ApplyPhysicsEffect(Rigidbody2D rigidbody)
    {
        if (!rigidbody.TryGetComponent(out PlayerMovement movement))
            return;

        if (HasOilCleanerProtection(rigidbody.GetComponent<Collider2D>()))
        {
            movement.SetHazardSpeedMultiplier(1f);
            return;
        }

        movement.SetHazardSpeedMultiplier(viscosity);
    }

    private bool HasOilCleanerProtection(Component playerComponent)
    {
        if (playerComponent == null)
            return false;

        if (!playerComponent.TryGetComponent(out PlayerRuntimeInventory runtimeInventory))
            return false;

        return runtimeInventory.IsOilCleanerProtectionActive.Value;
    }
}