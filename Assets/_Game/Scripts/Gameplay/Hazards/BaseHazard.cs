using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseHazard : NetworkBehaviour
{
    [FormerlySerializedAs("_damagePerSecond")] [SerializeField] protected int damagePerSecond = 5;
    
    protected float _damageTimer;
    
    protected virtual void OnEnterHazard(Collider2D other) { }
    protected virtual void OnExitHazard(Collider2D other) { }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        OnEnterHazard(other); // OilSpill burada devreye girecek
    }

    // Unity'nin kendi Exit olayı
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        OnExitHazard(other); // OilSpill burada devreye girecek
    }
    
    protected virtual void ApplyPhysicsEffect(Rigidbody2D rigidbody) { }
    protected virtual void ApplyDamageEffect(PlayerHealth health)
    {
        _damageTimer += Time.deltaTime;
        if (_damageTimer >= 1f)
        {
            health.TakeDamage(damagePerSecond);
            _damageTimer = 0f;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsServer) return;
        if (other.TryGetComponent(out NetworkObject netObj))
        {
            if (other.TryGetComponent(out Rigidbody2D rb))
                ApplyPhysicsEffect(rb);

            if (other.TryGetComponent(out PlayerHealth playerHealth))
                ApplyDamageEffect(playerHealth);
        }
    }
}
