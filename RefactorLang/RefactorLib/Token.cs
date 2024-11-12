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

    // Defines the different tokens that can be expected by the Parser.
    // Note: Ident refers to Identifier, which can refer to the names of variables, classes, functions, etc.
    public record Token : IExp
    {
        public record TokenSymbol(Symbol Symbol) : Token();
        public record TokenIdent(string Ident) : Token();
        public record TokenNumber(int Number) : Token();
        public record TokenString(string String): Token();
    }


    // The IExp interface binds all of the following expressions to make them work with a single root parsing function.
    // The list of tokens is interpreted as an IExp[] and simplified recursively.
    public interface IExp { }

}