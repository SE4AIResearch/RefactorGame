using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubmitText : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnSubmit()
    {
        Debug.Log(inputField.text);
        ScriptCompiler.Compile(inputField.text);
    }
}
