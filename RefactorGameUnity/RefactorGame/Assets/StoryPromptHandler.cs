using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class StoryPromptHandler : MonoBehaviour
{
    public GameObject Overlay;
    public CurrentKitchenState CurrentKitchenState;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(true);
        Overlay.SetActive(true);
        CurrentKitchenState.ContextMenuUp = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Overlay.activeSelf) CurrentKitchenState.ContextMenuUp = true;
    }

    public void OnHover(GameObject Screen)
    {
        Image img = Screen.transform.Find("Watson").GetComponent<Image>();
        Sprite sprite = Resources.Load<Sprite>("Graphics/LevelStart/watsonCOOKS");
        img.sprite = sprite;
    }

    public void OffHover(GameObject Screen)
    {
        Image img = Screen.transform.Find("Watson").GetComponent<Image>();
        Sprite sprite = Resources.Load<Sprite>("Graphics/LevelStart/watsonConfused");
        img.sprite = sprite;
    }

    public void Open(GameObject Screen)
    {
        Transform buttonText = Screen.transform.Find("Button").Find("Text (TMP)");
        buttonText.GetComponent<TextMeshProUGUI>().text = "Continue Level";

        Screen.SetActive(true);
        Overlay.SetActive(true);
        CurrentKitchenState.ContextMenuUp = true;
    }

    public void Close(GameObject Screen) {
        Screen.SetActive(false);
        Overlay.SetActive(false);
        CurrentKitchenState.ContextMenuUp = false;
    }
}
