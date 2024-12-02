using System.Linq;

using RefactorLib;
using ParserLibrary;
using RefactorLang;
using Microsoft.FSharp.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using C5;

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
}