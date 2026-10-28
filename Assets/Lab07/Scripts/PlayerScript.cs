using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 360f; // Grados por segundo

    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        direction = Vector2.zero;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction.x--;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction.x++;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction.y++;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction.y--;

        direction.Normalize();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (direction.sqrMagnitude > 0f)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float nextAngle = Mathf.MoveTowardsAngle(
                rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(nextAngle);
        }
    }
}