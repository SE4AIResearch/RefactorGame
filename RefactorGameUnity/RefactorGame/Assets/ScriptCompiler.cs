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

    public static void Compile(string input)
    {
        List<Token> tokens = Tokenizer.TokenizeLine(input);

        string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));
        Debug.Log(result);

        Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

        try
        {
            Interpreter.Interpret(prog, new List<FoodItem> { FoodItem.BoiledPasta }, new HashBag<FoodItem> { FoodItem.Pasta, FoodItem.Sauce });
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
