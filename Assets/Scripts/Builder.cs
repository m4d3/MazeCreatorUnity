using Assets.MazeTools.Scripts;
using UnityEngine;

public class Builder : MonoBehaviour
{
    private Modes _currentMode;
    private Tower _currentTower;

    private MazeMoverMouse _moverComp;
    private SelectionComponent _selectionComp;
    public float BuildingSpeed;
    public int MovementSpeed = 3;
    public GameObject TowerPrefab;

    private void Awake()
    {
        _moverComp = gameObject.AddComponent<MazeMoverMouse>();
        _selectionComp = gameObject.AddComponent<SelectionComponent>();
        _selectionComp.Type = GetType();

        _moverComp.PathEndReachedCallback = PathEndReached;
    }

    private void PathEndReached()
    {
        if (_currentTower != null && _currentTower.GetComponent<MazePlacer>().IsPlaced
            && transform.position.Equals(_currentTower.transform.position))
            _currentMode = Modes.Building;
    }

    private void Start()
    {
        _moverComp.MoveableTiles = MazeMoverMouse.MovableTilesTypes.Border;
        _moverComp.Speed = MovementSpeed;
    }

    private void Update()
    {
        if (_selectionComp.IsSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _moverComp.MoveToTarget();
                _currentMode = Modes.Moving;
                if (_selectionComp.GetSelected(typeof(Tower)) != null)
                    _currentTower = _selectionComp.GetSelected(typeof(Tower)).GetComponent<Tower>();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                _currentTower =
                    Instantiate(TowerPrefab, transform.position, Quaternion.identity).gameObject.GetComponent<Tower>();
            }
        }

        if (_currentMode == Modes.Building)
            if (!_currentTower.Build(BuildingSpeed))
            {
                _currentTower = null;
                _currentMode = Modes.Idle;
            }
    }

    private enum Modes
    {
        Idle,
        Moving,
        Building,
        Repairing
    }
}