using RefactorLang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoverableFood : MonoBehaviour
{
    public static GameObject _HoverableFoodItem;
    public GameObject HoverableFoodItem;

    // Start is called before the first frame update
    void Start()
    {

        transform.Find("FoodName").gameObject.SetActive(false);

        transform.Find("FoodName").gameObject.GetComponent<TextMeshProUGUI>().text = gameObject.GetComponent<Image>().sprite.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static GameObject Create(string foodItem, Transform parent)
    {
        GameObject newInstance = Instantiate(_HoverableFoodItem, parent.transform);

        Sprite newSprite = Resources.Load<Sprite>($"Graphics/Food/{foodItem}");

        newInstance.GetComponent<Image>().sprite = newSprite;

        newInstance.transform.Find("FoodName").gameObject.GetComponent<TextMeshProUGUI>().text = newInstance.gameObject.GetComponent<Image>().sprite.name;

        return newInstance;
    }

    public void ShowName()
    {
        transform.Find("FoodName").gameObject.SetActive(true);
    }

    public void HideName()
    {
        transform.Find("FoodName").gameObject.SetActive(false);
    }    
}
