using System;
using Unity.Netcode;
using UnityEngine;

public class AttachmentController : NetworkBehaviour
{
    [SerializeField] Transform _attachmentPoint;
    [SerializeField] float _attachmentDistance;

    private Interfaces.IAttachable _attachableItem;

    private void Update()
    {
        if (!IsOwner || !GameManager.Instance.IsGameActive.Value) return;
        if (_attachableItem != null && ((MonoBehaviour)_attachableItem == null))
            _attachableItem = null;
        // Geri dönüşüm kutusunda obje Despawn edildiği için objeyi bırakmayı beklemeden obje yok oluyor.
        // Bu durumda hayalet obje meydana geliyor. _attachableItem null değilse ama MonoBehaviour da değilse bu durumda HAYALETTİR.

        if (Input.GetMouseButtonDown(0)) // --> tıklandığında
        {
            Debug.Log("Click Down");
            if (_attachableItem == null) // --> elimiz boş ise
            {
                Debug.Log("Elimiz boş alınabilir");
                TryToPickupItem();
            }
        }

        if (Input.GetMouseButtonUp(0)) // --> tıklama bitttiğinde
        {
            Debug.Log("Click Up");
            if (_attachableItem != null) // --> Her click up anında olmaz elimiz dolu olmalı
            {
                var networkItem = _attachableItem as NetworkBehaviour;
                if (networkItem != null)
                {
                    _attachableItem.DetachServerRpc();
                    _attachableItem = null;
                }
                else
                {
                    Debug.Log("Spawn edilememiş bir obje. Bırakılamıyor.");
                    _attachableItem = null;
                }
            }
        }
    }

    // Raycast kaldırıldı OnMouseButton eklendi
    private void TryToPickupItem()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z; // Olmazsa daima Player'ı takip ediyor.
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Debug.Log(Camera.main.ScreenToWorldPoint(mouseScreenPos));

        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);
        if (hitCollider != null)
        {
            float dis = Vector2.Distance(transform.position, mouseWorldPos);
            if (dis < _attachmentDistance)
            {
                Debug.Log("Hit " + hitCollider.gameObject.name);
                Interfaces.IAttachable attachable = hitCollider.gameObject.GetComponent<Interfaces.IAttachable>();
                if (attachable != null)
                {
                    var networkItem = attachable as NetworkBehaviour;
                    if (networkItem != null && networkItem.IsSpawned)
                    {
                        attachable.AttachServerRpc();
                        _attachableItem = attachable;
                    }
                    else Debug.Log("Obje ağda spawn edilmedi.");
                }
            }
            else
            {
                Debug.Log("So far: " + dis);
            }
        }
    }

    public Transform GetAttachmentTransform()
    {
        return _attachmentPoint.transform;
    }
}