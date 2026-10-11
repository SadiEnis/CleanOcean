using Unity.Netcode;
using UnityEngine;

public class WaterFlow : BaseHazard
{
    enum Direction
    {
        Right,
        Left,
    }
    [SerializeField] private Direction direction = Direction.Right;
    [SerializeField] private float pushForce = 50f; 
    private Vector2 GetPushVector()
    {
        switch (direction)
        {
            case Direction.Right: return Vector2.right; // (1, 0)
            case Direction.Left:  return Vector2.left;  // (-1, 0)
            default: return Vector2.right;
        }
    }

    protected override void ApplyPhysicsEffect(Rigidbody2D rigidbody)
    {
        rigidbody.AddForce(GetPushVector() * pushForce, ForceMode2D.Force);
    }
}
