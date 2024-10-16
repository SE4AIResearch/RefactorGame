using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorLang
{
    enum Symbol
    {
        VAR,    // var (variable declaration)
        FUNC,   // func (function declaration)
        CLASS,  // class (class declaration)

        TRUE,   // true (boolean)
        FALSE,  // false (boolean)

        EQ,     // =
        EQEQ,   // ==
        NEQ,    // !=
        PLUS,   // +
        DASH,   // -
        EOL,    // <eol>

        LBRACE, // {
        RBRACE, // }
        LPAREN, // (
        RPAREN, // )

        IF,     // if
        ELSE,   // else
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
                { "var", Symbol.VAR },
                { "func", Symbol.FUNC },
                { "class", Symbol.CLASS },

                { "true", Symbol.TRUE },
                { "false", Symbol.FALSE },

                { "=", Symbol.EQ },
                { "==", Symbol.EQEQ },
                { "!=", Symbol.NEQ },
                { "+", Symbol.PLUS },
                { "-", Symbol.DASH },
                { "\r\n", Symbol.EOL },

                { "{", Symbol.LBRACE },
                { "}", Symbol.RBRACE },
                { "(", Symbol.LPAREN },
                { ")", Symbol.RPAREN },

                { "if", Symbol.IF },
                { "else", Symbol.ELSE },
            };

            string[] words = text.Replace("\t", "").Replace("\r\n", " \r\n ").Split(new char[] { ' ' });
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
                else if (word != "")
                {
                    output.Add(new Token.TokenIdent(word));
                }
            }

            return output;
        }
    }
}
