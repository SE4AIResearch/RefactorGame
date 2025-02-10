namespace ParserLibrary

open RefactorLib
open Grammar

module Parser =
    type plabel = string
    type perror = string

    type parser<'a> = {
        parserFn : (token list -> result<'a>)
        label : plabel
    } 
    and result<'a> =
    | Success of 'a * token list
    | Failure of plabel * perror

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

    let prefix1 (p: parser<'a>) (op: parser<'a -> 'a>) : parser<'a> =
        let rec rest () =
            (op >>= fun f -> rest () |>> f) <|> p
        rest ()