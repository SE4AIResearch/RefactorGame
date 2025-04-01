namespace ParserLibrary

open RefactorLib
open Grammar
open Parser
open RefactorLangParserLib

module RefactorLangParser =
    let parseExp, parseExpRef = createParserForwardedToRef<exp>()
    let parseStmt, parseStmtRef = createParserForwardedToRef<stmt>()

    let mutable stmtCounter = 0

    parseExpRef.Value <-
        let retBinop (s: Symbol) (e: exp * exp -> binop) (p: prec): parser<prec * (exp -> exp -> exp)> = parseSymbol s >>. returnP (p, (fun x y -> Binop (e (x, y))))
        let retUnop (s: Symbol) (u: exp -> unop): parser<exp -> exp> = parseSymbol s >>. returnP (fun x -> Unop (u (x)))
        let parseBinop = choice [
            retBinop Symbol.PLUS Add 90
            retBinop Symbol.DASH Sub 90
            retBinop Symbol.STAR Mul 100
            retBinop Symbol.FSLASH Div 100
            retBinop Symbol.MOD Mod 100
            retBinop Symbol.AND And 50
            retBinop Symbol.OR Or 40
            retBinop Symbol.GTE Gte 70
            retBinop Symbol.LTE Lte 70
            retBinop Symbol.GT Gt 70
            retBinop Symbol.LT Lt 70
            retBinop Symbol.EQEQ Eq 60
            retBinop Symbol.NEQ Neq 60
        ]
        let parseUnop = choice [
            retUnop Symbol.NOT Not
            retUnop Symbol.DASH Neg
            retUnop Symbol.HASH Len
        ]
        let parseTerm = choice [
            parseAnyIdent .>>. betweenSymbols Symbol.LPAREN (sep parseExp (parseSymbol Symbol.COMMA)) Symbol.RPAREN |>> FCall
            parseAnyIdent .>>. betweenSymbols Symbol.LBRACK parseExp Symbol.RBRACK |>> Idx
            parseAnyString |>> CStr
            parseAnyNumber |>> CNum
            parseAnyBool |>> CBool
            parseAnyIdent |>> CVar
            betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN
        ]
        let andUnop = prefix1 parseTerm parseUnop
        let andBinop = chainl1prec andUnop parseBinop 0
        andBinop

    let parseBlock : parser<block> =
        betweenSymbols Symbol.LBRACE (many parseStmt) Symbol.RBRACE .>> newlines

    parseStmtRef.Value <-
        let parseIf = parseSymbol Symbol.IF >>. betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN .>>. parseBlock
        let parseElseIf = parseSymbol Symbol.ELSE >>. parseIf
        let parseITE = parseIf .>>. (many parseElseIf) .>>. opt (parseSymbol Symbol.ELSE >>. parseBlock)
        let parseWhile = parseSymbol Symbol.WHILE >>. betweenSymbols Symbol.LPAREN parseExp Symbol.RPAREN .>>. parseBlock
        let parseFDecl = parseSymbol Symbol.FUNC >>. parseAnyIdent .>>. betweenSymbols Symbol.LPAREN (sep parseAnyIdent (parseSymbol Symbol.COMMA)) Symbol.RPAREN .>>. parseBlock
        let parseVDecl = parseSymbol Symbol.VAR >>. parseAnyIdent .>>. (parseSymbol Symbol.EQ >>. parseExp)
        let parseAssn = parseAnyIdent .>>. (parseSymbol Symbol.EQ >>. parseExp)
        let parseKeywordStmt = parseAnyKeyword .>>. betweenSymbols Symbol.LPAREN (sep parseExp (parseSymbol Symbol.COMMA)) Symbol.RPAREN

        let countKeywordStmt = 
            parseKeywordStmt
            |>> fun (kw, ps) -> 
                stmtCounter <- (stmtCounter + 1)
                match kw with 
                | TokenKeyword k -> KStmt(k, ps) 
                | TokenIdent k -> FStmt(k, ps)
                | _ -> failwith "this should not be possible"

        choice [
            betweenNewlines countKeywordStmt
            betweenNewlines parseITE |>> fun (((ie, bl), eiebs), ebl) ->
                stmtCounter <- (stmtCounter + 1 + eiebs.Length)
                match ebl with | Some _ -> stmtCounter <- (stmtCounter + 1) | None -> ()
                IfThenElse (ie, bl, eiebs, ebl)
            betweenNewlines parseWhile |>> fun (exp, block) ->
                stmtCounter <- (stmtCounter + 1)
                While(exp, block)
            betweenNewlines parseFDecl |>> fun ((id, ids), bl) -> FDecl(id, ids, bl)
            betweenNewlines parseVDecl |>> fun (id, exp) ->
                stmtCounter <- (stmtCounter + 1)
                VDecl(id, exp)
            betweenNewlines parseAssn |>> fun (id, exp) ->
                stmtCounter <- (stmtCounter + 1)
                Assn(id, exp)
            betweenNewlines ((parseSymbol Symbol.RETURN) >>. parseExp) |>> RetVal
            betweenNewlines (parseSymbol Symbol.RETURN) |>> fun (_) -> RetVoid
        ]

    let parseProg : parser<prog> =
        betweenNewlines (many parseStmt) .>> parseSymbol Symbol.EOF |>> Prog

    let parseToString (tokens: List<Token>) : string = 
        stmtCounter <- 0
        let result = (run parseProg (convertSymbols tokens))
        sprintf "%s" (printResult result)

    let parseToProg (tokens: List<Token>) : prog * int =
        stmtCounter <- 0
        let result = (run parseProg (convertSymbols tokens))
        match (result) with
        | Success (p, _) -> p, stmtCounter
        | Failure (f1, f2) -> failwith "did not compile"

    let parseJustExp (tokens: List<Token>) =
        match (run parseExp (convertSymbols tokens)) with
        | Success (p, _) -> p
        | Failure (f1, f2) -> failwith "did not compile"
        


