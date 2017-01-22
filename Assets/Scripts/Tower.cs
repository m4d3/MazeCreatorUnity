using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float BuildingTime;
    private MazePlacer _placerComp;
    private SelectionComponent _selectionComp;
    private bool _isBuild;
    private float _buildingStatus;

    private void Awake()
    {
        _placerComp = gameObject.AddComponent<MazePlacer>();
        _selectionComp = gameObject.AddComponent<SelectionComponent>();
        _selectionComp.Type = GetType();
        transform.localScale = new Vector3(1, 0.1f, 1);
    }

    private void BuildingFinished()
    {
        _isBuild = true;
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public bool Build(float buildingSpeed)
    {
        if (_isBuild) return false;

        if (_buildingStatus < BuildingTime)
        {
            _buildingStatus += buildingSpeed * Time.deltaTime;
            transform.localScale = new Vector3(1.0f, _buildingStatus / BuildingTime, 1.0f);
            return true;
        }
        else
        {
            BuildingFinished();
            return false;
        }
    }
}
