/* Class for Maze creation at runtime */
/* Author: Matthias Ewald - github.com/m4d3 */


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeRuntime : MonoBehaviour
    {
        private List<Vector2> _cells;
        private bool _clearTiles;
        private GameObject _container;

        private MazeData _data;
        private Vector2 _endTile;
        private Vector2 _entryTile;
        private List<Vector2> _floor;
        private bool _generateCollider;
        private List<Vector2> _pathBorder;
        private List<Vector2> _solvedPath;

        private List<Vector2> _unvisitedCells;
        private List<Vector2> _walls;

        public bool AddBorder;
        public bool AddFloor;
        public bool AddPath;
        public bool AddWalls;
        public int BorderWidth;
        public bool CutCorners;

        public int Height = 10;

        public int MinPathLength;
        public bool RandomSolve;

        public bool StopCreation;

        public GameObject TilePrefab;
        public int Width = 10;

        private void Awake()
        {
            _container = transform.gameObject;
            _data = _container.AddComponent<MazeData>();
        }

        private void Start()
        {
            _clearTiles = true;
            if (!StopCreation)
                StartCoroutine("CreateLoop");
            else
                Create();
        }

        private void Create()
        {
            CreateMaze();
            _solvedPath = new List<Vector2>();

            if (RandomSolve)
                while (_solvedPath.Count < MinPathLength)
                {
                    _solvedPath.Clear();
                    _entryTile = _endTile = _floor[Random.Range(0, _floor.Count)];
                    while (_endTile == _entryTile) _endTile = _floor[Random.Range(0, _floor.Count)];
                    _solvedPath = SolveMaze(_floor, _entryTile, _endTile);
                }
            else if (MinPathLength > 0)
                while (_solvedPath.Count < MinPathLength)
                {
                    CreateMaze();
                    _solvedPath = SolveMaze(_floor, new Vector2(1, 1), new Vector2(Width * 2 - 1, Height * 2 - 1));
                }
            else
                _solvedPath = SolveMaze(_floor, new Vector2(1, 1), new Vector2(Width * 2 - 1, Height * 2 - 1));

            _data.PathTiles = new List<Vector2>();
            _data.PathTiles.AddRange(_solvedPath);

            if (_solvedPath.Count > 0)
            {
                if (AddPath)
                    DrawTiles(_solvedPath, "Maze_Path");

                if (AddBorder)
                {
                    _pathBorder = new List<Vector2>();
                    _pathBorder = BuildBorder(_solvedPath, !CutCorners);
                    for (int i = 0; i < BorderWidth - 1; i++)
                        _pathBorder.AddRange(BuildBorder(_pathBorder, !CutCorners));
                    foreach (Vector2 tile in _solvedPath) _pathBorder.Remove(tile);

                    DrawTiles(_pathBorder, "Path_Border");

                    _data.BorderTiles = new List<Vector2>();
                    _data.BorderTiles.AddRange(_pathBorder);
                }
            }
            else
            {
                Debug.Log("No solved path found");
            }
        }

        private IEnumerator CreateLoop()
        {
            while (!StopCreation)
            {
                Create();

                yield return new WaitForSeconds(1.0f);
            }
        }

        private void CreateMaze()
        {
            if (Width <= 0 || Height <= 0) return;

            foreach (Transform child in _container.transform) DestroyImmediate(child.gameObject);

            _unvisitedCells = new List<Vector2>();
            _walls = new List<Vector2>();
            _cells = new List<Vector2>();
            _floor = new List<Vector2>();

            for (int x = 0; x < 1 + Width * 2; x++)
            for (int y = 0; y < 1 + Height * 2; y++)
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

            DrawMaze();

            _data.WallTiles = _walls;
            _data.FloorTiles = _floor;
        }

        private void DrawMaze()
        {
            if (AddWalls) DrawTiles(_walls, "Maze_Walls");

            if (AddFloor) DrawTiles(_floor, "Maze_Floor", -1.0f);
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
                MeshCollider genCollider = objContainer.AddComponent(typeof(MeshCollider)) as MeshCollider;
                if (genCollider != null) genCollider.sharedMesh = objContainer.GetComponent<MeshFilter>().sharedMesh;
            }

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

                foreach (Vector2 tile in neighbors)
                    if (!border.Contains(tile) && !tiles.Contains(tile)) border.Add(tile);
            }

            return border;
        }

        private void DrawTiles(List<Vector2> tiles, string containerName, float zOffset = 0)
        {
            if (_container.transform.FindChild(containerName) && _clearTiles)
                Destroy(_container.transform.FindChild(containerName).gameObject);

            GameObject parentContainer = new GameObject(containerName);
            parentContainer.transform.parent = _container.transform;

            foreach (Vector2 tile in tiles)
            {
                GameObject newTile = TilePrefab == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                    : Instantiate(TilePrefab);

                newTile.transform.position = new Vector3(tile.x * newTile.transform.localScale.x, zOffset,
                    tile.y * newTile.transform.localScale.z);
                newTile.transform.parent = parentContainer.transform;

                if (newTile.GetComponent<MazeTile>())
                {
                    MazeTile currentTile = newTile.GetComponent<MazeTile>();
                    switch (containerName)
                    {
                        case "Maze_Floor":
                            currentTile.Type = MazeTile.MazeTileTypes.Floor;
                            break;
                        case "Maze_Wall":
                            currentTile.Type = MazeTile.MazeTileTypes.Wall;
                            break;
                        case "Path_Border":
                            currentTile.Type = MazeTile.MazeTileTypes.Border;
                            break;
                    }
                }
            }

            parentContainer.transform.position = _container.transform.position;
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
}