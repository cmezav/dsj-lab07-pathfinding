#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Creates the eight-wall starting scene from the laboratory guide on first import.
// This script deliberately has no dependency on the third-party A* package.
namespace DSJ.Lab07.EditorTools
{
    public static class Lab07SceneBuilder
    {
        private const string Root = "Assets/Lab07";
        private const string Generated = Root + "/Generated";
        public const string ScenePath = "Assets/Lab07/Scenes/Lab07_Inicio.unity";

        [MenuItem("Tools/DSJ Lab 07/Open Starting Scene")]
        public static void OpenScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                BuildScene();
                return;
            }
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Tools/DSJ Lab 07/Build Starting Scene (if missing)")]
        public static void BuildScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                OpenScene();
                return;
            }

            EnsureFolders();
            NameEnvironmentLayer();

            Sprite square = MakeSprite("WhiteSquare", false);
            Sprite circle = MakeSprite("RoundMarker", true);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 6.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.16f, 0.23f, 1f);
            camera.gameObject.AddComponent<AudioListener>();

            Transform environment = new GameObject("Environment").transform;
            Color white = new Color(0.96f, 0.96f, 0.93f, 1f);

            // Four boundary walls. The four short internal walls leave passages
            // around them so the eventual enemy can find a path to the player.
            AddWall(environment, square, "wall",     0f,  5.1f, 18.4f, 0.45f, white);
            AddWall(environment, square, "wall (1)", 0f, -5.1f, 18.4f, 0.45f, white);
            AddWall(environment, square, "wall (2)", -9.2f, 0f, 0.45f, 10.65f, white);
            AddWall(environment, square, "wall (3)",  9.2f, 0f, 0.45f, 10.65f, white);
            AddWall(environment, square, "wall (4)", -5.1f, 0f, 0.42f, 3.5f, white);
            AddWall(environment, square, "wall (5)",  5.1f, 0f, 0.42f, 3.5f, white);
            AddWall(environment, square, "wall (6)",  0f, 2.25f, 4.5f, 0.42f, white);
            AddWall(environment, square, "wall (7)",  0f,-2.25f, 4.5f, 0.42f, white);

            AddCharacter("Player", new Vector2(-6.65f, 0f), circle,
                new Color(0.24f, 0.77f, 1f), true);
            AddCharacter("Enemy", new Vector2(0f, 0f), circle,
                new Color(1f, 0.56f, 0.22f), false);

            new GameObject("PathFinding (add AstarPath after import)");

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.in2DMode = true;
            }
            Debug.Log("Lab 07 listo: escena 2D, 8 paredes con BoxCollider2D, Player y Enemy provisionales.");
        }

        private static void AddWall(Transform parent, Sprite square, string name,
            float x, float y, float width, float height, Color color)
        {
            GameObject wall = new GameObject(name);
            wall.layer = 8; // Environment
            wall.transform.SetParent(parent, false);
            wall.transform.position = new Vector3(x, y, 0f);
            wall.transform.localScale = new Vector3(width, height, 1f);
            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            wall.AddComponent<BoxCollider2D>();
        }

        private static void AddCharacter(string name, Vector2 position, Sprite sprite,
            Color color, bool player)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = new Vector3(position.x, position.y, -0.1f);
            obj.transform.localScale = Vector3.one * 0.8f;
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 2;
            obj.AddComponent<CircleCollider2D>();
            if (player)
            {
                Rigidbody2D body = obj.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        private static Sprite MakeSprite(string name, bool round)
        {
            string path = Generated + "/" + name + ".png";
            string absolutePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
            if (!File.Exists(absolutePath))
            {
                Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float dx = x - 31.5f;
                    float dy = y - 31.5f;
                    pixels[y * 64 + x] = !round || dx * dx + dy * dy < 29f * 29f
                        ? Color.white : Color.clear;
                }
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 64f)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64f;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new System.InvalidOperationException("No se pudo importar " + path);
            return sprite;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(Generated))
                AssetDatabase.CreateFolder(Root, "Generated");
            if (!AssetDatabase.IsValidFolder(Root + "/Scenes"))
                AssetDatabase.CreateFolder(Root, "Scenes");
        }

        private static void NameEnvironmentLayer()
        {
            Object[] settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (settings == null || settings.Length == 0) return;
            SerializedObject tags = new SerializedObject(settings[0]);
            SerializedProperty layers = tags.FindProperty("layers");
            if (layers == null || layers.arraySize <= 8) return;
            layers.GetArrayElementAtIndex(8).stringValue = "Environment";
            tags.ApplyModifiedProperties();
        }
    }

    [InitializeOnLoad]
    internal static class Lab07FirstImport
    {
        static Lab07FirstImport()
        {
            EditorApplication.delayCall += BuildWhenReady;
        }

        private static void BuildWhenReady()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildWhenReady;
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Lab07SceneBuilder.ScenePath) == null)
                Lab07SceneBuilder.BuildScene();
        }
    }
}
#endif
