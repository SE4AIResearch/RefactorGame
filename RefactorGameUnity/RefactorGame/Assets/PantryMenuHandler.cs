using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PantryMenuHandler : MonoBehaviour
{
    public CurrentKitchenState Kitchen;
    public GameObject HoverableFoodItem;
    public PantryHandler PantryHandler;
    public GameObject Overlay;

    // Start is called before the first frame update
    void Start()
    {
        Kitchen.OnStateChanged += UpdateAppearance;
        Overlay.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        Overlay.gameObject.SetActive(true);
        Kitchen.ContextMenuUp = true;
    }    

    public void CloseMenu()
    {
        gameObject.SetActive(false);
        Overlay.gameObject.SetActive(false);
        Kitchen.ContextMenuUp = false;

        PantryHandler.ToggleDoor();
    }

    void UpdateAppearance(KitchenState state)
    {
        if (HoverableFood._HoverableFoodItem == null)
            HoverableFood._HoverableFoodItem = HoverableFoodItem;

        var child = gameObject.transform.Find("FoodItems");

        foreach (Transform t in child.transform.Cast<Transform>().ToList())
        {
            Destroy(t.gameObject);
        }

        foreach (string food in state.Pantry)
        {
            HoverableFood.Create(food, child.transform);
        }
    }
}
