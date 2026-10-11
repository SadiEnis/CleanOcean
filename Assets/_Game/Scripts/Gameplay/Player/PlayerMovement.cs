using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Transform _visual;

    Rigidbody2D _rb;
    PlayerHealth _health;
    Animator _animator;
    SpriteRenderer _spriteRenderer;

    private NetworkVariable<Vector2> _networkMoveInput = new NetworkVariable<Vector2>(
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private Vector2 _localInput;
    private int _lastHorizontalDirection = 1;
    private float _passiveSpeedMultiplier = 1f;
    private float _hazardSpeedMultiplier = 1f;

    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<PlayerHealth>();
        _animator = _visual.GetComponent<Animator>();
        _spriteRenderer = _visual.GetComponent<SpriteRenderer>();
    }

    public void SetPassiveSpeedMultiplier(float multiplier)
    {
        _passiveSpeedMultiplier = multiplier;
    }

    public void SetHazardSpeedMultiplier(float multiplier)
    {
        _hazardSpeedMultiplier = multiplier;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_health.IsDead.Value || (GameManager.Instance != null && !GameManager.Instance.IsGameActive.Value))
        {
            _networkMoveInput.Value = Vector2.zero;
            return;
        }

        _localInput.x = Input.GetAxisRaw("Horizontal");
        _localInput.y = Input.GetAxisRaw("Vertical");

        if (_localInput.magnitude > 1) _localInput.Normalize();

        _networkMoveInput.Value = _localInput;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            ApplyMovement();
        }
    }

    private void LateUpdate()
    {
        UpdateAnimation();
        UpdateFacing();
    }

    private void ApplyMovement()
    {
        float finalSpeedMultiplier = _passiveSpeedMultiplier * _hazardSpeedMultiplier;
        Vector2 move = _networkMoveInput.Value * _moveSpeed * finalSpeedMultiplier;
        _rb.linearVelocity = move;
    }

    private void UpdateAnimation()
    {
        Vector2 moveInput = _networkMoveInput.Value;
        float speed = moveInput.magnitude;
        
        _animator.SetFloat("Speed", speed);
    }
    
    private void UpdateFacing()
    {
        Vector2 direction = _networkMoveInput.Value;
        
        if (direction.x > 0.001f)
            _spriteRenderer.flipY = false;
        else if (direction.x < -0.001f)
            _spriteRenderer.flipY = true;

        if (direction.sqrMagnitude < 0.001f)
        {
            ApplyIdleFacing();
            return;
        }
        
        ApplyMovementFacing(direction);
    }

    private void ApplyIdleFacing()
    {
        if (_lastHorizontalDirection == 1)
            _visual.rotation = Quaternion.Euler(0f,0f,0f);
        else
            _visual.rotation = Quaternion.Euler(0f, 0f, 180f);
    }

    private void ApplyMovementFacing(Vector2 direction)
    {
        if (direction.x > 0.01f)
            _lastHorizontalDirection = 1;
        else if (direction.x < -0.01f)
            _lastHorizontalDirection = -1;
            
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _visual.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}