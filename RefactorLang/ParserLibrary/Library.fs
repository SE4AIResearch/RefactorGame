namespace ParserLibrary

open RefactorLib

module Parser =
    type id = string

    type token =
    | TokenNumber of float
    | TokenIdent of id
    | TokenSymbol of Symbol

    type result<'a> =
    | Success of 'a * token list
    | Failure of string

    type parser<'a> = 
    | Parser of (token list -> result<'a>)

    type binop =
    | Add | Sub | Eq | Neq

    type exp =
    | CNum of float
    | CBool of bool
    | CVar of id
    | CStr of string
    | Bop of binop * exp * exp

    type stmt =
    | Ret
    | RetExp of exp
    | Decl of id * exp
    | Assn of exp * exp

    type block = stmt list

    type inst =
    | Static | NonStatic

    type decl =
    | VDecl of inst * id
    | FDecl of inst * id * (id list) * block

    type clas = Class of id * (decl list)

    type prog = Prog of clas list

    let rec convertSymbols (tokens: Token list) : token list =
        match tokens with
        | [] -> []
        | :? Token.TokenSymbol as ts :: t -> TokenSymbol ts.Symbol :: convertSymbols t
        | :? Token.TokenNumber as tn :: t -> TokenNumber tn.Number :: convertSymbols t
        | :? Token.TokenIdent as ti :: t -> TokenIdent ti.Ident :: convertSymbols t
        | _ -> raise (System.Exception "convertSymbols failed to match")

    let parseSymbol (symbol: Symbol) : parser<token> =
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure "No more input."
            | TokenSymbol ts :: t when ts = symbol -> Success (TokenSymbol ts, t)
            | _ -> Failure "todo: write better error message"
        Parser parseHelper

    let parseAnyNumber : parser<float> =
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure "No more input."
            | TokenNumber n :: t -> Success (n, t)
            | _ -> Failure "todo: write better error message"
        Parser parseHelper

    let parseAnyBool : parser<bool> =
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure "No more input."
            | TokenSymbol ts :: t when ts = Symbol.TRUE -> Success (true, t)
            | TokenSymbol ts :: t when ts = Symbol.FALSE -> Success (false, t)
            | _ -> Failure "todo: write better error message"
        Parser parseHelper 

    let parseAnyIdent : parser<id> =
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure "No more input."
            | TokenIdent ti :: t -> Success (ti, t)
            | _ -> Failure "todo: write better error message"
        Parser parseHelper 

    let parseBinop : parser<binop> =
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure "No more input."
            | TokenSymbol ts :: t when ts = Symbol.PLUS -> Success (Add, t)
            | TokenSymbol ts :: t when ts = Symbol.DASH -> Success (Sub, t)
            | TokenSymbol ts :: t when ts = Symbol.EQEQ -> Success (Eq, t)
            | TokenSymbol ts :: t when ts = Symbol.NEQ  -> Success (Neq, t)
            | _ -> Failure "todo: write better error message"
        Parser parseHelper
    
    let run (parser: parser<'a>) (stream: token list) : result<'a> =
        match parser with Parser p -> p stream

    let returnP (x: 'a) : parser<'a> =
        let parseHelper (stream: token list) =
            Success(x, stream)
        Parser parseHelper

    let andThen (parser1: parser<'a>) (parser2: parser<'b>) : parser<'a * 'b> =
        let parseHelper (stream: token list) =
            match run parser1 stream with
            | Failure err -> Failure err
            | Success (v1, tl1) -> 
                match run parser2 tl1 with
                | Failure err -> Failure err
                | Success (v2, tl2) -> Success ((v1, v2), tl2)
        Parser parseHelper

    let orElse (parser1: parser<'a>) (parser2: parser<'a>) : parser<'a> =
        let parseHelper (stream: token list) =
            match run parser1 stream with
            | Success (v, tl) -> Success (v, tl)
            | Failure _ -> run parser2 stream
        Parser parseHelper

    let choice (parsers: parser<'a> list) : parser<'a> =
        List.reduce orElse parsers

    let rec zeroOrMore (parser: parser<'a>) (stream: token list) =
        match run parser stream with
        | Failure err -> ([], stream)
        | Success (v, tl) -> 
            let v2, tl2 = zeroOrMore parser tl
            (v :: v2, tl2)

    let many (parser: parser<'a>) =
        let parseHelper (stream: token list) =
            Success (zeroOrMore parser stream)
        Parser parseHelper

    let mapP (f: 'a -> 'b) (parser: parser<'a>) : parser<'b> =
        let parseHelper (stream: token list) =
            match run parser stream with
            | Success (v, tl) -> Success (f v, tl)
            | Failure err -> Failure err
        Parser parseHelper

    let createParserForwardedToRef<'a>() : parser<'a> * parser<'a> ref =
        let dummyParser : parser<'a> =
            let parseHelper _ = failwith "unfixed forwarded parser"
            Parser parseHelper

        let parserRef = ref dummyParser
        let parseHelper stream =
            run parserRef.Value stream

        Parser parseHelper, parserRef

    let ( .>>. ) = andThen
    let ( <|> ) = orElse
    let ( <!> ) = mapP
    let ( |>> ) x f = mapP f x

    let ( .>> ) (p1: parser<'a>) (p2: parser<'b>) : parser<'a> =
        p1 .>>. p2 |> mapP (fun (a, _) -> a)
    let ( >>. ) (p1: parser<'a>) (p2: parser<'b>) : parser<'b> =
        p1 .>>. p2 |> mapP (fun (_, b) -> b)

    let between (p1: parser<'a>) (p2: parser<'b>) (p3: parser<'c>) : parser<'b> =
        p1 >>. p2 .>> p3

    let betweenSymbols (s1: Symbol) (p: parser<'a>) (s2: Symbol) : parser<'a> =
        parseSymbol s1 >>. p .>> parseSymbol s2

    let sepBy1 (p: parser<'a>) (sep: parser<'b>) : parser<'a list> =
        let sepThenP = sep >>. p
        p .>>. many sepThenP
        |>> fun (p, plist) -> p :: plist

    let sep (p: parser<'a>) (sep: parser<'b>) : parser<'a list> =
        sepBy1 p sep <|> returnP []

    let newlines = many (parseSymbol Symbol.EOL)

    let parseStatic : parser<inst> = 
        let parseHelper (stream: token list) =
            match run (parseSymbol Symbol.STATIC) stream with
            | Success (_, tl) -> Success (Static, tl)
            | Failure _ -> Success (NonStatic, stream)
        Parser parseHelper

    let parseExp, parseExpRef = createParserForwardedToRef<exp>()

    parseExpRef.Value <-
        choice [
        parseAnyNumber |>> CNum
        //parseExp .>>. parseBinop .>>. parseExp |>> fun ((e1, b), e2) -> Bop (b, e1, e2)
        parseAnyBool |>> CBool
        parseAnyIdent |>> CVar
        betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN
        ]

    let parseStmt : parser<stmt> =
        let parseRetExp = newlines >>. betweenSymbols Symbol.RETURN parseExp Symbol.EOL .>> newlines
        let parseRet = newlines >>. parseSymbol Symbol.RETURN >>. parseSymbol Symbol.EOL .>> newlines
        choice [
            parseRetExp |>> RetExp
            parseRet |>> fun (_) -> Ret
        ]
  
    let parseBlock : parser<block> = 
        betweenSymbols Symbol.LBRACE (many parseStmt) Symbol.RBRACE

    let parseDecl : parser<decl> =
        let parseVDecl = newlines >>. parseStatic .>> parseSymbol Symbol.FIELD .>>. parseAnyIdent .>> parseSymbol Symbol.EOL .>> newlines
        let parseFDecl = newlines >>. parseStatic .>> parseSymbol Symbol.FUNC .>>. parseAnyIdent .>>. betweenSymbols Symbol.LPAREN (sep parseAnyIdent (parseSymbol Symbol.COMMA)) Symbol.RPAREN .>>. parseBlock .>> newlines
        choice [
            parseVDecl |>> fun (i, id) -> VDecl (i, id)
            parseFDecl |>> fun (((i, id), ids), bl) -> FDecl (i, id, ids, bl)
        ]

    let parseClass : parser<clas> =
        let classDef = newlines >>. parseSymbol Symbol.CLASS >>. parseAnyIdent .>>. betweenSymbols Symbol.LBRACE (many parseDecl) Symbol.RBRACE .>> newlines
        classDef |>> fun (id, decls) -> Class (id, decls)

    let parseProg : parser<prog> =
        newlines >>. many parseClass .>> newlines .>> parseSymbol Symbol.EOF |>> Prog

    let parse (tokens: List<Token>) : prog = 
        match run parseProg (convertSymbols tokens) with
        | Success (v, _) -> v
        | Failure err -> raise (System.Exception err)