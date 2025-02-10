namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

namespace RefactorLib
{
    public enum Symbol
    {
        // Declarations
        VAR,    // var (variable declaration)
        FUNC,   // func (function declaration)

        // Assignment
        EQ,     // =

        // Return
        RETURN, // return

        // Booleans
        TRUE,   // true (boolean)
        FALSE,  // false (boolean)

        // Boolean Operators
        EQEQ,   // ==
        NEQ,    // !=
        AND,    // &&, "and"
        OR,     // ||, "or"
        NOT,    // !, "not"

        // Numeric Operators
        PLUS,   // +
        DASH,   // -
        STAR,   // *
        FSLASH, // /
        MOD,    // %
        GT,     // >
        GTE,    // >=
        LT,     // <
        LTE,    // <=

        // Other Operators
        HASH,   // #

        // Containers
        LBRACE, // {
        RBRACE, // }
        LPAREN, // (
        RPAREN, // )
        LBRACK, // [
        RBRACK, // ]

        // Conditionals
        IF,     // if
        ELSE,   // else

        // Loops
        WHILE,  // while
        FOR,    // for
        FOREACH, // foreach
        IN, // in

        // Puncuation
        COMMA,  // ,
        DOT,    // .

        // End-Of Tokens
        EOL,    // <eol>
        EOF,    // <eof>
    }

    public enum Keyword
    {
        GOTO,
        GET,
        PUT,
        TAKE,
        ACTIVATE,
        DELIVER,
        PRINT
    }

    // Defines the different tokens that can be expected by the Parser.
    // Note: Ident refers to Identifier, which can refer to the names of variables, classes, functions, etc.
    public record Token
    {
        public record TokenSymbol(Symbol Symbol) : Token;
        public record TokenIdent(string Ident) : Token;
        public record TokenNumber(int Number) : Token;
        public record TokenString(string String) : Token;
        public record TokenKeyword(Keyword Keyword) : Token;
    }
}