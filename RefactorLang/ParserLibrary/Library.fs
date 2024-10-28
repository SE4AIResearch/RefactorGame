namespace ParserLibrary

open RefactorLib

module Parser =
    type id = string

    type token =
    | TokenNumber of float
    | TokenIdent of id
    | TokenSymbol of Symbol

    type plabel = string
    type perror = string

    type parser<'a> = {
        parserFn : (token list -> result<'a>)
        label : plabel
    } 
    and result<'a> =
    | Success of 'a * token list
    | Failure of plabel * perror

    type binop =
    | Add of exp * exp
    | Sub of exp * exp
    | Mul of exp * exp
    | Div of exp * exp
    | Eq  of exp * exp
    | Neq of exp * exp
    and exp =
    | CNum of float
    | CBool of bool
    | CVar of id
    | CStr of string
    | Binop of binop

    type stmt =
    | Ret
    | RetExp of exp
    | VDecl of id * exp
    | Assn of id * exp
    | IfThenElse of exp * block * (exp * block) list * block option
    | While of exp * block
    and block = stmt list

    type inst =
    | Static | NonStatic

    type decl =
    | GDecl of inst * id
    | FDecl of inst * id * (id list) * block

    type clas = Class of id * (decl list)

    type prog = Prog of clas list

    let stringOfToken (token: token) : string =
        match token with
        | TokenIdent id -> id
        | TokenSymbol s -> s.ToString()
        | TokenNumber n -> n.ToString()

    let rec convertSymbols (tokens: Token list) : token list =
        match tokens with
        | [] -> []
        | :? Token.TokenSymbol as ts :: t -> TokenSymbol ts.Symbol :: convertSymbols t
        | :? Token.TokenNumber as tn :: t -> TokenNumber tn.Number :: convertSymbols t
        | :? Token.TokenIdent as ti :: t -> TokenIdent ti.Ident :: convertSymbols t
        | _ -> raise (System.Exception "convertSymbols failed to match")

    let setLabel (parser: parser<'a>) (newLabel: plabel) : parser<'a> =
        let parseHelper (stream: token list) =
            match parser.parserFn stream with
            | Success (v, tl) -> Success (v, tl)
            | Failure (_, err) -> Failure (newLabel, err)
        { parserFn = parseHelper; label = newLabel }

    let ( <?> ) = setLabel

    let getLabel (parser: parser<'a>) = parser.label

    let printResult result : string =
        match result with
        | Success (value, _) ->
            sprintf "%A" value
        | Failure (label,error) ->
            sprintf "Error parsing %s\n%s" label error

    let parseSymbol (symbol: Symbol) : parser<token> =
        let label = symbol.ToString()
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenSymbol ts :: t when ts = symbol -> Success (TokenSymbol ts, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let parseAnyNumber : parser<float> =
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
    
    let run (parser: parser<'a>) (stream: token list) : result<'a> =
        parser.parserFn stream

    let returnP (x: 'a) : parser<'a> =
        let label = "unknown"
        let parseHelper (stream: token list) =
            Success(x, stream)
        { parserFn = parseHelper; label = label}

    let bindP (f: 'a -> parser<'b>) (p: parser<'a>) : parser<'b> =
        let label = "unknown"
        let parseHelper (stream: token list) =
            match run p stream with
            | Failure (lbl, err) -> Failure (lbl, err)
            | Success (v, tl) -> run (f v) tl
        { parserFn = parseHelper; label = label }

    let ( >>= ) p f = bindP f p

    let andThen (parser1: parser<'a>) (parser2: parser<'b>) : parser<'a * 'b> =
        let label = sprintf "%s andThen %s" (getLabel parser1) (getLabel parser2)
        parser1 >>= (fun p1result ->
        parser2 >>= (fun p2result ->
            returnP (p1result, p2result)))
        <?> label

    let orElse (parser1: parser<'a>) (parser2: parser<'a>) : parser<'a> =
        let label = sprintf "%s orElse %s" (getLabel parser1) (getLabel parser2)
        let parseHelper (stream: token list) =
            match run parser1 stream with
            | Success (v, tl) -> Success (v, tl)
            | Failure _ -> run parser2 stream
        { parserFn = parseHelper; label = label }

    let choice (parsers: parser<'a> list) : parser<'a> =
        List.reduce orElse parsers

    let rec zeroOrMore (parser: parser<'a>) (stream: token list) =
        match run parser stream with
        | Failure _ -> ([], stream)
        | Success (v, tl) -> 
            let v2, tl2 = zeroOrMore parser tl
            (v :: v2, tl2)

    let many (parser: parser<'a>) : parser<'a list> =
        let label = "unknown"
        let parseHelper (stream: token list) =
            Success (zeroOrMore parser stream)
        { parserFn = parseHelper; label = label}

    let mapP (f: 'a -> 'b) (parser: parser<'a>) : parser<'b> =
        let label = "unknown"
        let parseHelper (stream: token list) =
            match run parser stream with
            | Success (v, tl) -> Success (f v, tl)
            | Failure (err, lbl) -> Failure (err, lbl)
        { parserFn = parseHelper; label = label}

    let createParserForwardedToRef<'a>() : parser<'a> * parser<'a> ref =
        let dummyParser : parser<'a> =
            let parseHelper _ = failwith "unfixed forwarded parser"
            { parserFn = parseHelper; label = "unknown" }

        let parserRef = ref dummyParser
        let parseHelper stream =
            run parserRef.Value stream

        { parserFn = parseHelper; label = "unknown" }, parserRef

    let ( .>>. ) = andThen
    let ( <|> ) = orElse
    let ( <!> ) = mapP
    let ( |>> ) x f = mapP f x

    let ( .>> ) (p1: parser<'a>) (p2: parser<'b>) : parser<'a> =
        p1 .>>. p2 |> mapP (fun (a, _) -> a)
    let ( >>. ) (p1: parser<'a>) (p2: parser<'b>) : parser<'b> =
        p1 .>>. p2 |> mapP (fun (_, b) -> b)

    let opt (parser: parser<'a>) : parser<'a option> =
        parser |>> Some <|> returnP None

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

    let rec chainl1 (p: parser<'a>) (op: parser<'a -> 'a -> 'a>) : parser<'a> =
        let rec rest (acc: 'a) : parser<'a> =
            (op >>= fun f ->
                p >>= fun v ->
                    rest (f acc v)) <|> returnP acc
        p >>= rest

    let newlines = many (parseSymbol Symbol.EOL)
    let betweenNewlines p = newlines >>. p .>> newlines

    let parseStatic : parser<inst> = 
        let label = "static"
        let parseHelper (stream: token list) =
            match run (parseSymbol Symbol.STATIC) stream with
            | Success (_, tl) -> Success (Static, tl)
            | Failure _ -> Success (NonStatic, stream)
        { parserFn = parseHelper; label = label }

    let parseExp, parseExpRef = createParserForwardedToRef<exp>()
    let parseStmt, parseStmtRef = createParserForwardedToRef<stmt>()

    parseExpRef.Value <-
        let parseTerm = choice [
            parseAnyNumber |>> CNum
            parseAnyBool |>> CBool
            parseAnyIdent |>> CVar
            betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN
        ]
        let retBinop (s: Symbol) (e: exp * exp -> binop) : parser<exp -> exp -> exp> = parseSymbol s >>. returnP (fun x y -> Binop (e (x, y)))
        let parseBinop = choice [
            retBinop Symbol.PLUS Add
            retBinop Symbol.DASH Sub
            retBinop Symbol.EQEQ Eq
        ]
        chainl1 parseTerm parseBinop

    let parseBlock : parser<block> = 
        betweenSymbols Symbol.LBRACE (many parseStmt) Symbol.RBRACE .>> newlines

    parseStmtRef.Value <-
        let parseIf = parseSymbol Symbol.IF >>. betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN .>>. parseBlock
        let parseElseIf = parseSymbol Symbol.ELSE >>. parseIf
        let parseITE = parseIf .>>. (many parseElseIf) .>>. opt (parseSymbol Symbol.ELSE >>. parseBlock)
        let parseWhile = parseSymbol Symbol.WHILE >>. betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN .>>. parseBlock
        let parseAssn = parseAnyIdent .>> parseSymbol Symbol.EQ .>>. parseExp
        let parseVDecl = parseSymbol Symbol.VAR >>. parseAssn
        let parseRetExp = betweenSymbols Symbol.RETURN parseExp Symbol.EOL
        let parseRet = parseSymbol Symbol.RETURN >>. parseSymbol Symbol.EOL
        choice [
            betweenNewlines parseVDecl |>> VDecl
            betweenNewlines parseAssn |>> Assn
            betweenNewlines parseITE |>> fun (((ie, bl), eiebs), ebl) -> IfThenElse (ie, bl, eiebs, ebl)
            betweenNewlines parseWhile |>> While
            betweenNewlines parseRetExp |>> RetExp
            betweenNewlines parseRet >>. returnP Ret
        ]

    let parseDecl : parser<decl> =
        let parseGDecl = parseStatic .>> parseSymbol Symbol.FIELD .>>. parseAnyIdent .>> parseSymbol Symbol.EOL
        let parseFDecl = parseStatic .>> parseSymbol Symbol.FUNC .>>. parseAnyIdent .>>. betweenSymbols Symbol.LPAREN (sep parseAnyIdent (parseSymbol Symbol.COMMA)) Symbol.RPAREN .>>. parseBlock
        choice [
            betweenNewlines parseGDecl |>> GDecl
            betweenNewlines parseFDecl |>> fun (((i, id), ids), bl) -> FDecl (i, id, ids, bl)
        ]

    let parseClass : parser<clas> =
        let classDef = parseSymbol Symbol.CLASS >>. parseAnyIdent .>>. betweenSymbols Symbol.LBRACE (many parseDecl) Symbol.RBRACE
        betweenNewlines classDef |>> Class

    let parseProg : parser<prog> =
        betweenNewlines (many parseClass) .>> parseSymbol Symbol.EOF |>> Prog

    let parse (tokens: List<Token>) = 
        let result = (run parseProg (convertSymbols tokens))
        sprintf "%s" (printResult result)
