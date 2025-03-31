using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class UpdateOrderStatus : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    void Start()
    {
        kitchen.OnStateChanged += UpdateStatusIcon;

        GameObject orderInfo = this.gameObject.transform.Find("OrderInfo").gameObject;
        Transform status = orderInfo.transform.Find("Status");
        status.GetComponent<Image>().gameObject.SetActive(false);

        for (int i = 1; i < 6; i++) {
            GameObject order = this.gameObject.transform.Find($"Order {i}").gameObject;
            GameObject display = order.transform.Find("Display").gameObject;
            Image image = display.transform.Find("Status").GetComponent<Image>();
            image.gameObject.SetActive(false);
        }

    }

    void Update()
    {
        // kitchen.OnStateChanged += UpdateButtonColor;
    }

    public void UpdateStatusIcon(KitchenState newState)
    {
        int index = newState.SelectedTestCase;

        TestStatus status = kitchen.KitchenState.TestCaseStatus[index];
        
        GameObject order = this.gameObject.transform.Find($"Order {index+1}").gameObject;
        GameObject display = order.transform.Find("Display").gameObject;
        Image tabIcon = display.transform.Find("Status").GetComponent<Image>();

        GameObject orderInfo = this.gameObject.transform.Find("OrderInfo").gameObject;
        Image infoIcon = orderInfo.transform.Find("Status").GetComponent<Image>();

        if (status == TestStatus.NotRun) {
            tabIcon.gameObject.SetActive(false);
            infoIcon.gameObject.SetActive(false);
            return;
        }

        string path = "Graphics/Tabs/";

        if (status == TestStatus.Running) {
            path += "runningIcon";

        } else if (status == TestStatus.Failed) {
            path += "failedIcon";

        } else if (status == TestStatus.Passed) {
            path += "passedIcon";
        
        } else if (status == TestStatus.Warning) {
            path += "cautionIcon";
        }

        Sprite sprite = Resources.Load<Sprite>(path);
        tabIcon.sprite = sprite;
        infoIcon.sprite = sprite;

        tabIcon.gameObject.SetActive(true);
        infoIcon.gameObject.SetActive(true);

    }

    public void OnDestroy()
    {
        kitchen.OnStateChanged -= UpdateStatusIcon;
    }
}
