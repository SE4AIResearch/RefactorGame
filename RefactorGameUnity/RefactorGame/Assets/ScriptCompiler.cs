using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParserLibrary;
using RefactorLang;
using RefactorLib;
using Microsoft.FSharp.Collections;
using System;
using System.IO;
using C5;

public class ScriptCompiler : MonoBehaviour
{

    public static List<UnityPackage> Compile(string input)
    {
        List<Token> tokens = Tokenizer.TokenizeLine(input);

        string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));

        Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

        try
        {
            Interpreter interpreter = new Interpreter(new List<string> { "Potato Soup" }, new List<string> { "Broth", "Broth", "Potato", "Tomato" });
            interpreter.Interpret(prog);

            return interpreter.OutputLog;
        }
        catch (ArgumentException ex)
        {
            Debug.Log(ex);
            return null;
        }
    }
}
