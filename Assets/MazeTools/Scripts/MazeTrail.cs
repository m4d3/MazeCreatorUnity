using System.Collections.Generic;
using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeTrail : MonoBehaviour
    {
        private Vector3[] _positions;
        private LineRenderer _trail;

    

        // Use this for initialization
        void Start ()
        {
            _trail = transform.gameObject.AddComponent<LineRenderer>();
        } 
	
        // Update is called once per frame
        void Update () {
            _trail.SetPositions(_positions);
        }

        public void SetPositions(List<Vector2> tiles, int length) { 
    
            _positions = new Vector3[length];
            Vector2[] tmp = tiles.GetRange(0, length).ToArray();
            for (int i = 0; i < length - 1; i++)
            {
                _positions[i] = new Vector3(tmp[i].x, 0, tmp[i].y);
            }

        }
    }
}
