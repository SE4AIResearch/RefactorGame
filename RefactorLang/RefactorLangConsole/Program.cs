using System.Linq;

using RefactorLib;
using ParserLibrary;
using RefactorLang;
using Microsoft.FSharp.Collections;
using System.IO;
using System.Collections.Generic;
using System;

namespace RefactorLangConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string text = File.ReadAllText("./script.txt");
            List<Token> tokens = Tokenizer.TokenizeLine(text);

            string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));
            Console.WriteLine(result);

            Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));
            Interpreter.Interpret(prog, new List<FoodItem> { FoodItem.BoiledPasta });
        }
    }
}