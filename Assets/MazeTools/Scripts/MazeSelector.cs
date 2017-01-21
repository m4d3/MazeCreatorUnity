using UnityEngine;

public class MazeSelector : MonoBehaviour
{
    private GameObject _tile;

    public MazeTile.MazeTileTypes[] TypeFilter;

    public GameObject SelectedTile { get; private set; }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (_tile && _tile != SelectedTile) _tile.GetComponent<Renderer>().material.SetColor("_Color", Color.white);

        RaycastHit hitInfo;
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        if (!hit) return;
        if (!hitInfo.transform.gameObject.GetComponent<MazeTile>() || hitInfo.transform.gameObject == SelectedTile) return;

        if (TypeFilter.Length > 0)
        {
            foreach (MazeTile.MazeTileTypes type in TypeFilter)
            {
                _tile = hitInfo.transform.gameObject;

                if (_tile.GetComponent<MazeTile>().Type == type) 
                    _tile.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                else
                    _tile = null;
                
            }
        }
        else
        {
            _tile = hitInfo.transform.gameObject;
            _tile.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }

        if (Input.GetMouseButtonDown(0) && _tile != null)
        {
            if (SelectedTile)
            {
                SelectedTile.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }
            SelectedTile = _tile;
            SelectedTile.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        }
    }
}