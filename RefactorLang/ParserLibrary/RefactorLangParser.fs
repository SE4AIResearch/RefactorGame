namespace ParserLibrary

open RefactorLib
open Grammar
open Parser
open RefactorLangParserLib

module RefactorLangParser =
    let parseExp, parseExpRef = createParserForwardedToRef<exp>()
    let parseStmt, parseStmtRef = createParserForwardedToRef<stmt>()

    parseExpRef.Value <-
        let retBinop (s: Symbol) (e: exp * exp -> binop) : parser<exp -> exp -> exp> = parseSymbol s >>. returnP (fun x y -> Binop (e (x, y)))
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
        let parseTerm = choice [
            parseAnyIdent .>>. betweenSymbols Symbol.LBRACK parseExp Symbol.RBRACK |>> Idx
            parseAnyString |>> CStr
            parseAnyNumber |>> CNum
            parseAnyBool |>> CBool
            parseAnyIdent |>> CVar
            betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN
        ]
        let andBinop = chainl1 parseTerm parseBinop
        andBinop

    let parseBlock : parser<block> =
        betweenSymbols Symbol.LBRACE (many parseStmt) Symbol.RBRACE .>> newlines

    let parseInExp : parser<exp> =
        (parseAnyIdent .>>. (parseSymbol Symbol.IN >>. parseAnyIdent)) |>> In

    parseStmtRef.Value <-
        let parseIf = parseSymbol Symbol.IF >>. betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN .>>. parseBlock
        let parseElseIf = parseSymbol Symbol.ELSE >>. parseIf
        let parseITE = parseIf .>>. (many parseElseIf) .>>. opt (parseSymbol Symbol.ELSE >>. parseBlock)
        let parseWhile = parseSymbol Symbol.WHILE >>. betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN .>>. parseBlock
        let parseForEach = parseSymbol Symbol.FOREACH >>. betweenSymbols Symbol.LPAREN parseInExp Symbol.RPAREN .>>. parseBlock
        let parseKeywordStmt = parseAnyKeyword .>>. betweenSymbols Symbol.LPAREN (sep parseExp (parseSymbol Symbol.COMMA)) Symbol.RPAREN
        choice [
            betweenNewlines parseKeywordStmt |>> fun (kw, ps) -> match kw with TokenKeyword k -> KCall(k, ps)
            betweenNewlines parseITE |>> fun (((ie, bl), eiebs), ebl) -> IfThenElse (ie, bl, eiebs, ebl)
            betweenNewlines parseForEach |>> fun (e, bl) -> ForEach (e, bl)
            betweenNewlines parseWhile |>> fun (exp, bl) -> While(exp, bl)
        ]

    let parseProg : parser<prog> =
        betweenNewlines (many parseStmt) .>> parseSymbol Symbol.EOF |>> Prog

    let parseToString (tokens: List<Token>) : string = 
        let result = (run parseProg (convertSymbols tokens))
        sprintf "%s" (printResult result)

    let parseToProg (tokens: List<Token>) : prog =
        match (run parseProg (convertSymbols tokens)) with
        | Success (p, _) -> p
        | Failure _ -> failwith "did not compile"
