using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazePlacer : MonoBehaviour
    {
        private MazeSelector _selector;

        // Use this for initialization
        void Start ()
        {
            _selector = GameObject.FindObjectOfType<MazeSelector>();
        }
	
        // Update is called once per frame
        void Update () {
            if (_selector && _selector.SelectedTile != null)
            {
                transform.position = _selector.SelectedTile.transform.position;
                transform.Translate(Vector3.up * 1);
            }
        }
    }
}
