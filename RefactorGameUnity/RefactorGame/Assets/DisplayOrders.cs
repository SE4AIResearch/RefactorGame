using System;
using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using UnityEngine;
using UnityEngine.UI;

public class DisplayOrders : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += UpdateSelection;

        this.transform.Find("Order 1").gameObject.SetActive(false);
        this.transform.Find("Order 2").gameObject.SetActive(false);
        this.transform.Find("Order 3").gameObject.SetActive(false);
        this.transform.Find("Order 4").gameObject.SetActive(false);
        this.transform.Find("Order 5").gameObject.SetActive(false);

        this.transform.Find("Order 1").GetComponent<Button>().Select();

        Color orderColor = this.transform.Find("Order 1").GetComponent<Image>().color;
        this.transform.Find("OrderInfo").GetComponent<Image>().color = orderColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSelection(KitchenState newState)
    {
        for (int i = 0; i < newState.TestCases.Count; i++) {
            this.transform.Find($"Order {i+1}").gameObject.SetActive(true);
        }

        int index = newState.SelectedTestCase;

        Color orderColor = this.transform.Find($"Order {index + 1}").GetComponent<Image>().color;
        this.transform.Find("OrderInfo").GetComponent<Image>().color = orderColor;

        if (newState.TestCaseStatus[index] == TestStatus.Running) {
            this.transform.Find("Order 1").GetComponent<Button>().enabled = false;
            this.transform.Find("Order 2").GetComponent<Button>().enabled = false;
            this.transform.Find("Order 3").GetComponent<Button>().enabled = false;
            this.transform.Find("Order 4").GetComponent<Button>().enabled = false;
            this.transform.Find("Order 5").GetComponent<Button>().enabled = false;

        } else {
            this.transform.Find("Order 1").GetComponent<Button>().enabled = true;
            this.transform.Find("Order 2").GetComponent<Button>().enabled = true;
            this.transform.Find("Order 3").GetComponent<Button>().enabled = true;
            this.transform.Find("Order 4").GetComponent<Button>().enabled = true;
            this.transform.Find("Order 5").GetComponent<Button>().enabled = true;
        }
    }

    private void OnDestroy()
    {
        kitchen.OnStateChanged -= UpdateSelection;
    }
}
