using System.Collections;
using System.Collections.Generic;
using Assets.MazeTools.Scripts;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private MazeMover _movingScript;

	// Use this for initialization
	void Start ()
	{
	    _movingScript = GetComponent<MazeMover>();
	    _movingScript.ReachedEndCallback = ReachedEnd;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void ReachedEnd()
    {
        Destroy(this.gameObject);
    }
}
