#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DSJ.Lab07.EditorTools
{
    // Ejecutar una vez desde Tools > DSJ Lab 07 > Crear laberinto grande.
    // Conserva la escena original y todos los componentes del Player y Enemy.
    public static class Lab07MazeBuilder
    {
        private const string Source = "Assets/Lab07/Scenes/Lab07_Inicio.unity";
        private const string Destination = "Assets/Lab07/Scenes/Lab07_Laberinto.unity";
        private const int Columns = 15;
        private const int Rows = 9;
        private const float Cell = 1.65f;
        private const float Thickness = 0.38f;

        [MenuItem("Tools/DSJ Lab 07/Crear laberinto grande")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Sal de Play antes de crear el laberinto.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Source) == null)
            {
                Debug.LogError("Falta la escena inicial: " + Source);
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            bool destinationExists = AssetDatabase.LoadAssetAtPath<SceneAsset>(Destination) != null;
            if (!destinationExists)
            {
                Scene sourceScene = EditorSceneManager.OpenScene(Source, OpenSceneMode.Single);
                if (!EditorSceneManager.SaveScene(sourceScene, Destination, true))
                {
                    Debug.LogError("No se pudo copiar la escena inicial.");
                    return;
                }
            }

            Scene mazeScene = EditorSceneManager.OpenScene(Destination, OpenSceneMode.Single);
            if (GameObject.Find("Paredes_Laberinto") != null)
            {
                Debug.Log("El laberinto ya está construido; se abrió sin reemplazarlo.");
                return;
            }
            GameObject player = GameObject.Find("Player");
            GameObject enemy = GameObject.Find("Enemy");
            GameObject environment = GameObject.Find("Environment");
            AstarPath astar = UnityEngine.Object.FindFirstObjectByType<AstarPath>();
            if (player == null || enemy == null || environment == null)
            {
                Debug.LogError("Faltan Player, Enemy o Environment en la escena copiada.");
                return;
            }

            SpriteRenderer oldWall = environment.GetComponentInChildren<SpriteRenderer>();
            Sprite wallSprite = oldWall != null ? oldWall.sprite : null;
            if (wallSprite == null)
            {
                Debug.LogError("No se encontró el sprite de las paredes originales.");
                return;
            }

            for (int i = environment.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(environment.transform.GetChild(i).gameObject);

            Transform wallsRoot = new GameObject("Paredes_Laberinto").transform;
            wallsRoot.SetParent(environment.transform, false);

            // Muros sobre los bordes de cada celda. DFS garantiza un camino entre
            // todas las celdas; las aperturas extra añaden bucles y rutas alternativas.
            bool[,] vertical = new bool[Columns + 1, Rows];
            bool[,] horizontal = new bool[Columns, Rows + 1];
            for (int x = 0; x <= Columns; x++)
                for (int y = 0; y < Rows; y++) vertical[x, y] = true;
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y <= Rows; y++) horizontal[x, y] = true;

            bool[,] visited = new bool[Columns, Rows];
            var stack = new Stack<Vector2Int>();
            var random = new System.Random(7026);
            stack.Push(new Vector2Int(0, 0));
            visited[0, 0] = true;
            while (stack.Count > 0)
            {
                Vector2Int p = stack.Peek();
                var choices = new List<Vector2Int>(4);
                if (p.x > 0 && !visited[p.x - 1, p.y]) choices.Add(new Vector2Int(-1, 0));
                if (p.x + 1 < Columns && !visited[p.x + 1, p.y]) choices.Add(new Vector2Int(1, 0));
                if (p.y > 0 && !visited[p.x, p.y - 1]) choices.Add(new Vector2Int(0, -1));
                if (p.y + 1 < Rows && !visited[p.x, p.y + 1]) choices.Add(new Vector2Int(0, 1));
                if (choices.Count == 0) { stack.Pop(); continue; }
                Vector2Int d = choices[random.Next(choices.Count)];
                if (d.x != 0) vertical[p.x + (d.x > 0 ? 1 : 0), p.y] = false;
                else horizontal[p.x, p.y + (d.y > 0 ? 1 : 0)] = false;
                Vector2Int next = p + d;
                visited[next.x, next.y] = true;
                stack.Push(next);
            }

            // Plaza central abierta de 3 x 3 celdas, como en la referencia.
            for (int x = 6; x <= 8; x++)
                for (int y = 3; y <= 5; y++)
                {
                    if (x < 8) vertical[x + 1, y] = false;
                    if (y < 5) horizontal[x, y + 1] = false;
                }

            // Despejar conexiones alrededor de la plaza y abrir algunos atajos.
            vertical[6, 4] = false;
            vertical[9, 4] = false;
            horizontal[7, 3] = false;
            horizontal[7, 6] = false;
            for (int x = 1; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    if (vertical[x, y] && random.NextDouble() < 0.07) vertical[x, y] = false;
            for (int x = 0; x < Columns; x++)
                for (int y = 1; y < Rows; y++)
                    if (horizontal[x, y] && random.NextDouble() < 0.07) horizontal[x, y] = false;

            // Fusionar tramos consecutivos: menos objetos y colliders en la escena.
            int count = 0;
            for (int y = 0; y <= Rows; y++)
                for (int x = 0; x < Columns;)
                {
                    if (!horizontal[x, y]) { x++; continue; }
                    int start = x;
                    while (x < Columns && horizontal[x, y]) x++;
                    AddWall(wallsRoot, wallSprite, ++count,
                        ((start + x) * 0.5f - Columns * 0.5f) * Cell,
                        (y - Rows * 0.5f) * Cell,
                        (x - start) * Cell + Thickness, Thickness);
                }
            for (int x = 0; x <= Columns; x++)
                for (int y = 0; y < Rows;)
                {
                    if (!vertical[x, y]) { y++; continue; }
                    int start = y;
                    while (y < Rows && vertical[x, y]) y++;
                    AddWall(wallsRoot, wallSprite, ++count,
                        (x - Columns * 0.5f) * Cell,
                        ((start + y) * 0.5f - Rows * 0.5f) * Cell,
                        Thickness, (y - start) * Cell + Thickness);
                }

            player.transform.position = CellCenter(0, 0);
            enemy.transform.position = CellCenter(Columns - 1, Rows - 1);
            GameObject enemy2 = UnityEngine.Object.Instantiate(enemy);
            enemy2.name = "Enemy_2";
            enemy2.transform.position = CellCenter(Columns - 1, 0);
            SpriteRenderer enemy2Renderer = enemy2.GetComponent<SpriteRenderer>();
            if (enemy2Renderer != null) enemy2Renderer.color = new Color(1f, 0.3f, 0.42f, 1f);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 9.2f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
            }

            EditorSceneManager.MarkSceneDirty(mazeScene);
            EditorSceneManager.SaveScene(mazeScene);
            Debug.Log("Laberinto creado: " + count + " tramos y dos enemigos. Escena guardada: " + Destination);

            // El escaneo va al final. Si A* falla, las paredes y el segundo enemigo
            // ya quedan guardados y el grafo se puede escanear desde el Inspector.
            try
            {
                GridGraph grid = astar != null && astar.data != null ? astar.data.gridGraph : null;
                if (grid == null)
                {
                    Debug.LogWarning("Laberinto listo. Configura el Grid Graph para cubrir 28 x 18 unidades y pulsa Scan.");
                    return;
                }
                grid.center = Vector3.zero;
                grid.SetDimensions(82, 52, 0.35f);
                grid.collision.mask = 1 << 8; // Environment
                grid.collision.use2D = true;
                grid.collision.collisionCheck = true;
                grid.collision.diameter = 2.8f;
                grid.cutCorners = false;
                astar.Scan();
                EditorUtility.SetDirty(astar);
                EditorSceneManager.MarkSceneDirty(mazeScene);
                EditorSceneManager.SaveScene(mazeScene);
                Debug.Log("Grid Graph ampliado y escaneado para el nuevo laberinto.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Laberinto guardado, pero A* requiere Scan manual: " + ex.Message);
            }
        }

        private static Vector3 CellCenter(int x, int y)
        {
            return new Vector3((x + 0.5f - Columns * 0.5f) * Cell,
                (y + 0.5f - Rows * 0.5f) * Cell, -0.1f);
        }

        private static void AddWall(Transform parent, Sprite sprite, int index,
            float x, float y, float width, float height)
        {
            var wall = new GameObject("wall_maze_" + index.ToString("D3"));
            wall.layer = 8;
            wall.transform.SetParent(parent, false);
            wall.transform.position = new Vector3(x, y, 0f);
            wall.transform.localScale = new Vector3(width, height, 1f);
            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.95f, 0.95f, 0.92f, 1f);
            wall.AddComponent<BoxCollider2D>();
        }
    }
}
#endif
