using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class EnemyRotation : MonoBehaviour
{
    public float turnSpeed = 360f;
    public float stopTurningDistance = 0.9f;

    private AIPath ai;

    void Awake()
    {
        ai = GetComponent<AIPath>();
        ai.enableRotation = false;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
            body.freezeRotation = true;
    }

    void LateUpdate()
    {
        Vector2 toTarget = (Vector2)(ai.destination - transform.position);
        if (toTarget.sqrMagnitude <= stopTurningDistance * stopTurningDistance)
            return;

        Vector3 desired = ai.desiredVelocity;
        Vector2 direction = new Vector2(desired.x, desired.y);
        if (direction.sqrMagnitude < 0.04f)
            return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            targetAngle,
            turnSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}