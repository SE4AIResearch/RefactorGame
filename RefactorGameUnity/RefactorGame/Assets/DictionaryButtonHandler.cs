using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryButtonHandler : MonoBehaviour
{

    public GameObject Dictionary;
    public GameObject Overlay;
    public bool HideOverlay = false;

    // Start is called before the first frame update
    void Start()
    {
        if (HideOverlay)
        {
            Overlay.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDictionary()
    {
        Dictionary.SetActive(true);
        Overlay.SetActive(true);
    }

    public void HideDictionary()
    {
        Dictionary.SetActive(false);
        Overlay.SetActive(false);
    }
}
