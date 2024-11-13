namespace ParserLibrary

open RefactorLib
open Grammar
open Parser

module RefactorLangParser =
    let stringOfToken (token: token) : string =
        match token with
        | TokenIdent id -> id
        | TokenSymbol s -> s.ToString()
        | TokenNumber n -> n.ToString()
        | TokenString s -> s

    let rec convertSymbols (tokens: Token list) : token list =
        match tokens with
        | [] -> []
        | :? TokenSymbol as ts :: t -> TokenSymbol ts.Symbol :: convertSymbols t
        | :? TokenNumber as tn :: t -> TokenNumber tn.Number :: convertSymbols t
        | :? TokenIdent as ti :: t -> TokenIdent ti.Ident :: convertSymbols t
        | :? TokenString as ts :: t -> TokenString ts.String :: convertSymbols t
        | _ -> raise (System.Exception "convertSymbols failed to match")

    let parseSymbol (symbol: Symbol) : parser<token> =
        let label = symbol.ToString()
        let parseHelper (stream: token list) =
            match stream with
            | [] -> Failure (label, "No more input.")
            | TokenSymbol ts :: t when ts = symbol -> Success (TokenSymbol ts, t)
            | h :: _ -> Failure (label, (sprintf "unexpected '%s'" (stringOfToken h)))
        { parserFn = parseHelper; label = label }

    let betweenSymbols (s1: Symbol) (p: parser<'a>) (s2: Symbol) : parser<'a> =
        parseSymbol s1 >>. p .>> parseSymbol s2

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

    let parseAnyNumber : parser<float32> =
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
        let retBinop (s: Symbol) (e: exp * exp -> binop) : parser<exp -> exp -> exp> = parseSymbol s >>. returnP (fun x y -> Binop (e (x, y)))
        let retProj : parser<exp -> id -> exp> = parseSymbol Symbol.DOT >>. returnP (fun x y -> Proj (x, y))
        let parseBinop = choice [
            retBinop Symbol.PLUS Add
            retBinop Symbol.DASH Sub
            retBinop Symbol.STAR Mul
            retBinop Symbol.FSLASH Div
            retBinop Symbol.MOD Mod
            retBinop Symbol.AND And
            retBinop Symbol.OR Or
            retBinop Symbol.EQEQ Eq
        ]
        let parseUnop = choice [
            parseSymbol Symbol.DASH >>. returnP Neg
            parseSymbol Symbol.NOT >>. returnP Not
        ]
        let parseTerm = choice [
            parseAnyIdent .>>. betweenSymbols Symbol.LPAREN (sep parseExp (parseSymbol Symbol.COMMA)) Symbol.RPAREN |>> FCall
            parseAnyString |>> CStr
            parseAnyNumber |>> CNum
            parseAnyBool |>> CBool
            parseAnyIdent |>> CVar
            parseUnop .>>. parseExp |>> fun (x, y) -> Unop (x (y))
            betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN
        ]
        let andProj = chainl1ab parseTerm retProj parseAnyIdent
        let andBinop = chainl1 parseTerm parseBinop
        andProj <|> andBinop

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

    let parse (tokens: List<Token>) : string = 
        let result = (run parseProg (convertSymbols tokens))
        sprintf "%s" (printResult result)
