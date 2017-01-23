using System;
using UnityEngine;

public class SelectionComponent : MonoBehaviour
{
    public enum SelectionTypes
    {
        Highlight,
        Select,
        HighlightAndSelect,
        None
    }

    private SelectionManager _manager;
    public SelectionTypes SelectionType = SelectionTypes.Select;
    public Type Type;

    public bool IsSelected { get; set; }

    private void Start()
    {
        if (!GetComponent<Collider>()) Debug.Log("MazeTools: to use the Selection component, add a Collider!");

        if (SelectionManager.Instance != null) _manager = SelectionManager.Instance;
        else
        {
            Debug.Log("MazeTools: Please add a SelectionManager to your scene");
        }
    }

    private void OnMouseDown()
    {
        if (SelectionType == SelectionTypes.Highlight) return;

        _manager.DeselectType(Type);
        _manager.Select(gameObject);
        IsSelected = true;
    }

    private void OnMouseOver()
    {
        switch (SelectionType)
        {
            case SelectionTypes.Highlight:
                _manager.DeselectType(Type);
                break;
            case SelectionTypes.Select:
                return;
            case SelectionTypes.HighlightAndSelect:
                if (_manager.SelectedObject == gameObject) return;
                _manager.DeselectType(Type);
                break;
            case SelectionTypes.None:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }

        gameObject.GetComponent<Renderer>().material.color = Color.gray;
        IsSelected = true;
    }

    public GameObject GetSelected(System.Type type)
    {
        return _manager.GetSelectedOfType(type);
    }
}