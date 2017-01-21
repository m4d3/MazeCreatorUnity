using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeTile : MonoBehaviour {

        public enum MazeTileTypes
        {
            Wall,
            Floor,
            Blocked,
            Border,
            Path
        }

        public MazeTileTypes Type;

        public bool DrawCube;

        private Vector2 _coordinates;

        public MazeTile(Vector2 pos)
        {
            _coordinates = pos;
        }

        // Use this for initialization
        void Start () {
            GameObject go =transform.gameObject;

            if (DrawCube)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Mesh mesh = cube.GetComponent<MeshFilter>().sharedMesh;
                Material mat = cube.GetComponent<MeshRenderer>().sharedMaterial;
                GameObject.Destroy(cube);

                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                go.AddComponent<BoxCollider>();
            }
        }
	
        // Update is called once per frame
        void Update () {
		
        }
    }
}
