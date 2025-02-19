namespace ParserLibrary

open RefactorLib
open Grammar
open Parser

module RefactorLangParserLib =
    let stringOfToken (token: token) : string =
        match token with
        | TokenIdent id -> id
        | TokenSymbol s -> s.ToString()
        | TokenNumber n -> n.ToString()
        | TokenString s -> s
        | TokenKeyword k -> k.ToString()

    let rec convertSymbols (tokens: Token list) : token list =
        match tokens with
        | [] -> []
        | :? Token.TokenSymbol as ts :: t -> TokenSymbol ts.Symbol :: convertSymbols t
        | :? Token.TokenNumber as tn :: t -> TokenNumber tn.Number :: convertSymbols t
        | :? Token.TokenIdent as ti :: t -> TokenIdent ti.Ident :: convertSymbols t
        | :? Token.TokenString as ts :: t -> TokenString ts.String :: convertSymbols t
        | :? Token.TokenKeyword as tk :: t -> TokenKeyword tk.Keyword :: convertSymbols t
        | _ -> raise (System.Exception "convertSymbols failed to match")

    let parseSymbol (symbol: Symbol) : parser<token> =
        let label = symbol.ToString()
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenSymbol ts :: t when ts = symbol -> Success (TokenSymbol ts, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let parseKeyword (keyword: Keyword) : parser<token> =
        let label = "keyword"
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenKeyword tk :: t when tk = keyword -> Success (TokenKeyword tk, t)
            | h :: t -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let betweenSymbols (s1: Symbol) (p: parser<'a>) (s2: Symbol) : parser<'a> =
        parseSymbol s1 >>. p .>> parseSymbol s2

    let betweenParsers (p1: parser<'a>) (s: Symbol) (p2: parser<'a>) : parser<'a> =
        p1 .>>. parseSymbol s >>. p2

    let newlines = many (parseSymbol Symbol.EOL)
    let betweenNewlines p = newlines >>. p .>> newlines

    let parseAnyString : parser<string> =
        let label = "string"
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenString n :: t -> Success (n, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let parseAnyNumber : parser<int> =
        let label = "number"
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenNumber n :: t -> Success (n, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let parseAnyBool : parser<bool> =
        let label = "boolean"
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenSymbol ts :: t when ts = Symbol.TRUE -> Success (true, t)
            | TokenSymbol ts :: t when ts = Symbol.FALSE -> Success (false, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let parseAnyIdent : parser<id> =
        let label = "identifier"
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenIdent ti :: t -> Success (ti, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let parseAnyKeyword : parser<token> =
        let label = "keyword"
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenKeyword tk :: t -> Success (TokenKeyword tk, t)
            | TokenIdent ti :: t -> Success (TokenIdent ti, t)
            | h :: t -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }
