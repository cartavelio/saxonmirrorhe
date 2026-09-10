package net.sf.saxon.expr.parser;

/**
 * A {@code Symbol} is an abstraction of a token. In some cases there is a one-to-one correspondence
 * between symbols and tokens (for example the token {@code "!"} and the symbol
 * {@link #BANG}. Often however the meaning of a token depends on context,
 * and the symbol represents the deciphered meaning of the token, given its context.
 *
 * <p>Many symbols, but not all, represent XPath operators.</p>
 */
public enum Symbol {

    AND,
    OR,
    PLUS,
    MINUS,
    DIV,
    IDIV,
    MOD,
    EQ, NE, LT, LE, GT, GE,
    FEQ, FNE, FLT, FLE, FGT, FGE,
    IS, PRECEDES, FOLLOWS,
    UNION, INTERSECT, EXCEPT,
    FOR, LET, SOME, EVERY
}

