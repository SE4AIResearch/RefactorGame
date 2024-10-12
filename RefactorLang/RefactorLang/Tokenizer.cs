using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorLang
{
    enum Symbol
    {
        TNUM,   // num (int declaration)
        EQ,     // =
        PLUS,   // +
        DASH,   // -
        SEMI    // ;
    }
    record Token : IExp
    {
        public record TokenSymbol(Symbol Symbol) : Token();
        public record TokenIdent(string Ident) : Token();
        public record TokenNumber(int Number) : Token();
    }
    internal class Tokenizer
    {
        public static List<Token> TokenizeLine(string text)
        {
            List<Token> output = new();

            Dictionary<string, Symbol> tokenLookup = new Dictionary<string, Symbol>
            {
                { "num", Symbol.TNUM },
                { "=", Symbol.EQ },
                { "+", Symbol.PLUS },
                { "-", Symbol.DASH },
                { ";", Symbol.SEMI }
            };

            string[] words = text.Replace("\r\n", "\r").Split(new char[] { ' ', '\r' });
            foreach (string word in words)
            {
                if (tokenLookup.TryGetValue(word, out Symbol symbol))
                {
                    output.Add(new Token.TokenSymbol(symbol));
                }
                else if (int.TryParse(word, out int number))
                {
                    output.Add(new Token.TokenNumber(number));
                }
                else
                {
                    output.Add(new Token.TokenIdent(word));
                }
            }

            return output;
        }
    }
}
