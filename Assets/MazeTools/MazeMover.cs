using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeMover : MonoBehaviour {

    public float speed = 10;
    List<Vector2> floorTiles;
    List<Vector2> visitedTiles;
    List<Vector2> solvedPath;

    public enum MovementTypes {
        random,
        solved
    }

    public MovementTypes movementType;

    Vector2 moveTarget;
    Vector2 currentPos;
    Vector2 target;
    float alpha = 0;
    int solvedTileIndex = 0;

    // Use this for initialization
    void Start() {
        floorTiles = GameObject.Find("Maze").GetComponent<MazeData>().floorTiles;
        solvedPath = GameObject.Find("Maze").GetComponent<MazeData>().pathTiles;
        currentPos = new Vector2(1, 1);
        moveTarget = new Vector2(1, 1);
        visitedTiles = new List<Vector2>();
    }

    // Update is called once per frame
    void Update() {
        if(target != moveTarget) {
            alpha += Time.deltaTime * speed;
            if(alpha>=1) {
                alpha = 1;
            }
        } else {
            alpha = 0;
            visitedTiles.Add(moveTarget);
            GetNextMoveTarget();
        }
        target = Vector2.Lerp(currentPos, moveTarget, alpha);

        transform.position = new Vector3(target.x, 1, target.y);
    }

    void GetNextMoveTarget() {
        currentPos = moveTarget;
        switch(movementType) {
            case MovementTypes.random:
                List<Vector2> neighbors = SearchNeighbors(currentPos);
                if (neighbors.Count == 0) {
                    visitedTiles.Clear();
                    visitedTiles.Add(currentPos);
                    neighbors = SearchNeighbors(currentPos);
                }
                moveTarget = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                break;
            case MovementTypes.solved:
                if (solvedTileIndex < solvedPath.Count - 1) {
                    solvedTileIndex++;
                    moveTarget = solvedPath[solvedTileIndex];
                }
                break;
            default:
                break;
        }
       
    }

    List<Vector2> SearchNeighbors(Vector2 cell) {
        List<Vector2> neighbors = new List<Vector2>();

        neighbors.Add(new Vector2(cell.x + 1, cell.y));
        neighbors.Add(new Vector2(cell.x - 1, cell.y));
        neighbors.Add(new Vector2(cell.x, cell.y + 1));
        neighbors.Add(new Vector2(cell.x, cell.y - 1));

        List<Vector2> possibleCells = new List<Vector2>();

        for (int i = 0; i < neighbors.Count; i++) {
            if (floorTiles.Contains(neighbors[i]) && !visitedTiles.Contains(neighbors[i])) {
                possibleCells.Add(neighbors[i]);
            }
        }

        return possibleCells;
    }
}
