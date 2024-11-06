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
    // Start is called before the first frame update
    void Start()
    {
        string text = File.ReadAllText("./Assets/script.txt");
        List<Token> tokens = Tokenizer.TokenizeLine(text);

        string result = RefactorLangParser.parse(ListModule.OfSeq(tokens));
        Debug.Log(result);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
