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
            Test();
        }

        static void Test()
        {
            string text = File.ReadAllText("./script.txt");
            List<Token> tokens = Tokenizer.TokenizeLine(text);

            string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));
            Console.WriteLine(result);

            Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

            Interpreter interpreter = new Interpreter(["Potato Soup"], ["Broth", "Broth", "Potato", "Tomato"]);

            try
            {
                interpreter.Interpret(prog);

                Console.WriteLine("=========================");
                interpreter.PrintOutput();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=========================");
                interpreter.PrintOutput();
            }
        }
    }
}