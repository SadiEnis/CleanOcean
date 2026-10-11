using System;
using Unity.Netcode;
using UnityEngine;
using ScriptableObjects;

public class TrashItem : NetworkBehaviour, Interfaces.IAttachable, Interfaces.IRecyclable, Interfaces.ISpawnable

{
    [SerializeField] private NetworkVariable<bool> isHeld = new NetworkVariable<bool>(false);
    [SerializeField] private TrashSO _trashData;

    private NetworkVariable<NetworkObjectReference> _holdingPlayer = new NetworkVariable<NetworkObjectReference>();
    private AttachmentController _attachmentController;
    private NetworkObject _networkObject;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriterenderer;

    public NetworkVariable<NetworkObjectReference> GetHoldingPlayer => _holdingPlayer;
    public int GetScoreValue => _trashData != null ? _trashData.ScoreValue : 0;
    public TrashSO TrashData => _trashData;


    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _spriterenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (_trashData == null) return;
        if (_spriterenderer != null)
            _spriterenderer.sprite = _trashData.TrashSprite;
        gameObject.name = _trashData.TrashName;

        if (GameEventsManager.Instance != null)
            GameEventsManager.Instance.OnEnterRecyclableObject += TrashItem_OnEnterRecyclableObject;
    }

    public override void OnDestroy()
    {
        if (GameEventsManager.Instance != null)
            GameEventsManager.Instance.OnEnterRecyclableObject -= TrashItem_OnEnterRecyclableObject;
        base.OnDestroy();
    }

    private void TrashItem_OnEnterRecyclableObject(object sender, GameEventsManager.OnEnterRecyclableObjectArgs e)
    {
        if (e.recyclableObject as MonoBehaviour != this) return;
        RecycleServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    public void AttachServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isHeld.Value) return; // Zaten tutuluyorsa Attach metodu aktif olmayacak

        ulong requesterClientId = rpcParams.Receive.SenderClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(requesterClientId, out var reference))
        {
            Debug.Log("1");
            NetworkObject playerNetworkObject = reference.PlayerObject;
            if (playerNetworkObject != null)
            {
                Debug.Log("2");
                _networkObject.ChangeOwnership(NetworkManager.ServerClientId);
                _holdingPlayer.Value = new NetworkObjectReference(playerNetworkObject);
                isHeld.Value = true;
                SetVisualsClientRpc(isHeld.Value);
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void DetachServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!isHeld.Value) return;

        isHeld.Value = false;
        _holdingPlayer.Value = new NetworkObjectReference();
        SetVisualsClientRpc(isHeld.Value);
    }

    [ClientRpc]
    private void SetVisualsClientRpc(bool isGrabbed)
    {
        if (isGrabbed)
        {
            gameObject.layer = LayerMask.NameToLayer("HeldTrash");
            if (_rb)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            if (_collider)
            {
                _collider.enabled = true;
                _collider.isTrigger = true;
            }
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Trash");
            if (_rb)
                _rb.isKinematic = false;
            if (_collider)
            {
                _collider.enabled = true;
                _collider.isTrigger = false;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RecycleServerRpc()
    {
        if (!IsServer) return;
        if (isHeld.Value)
        {
            isHeld.Value = false;
        }
        
        
        GameEventsManager.Instance.TriggerOnRecycledItem(this);
        NetworkDespawn();
    }

    public void NetworkDespawn()
    {
        if (IsServer && IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
            // SFX VFX eklenebilir
        }
    }

    private void Update()
    {
        if (isHeld.Value) // --> cisim tutuluyorsa
        {
            if (_holdingPlayer.Value.TryGet(out NetworkObject player))
            {
                if (_attachmentController == null)
                    _attachmentController = player.GetComponent<AttachmentController>();
                if (_attachmentController != null)
                {
                    transform.position = _attachmentController.GetAttachmentTransform().position;
                    transform.rotation = _attachmentController.GetAttachmentTransform().rotation;
                }
            }
        }
        else
        {
            if (_attachmentController != null) _attachmentController = null;
        }
    }
}