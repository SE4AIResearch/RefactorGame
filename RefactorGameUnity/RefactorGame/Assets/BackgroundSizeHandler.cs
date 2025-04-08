using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BackgroundSizeHandler : MonoBehaviour
{
    public GameObject LocationMap;
    public GameObject Chef;
    private Vector3 LastViewpoint;
    private float LocationMapOffset = 386.62183f;
    private float ChefOffset = 425.31290605468746f;

    // Start is called before the first frame update
    void Start()
    {
        this.LastViewpoint = Camera.main.ViewportToWorldPoint(Vector3.zero);
        UpdateBackgroundSize(this.LastViewpoint);
        UpdateObjectSize(LocationMap, this.LastViewpoint, LocationMapOffset);
        UpdateObjectSize(Chef, this.LastViewpoint, ChefOffset);
    }

    // Update is called once per frame
    void Update()
    {
        var newViewpoint = Camera.main.ViewportToWorldPoint(Vector3.zero);
        
        if (newViewpoint != this.LastViewpoint)
        {
            UpdateBackgroundSize(newViewpoint);
            UpdateObjectSize(LocationMap, newViewpoint, LocationMapOffset);
            UpdateObjectSize(Chef, newViewpoint, ChefOffset);
            this.LastViewpoint = newViewpoint;
        }
        
    }

    private void UpdateBackgroundSize(Vector3 viewpoint)
    {
        float leftEdge = viewpoint.x;
        float zPosition = transform.position.z;

        Renderer renderer = GetComponent<SpriteRenderer>();
        Bounds objectBounds = renderer.bounds;

        float objectLeftEdge = leftEdge + objectBounds.extents.x;
        transform.position = new Vector3(objectLeftEdge, 0f, zPosition);
    }

    private void UpdateObjectSize(GameObject obj, Vector3 viewpoint, float offset)
    {
        Vector3 position = obj.transform.position;
        position.x = viewpoint.x + offset;
        obj.transform.position = position;
    }
}
