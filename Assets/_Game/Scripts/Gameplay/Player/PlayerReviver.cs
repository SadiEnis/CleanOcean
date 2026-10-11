using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerReviver : NetworkBehaviour
{
    [SerializeField] private float reviveRange = 2f;
    [SerializeField] private LayerMask layerMask;

    private PlayerHealth _myOwnHealth; // Baygın olup olmadığımı tutmak için..

    public override void OnNetworkSpawn()
    {
        _myOwnHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_myOwnHealth.IsDead.Value) return;

        if (Input.GetMouseButtonDown(0))
            TryReviveTeammate();
    }

    private void TryReviveTeammate()
    {
        Vector3 mouseScreenPos =  Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, layerMask);

        if (hit != null)
        {
            if (hit.TryGetComponent(out PlayerHealth targetHealth))
            {
                if (targetHealth == _myOwnHealth) return;
                if (!targetHealth.IsDead.Value) return;

                float dis = Vector2.Distance(transform.position, targetHealth.transform.position);
                if (dis <= reviveRange)
                    targetHealth.ReviveServerRpc();
                else
                    Debug.Log("Hedeften uzaksın!");
            }
        }
    }
}