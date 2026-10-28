#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Lab07EscapeSetup
{
    private const string Maze = "Assets/Lab07/Scenes/Lab07_Laberinto.unity";
    private const float Top = 7.425f;
    private const float Bottom = -7.425f;

    [MenuItem("Tools/DSJ Lab 07/Agregar entrada salida y vida")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Sal de Play antes de configurar la victoria.");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Maze) == null)
        {
            Debug.LogError("Primero crea Lab07_Laberinto.unity.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene = EditorSceneManager.OpenScene(Maze, OpenSceneMode.Single);
        GameObject root = GameObject.Find("Paredes_Laberinto");
        GameObject player = GameObject.Find("Player");
        GameObject first = GameObject.Find("Enemy");
        GameObject second = GameObject.Find("Enemy_2");
        if (root == null || player == null || first == null || second == null)
        {
            Debug.LogError("Falta Paredes_Laberinto, Player, Enemy o Enemy_2.");
            return;
        }

        GameObject exit = GameObject.Find("Salida_Victoria");
        if (exit == null)
        {
            Transform topWall = FindLongBorder(root.transform, Top);
            Transform bottomWall = FindLongBorder(root.transform, Bottom);
            if (topWall == null || bottomWall == null)
            {
                Debug.LogError("No encontré las paredes exteriores. Revisa que el laberinto esté completo.");
                return;
            }
            SplitBorder(topWall, 0f, 1.65f, "Salida");
            SplitBorder(bottomWall, -9.9f, 1.65f, "Entrada");
            player.transform.position = new Vector3(-9.9f, -6.6f, -0.1f);
            exit = new GameObject("Salida_Victoria");
            exit.transform.position = new Vector3(0f, 8.05f, 0f);
            exit.transform.localScale = new Vector3(1.5f, 0.45f, 1f);
            SpriteRenderer oldWall = root.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer renderer = exit.AddComponent<SpriteRenderer>();
            renderer.sprite = oldWall.sprite;
            renderer.color = new Color(0.18f, 0.95f, 0.35f, 1f);
            renderer.sortingOrder = 1;
            BoxCollider2D trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
        }
        Lab07Game game = player.GetComponent<Lab07Game>();
        if (game == null) game = player.AddComponent<Lab07Game>();
        game.enabled = true;
        BoxCollider2D exitCollider = exit.GetComponent<BoxCollider2D>();
        if (exitCollider == null) exitCollider = exit.AddComponent<BoxCollider2D>();
        exitCollider.isTrigger = true;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == Maze)) scenes.Add(new EditorBuildSettingsScene(Maze, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Player tiene Lab07Game: " + (player.GetComponent<Lab07Game>() != null) +
                  "; salida con trigger: " + exitCollider.isTrigger +
                  ". Barra de vida y victoria/derrota reparadas.");
    }

    private static Transform FindLongBorder(Transform root, float y)
    {
        foreach (Transform child in root)
            if (Mathf.Abs(child.position.y - y) < 0.05f && child.localScale.x > 20f)
                return child;
        return null;
    }

    private static void SplitBorder(Transform wall, float gapCenter, float gapWidth, string label)
    {
        float left = wall.position.x - wall.localScale.x * 0.5f;
        float right = wall.position.x + wall.localScale.x * 0.5f;
        float gapLeft = gapCenter - gapWidth * 0.5f;
        float gapRight = gapCenter + gapWidth * 0.5f;
        GameObject rightHalf = Object.Instantiate(wall.gameObject, wall.parent);
        rightHalf.name = "wall_" + label + "_derecha";
        wall.name = "wall_" + label + "_izquierda";
        SetSpan(wall, left, gapLeft);
        SetSpan(rightHalf.transform, gapRight, right);
    }

    private static void SetSpan(Transform wall, float left, float right)
    {
        Vector3 pos = wall.position;
        pos.x = (left + right) * 0.5f;
        wall.position = pos;
        Vector3 scale = wall.localScale;
        scale.x = right - left;
        wall.localScale = scale;
    }
}
#endif
