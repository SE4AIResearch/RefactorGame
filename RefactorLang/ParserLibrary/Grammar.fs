namespace ParserLibrary

open RefactorLib

module Grammar =
    type id = string

    type token =
    | TokenString of string
    | TokenNumber of int
    | TokenIdent of id
    | TokenSymbol of Symbol
    | TokenKeyword of Keyword

    type binop =
    | Add of exp * exp
    | Sub of exp * exp
    | Mul of exp * exp
    | Div of exp * exp
    | Mod of exp * exp
    | And of exp * exp
    | Or  of exp * exp
    | Gt  of exp * exp
    | Gte of exp * exp
    | Lt  of exp * exp
    | Lte of exp * exp
    | Eq  of exp * exp
    | Neq of exp * exp
    and unop =
    | Not of exp
    | Neg of exp
    and exp =
    | CNum of int
    | CBool of bool
    | CVar of id
    | CStr of string
    | Binop of binop
    | Unop of unop
    | FCall of id * exp list
    | Idx of id * exp

    type stmt =
    | FDecl of id * (id list) * block
    | VDecl of id * exp
    | Assn of id * exp
    | IfThenElse of exp * block * (exp * block) list * block option
    | While of exp * block
    | KCall of Keyword * exp list
    | RetVal of exp
    | RetVoid
    and block = stmt list

    type prog = Prog of stmt list
