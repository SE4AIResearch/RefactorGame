using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using RefactorLang;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayOrderInfo : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += UpdateInfo;
        GameObject displays = this.transform.Find("Displays").gameObject;

        displays.transform.Find("Display 1").gameObject.SetActive(false);
        displays.transform.Find("Display 2").gameObject.SetActive(false);
        displays.transform.Find("Display 3").gameObject.SetActive(false);
        displays.transform.Find("Display 4").gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {


    }

    void UpdateInfo(KitchenState newState)
    {
        Dictionary<string, int> orderCount = new Dictionary<string, int>();

        int index = newState.SelectedTestCase;

        // Determine how many of each food is being order
        foreach (var order in newState.TestCases[index])
        {
            if (orderCount.ContainsKey(order))
            {
                orderCount[order]++;
            }
            else
            {
                orderCount[order] = 1;
            }
        }

        // Make displays reappear, one per unique food
        int displayIndex = 1;
        foreach (KeyValuePair<string, int> entry in orderCount)
        {
            MakeInfo(displayIndex, entry.Key, entry.Value);
            displayIndex++;
        }
        HideUnusedDisplays(displayIndex);
    }

    void MakeInfo(int index, string food, int count)
    {
        GameObject displays = this.transform.Find("Displays").gameObject;
        GameObject display = displays.transform.Find($"Display {index}").gameObject;
        display.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = count.ToString();
        display.transform.Find("Image").GetComponent<UnityEngine.UI.Image>().sprite = GetFoodSprite(food);
        display.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = food;
        display.SetActive(true);
    }

    void HideUnusedDisplays(int startIndex)
    {
        for (int i = startIndex; i <= 4; i++)
        {
            GameObject displays = this.transform.Find("Displays").gameObject;
            GameObject display = displays.transform.Find($"Display {i}").gameObject;
            display.SetActive(false);
        }
    }

    Sprite GetFoodSprite(string food)
    {
        string path = $"Graphics/Food/{food}";
        return Resources.Load<Sprite>(path);
    }

    private void OnDestroy()
    {
        kitchen.OnStateChanged -= UpdateInfo;
    }
}

