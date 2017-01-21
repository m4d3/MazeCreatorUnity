/* Maze Creation Script by Matthias Ewald
 *  
 * Using Recursive Backtracking Algorithm
 * 
*/

using System.Collections.Generic;
using System.Linq;
using Assets.MazeTools.Scripts;
using UnityEditor;
using UnityEngine;

public class MazeCreator : EditorWindow
{
    private bool _addFloor;
    private bool _addWalls;
    private int _borderWidth;
    private List<Vector2> _cells;
    private bool _clearTiles;
    private GameObject _container;
    private bool _cutCorners;

    private MazeData _data;
    private Vector2 _endTile;
    private Vector2 _entryTile;
    private List<Vector2> _floor;
    private GameObject _floorPrefab;
    private bool _generateCollider;
    private int _height;
    private int _minPathLength;
    private List<Vector2> _pathBorder;
    private float _processTime;
    private bool _randomizeMaze;
    private List<Vector2> _solvedPath;

    private List<Vector2> _unvisitedCells;
    private GameObject _wallPrefab;
    private List<Vector2> _walls;
    private int _width;

    [MenuItem("MazeCreator/Show Maze Creator")]
    private static void Create()
    {
        MazeCreator window = (MazeCreator) GetWindow(typeof(MazeCreator));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Maze Parameters");

        EditorGUILayout.BeginHorizontal();

        _width = EditorGUILayout.IntField("Maze Width:", _width);
        _height = EditorGUILayout.IntField("Maze Height:", _height);

        EditorGUILayout.EndHorizontal();

        _container = EditorGUILayout.ObjectField("Container", _container, typeof(Object), true) as GameObject;
        _wallPrefab = EditorGUILayout.ObjectField("Wall", _wallPrefab, typeof(Object), true) as GameObject;
        _floorPrefab = EditorGUILayout.ObjectField("Floor", _floorPrefab, typeof(Object), true) as GameObject;

        EditorGUILayout.BeginHorizontal();

        _addFloor = EditorGUILayout.Toggle("Add Floor", _addFloor);
        _addWalls = EditorGUILayout.Toggle("Add Walls", _addWalls);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Maze"))
        {
            Debug.Log("Creating Maze");
            _processTime = Time.realtimeSinceStartup;
            CreateMaze();
            _processTime = Time.realtimeSinceStartup - _processTime;
            Debug.Log("Maze created in: " + _processTime + "seconds");
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Combine Meshes"))
            if (_container != null && _container.transform.childCount > 0)
            {
                CombineMeshes(_container.transform.FindChild("Maze_Walls").gameObject);
                if (_container.transform.FindChild("Maze_Floor"))
                    CombineMeshes(_container.transform.FindChild("Maze_Floor").gameObject);

                if (_container.transform.FindChild("Maze_Path"))
                    CombineMeshes(_container.transform.FindChild("Maze_Path").gameObject);
            }

        _generateCollider = EditorGUILayout.ToggleLeft("Generate Collider", _generateCollider);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Maze Solving");
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Set Selected As Start"))
            _entryTile = new Vector2((int) Selection.transforms[0].position.x, (int) Selection.transforms[0].position.z);

        if (GUILayout.Button("Set Selected As End"))
            _endTile = new Vector2((int) Selection.transforms[0].position.x, (int) Selection.transforms[0].position.z);

        if (GUILayout.Button("Solve Maze")) {
            _solvedPath = SolveMaze(_floor, _entryTile, _endTile);

            _data.PathTiles = _solvedPath;

            DrawTiles(_solvedPath, "Maze_Path");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("RandomSolve"))
        {
            if (_randomizeMaze)
                CreateMaze();

            _solvedPath.Clear();

            while (_solvedPath.Count < _minPathLength)
            {
                _entryTile = _endTile = _floor[Random.Range(0, _floor.Count)];
                while (_endTile == _entryTile) _endTile = _floor[Random.Range(0, _floor.Count)];
                _solvedPath = SolveMaze(_floor, _entryTile, _endTile);
            }

            _data.PathTiles = new List<Vector2>();
            _data.PathTiles.AddRange(_solvedPath);
            _data.PathTiles.Reverse();

            DrawTiles(_solvedPath, "Maze_Path");
        }

        _minPathLength = EditorGUILayout.IntField("Minimum path length:", _minPathLength);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        _clearTiles = EditorGUILayout.Toggle("Clear Existing", _clearTiles);
        _randomizeMaze = EditorGUILayout.Toggle("randomizeMaze", _randomizeMaze);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Maze Path Border");
        EditorGUILayout.BeginHorizontal();

        _borderWidth = EditorGUILayout.IntField("Border Width:", _borderWidth);
        _cutCorners = EditorGUILayout.Toggle("Cut Corners", _cutCorners);

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Build Pathborder")) {
            _pathBorder = new List<Vector2>();

            _pathBorder = BuildBorder(_solvedPath, !_cutCorners);

            for (int i = 0; i < _borderWidth - 1; i++) _pathBorder.AddRange(BuildBorder(_pathBorder, !_cutCorners));
            foreach (Vector2 tile in _solvedPath) _pathBorder.Remove(tile);

            DrawTiles(_pathBorder, "Path_Border");
        }
    }

