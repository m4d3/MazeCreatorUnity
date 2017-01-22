using System;
using System.Collections;
using System.Collections.Generic;
using Assets.MazeTools.Scripts;
using UnityEngine;

public class MazePlacer : MonoBehaviour {

    public MazeTile.MazeTileTypes PlaceableTiles;

    private List<Vector2> _tiles;
    public bool IsPlaced { get; private set; }

    private void Awake() {

    }

    private void GetTiles() {
        switch (PlaceableTiles)
        {
            case MazeTile.MazeTileTypes.Wall:
                _tiles = FindObjectOfType<MazeData>().WallTiles;
                break;
            case MazeTile.MazeTileTypes.Floor:
                _tiles = FindObjectOfType<MazeData>().FloorTiles;
                break;
            case MazeTile.MazeTileTypes.Blocked:
                break;
            case MazeTile.MazeTileTypes.Border:
                _tiles = FindObjectOfType<MazeData>().BorderTiles;
                break;
            case MazeTile.MazeTileTypes.Path:
                _tiles = FindObjectOfType<MazeData>().PathTiles;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update() {
        if (IsPlaced) return;

        GetTiles();
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        if (!hit) return;

        Vector2 newTarget = new Vector2((int)hitInfo.transform.position.x, (int)hitInfo.transform.position.z);
        if (!_tiles.Contains(newTarget)) return;

        transform.position = new Vector3(newTarget.x, transform.position.y, newTarget.y);

        if (Input.GetMouseButtonDown(0))
        {
            IsPlaced = true;
        }
    }
}
