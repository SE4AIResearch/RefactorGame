using ParserLibrary;
using RefactorLang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading;
using TMPro;
using UnityEngine;
using TextEditor = InGameTextEditor.TextEditor;

public class ChefExecute : MonoBehaviour
{
    public GameObject LocationMap;
    public TextMeshProUGUI ActionDisplay;
    public CurrentKitchenState Kitchen;
    // public UpdateTestStatus TestStatusHandler;
    public TextEditor textEditor;
    public GameObject Tutorial;

    private List<UnityPackage> Actions;
    private bool executing = false;
    private float timer = 0;
    private string text;

    private string currentChefLocation;
    public AudioHandler AudioHandler;

    private CompilationStats CompilationStats;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.Find("Food").gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!executing)
        {
            if (Kitchen.LoadedPuzzle.Name == "Welcome To Refactoria")
                Tutorial.SetActive(true);
        }
        else
            Tutorial.SetActive(false);

        if (!executing) return;

        timer -= Time.deltaTime;

        if (timer > 0) return;

        timer+=0.7f;

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

                currentChefLocation = loc.ToString();
                AudioHandler.Play("move");
                break;

            case UnityAction.PickUp(FoodItem item):
                switch (currentChefLocation)
                {
                    case "Pantry { }":
                        AudioHandler.Play("door_open");
                        break;
                    case "Window { }":
                        break;
                    default:
                        AudioHandler.Play("pick_up");
                        break;
                }
                AddFoodItem(item);
                break;

            case UnityAction.PutDown:
                switch (currentChefLocation)
                {
                    case "Pantry { }":
                        break;
                    case "Window { }":
                        AudioHandler.Play("deliver");
                        break;
                    default:
                        AudioHandler.Play("put_down");
                        break;
                }

                RemoveFoodItem();
                break;

            case UnityAction.Success:
                Kitchen.FinishTestWithStatus(TestStatus.Passed, text, CompilationStats);
                break;

            case UnityAction.Failure:
                Kitchen.FinishTestWithStatus(TestStatus.Failed, text, CompilationStats);
                break;

            case UnityAction.Use:
                AudioHandler.Play("activate");
                break;

            default:
                Debug.Log("(not implemented)");
                break;
        }

        ActionDisplay.text = package.Message;

        if (Actions.Count == 0)
            executing = false;
    }

    public void Execute(List<UnityPackage> actions, string text, CompilationStats compilationStats)
    {
        Kitchen.UpdateTestCaseStatus(TestStatus.Running);
        Actions = actions;
        executing = true;
        timer = 1;
        this.text = text;
        this.CompilationStats = compilationStats;
    }

    public void StopExecution()
    {
        Kitchen.FinishTestWithStatus(TestStatus.NotRun, text, CompilationStats);
        executing = false;
        Actions = null;
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
