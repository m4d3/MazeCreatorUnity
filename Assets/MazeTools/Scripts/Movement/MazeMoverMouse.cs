using System.Collections.Generic;
using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeMoverMouse : MonoBehaviour
    {
        public float Speed;

        public enum MovableTilesTypes
        {
            Floor,
            Walls,
            Border,
            Path
        }

        public MovableTilesTypes MoveableTiles;
        public bool SetTargetOnMouseDown;

        public delegate void OnEndReached();
        public OnEndReached PathEndReachedCallback;

        private Vector2 _currentPosition;
        private Vector2 _moveTarget;
        private List<Vector2> _solvedPath;
        private List<Vector2> _tiles;
        private int _solvedTileIndex;
        private bool _solved;
        private MazeData _data;

        private void Awake() {
            _solvedPath = new List<Vector2>();
        }

        private void Start() {
            _moveTarget = _currentPosition = new Vector2((int)transform.position.x, (int)transform.position.z);
        }

        private void Update() {
            if (_currentPosition != _moveTarget) {
                _currentPosition = Vector2.MoveTowards(_currentPosition, _moveTarget, Time.deltaTime * Speed);
                transform.position = new Vector3(_currentPosition.x, transform.position.y, _currentPosition.y);
            } else if (_solved) {
                GetNextMoveTarget();
            }

            if (Input.GetMouseButtonDown(0) && SetTargetOnMouseDown)
            {
                MoveToTarget();
            }
        }

        public void MoveToTarget()
        {
            _solved = false;
            GetData();

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (!hit) return;
            Vector2 newTarget = new Vector2((int) hitInfo.transform.position.x, (int) hitInfo.transform.position.z);
            if (!_tiles.Contains(newTarget)) return;

            SolveMaze(_tiles, newTarget);
            //GetNextMoveTarget();

            _solved = true;
        }

        private void GetData()
        {
            if (_data) return;

            _data = GameObject.Find("Maze").GetComponent<MazeData>();

            switch (MoveableTiles) {
                case MovableTilesTypes.Border:
                    _tiles = _data.BorderTiles;
                    break;
                case MovableTilesTypes.Floor:
                    _tiles = _data.FloorTiles;
                    break;
                case MovableTilesTypes.Walls:
                    _tiles = _data.WallTiles;
                    break;
                case MovableTilesTypes.Path:
                    _tiles = _data.PathTiles;
                    break;
                default:
                    break;
            }
        }

        private void GetNextMoveTarget()
        {
            _currentPosition = _moveTarget;

            if (_solvedTileIndex >= _solvedPath.Count - 1)
            {
                if(PathEndReachedCallback != null)
                    PathEndReachedCallback();
                return;
            }

            _solvedTileIndex++;
            _moveTarget = _solvedPath[_solvedTileIndex];
        }

        private void SolveMaze(ICollection<Vector2> maze, Vector2 target) {

            if (maze == null) return;

            PathTile start = new PathTile(new Vector2((int)_currentPosition.x, (int)_currentPosition.y));
            PathTile end = new PathTile(target);

            if(_solvedPath.Count > 1)
                if(_solvedPath[_solvedPath.Count - 1] == target) return;

            _solvedTileIndex = 0;
            _solvedPath.Clear();

            List<Vector2> visitedCells = new List<Vector2>();

            Queue<PathTile> queue = new Queue<PathTile>();
            queue.Enqueue(start);

            bool finished = false;

            while (queue.Count > 0 && !finished)
            {
                PathTile current = queue.Dequeue();

                if (visitedCells.Contains(current.Position)) continue;
                List<Vector2> neighbors = new List<Vector2>
                {
                    new Vector2(current.Position.x + 1, current.Position.y),
                    new Vector2(current.Position.x - 1, current.Position.y),
                    new Vector2(current.Position.x, current.Position.y + 1),
                    new Vector2(current.Position.x, current.Position.y - 1)
                };


                foreach (Vector2 path in neighbors)
                    if (maze.Contains(path) && !visitedCells.Contains(path))
                    {
                        if (path == end.Position)
                        {
                            end.Parent = current;
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

            PathTile curTile = end;

            while (curTile.Parent != null)
            {
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