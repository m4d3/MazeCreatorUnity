using System.Collections.Generic;
using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeData : MonoBehaviour {

        public List<Vector2> FloorTiles;
        public List<Vector2> WallTiles;
        public List<Vector2> PathTiles;
        public List<Vector2> BorderTiles;
    }
}
