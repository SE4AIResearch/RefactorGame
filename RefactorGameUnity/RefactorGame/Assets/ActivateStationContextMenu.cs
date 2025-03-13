using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateStationContextMenu : MonoBehaviour
{
    readonly DetectMouse DetectMouse;
    public StationContextMenu StationContextMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        StationContextMenu.OpenMenu();
    }
}
