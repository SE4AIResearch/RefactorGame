using RefactorLang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using static UnityEngine.Rendering.ReloadAttribute;

public class ChefExecute : MonoBehaviour
{
    public GameObject LocationMap;
    public TextMeshProUGUI ActionDisplay;
    public CurrentKitchenState Kitchen;
    // public UpdateTestStatus TestStatusHandler;

    private List<UnityPackage> Actions;
    private bool executing = false;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.Find("Food").gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!executing) return;

        timer -= Time.deltaTime;

        if (timer > 0) return;

        timer++;

        UnityPackage package = Actions[0];
        Actions.RemoveAt(0);

        switch (package.Action)
        {
            case UnityAction.ChefMove(ChefLocation loc):
                Transform target = LocationMap.transform.Find(Interpreter.StringOfLocation(loc));
                transform.position = target.position;

                Vector3 foodPosition = target.position;
                foodPosition.y -= 130f;
                this.transform.Find("Food").gameObject.transform.position = foodPosition;
                break;

            case UnityAction.PickUp(FoodItem item):
                AddFoodItem(item);
                break;

            case UnityAction.PutDown:
                RemoveFoodItem();
                break;

            case UnityAction.Success:
                Kitchen.UpdateTestCaseStatus(TestStatus.Passed);
                break;

            case UnityAction.Failure:
                Kitchen.UpdateTestCaseStatus(TestStatus.Failed);
                break;

            default:
                Debug.Log("(not implemented)");
                break;
        }

        ActionDisplay.text = package.Message;

        if (Actions.Count == 0)
            executing = false;
    }

    public void Execute(List<UnityPackage> actions)
    {
        if (actions == null)
        {
            Kitchen.UpdateTestCaseStatus(TestStatus.Failed);
            Debug.Log("Compilation failed...");
            return;
        }

        Kitchen.UpdateTestCaseStatus(TestStatus.Running);
        Actions = actions;
        executing = true;
        timer = 1;
    }

    public void AddFoodItem(FoodItem item)
    {
        string assetPath = FoodItemGraphicPath(item);
        Sprite foodSprite = Resources.Load<Sprite>(assetPath);

        if (foodSprite != null)
        {
            this.transform.Find("Food").gameObject.SetActive(true);
            SpriteRenderer renderer = this.transform.Find("Food").GetComponent<SpriteRenderer>();
            renderer.sprite = foodSprite;
        }
        else
        {
            this.transform.Find("Food").gameObject.SetActive(false);
            Debug.Log("Failed to load sprite: " + assetPath);
        }
    }

    public void RemoveFoodItem()
    {
        this.transform.Find("Food").gameObject.SetActive(false);
        SpriteRenderer renderer = this.transform.Find("Food").GetComponent<SpriteRenderer>();
        renderer.sprite = null;
    }

    public string FoodItemGraphicPath(FoodItem item)
    {
        string path = "Graphics/Food/";

        switch (item)
        {
            case FoodItem.Some(string food):
                path += food;
                break;
            case FoodItem.None:
                path += "None.png";
                break;

        }

        return path;
    }
}
