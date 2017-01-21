using System.Collections;
using System.Collections.Generic;
using Assets.MazeTools.Scripts;
using UnityEngine;

public class Builder : MonoBehaviour {

    public bool _isSelected;

    private MazeMoverMouse moverScript;

    // Use this for initialization
	void Start ()
	{
	    moverScript = this.GetComponent<MazeMoverMouse>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButtonDown(0))
	    {
	        if (_isSelected)
	        {
	            moverScript.MoveToTarget();
	        } else
	            CheckSelection();
	    }
	}

    void CheckSelection()
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        if (hit)
        {
            if (hitInfo.transform.gameObject == this.gameObject)
            {
                _isSelected = true;
            }
        }
    }
}
