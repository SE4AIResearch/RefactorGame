namespace RefactorLib
{
    public enum Symbol
    {
        // Declarations
        VAR,    // var (variable declaration)
        FUNC,   // func (function declaration)
        CLASS,  // class (class declaration)
        STATIC, // static
        FIELD,  // field

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
        POTADD,
        POTREMOVE,
        BOIL,
        DELIVER
    }

    // Defines the different tokens that can be expected by the Parser.
    // Note: Ident refers to Identifier, which can refer to the names of variables, classes, functions, etc.
    public interface Token : IExp { }

    public class TokenSymbol : Token
    {
        public Symbol Symbol;
        public TokenSymbol(Symbol symbol)
        {
            Symbol = symbol;
        }
    }

    public class TokenIdent : Token
    {
        public string Ident;
        public TokenIdent(string ident)
        {
            Ident = ident;
        }
    }

    public class TokenNumber : Token
    {
        public float Number;
        public TokenNumber(float number)
        {
            Number = number;
        }
    }

    public class TokenString : Token
    {
        public string String;
        public TokenString(string str)
        {
            String = str;
        }
    }

    public class TokenKeyword : Token
    {
        public Keyword Keyword;
        public TokenKeyword(Keyword keyword)
        {
            Keyword = keyword;
        }
    }

    // The IExp interface binds all of the following expressions to make them work with a single root parsing function.
    // The list of tokens is interpreted as an IExp[] and simplified recursively.
    public interface IExp { }

}