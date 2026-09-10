package net.sf.saxon.expr.parser;

import net.sf.saxon.transpile.CSharpSimpleEnum;

/**
 * A {@code Symbol} is an abstraction of a token. In some cases there is a one-to-one correspondence
 * between symbols and tokens (for example the token {@code "!"} and the symbol
 * {@link #BANG}. Often however the meaning of a token depends on context,
 * and the symbol represents the deciphered meaning of the token, given its context.
 *
 * <p>Many symbols, but not all, represent XPath operators.</p>
 */
@CSharpSimpleEnum
public enum OperatorSymbol {

    AND,
    OR,
    PLUS,
    TIMES,
    MINUS,
    DIV,
    IDIV,
    MOD,
    NEGATE,
    EQUALS, NE, LT, LE, GT, GE,
    FEQ, FNE, FLT, FLE, FGT, FGE,
    IS, PRECEDES, FOLLOWS,
    UNION, INTERSECT, EXCEPT,
    OTHERWISE, CONCAT, TO,
    CAST_AS, CASTABLE_AS, TREAT_AS, INSTANCE_OF,
    FAT_ARROW, MAPPING_ARROW,
    SLASH, SLASH_SLASH,
    BANG, LOOKUP, DEEP_LOOKUP,
    AM_FILTER,
    NOT_AN_OPERATOR;


    @Override
    public String toString() {
        switch(this) {
            case AND: return "and";
            case OR: return "or";
            case PLUS: return "+" ;
            case TIMES: return "*";
            case MINUS: return "-";
            case NEGATE: return "-";
            case DIV: return "div";
            case IDIV: return "idiv";
            case MOD: return "mod";
            case EQUALS: return "=";
            case NE: return "!=";
            case LT: return "<";
            case LE: return "<=";
            case GT: return ">";
            case GE: return ">=";
            case FEQ: return "eq";
            case FNE: return "ne";
            case FLT: return "lt";
            case FLE: return "le";
            case FGT: return "gt";
            case FGE: return "ge";
            case IS: return "is";
            case PRECEDES: return "<<";
            case FOLLOWS: return ">>";
            case UNION: return "union";
            case INTERSECT: return "intersect";
            case EXCEPT: return "except";
            case OTHERWISE: return "otherwise";
            case CONCAT: return "||";
            case TO: return "to";
            case CAST_AS: return "cast as";
            case CASTABLE_AS: return "castable as";
            case INSTANCE_OF: return "instance of";
            case TREAT_AS: return "treat as";
            case FAT_ARROW: return "=>";
            case MAPPING_ARROW: return "=!>";
            case SLASH: return "/";
            case SLASH_SLASH: return "//";
            case BANG: return "!";
            case LOOKUP: return "?";
            case DEEP_LOOKUP: return "??";
            case AM_FILTER: return "?[]";
            default: return "<unknown op>";
        }
    }
}

