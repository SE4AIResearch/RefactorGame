namespace RefactorLib
{
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
        STAR,   // *
        FSLASH, // /
        MOD,    // %
        AND,    // &&, "and"
        OR,     // ||, "or"
        NOT,    // !, "not"

        EOL,    // <eol>
        EOF,    // <eof>

        LBRACE, // {
        RBRACE, // }
        LPAREN, // (
        RPAREN, // )

        IF,     // if
        ELSE,   // else

        WHILE,  // while
        FOR,    // for

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


    // The IExp interface binds all of the following expressions to make them work with a single root parsing function.
    // The list of tokens is interpreted as an IExp[] and simplified recursively.
    public interface IExp { }

}