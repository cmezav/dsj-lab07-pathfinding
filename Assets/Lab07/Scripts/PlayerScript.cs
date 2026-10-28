using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        direction = Vector2.zero;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) direction.x--;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) direction.x++;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) direction.y++;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) direction.y--;

        direction.Normalize();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }
}