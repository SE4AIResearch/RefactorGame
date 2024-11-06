namespace ParserLibrary

open RefactorLib

module Grammar =
    type id = string

    type token =
    | TokenNumber of float32
    | TokenIdent of id
    | TokenSymbol of Symbol

    type binop =
    | Add of exp * exp
    | Sub of exp * exp
    | Mul of exp * exp
    | Div of exp * exp
    | Mod of exp * exp
    | And of exp * exp
    | Or  of exp * exp
    | Eq  of exp * exp
    | Neq of exp * exp
    and unop =
    | Not of exp
    | Neg of exp
    and exp =
    | CNum of float32
    | CBool of bool
    | CVar of id
    | CStr of string
    | Binop of binop
    | Unop of unop
    | FCall of id * exp list
    | Proj of exp * id

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
