using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using Unity.VisualScripting;
using UnityEngine;
using TextEditor = InGameTextEditor.TextEditor;
using Image = UnityEngine.UI.Image;

public class CodeChangeHandler : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        TextEditor editor = this.gameObject.transform.Find("TextEditor").GetComponent<TextEditor>();
        if (kitchen.LastSolution != null && kitchen.LastSolution.Text.Equals(editor.Text))
        {
            return;
        }

        GameObject OrderSelector = GameObject.Find("OrderSelector");

        for (int i = 0; i < kitchen.KitchenState.TestCases.Count; i++)
        {
            GameObject order = OrderSelector.transform.Find($"Order {i + 1}").gameObject;
            GameObject display = order.transform.Find("Display").gameObject;
            Image tabIcon = display.transform.Find("Status").GetComponent<Image>();
            tabIcon.gameObject.SetActive(false);
        }

        GameObject orderInfo = OrderSelector.transform.Find("OrderInfo").gameObject;
        Image infoIcon = orderInfo.transform.Find("Status").GetComponent<Image>();
        infoIcon.gameObject.SetActive(false);

        kitchen.ResetTestCaseStatus();
    }
}
