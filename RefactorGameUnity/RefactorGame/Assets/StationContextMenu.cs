using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationContextMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CloseMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
