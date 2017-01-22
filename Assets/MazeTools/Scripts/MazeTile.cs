using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeTile : MonoBehaviour
    {
        public enum MazeTileTypes
        {
            Wall,
            Floor,
            Blocked,
            Border,
            Path
        }

        private Vector2 _coordinates;

        public bool DrawCube;
        public bool IsSelectable;
        public bool IsHighlightable;

        public MazeTileTypes Type;

        public MazeTile(Vector2 pos)
        {
            _coordinates = pos;
        }

        private void Awake()
        {
            if (DrawCube)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Mesh mesh = cube.GetComponent<MeshFilter>().sharedMesh;
                Material mat = cube.GetComponent<MeshRenderer>().sharedMaterial;
                Destroy(cube);

                gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = mat;
                gameObject.AddComponent<BoxCollider>();
            }

            if (IsSelectable)
            {
                SelectionComponent sc = gameObject.AddComponent<SelectionComponent>();
                sc.HighlightOnly = IsHighlightable;
                sc.Type = GetType();
            }
        }
    }
}