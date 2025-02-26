using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InGameTextEditor;
using ParserLibrary;
using RefactorLang;
using RefactorLib;
using UnityEngine.UI;
using System;

public class SubmitText : MonoBehaviour
{
    public InGameTextEditor.TextEditor textEditor;
    public GameObject chef;

    public void OnSubmit()
    {
        string text = textEditor.Text;

        List<UnityPackage> outputLog = ScriptCompiler.Compile(text);

        chef.GetComponent<ChefExecute>().Execute(outputLog);
    }
}
