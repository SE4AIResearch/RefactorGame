using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParserLibrary;
using RefactorLang;
using RefactorLib;
using Microsoft.FSharp.Collections;
using System;
using System.IO;

public class ScriptCompiler : MonoBehaviour
{
    enum Location
    {
        Pantry,
        Stove1,
        Stove2,
        Counter
    }

    Dictionary<string, object> VariableMap = new Dictionary<string, object>();
    Location ChefLocation = Location.Pantry;

    List<string> Orders = new List<string>();
    List<string> PantryContents = new List<string>();
    List<string> TestCaseOutput = new List<string>();
    List<string> CurrentOutput = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        string text = File.ReadAllText("./Assets/script.txt");
        List<Token> tokens = Tokenizer.TokenizeLine(text);

        string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));
        Debug.Log(result);

        Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
