using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.MazeTools.Scripts
{
    public class MazeMover : MonoBehaviour
    {
        public enum MovementTypes
        {
            random,
            solved
        }

        private List<Vector2> _floorTiles;

        public MovementTypes MovementType;

        private Vector2 _moveTarget;
        private List<Vector2> _solvedPath;
        private int _solvedTileIndex;

        public float Speed = 10;
        private Vector2 _target;
        private List<Vector2> _visitedTiles;
        public float ZOffset = 0;
        public bool RandomStart;

        // Use this for initialization
        private void Start()
        {
            _visitedTiles = new List<Vector2>();

            _floorTiles = GameObject.Find("Maze").GetComponent<MazeData>().FloorTiles;
            _solvedPath = GameObject.Find("Maze").GetComponent<MazeData>().PathTiles;

            switch (MovementType) {
                case MovementTypes.random:
                    if (RandomStart)
                        _target = _moveTarget = _floorTiles[Random.Range(0, _floorTiles.Count)];
                    else
                    {
                        _target =_moveTarget = new Vector2((int)transform.position.x, (int)transform.position.z);
                    }
                    break;
                case MovementTypes.solved:
                    _target = _moveTarget = _solvedPath[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            transform.position = new Vector3(_target.x, ZOffset, _target.y);

            //GetNextMoveTarget();

        }

        // Update is called once per frame
        private void Update()
        {
            if (_target != _moveTarget)
            {
                _target = Vector2.MoveTowards(_target, _moveTarget, Time.deltaTime * Speed);

                transform.position = new Vector3(_target.x, ZOffset, _target.y);
            } else
            {
                _visitedTiles.Add(_moveTarget);
                GetNextMoveTarget();
            }
        }

        private void GetNextMoveTarget()
        {
            switch (MovementType)
            {
                case MovementTypes.random:
                    List<Vector2> neighbors = SearchNeighbors(_target);
                    if (neighbors.Count == 0)
                    {
                        _visitedTiles.Clear();
                        _visitedTiles.Add(_target);
                        Debug.Log("Clearing visited tiles");
                        neighbors = SearchNeighbors(_target);
                    }

                    _moveTarget = neighbors[Random.Range(0, neighbors.Count)];
                    break;
                case MovementTypes.solved:
                    if (_solvedTileIndex < _solvedPath.Count - 1)
                    {
                        _solvedTileIndex++;
                        _moveTarget = _solvedPath[_solvedTileIndex];
                    }
                    else
                    {
                        OnEndReached();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEndReached()
        {
            Destroy(transform.gameObject);
        }

        private List<Vector2> SearchNeighbors(Vector2 cell)
        {
            List<Vector2> neighbors = new List<Vector2>
            {
                new Vector2(cell.x + 1, cell.y),
                new Vector2(cell.x - 1, cell.y),
                new Vector2(cell.x, cell.y + 1),
                new Vector2(cell.x, cell.y - 1)
            };


            List<Vector2> possibleCells = new List<Vector2>();

            foreach (Vector2 tile in neighbors)
                if (_floorTiles.Contains(tile) && !_visitedTiles.Contains(tile))
                    possibleCells.Add(tile);

            return possibleCells;
        }
    }
}