using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class EnemyRotation : MonoBehaviour
{
    public float turnSpeed = 360f;

    private AIPath ai;

    void Awake()
    {
        ai = GetComponent<AIPath>();
        ai.enableRotation = false;
    }

    void LateUpdate()
    {
        Vector2 direction = ai.velocity;
        if (direction.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            targetAngle,
            turnSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}