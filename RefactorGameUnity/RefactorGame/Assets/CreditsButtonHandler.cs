using C5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsButtonHandler : MonoBehaviour
{

    public GameObject Credits;
    public GameObject Overlay;
    public bool HideOverlay = false;
    public CurrentKitchenState CurrentKitchenState;

    // Start is called before the first frame update
    void Start()
    {
        if (HideOverlay)
        {
            Overlay.SetActive(false);
        }
        Credits.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowCredits()
    {
        if (CurrentKitchenState != null) {
            if (CurrentKitchenState.ContextMenuUp) return;

            CurrentKitchenState.ContextMenuUp = true;
        }

        Credits.SetActive(true);
        Overlay.SetActive(true);
    }

    public void HideCredits()
    {
        Credits.SetActive(false);
        Overlay.SetActive(false);
        if (CurrentKitchenState != null) { CurrentKitchenState.ContextMenuUp = false; }
    }
}
