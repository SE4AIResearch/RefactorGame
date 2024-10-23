namespace ParserLibrary

open FParsec

module Parser =
    type id = string

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
    | VDecl of inst * id * exp
    | FDecl of inst * id * (id list) * block

    type clas = id * (decl list)

    type prog = clas list