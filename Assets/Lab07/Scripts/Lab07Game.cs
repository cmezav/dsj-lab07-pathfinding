using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class Lab07Game : MonoBehaviour
{
    [Min(1f)] public float maxHealth = 100f;
    [Min(0f)] public float damagePerSecond = 18f;

    private float health;
    private bool finished;
    private bool victory;
    private Collider2D playerCollider;
    private Collider2D[] enemyColliders;
    private Collider2D exitCollider;

    private void Awake()
    {
        health = maxHealth;
        playerCollider = GetComponent<Collider2D>();
        GameObject enemy1 = GameObject.Find("Enemy");
        GameObject enemy2 = GameObject.Find("Enemy_2");
        enemyColliders = new Collider2D[]
        {
            enemy1 != null ? enemy1.GetComponent<Collider2D>() : null,
            enemy2 != null ? enemy2.GetComponent<Collider2D>() : null
        };
        GameObject exit = GameObject.Find("Salida_Victoria");
        exitCollider = exit != null ? exit.GetComponent<Collider2D>() : null;
    }

    private void FixedUpdate()
    {
        if (finished || playerCollider == null) return;
        foreach (Collider2D enemy in enemyColliders)
        {
            if (enemy == null || !enemy.enabled) continue;
            ColliderDistance2D contact = playerCollider.Distance(enemy);
            if (contact.isValid && contact.distance <= 0.05f)
                health = Mathf.Max(0f, health - damagePerSecond * Time.fixedDeltaTime);
        }
        if (health <= 0f) { EndGame(false); return; }

        if (exitCollider != null && exitCollider.enabled)
        {
            ColliderDistance2D exitContact = playerCollider.Distance(exitCollider);
            if (exitContact.isValid && exitContact.distance <= 0f) EndGame(true);
        }
    }

    private void EndGame(bool won)
    {
        victory = won;
        finished = true;
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        if (finished) Time.timeScale = 1f;
    }

    private void OnGUI()
    {
        float width = Mathf.Min(Screen.width - 32f, 290f);
        GUI.Box(new Rect(16f, 16f, width, 72f), "ESCAPA DEL LABERINTO");
        GUI.Label(new Rect(28f, 42f, width - 24f, 22f), "Vida: " + Mathf.CeilToInt(health) + " / " + Mathf.CeilToInt(maxHealth));
        Rect bar = new Rect(28f, 66f, width - 24f, 12f);
        GUI.color = new Color(0.22f, 0.22f, 0.22f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        bar.width *= Mathf.Clamp01(health / maxHealth);
        GUI.color = health < 30f ? new Color(1f, 0.25f, 0.25f) : new Color(0.28f, 0.85f, 0.42f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (!finished) return;
        float x = Screen.width * 0.5f - 160f;
        float y = Screen.height * 0.5f - 58f;
        GUI.Box(new Rect(x, y, 320f, 116f), victory ? "¡ESCAPASTE!" : "TE ATRAPARON");
        GUI.Label(new Rect(x + 52f, y + 34f, 230f, 24f), victory ? "Encontraste la salida." : "Te quedaste sin vida.");
        if (GUI.Button(new Rect(x + 72f, y + 70f, 176f, 32f), "VOLVER A INTENTAR"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
