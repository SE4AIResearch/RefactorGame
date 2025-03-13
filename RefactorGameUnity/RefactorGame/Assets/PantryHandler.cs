using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PantryHandler : MonoBehaviour
{
    public bool DoorOpen = false;

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
        DoorOpen = !DoorOpen;

        Sprite newSprite = Resources.Load<Sprite>($"Graphics/pantryDoor{(DoorOpen ? "Open" : "Closed")}");
        SpriteRenderer renderer = this.gameObject.transform.Find("Image").GetComponent<SpriteRenderer>();

        renderer.sprite = newSprite;
    }
}
