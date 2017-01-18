using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeMover : MonoBehaviour
{
    public enum MovementTypes
    {
        random,
        solved
    }

    private float _alpha;
    private Vector2 currentPos;
    private List<Vector2> floorTiles;

    public MovementTypes movementType;

    private Vector2 moveTarget;
    private List<Vector2> solvedPath;
    private int solvedTileIndex;

    public float speed = 10;
    private Vector2 target;
    private List<Vector2> visitedTiles;
    public float zOffset = 0;

    // Use this for initialization
    private void Start()
    {
        moveTarget = new Vector2(1,1);
        visitedTiles = new List<Vector2>();

        floorTiles = GameObject.Find("Maze").GetComponent<MazeData>().floorTiles;
        solvedPath = GameObject.Find("Maze").GetComponent<MazeData>().pathTiles;

        switch (movementType) {
            case MovementTypes.random:
                moveTarget = floorTiles[Random.Range(0, floorTiles.Count)];
                transform.position = moveTarget;
                GetComponent<TrailRenderer>().startColor = GetComponent<Light>().color = Random.ColorHSV();
                break;
            case MovementTypes.solved:
                moveTarget = solvedPath[0];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        GetNextMoveTarget();

    }

    // Update is called once per frame
    private void Update()
    {
        if (target != moveTarget)
        {
            _alpha += Time.deltaTime * speed;
            if (_alpha >= 1) _alpha = 1;
        }
        else
        {
            _alpha = 0;
            visitedTiles.Add(moveTarget);
            GetNextMoveTarget();
        }
        target = Vector2.Lerp(currentPos, moveTarget, _alpha);
        transform.position = new Vector3(target.x, zOffset, target.y);
    }

    private void GetNextMoveTarget()
    {
        currentPos = moveTarget;
        switch (movementType)
        {
            case MovementTypes.random:
                var neighbors = SearchNeighbors(currentPos);
                if (neighbors.Count == 0)
                {
                    visitedTiles.Clear();
                    visitedTiles.Add(currentPos);
                    neighbors = SearchNeighbors(currentPos);
                }
                moveTarget = neighbors[Random.Range(0, neighbors.Count)];
                break;
            case MovementTypes.solved:
                if (solvedTileIndex < solvedPath.Count - 1)
                {
                    solvedTileIndex++;
                    moveTarget = solvedPath[solvedTileIndex];
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
        var neighbors = new List<Vector2>
        {
            new Vector2(cell.x + 1, cell.y),
            new Vector2(cell.x - 1, cell.y),
            new Vector2(cell.x, cell.y + 1),
            new Vector2(cell.x, cell.y - 1)
        };


        var possibleCells = new List<Vector2>();

        foreach (Vector2 tile in neighbors)
            if (floorTiles.Contains(tile) && !visitedTiles.Contains(tile))
                possibleCells.Add(tile);

        return possibleCells;
    }
}