using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{

    public static SelectionManager Instance { get; private set; }
    public GameObject SelectedObject;
    public List<SelectionComponent> Selections;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {

    }

    private void Update() {
        if (Input.GetMouseButtonDown(1))
        {
            DeselectAll();
        }
    }

    public void DeselectType(System.Type type)
    {
        SelectionComponent[] objects = FindObjectsOfType<SelectionComponent>();
        if (objects.Length <= 0) return;
        foreach (SelectionComponent sc in objects)
        {
            if(sc.Type != type || !sc.IsSelected) continue;

            sc.IsSelected = false;
            sc.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void DeselectAll()
    {
        foreach (SelectionComponent selection in Selections) {
            DeselectType(selection.Type);
        }
        Selections.Clear();
        SelectedObject = null;
    }

    public void Select(GameObject go)
    {
        SelectedObject = go;
        go.GetComponent<Renderer>().material.color = Color.magenta;
        bool selectionAdded = false;
        for (int i = 0; i < Selections.Count; i++)
        {
            if (Selections[i].Type != go.GetComponent<SelectionComponent>().Type) continue;

            Selections[i] = go.GetComponent<SelectionComponent>();
            selectionAdded = true;
        }
        if (!selectionAdded)
        {
            Selections.Add(go.GetComponent<SelectionComponent>());
        }
    }

    public GameObject GetSelectedOfType(System.Type type)
    {
        GameObject selected = null;
        foreach (SelectionComponent selection in Selections)
        {
            if (selection.Type != type) continue;
            selected = selection.gameObject;
        }

        return selected;
    }
}
