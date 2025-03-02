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

    private List<UnityPackage> Actions;
    private bool executing = false;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
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
            Debug.Log("Compilation failed...");
            return;
        }

        Actions = actions;
        executing = true;
        timer = 1;
    }
}
