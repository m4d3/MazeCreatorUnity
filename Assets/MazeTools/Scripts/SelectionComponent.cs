using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionComponent : MonoBehaviour {
    private SelectionManager _manager;

    public bool IsSelected { get; set; }
    public bool HighlightOnly;
    public System.Type Type;

    private void Start()
    {
        if (!GetComponent<Collider>()) {
            Debug.Log("MazeTools: to use the Selection component, add a Collider!");
        }

        if (SelectionManager.Instance != null) _manager = SelectionManager.Instance;
        else Debug.Log("MazeTools: Please add a SelectionManager to your scene");
    }

    private void OnMouseDown() {
        if (HighlightOnly) return;

        _manager.DeselectType(Type);
        _manager.Select(this.gameObject);
        IsSelected = true;
    }

    private void OnMouseOver()
    {
        if (!HighlightOnly) return;

        _manager.DeselectType(Type);
        gameObject.GetComponent<Renderer>().material.color = Color.gray;
    }
}
