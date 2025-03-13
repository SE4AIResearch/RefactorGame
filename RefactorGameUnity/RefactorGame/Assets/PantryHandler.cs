using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PantryHandler : MonoBehaviour
{
    private bool DoorOpen = false;
    public PantryMenuHandler PantryMenuHandler;
    public CurrentKitchenState Kitchen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleDoor()
    {
        DoorOpen = !DoorOpen;

        Sprite newSprite = Resources.Load<Sprite>($"Graphics/pantryDoor{(DoorOpen ? "Open" : "Closed")}");
        SpriteRenderer renderer = this.gameObject.transform.Find("Image").GetComponent<SpriteRenderer>();

        renderer.sprite = newSprite;

        if (DoorOpen)
        {
            PantryMenuHandler.OpenMenu();
        }
    }

    private void OnMouseDown()
    {
        if (Kitchen.ContextMenuUp) return;

        ToggleDoor();
    }
}
