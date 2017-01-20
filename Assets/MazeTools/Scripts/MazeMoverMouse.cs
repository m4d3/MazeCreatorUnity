using System.Collections.Generic;
using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeMoverMouse : MonoBehaviour
    {
        public float Speed;

        private GameObject tile;

        private Vector2 _currentPosition;
        private Vector2 _moveTarget;

        private List<Vector2> _solvedPath;
        private List<Vector2> _floorTiles;
        private int _solvedTileIndex;
        private bool solved;

        // Use this for initialization
        void Start()
        {
            _moveTarget = _currentPosition = new Vector2((int) transform.position.x, (int) transform.position.z);
            _floorTiles = GameObject.Find("Maze").GetComponent<MazeData>().FloorTiles;

        }

        // Update is called once per frame
        void Update()
        {
            if (tile) {
                tile.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }

            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                if (hitInfo.transform.parent && hitInfo.transform.parent.name.Equals("Maze_Floor"))
                {
                    if (tile)
                    {
                        tile.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                    }
                    tile = hitInfo.transform.gameObject;
                    tile.GetComponent<Renderer>().material.SetColor("_Color", Color.red);

                    if (Input.GetMouseButtonDown(0))
                    {
                        SolveMaze(_floorTiles, new Vector2((int) tile.transform.position.x, (int) tile.transform.position.z));
                        _solvedTileIndex = 0;
                        solved = true;
                    }
                }
            }
            if (_currentPosition != _moveTarget)
            {
                _currentPosition = Vector2.MoveTowards(_currentPosition, _moveTarget, Time.deltaTime * Speed);
                transform.position = new Vector3(_currentPosition.x, transform.position.y, _currentPosition.y);
            }
            else if(solved)
            {
                GetNextMoveTarget();
            }
        }

        private void OnDrawGizmos()
        {
        
        }

        private void GetNextMoveTarget()
        {
            _currentPosition = _moveTarget;

            if (_solvedTileIndex < _solvedPath.Count - 1)
            {
                _solvedTileIndex++;
                _moveTarget = _solvedPath[_solvedTileIndex];
            }
        }

        private void SolveMaze(ICollection<Vector2> maze, Vector2 target) {
            // entryTile = new Vector2(1, 1);
            // endTile = new Vector2(width * 2 - 1, height * 2 - 1);

            PathTile start = new PathTile(_currentPosition);
            PathTile end = new PathTile(target);

            _solvedPath = new List<Vector2>();
            List<Vector2> visitedCells = new List<Vector2>();

            Queue<PathTile> queue = new Queue<PathTile>();
            queue.Enqueue(start);

            bool finished = false;

            while (queue.Count > 0 && !finished) {
                PathTile current = queue.Dequeue();

                if (!visitedCells.Contains(current.Position)) {
                    List<Vector2> neighbors = new List<Vector2>
                    {
                        new Vector2(current.Position.x + 1, current.Position.y),
                        new Vector2(current.Position.x - 1, current.Position.y),
                        new Vector2(current.Position.x, current.Position.y + 1),
                        new Vector2(current.Position.x, current.Position.y - 1)
                    };


                    foreach (Vector2 path in neighbors)
                        if (maze.Contains(path) && !visitedCells.Contains(path)) {
                            if (path == end.Position) {
                                end.Parent = current;
                                finished = true;
                            } else {
                                PathTile tile = new PathTile(path) { Parent = current };
                                queue.Enqueue(tile);
                            }
                        }
                }
                visitedCells.Add(current.Position);
            }

            PathTile curTile = end;

            while (curTile.Parent != null) {
                _solvedPath.Add(curTile.Position);
                curTile = curTile.Parent;
            }

            _solvedPath.Add(start.Position);
            _solvedPath.Reverse();
        }

        private class PathTile {
            public PathTile Parent;
            public Vector2 Position;

            public PathTile(Vector2 pos) {
                Position = pos;
            }
        }
    }
}
