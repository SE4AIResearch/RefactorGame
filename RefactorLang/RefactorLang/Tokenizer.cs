using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 *      The Tokenizer goes through the provided code in the form of a script and converts the concrete syntax
 *      provided into the abstract syntax defined in the RefactorLang Specification document.
*/

namespace RefactorLang
{
    // Defines all of the reserved symbols and words required for the language.
    public enum Symbol
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
        EOF,    // <eof>

        LBRACE, // {
        RBRACE, // }
        LPAREN, // (
        RPAREN, // )

        IF,     // if
        ELSE,   // else

        STATIC, // static
        FIELD,  // field
        RETURN, // return

        COMMA,  // ,
    }

    // Defines the different tokens that can be expected by the Parser.
    // Note: Ident refers to Identifier, which can refer to the names of variables, classes, functions, etc.
    public record Token : IExp
    {
        public record TokenSymbol(Symbol Symbol) : Token();
        public record TokenIdent(string Ident) : Token();
        public record TokenNumber(int Number) : Token();
    }
    internal class Tokenizer
    {
        // Attempts to "tokenize" a line of RefactorLang by converting words and symbols into a list of tokens.
        public static List<Token> TokenizeLine(string text)
        {
            List<Token> output = new();

            // Conversion list from concrete syntax to Symbols.
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

                { "static", Symbol.STATIC },
                { "field", Symbol.FIELD },
                { "return", Symbol.RETURN },

                { ",", Symbol.COMMA },
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

            return output.Append(new Token.TokenSymbol(Symbol.EOF)).ToList();
        }
    }
}
