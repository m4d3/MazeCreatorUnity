using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MazeTrail : MonoBehaviour
{
    private Vector3[] positions;
    private LineRenderer trail;

    

    // Use this for initialization
	void Start ()
	{
        trail = transform.gameObject.AddComponent<LineRenderer>();
	} 
	
	// Update is called once per frame
	void Update () {
        trail.SetPositions(positions);
    }

    public void SetPositions(List<Vector2> tiles, int length) { 
    
        positions = new Vector3[length];
        Vector2[] tmp = tiles.GetRange(0, length).ToArray();
        for (int i = 0; i < length - 1; i++)
        {
            positions[i] = new Vector3(tmp[i].x, 0, tmp[i].y);
        }

    }
}
