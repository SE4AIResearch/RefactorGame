using System;
using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTestCases : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += UpdateSelection;

        this.transform.Find("Case 1").gameObject.SetActive(false);
        this.transform.Find("Case 2").gameObject.SetActive(false);
        this.transform.Find("Case 3").gameObject.SetActive(false);
        this.transform.Find("Case 4").gameObject.SetActive(false);
        this.transform.Find("Case 5").gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSelection(KitchenState newState)
    {
        for (int i = 0; i < newState.TestCases.Count; i++) {
            this.transform.Find($"Case {i+1}").gameObject.SetActive(true);
        }
    }
}
