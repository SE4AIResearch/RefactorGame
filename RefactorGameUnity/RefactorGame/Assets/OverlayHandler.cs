using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayHandler : MonoBehaviour
{
    private RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        this.rectTransform = this.gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        // rectTransform.anchorMin = new Vector2(0, 0);
        // rectTransform.anchorMax = new Vector2(1, 1);
        // rectTransform.offsetMin = Vector2.zero;
        // rectTransform.offsetMax = Vector2.zero;
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
