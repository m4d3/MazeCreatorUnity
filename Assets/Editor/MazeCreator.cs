/* Maze Creation Script by Matthias Ewald
 *  
 * Using Recursive Backtracking Algorithm
 * 
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class MazeCreator : EditorWindow {
    int width = 0;
    int height = 0;

    Vector2 currentCell;
    List<Vector2> unvisitedCells;
    List<Vector2> cells;
    List<Vector2> walls;
    List<Vector2> floor;
    GameObject container;
    GameObject wallPrefab;
    GameObject floorPrefab;
    bool addFloor;
    bool combineMesh;
    bool generateCollider;
    bool randomize;
    float processTime = 0;

    [MenuItem("Maze And Grid Tools/Show Maze Creator")]
    static void Create() {
        MazeCreator window = (MazeCreator)EditorWindow.GetWindow(typeof(MazeCreator));
    }

    void OnGUI() {
        width = EditorGUILayout.IntField("Maze Width:", width);
        height = EditorGUILayout.IntField("Maze Height:", height);
        container = EditorGUILayout.ObjectField("Container", container, typeof(UnityEngine.Object), true) as GameObject;
        wallPrefab = EditorGUILayout.ObjectField("Wall", wallPrefab, typeof(UnityEngine.Object), true) as GameObject;
        floorPrefab = EditorGUILayout.ObjectField("Floor", floorPrefab, typeof(UnityEngine.Object), true) as GameObject;
        addFloor = EditorGUILayout.Toggle("Add Floor", addFloor);
        generateCollider = EditorGUILayout.Toggle("Generate Collider", generateCollider);


        if (GUILayout.Button("Create Maze")) {
            Debug.Log("Creating Maze");
            processTime = Time.realtimeSinceStartup;
            CreateMaze();
            processTime = Time.realtimeSinceStartup - processTime;
            Debug.Log("Maze created in: " + processTime + "seconds");
        }

        if (GUILayout.Button("Combine Meshes")) {
            if (container.transform.childCount > 0) {
                CombineMeshes(container.transform.FindChild("Maze_Walls").gameObject);
                if (container.transform.FindChild("Maze_Floor")) {
                    CombineMeshes(container.transform.FindChild("Maze_Floor").gameObject);
                }
            }
        }
    }

    void CreateMaze() {

        if (width > 0 && height > 0) {
            if (container == null) {
                container = new GameObject("Maze");
            } else {
                foreach (Transform child in container.transform) {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }

            unvisitedCells = new List<Vector2>();
            walls = new List<Vector2>();
            cells = new List<Vector2>();
            floor = new List<Vector2>();

            GameObject wallsContainer = new GameObject("Maze_Walls");
            wallsContainer.transform.parent = container.transform;

            for (int x = 0; x < 1 + width * 2; x++) {
                for (int y = 0; y < 1 + height * 2; y++) {
                    if ((x + 1) % 2 == 0 && (y + 1) % 2 == 0) {
                        unvisitedCells.Add(new Vector2(x, y));
                        floor.Add(new Vector2(x, y));
                    } else {
                        walls.Add(new Vector2(x, y));
                    }
                }
            }

            Vector2 startCell = unvisitedCells[(int)UnityEngine.Random.Range(0, unvisitedCells.Count)];

            cells.Add(startCell);

            while (cells.Count > 0) {
                CheckNeighbors(cells[cells.Count - 1]);
            }

            foreach (Vector2 wallPos in walls) {
                GameObject wall;

                if (wallPrefab != null) {
                    wall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                } else {
                    wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
                wall.transform.position = new Vector3(wallPos.x * wall.transform.localScale.x, 0, wallPos.y * wall.transform.localScale.z);
                wall.transform.parent = wallsContainer.transform;
            }

            if (addFloor) {
                GameObject floorContainer = new GameObject("Maze_Floor");
                floorContainer.transform.parent = container.transform;

                foreach (Vector2 floorPos in floor) {
                    GameObject floorObj;

                    if (floorPrefab != null) {
                        floorObj = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    } else {
                        floorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (wallPrefab != null) {
                            floorObj.transform.localScale = new Vector3(wallPrefab.transform.localScale.x, 0.1f, wallPrefab.transform.localScale.z);
                            floorObj.transform.position = new Vector3(floorPos.x * wallPrefab.transform.localScale.x, -wallPrefab.transform.localScale.y / 2, floorPos.y * wallPrefab.transform.localScale.z);
                        } else {
                            floorObj.transform.localScale = new Vector3(1, 0.1f, 1);
                            floorObj.transform.position = new Vector3(floorPos.x, -0.5f, floorPos.y);
                        }
                    }

                    floorObj.transform.parent = floorContainer.transform;
                }
            }
        } else {
            this.ShowNotification(new GUIContent("Please define width and height"));
        }
    }

    void CheckNeighbors(Vector2 cell) {
        unvisitedCells.Remove(cell);
        Vector2 newCell;
        List<Vector2> neighbors = new List<Vector2>();

        neighbors.Add(new Vector2(cell.x + 2, cell.y));
        neighbors.Add(new Vector2(cell.x - 2, cell.y));
        neighbors.Add(new Vector2(cell.x, cell.y + 2));
        neighbors.Add(new Vector2(cell.x, cell.y - 2));

        List<Vector2> newCells = new List<Vector2>();

        for (int i = 0; i < neighbors.Count; i++) {
            if (unvisitedCells.Contains(neighbors[i])) {
                newCells.Add(neighbors[i]);
            }
        }

        if (newCells.Count > 0) {
            int side = (int)UnityEngine.Random.Range(0, newCells.Count);
            newCell = newCells[side];
            cells.Add(newCell);
            Vector2 passage = Vector2.zero;

            if (newCells[side].x > cell.x) {
                passage = new Vector2(cell.x + 1, cell.y);
            }
            if (newCells[side].x < cell.x) {
                passage = new Vector2(cell.x - 1, cell.y);
            }
            if (newCells[side].y > cell.y) {
                passage = new Vector2(cell.x, cell.y + 1);
            }
            if (newCells[side].y < cell.y) {
                passage = new Vector2(cell.x, cell.y - 1);
            }

            walls.Remove(passage);
            if (!floor.Contains(passage)) {
                floor.Add(passage);
            }

        } else {
            cells.RemoveAt(cells.Count - 1);
        }
    }

    void CombineMeshes(GameObject objContainer) {
        objContainer.AddComponent<MeshFilter>();
        objContainer.AddComponent<MeshRenderer>();
        MeshFilter[] meshFilters = objContainer.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        objContainer.transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        objContainer.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        if (generateCollider) {
            MeshCollider collider = objContainer.AddComponent(typeof(MeshCollider)) as MeshCollider;
            collider.sharedMesh = objContainer.GetComponent<MeshFilter>().sharedMesh;
        }

        //if (material != null) {
        //    renderer.material = material;
        //}        

        objContainer.renderer.material = new Material(Shader.Find("Diffuse"));
        objContainer.gameObject.SetActive(true);

        for (int j = meshFilters.Length - 1; j > 0; j--) {
            GameObject.DestroyImmediate(meshFilters[j].gameObject);
        }
    }
}
