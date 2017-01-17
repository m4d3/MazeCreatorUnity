/* Maze Creation Script by Matthias Ewald
 *  
 * Using Recursive Backtracking Algorithm
 * 
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MazeCreator : EditorWindow
{
    private bool addFloor;
    private bool addWalls;
    private List<Vector2> cells;
    private bool clearTiles;
    private GameObject container;

    private MazeData data;
    private Vector2 endTile;
    private Vector2 entryTile;
    private List<Vector2> floor;
    private GameObject floorPrefab;
    private bool generateCollider;
    private int height;
    private int minPathLength;
    private float processTime;
    private bool randomizeMaze;
    private List<Vector2> solvedPath;

    private List<Vector2> unvisitedCells;
    private GameObject wallPrefab;
    private List<Vector2> walls;
    private int width;

    private Vector2 startTile;

    [MenuItem("Maze And Grid Tools/Show Maze Creator")]
    private static void Create()
    {
        var window = (MazeCreator) GetWindow(typeof(MazeCreator));
    }

    private void OnGUI()
    {
        width = EditorGUILayout.IntField("Maze Width:", width);
        height = EditorGUILayout.IntField("Maze Height:", height);
        container = EditorGUILayout.ObjectField("Container", container, typeof(Object), true) as GameObject;
        wallPrefab = EditorGUILayout.ObjectField("Wall", wallPrefab, typeof(Object), true) as GameObject;
        floorPrefab = EditorGUILayout.ObjectField("Floor", floorPrefab, typeof(Object), true) as GameObject;
        addFloor = EditorGUILayout.Toggle("Add Floor", addFloor);
        addWalls = EditorGUILayout.Toggle("Add Walls", addWalls);
        generateCollider = EditorGUILayout.Toggle("Generate Collider", generateCollider);


        if (GUILayout.Button("Create Maze"))
        {
            Debug.Log("Creating Maze");
            processTime = Time.realtimeSinceStartup;
            CreateMaze();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log("Maze created in: " + processTime + "seconds");
        }

        if (GUILayout.Button("Combine Meshes"))
            if (container.transform.childCount > 0)
            {
                CombineMeshes(container.transform.FindChild("Maze_Walls").gameObject);
                if (container.transform.FindChild("Maze_Floor"))
                    CombineMeshes(container.transform.FindChild("Maze_Floor").gameObject);

                if (container.transform.FindChild("Maze_Path"))
                    CombineMeshes(container.transform.FindChild("Maze_Path").gameObject);
            }

        if (GUILayout.Button("Solve Maze"))
        {
            SolveMaze(floor);
            DrawTiles(solvedPath, "Maze_Path");
        }

        if (GUILayout.Button("Set Selected As Start"))
        {
            entryTile = new Vector2((int) Selection.transforms[0].position.x, (int) Selection.transforms[0].position.z);
        }

        if (GUILayout.Button("Set Selected As End"))
            endTile = new Vector2((int) Selection.transforms[0].position.x, (int) Selection.transforms[0].position.z);

        if (GUILayout.Button("RandomSolve"))
        {
            if (randomizeMaze)
                CreateMaze();

            solvedPath.Clear();

            while (solvedPath.Count < minPathLength)
            {
                entryTile = endTile = floor[Random.Range(0, floor.Count)];
                while (endTile == entryTile) endTile = floor[Random.Range(0, floor.Count)];
                SolveMaze(floor);
            }

            DrawTiles(solvedPath, "Maze_Path");
        }

        minPathLength = EditorGUILayout.IntField("Minimum path length:", minPathLength);

        clearTiles = EditorGUILayout.Toggle("Clear Existing", clearTiles);
        randomizeMaze = EditorGUILayout.Toggle("randomizeMaze", randomizeMaze);
    }

    private void CreateMaze()
    {
        if (width > 0 && height > 0)
        {
            if (container == null) container = new GameObject("Maze");
            else foreach (Transform child in container.transform) DestroyImmediate(child.gameObject);

            data = !container.GetComponent<MazeData>() ? container.AddComponent<MazeData>() : container.GetComponent<MazeData>();

            unvisitedCells = new List<Vector2>();
            walls = new List<Vector2>();
            cells = new List<Vector2>();
            floor = new List<Vector2>();

            for (var x = 0; x < 1 + width * 2; x++)
            for (var y = 0; y < 1 + height * 2; y++)
                if ((x + 1) % 2 == 0 && (y + 1) % 2 == 0)
                {
                    unvisitedCells.Add(new Vector2(x, y));
                    floor.Add(new Vector2(x, y));
                }
                else
                {
                    walls.Add(new Vector2(x, y));
                }

            var startCell = unvisitedCells[Random.Range(0, unvisitedCells.Count)];

            cells.Add(startCell);

            while (cells.Count > 0) CheckNeighbors(cells[cells.Count - 1]);

            if (addWalls)
            {
                var wallsContainer = new GameObject("Maze_Walls");
                wallsContainer.transform.parent = container.transform;

                foreach (var wallPos in walls)
                {
                    GameObject wall;

                    if (wallPrefab != null)
                        wall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity);
                    else
                        wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.position = new Vector3(wallPos.x * wall.transform.localScale.x, 0,
                        wallPos.y * wall.transform.localScale.z);
                    wall.transform.parent = wallsContainer.transform;
                }
            }

            if (addFloor)
            {
                var floorContainer = new GameObject("Maze_Floor");
                floorContainer.transform.parent = container.transform;

                foreach (var floorPos in floor)
                {
                    GameObject floorObj;

                    if (floorPrefab != null)
                    {
                        floorObj = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity);
                    }
                    else
                    {
                        floorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (wallPrefab != null)
                        {
                            floorObj.transform.localScale = new Vector3(wallPrefab.transform.localScale.x, 0.1f,
                                wallPrefab.transform.localScale.z);
                            floorObj.transform.position = new Vector3(floorPos.x * wallPrefab.transform.localScale.x,
                                -wallPrefab.transform.localScale.y / 2, floorPos.y * wallPrefab.transform.localScale.z);
                        }
                        else
                        {
                            floorObj.transform.localScale = new Vector3(1, 0.1f, 1);
                            floorObj.transform.position = new Vector3(floorPos.x, -0.5f, floorPos.y);
                        }
                    }

                    floorObj.transform.parent = floorContainer.transform;
                }
            }

            data.wallTiles = walls;
            data.floorTiles = floor;
        }
        else
        {
            ShowNotification(new GUIContent("Please define width and height"));
        }
    }

    private void CheckNeighbors(Vector2 cell)
    {
        unvisitedCells.Remove(cell);
        Vector2 newCell;
        var neighbors = new List<Vector2>();

        neighbors.Add(new Vector2(cell.x + 2, cell.y));
        neighbors.Add(new Vector2(cell.x - 2, cell.y));
        neighbors.Add(new Vector2(cell.x, cell.y + 2));
        neighbors.Add(new Vector2(cell.x, cell.y - 2));

        var newCells = new List<Vector2>();

        for (var i = 0; i < neighbors.Count; i++) if (unvisitedCells.Contains(neighbors[i])) newCells.Add(neighbors[i]);

        if (newCells.Count > 0)
        {
            var side = Random.Range(0, newCells.Count);
            newCell = newCells[side];
            cells.Add(newCell);
            var passage = Vector2.zero;

            if (newCells[side].x > cell.x) passage = new Vector2(cell.x + 1, cell.y);
            if (newCells[side].x < cell.x) passage = new Vector2(cell.x - 1, cell.y);
            if (newCells[side].y > cell.y) passage = new Vector2(cell.x, cell.y + 1);
            if (newCells[side].y < cell.y) passage = new Vector2(cell.x, cell.y - 1);

            walls.Remove(passage);
            if (!floor.Contains(passage)) floor.Add(passage);
        }
        else
        {
            cells.RemoveAt(cells.Count - 1);
        }
    }

    private void CombineMeshes(GameObject objContainer)
    {
        objContainer.AddComponent<MeshFilter>();
        objContainer.AddComponent<MeshRenderer>();
        var meshFilters = objContainer.GetComponentsInChildren<MeshFilter>();
        var combine = new CombineInstance[meshFilters.Length];
        var i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        objContainer.transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        objContainer.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        if (generateCollider)
        {
            var collider = objContainer.AddComponent(typeof(MeshCollider)) as MeshCollider;
            collider.sharedMesh = objContainer.GetComponent<MeshFilter>().sharedMesh;
        }

        //if (material != null) {
        //    renderer.material = material;
        //}        

        objContainer.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
        objContainer.gameObject.SetActive(true);

        for (var j = meshFilters.Length - 1; j > 0; j--) DestroyImmediate(meshFilters[j].gameObject);
    }

    private void SolveMaze(List<Vector2> maze)
    {
        // entryTile = new Vector2(1, 1);
        // endTile = new Vector2(width * 2 - 1, height * 2 - 1);

        var start = new PathTile(entryTile);
        var end = new PathTile(endTile);

        solvedPath = new List<Vector2>();
        var visitedCells = new List<Vector2>();

        var queue = new Queue<PathTile>();
        queue.Enqueue(start);

        var finished = false;

        while (queue.Count > 0 && !finished)
        {
            var current = queue.Dequeue();

            if (!visitedCells.Contains(current.position))
            {
                var neighbors = new List<Vector2>();

                neighbors.Add(new Vector2(current.position.x + 1, current.position.y));
                neighbors.Add(new Vector2(current.position.x - 1, current.position.y));
                neighbors.Add(new Vector2(current.position.x, current.position.y + 1));
                neighbors.Add(new Vector2(current.position.x, current.position.y - 1));

                for (var i = 0; i < neighbors.Count; i++)
                    if (maze.Contains(neighbors[i]) && !visitedCells.Contains(neighbors[i]))
                    {
                        Debug.Log("Neighbor found");
                        if (neighbors[i] == end.position)
                        {
                            end.parent = current;
                            finished = true;
                        }
                        else
                        {
                            var tile = new PathTile(neighbors[i]);
                            tile.parent = current;
                            queue.Enqueue(tile);
                        }
                    }
            }
            visitedCells.Add(current.position);
        }

        var curTile = end;

        while (curTile.parent != null)
        {
            solvedPath.Add(curTile.position);
            curTile = curTile.parent;
        }

        solvedPath.Add(start.position);

        data.pathTiles = new List<Vector2>();
        data.pathTiles.AddRange(solvedPath);
        data.pathTiles.Reverse();
    }

    private void DrawTiles(List<Vector2> tiles, string name)
    {
        if (GameObject.Find(name) && clearTiles)
            DestroyImmediate(GameObject.Find(name));
        var parentContainer = new GameObject(name);
        parentContainer.transform.parent = container.transform;

        foreach (var tile in tiles)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

            wall.transform.position = new Vector3(tile.x * wall.transform.localScale.x, 1,
                tile.y * wall.transform.localScale.z);
            wall.transform.parent = parentContainer.transform;
        }
    }

    private class PathTile
    {
        public PathTile parent;
        public Vector2 position;

        public PathTile(Vector2 pos)
        {
            position = pos;
        }
    }
}