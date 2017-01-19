using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.MazeTools {
    public class MazeMoverKeyboard:MonoBehaviour
    {

        private Vector2 _currentPosition;
        private Vector2 _moveTarget;
        private List<Vector2> _floorTiles;
        private int _dirX = 0;
        private int _dirY = 0;
        private bool _buttonDown;

        public float Speed;

        private void Start()
        {
            _floorTiles = GameObject.Find("Maze").GetComponent<MazeData>().FloorTiles;
            _moveTarget = _currentPosition = new Vector2((int)transform.position.x, (int)transform.position.z);
        }

        private void Update() {
            if (_currentPosition != _moveTarget)
            {
                _currentPosition = Vector2.MoveTowards(_currentPosition, _moveTarget, Time.deltaTime * Speed);
                transform.position = new Vector3(_currentPosition.x, transform.position.y, _currentPosition.y);
            }
            else
            {

                _dirX = _dirY = 0;

                if (Input.GetKey(KeyCode.A))
                {
                    _dirX = -1;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    _dirX = 1;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    _dirY = 1;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    _dirY = -1;
                }

                Vector2 tile = new Vector2(_currentPosition.x + _dirX, _currentPosition.y + _dirY);
                if (_floorTiles.Contains(tile))
                {
                    _moveTarget = tile;
                }
            }
        }
    }
}
