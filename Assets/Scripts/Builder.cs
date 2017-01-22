using Assets.MazeTools.Scripts;
using UnityEngine;

public class Builder : MonoBehaviour
{
    public int MovementSpeed = 3;

    private MazeMoverMouse _moverComp;
    private SelectionComponent _selectionComp;

    private void Awake()
    {
        _moverComp = gameObject.AddComponent<MazeMoverMouse>();
        _selectionComp = gameObject.AddComponent<SelectionComponent>();
        _selectionComp.Type = GetType();
    }

    private void Start()
    {
        _moverComp.MoveableTiles = MazeMoverMouse.MovableTilesTypes.Border;
        _moverComp.Speed = MovementSpeed;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            if (_selectionComp.IsSelected) _moverComp.MoveToTarget();
    }
}