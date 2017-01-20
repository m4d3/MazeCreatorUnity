/* Maze Creation Script by Matthias Ewald
 *  
 * Using Recursive Backtracking Algorithm
 * 
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.MazeTools.Editor
{
    public class MazeCreator : EditorWindow
    {
        private List<Vector2> _unvisitedCells;
        private int _width;
        private bool _addFloor;
        private GameObject _borderContainer;
        private int _borderWidth = 1;
        private List<Vector2> _cells;
        private GameObject _container;
        private bool _cutCorners;

        private MazeData _data;
        private Vector2 _endTile;
        private Vector2 _entryTile;
        private List<Vector2> _floor;
        private GameObject _floorPrefab;
        private bool _generateCollider;
        private int _height;
        private List<Vector2> _pathBorder;
        private float _processTime;
        private List<Vector2> _solvedPath;
        private GameObject _wallPrefab;
        private List<Vector2> _walls;

        [MenuItem("Maze And Grid Tools/Show Maze Creator")]
        private static void Create()
        {
            GetWindow(typeof(MazeCreator));
        }

        private void OnGUI()
        {
            _width = EditorGUILayout.IntField("Maze Width:", _width);
            _height = EditorGUILayout.IntField("Maze Height:", _height);
            _container = EditorGUILayout.ObjectField("Container", _container, typeof(Object), true) as GameObject;
            _wallPrefab = EditorGUILayout.ObjectField("Wall", _wallPrefab, typeof(Object), true) as GameObject;
            _floorPrefab = EditorGUILayout.ObjectField("Floor", _floorPrefab, typeof(Object), true) as GameObject;
            _addFloor = EditorGUILayout.Toggle("Add Floor", _addFloor);
            _generateCollider = EditorGUILayout.Toggle("Generate Collider", _generateCollider);


            if (GUILayout.Button("Create Maze"))
            {
                Debug.Log("Creating Maze");
                _processTime = Time.realtimeSinceStartup;
                CreateMaze();
                _processTime = Time.realtimeSinceStartup - _processTime;
                Debug.Log("Maze created in: " + _processTime + "seconds");
            }

            if (GUILayout.Button("Combine Meshes"))
                if (_container != null && _container.transform.childCount > 0)
                {
                    CombineMeshes(_container.transform.FindChild("Maze_Walls").gameObject);
                    if (_container.transform.FindChild("Maze_Floor"))
                        CombineMeshes(_container.transform.FindChild("Maze_Floor").gameObject);

                    if (_container.transform.FindChild("Maze_Path"))
                        CombineMeshes(_container.transform.FindChild("Maze_Path").gameObject);
                }

            if (GUILayout.Button("Solve Maze"))
            {
                SolveMaze(_floor);

                var pathContainer = new GameObject("Maze_Path");
                pathContainer.transform.parent = _container.transform;

                foreach (var tile in _solvedPath)
                {
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    wall.transform.position = new Vector3(tile.x * wall.transform.localScale.x, 1,
                        tile.y * wall.transform.localScale.z);
                    wall.transform.parent = pathContainer.transform;
                }

                _data.pathTiles = _solvedPath;
                _data.pathTiles.Reverse();
            }

            if (GUILayout.Button("Build Pathborder"))
            {
                _pathBorder = new List<Vector2>();
                _borderContainer = new GameObject("Maze_Path_Border");

                _pathBorder = BuildBorder(_solvedPath, !_cutCorners);

                for (var i = 0; i < _borderWidth - 1; i++) _pathBorder.AddRange(BuildBorder(_pathBorder, !_cutCorners));
                foreach (var tile in _solvedPath) _pathBorder.Remove(tile);

                foreach (var tile in _pathBorder)
                {
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    wall.transform.position = new Vector3(tile.x * wall.transform.localScale.x, 0,
                        tile.y * wall.transform.localScale.z);
                    wall.transform.parent = _borderContainer.transform;
                }
            }

            _borderWidth = EditorGUILayout.IntField("Border Width:", _borderWidth);

            _cutCorners = EditorGUILayout.Toggle("Cut Corners", _cutCorners);
        }

        private void CreateMaze()
        {
            if (_width > 0 && _height > 0)
            {
                if (_container == null) _container = new GameObject("Maze");
                else foreach (Transform child in _container.transform) DestroyImmediate(child.gameObject);

                _unvisitedCells = new List<Vector2>();
                _walls = new List<Vector2>();
                _cells = new List<Vector2>();
                _floor = new List<Vector2>();

                var wallsContainer = new GameObject("Maze_Walls");
                wallsContainer.transform.parent = _container.transform;

                for (var x = 0; x < 1 + _width * 2; x++)
                for (var y = 0; y < 1 + _height * 2; y++)
                    if ((x + 1) % 2 == 0 && (y + 1) % 2 == 0)
                    {
                        _unvisitedCells.Add(new Vector2(x, y));
                        _floor.Add(new Vector2(x, y));
                    }
                    else
                    {
                        _walls.Add(new Vector2(x, y));
                    }

                var startCell = _unvisitedCells[Random.Range(0, _unvisitedCells.Count)];

                _cells.Add(startCell);

                while (_cells.Count > 0) CheckNeighbors(_cells[_cells.Count - 1]);

                foreach (var wallPos in _walls)
                {
                    GameObject wall = _wallPrefab != null ? Instantiate(_wallPrefab, Vector3.zero, Quaternion.identity) : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.position = new Vector3(wallPos.x * wall.transform.localScale.x, 0,
                        wallPos.y * wall.transform.localScale.z);
                    wall.transform.parent = wallsContainer.transform;
                }

                if (_addFloor)
                {
                    var floorContainer = new GameObject("Maze_Floor");
                    floorContainer.transform.parent = _container.transform;

                    foreach (var floorPos in _floor)
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
                                floorObj.transform.position = new Vector3(
                                    floorPos.x * _wallPrefab.transform.localScale.x,
                                    -_wallPrefab.transform.localScale.y / 2,
                                    floorPos.y * _wallPrefab.transform.localScale.z);
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

                _data = _container.AddComponent<MazeData>();
                _data.wallTiles = _walls;
                _data.floorTiles = _floor;
                _data.pathTiles = _solvedPath;
                _data.pathTiles.Reverse();
            }
            else
            {
                ShowNotification(new GUIContent("Please define width and height"));
            }
        }

        private void CheckNeighbors(Vector2 cell)
        {
            _unvisitedCells.Remove(cell);
            var neighbors = new List<Vector2>
            {
                new Vector2(cell.x + 2, cell.y),
                new Vector2(cell.x - 2, cell.y),
                new Vector2(cell.x, cell.y + 2),
                new Vector2(cell.x, cell.y - 2)
            };


            var newCells = new List<Vector2>();

            foreach (Vector2 t in neighbors)
                if (_unvisitedCells.Contains(t)) newCells.Add(t);

            if (newCells.Count > 0)
            {
                var side = Random.Range(0, newCells.Count);
                var newCell = newCells[side];
                _cells.Add(newCell);
                var passage = Vector2.zero;

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

            if (_generateCollider)
            {
                var collider = objContainer.AddComponent(typeof(MeshCollider)) as MeshCollider;
                if (collider != null) collider.sharedMesh = objContainer.GetComponent<MeshFilter>().sharedMesh;
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
            _entryTile = new Vector2(1, 1);
            _endTile = new Vector2(_width * 2 - 1, _height * 2 - 1);

            var start = new PathTile(_entryTile);
            var end = new PathTile(_endTile);

            _solvedPath = new List<Vector2>();
            var visitedCells = new List<Vector2>();

            var queue = new Queue<PathTile>();
            queue.Enqueue(start);

            var finished = false;

            while (queue.Count > 0 && !finished)
            {
                var current = queue.Dequeue();

                if (!visitedCells.Contains(current.Position))
                {
                    var neighbors = new List<Vector2>
                    {
                        new Vector2(current.Position.x + 1, current.Position.y),
                        new Vector2(current.Position.x - 1, current.Position.y),
                        new Vector2(current.Position.x, current.Position.y + 1),
                        new Vector2(current.Position.x, current.Position.y - 1)
                    };


                    foreach (Vector2 t in neighbors)
                        if (maze.Contains(t) && !visitedCells.Contains(t))
                        {
                            Debug.Log("Neighbor found");
                            if (t == end.Position)
                            {
                                end.Parent = current;
                                finished = true;
                            }
                            else
                            {
                                var tile = new PathTile(t) {Parent = current};
                                queue.Enqueue(tile);
                            }
                        }
                }
                visitedCells.Add(current.Position);
            }

            var curTile = end;

            while (curTile.Parent != null)
            {
                _solvedPath.Add(curTile.Position);
                curTile = curTile.Parent;
            }

            _solvedPath.Add(start.Position);
        }

        private List<Vector2> BuildBorder(List<Vector2> tiles, bool corners)
        {
            var border = new List<Vector2>();

            foreach (var pathTile in tiles)
            {
                var neighbors = new List<Vector2>
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

                foreach (var tile in neighbors) if (!border.Contains(tile) && !tiles.Contains(tile)) border.Add(tile);
            }

            return border;
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