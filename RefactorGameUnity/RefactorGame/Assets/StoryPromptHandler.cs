using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StoryPromptHandler : MonoBehaviour
{
    public GameObject Overlay;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(true);
        Overlay.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open(GameObject Screen)
    {
        Transform buttonText = Screen.transform.Find("Button").Find("Text (TMP)");
        buttonText.GetComponent<TextMeshProUGUI>().text = "Continue";

        Screen.SetActive(true);
        Overlay.SetActive(true);
    }

    public void Close(GameObject Screen) {
        Screen.SetActive(false);
        Overlay.SetActive(false);
    }
}
