using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{

    public static SelectionManager Instance { get; private set; }
    public GameObject SelectedObject;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {

    }

    public void DeselectType(System.Type type)
    {
        SelectionComponent[] objects = FindObjectsOfType<SelectionComponent>();
        if (objects.Length <= 0) return;
        foreach (SelectionComponent sc in objects)
        {
            if(sc.Type != type) continue;

            sc.IsSelected = false;
            sc.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void Select(GameObject go)
    {
        SelectedObject = go;
        go.GetComponent<Renderer>().material.color = Color.magenta;
    }
}