    private void CreateMaze()
    {
        if (_width > 0 && _height > 0)
        {
            if (_container == null) _container = new GameObject("Maze");
            else foreach (Transform child in _container.transform) DestroyImmediate(child.gameObject);

            _data = !_container.GetComponent<MazeData>()
                ? _container.AddComponent<MazeData>()
                : _container.GetComponent<MazeData>();

            _unvisitedCells = new List<Vector2>();
            _walls = new List<Vector2>();
            _cells = new List<Vector2>();
            _floor = new List<Vector2>();

            for (int x = 0; x < 1 + _width * 2; x++)
            for (int y = 0; y < 1 + _height * 2; y++)
                if ((x + 1) % 2 == 0 && (y + 1) % 2 == 0)
                {
                    _unvisitedCells.Add(new Vector2(x, y));
                    _floor.Add(new Vector2(x, y));
                }
                else
                {
                    _walls.Add(new Vector2(x, y));
                }

            Vector2 startCell = _unvisitedCells[Random.Range(0, _unvisitedCells.Count)];

            _cells.Add(startCell);

            while (_cells.Count > 0) CheckNeighbors(_cells[_cells.Count - 1]);

            if (_addWalls)
            {
                GameObject wallsContainer = new GameObject("Maze_Walls");
                wallsContainer.transform.parent = _container.transform;

                foreach (Vector2 wallPos in _walls)
                {
                    GameObject wall = _wallPrefab != null
                        ? Instantiate(_wallPrefab, Vector3.zero, Quaternion.identity)
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.position = new Vector3(wallPos.x * wall.transform.localScale.x, 0,
                        wallPos.y * wall.transform.localScale.z);
                    wall.transform.parent = wallsContainer.transform;
                }
            }

            if (_addFloor)
            {
                GameObject floorContainer = new GameObject("Maze_Floor");
                floorContainer.transform.parent = _container.transform;

                foreach (Vector2 floorPos in _floor)
                {
                    GameObject floorObj;

                    if (_floorPrefab != null)
                    {
                        floorObj = Instantiate(_floorPrefab, Vector3.zero, Quaternion.identity);
                    }
                    else
                    {
                        floorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (_wallPrefab != null)
                        {
                            floorObj.transform.localScale = new Vector3(_wallPrefab.transform.localScale.x, 0.1f,
                                _wallPrefab.transform.localScale.z);
                            floorObj.transform.position = new Vector3(floorPos.x * _wallPrefab.transform.localScale.x,
                                -_wallPrefab.transform.localScale.y / 2, floorPos.y * _wallPrefab.transform.localScale.z);
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

            _data.WallTiles = _walls;
            _data.FloorTiles = _floor;
        }
        else
        {
            ShowNotification(new GUIContent("Please define width and height"));
        }
    }

    private void CheckNeighbors(Vector2 cell)
    {
        _unvisitedCells.Remove(cell);

        List<Vector2> neighbors = new List<Vector2>
        {
            new Vector2(cell.x + 2, cell.y),
            new Vector2(cell.x - 2, cell.y),
            new Vector2(cell.x, cell.y + 2),
            new Vector2(cell.x, cell.y - 2)
        };


        List<Vector2> newCells = neighbors.Where(t => _unvisitedCells.Contains(t)).ToList();

        if (newCells.Count > 0)
        {
            int side = Random.Range(0, newCells.Count);
            Vector2 newCell = newCells[side];
            _cells.Add(newCell);
            Vector2 passage = Vector2.zero;

            if (newCells[side].x > cell.x) passage = new Vector2(cell.x + 1, cell.y);
            if (newCells[side].x < cell.x) passage = new Vector2(cell.x - 1, cell.y);
            if (newCells[side].y > cell.y) passage = new Vector2(cell.x, cell.y + 1);
            if (newCells[side].y < cell.y) passage = new Vector2(cell.x, cell.y - 1);

            _walls.Remove(passage);
            if (!_floor.Contains(passage)) _floor.Add(passage);
        }
        else
        {
            _cells.RemoveAt(_cells.Count - 1);
        }
    }

    private void CombineMeshes(GameObject objContainer)
    {
        objContainer.AddComponent<MeshFilter>();
        objContainer.AddComponent<MeshRenderer>();
        MeshFilter[] meshFilters = objContainer.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        objContainer.transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        objContainer.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        if (_generateCollider)
        {
            MeshCollider collider = objContainer.AddComponent(typeof(MeshCollider)) as MeshCollider;
            if (collider != null) collider.sharedMesh = objContainer.GetComponent<MeshFilter>().sharedMesh;
        }

        //if (material != null) {
        //    renderer.material = material;
        //}        

        objContainer.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
        objContainer.gameObject.SetActive(true);

        for (int j = meshFilters.Length - 1; j > 0; j--) DestroyImmediate(meshFilters[j].gameObject);
    }

    public static List<Vector2> SolveMaze(ICollection<Vector2> maze, Vector2 start, Vector2 end)
    {
        // entryTile = new Vector2(1, 1);
        // endTile = new Vector2(width * 2 - 1, height * 2 - 1);

        PathTile startTile = new PathTile(start);
        PathTile endTile = new PathTile(end);

        List<Vector2> solvedPath = new List<Vector2>();
        List<Vector2> visitedCells = new List<Vector2>();

        Queue<PathTile> queue = new Queue<PathTile>();
        queue.Enqueue(startTile);

        bool finished = false;

        while (queue.Count > 0 && !finished)
        {
            PathTile current = queue.Dequeue();

            if (!visitedCells.Contains(current.Position))
            {
                List<Vector2> neighbors = new List<Vector2>
                {
                    new Vector2(current.Position.x + 1, current.Position.y),
                    new Vector2(current.Position.x - 1, current.Position.y),
                    new Vector2(current.Position.x, current.Position.y + 1),
                    new Vector2(current.Position.x, current.Position.y - 1)
                };


                foreach (Vector2 path in neighbors)
                    if (maze.Contains(path) && !visitedCells.Contains(path))
                        if (path == endTile.Position)
                        {
                            endTile.Parent = current;
                            finished = true;
                        }
                        else
                        {
                            PathTile tile = new PathTile(path) {Parent = current};
                            queue.Enqueue(tile);
                        }
            }
            visitedCells.Add(current.Position);
        }

        PathTile curTile = endTile;

        while (curTile.Parent != null)
        {
            solvedPath.Add(curTile.Position);
            curTile = curTile.Parent;
        }

        solvedPath.Add(startTile.Position);
        solvedPath.Reverse();

        return solvedPath;
    }

    private List<Vector2> BuildBorder(List<Vector2> tiles, bool corners)
    {
        List<Vector2> border = new List<Vector2>();

        foreach (Vector2 pathTile in tiles)
        {
            List<Vector2> neighbors = new List<Vector2>
            {
                new Vector2(pathTile.x + 1, pathTile.y),
                new Vector2(pathTile.x - 1, pathTile.y),
                new Vector2(pathTile.x, pathTile.y + 1),
                new Vector2(pathTile.x, pathTile.y - 1)
            };


            if (corners)
            {
                neighbors.Add(new Vector2(pathTile.x + 1, pathTile.y + 1));
                neighbors.Add(new Vector2(pathTile.x + 1, pathTile.y - 1));
                neighbors.Add(new Vector2(pathTile.x - 1, pathTile.y + 1));
                neighbors.Add(new Vector2(pathTile.x - 1, pathTile.y - 1));
            }

            foreach (Vector2 tile in neighbors) if (!border.Contains(tile) && !tiles.Contains(tile)) border.Add(tile);
        }

        return border;
    }

    private void DrawTiles(List<Vector2> tiles, string containerName)
    {
        if (GameObject.Find(containerName) && _clearTiles)
            DestroyImmediate(GameObject.Find(containerName));
        GameObject parentContainer = new GameObject(containerName);
        parentContainer.transform.parent = _container.transform;

        foreach (Vector2 tile in tiles)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

            wall.transform.position = new Vector3(tile.x * wall.transform.localScale.x, 1,
                tile.y * wall.transform.localScale.z);
            wall.transform.parent = parentContainer.transform;
        }
    }

    private class PathTile
    {
        public PathTile Parent;
        public Vector2 Position;

        public PathTile(Vector2 pos)
        {
            Position = pos;
        }
    }
}