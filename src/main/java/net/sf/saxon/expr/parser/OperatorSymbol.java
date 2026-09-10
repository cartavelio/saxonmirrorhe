package net.sf.saxon.expr.parser;

import java.util.HashMap;

/**
 * A {@code Symbol} is an abstraction of a token. In some cases there is a one-to-one correspondence
 * between symbols and tokens (for example the token {@code "!"} and the symbol
 * {@link #BANG}. Often however the meaning of a token depends on context,
 * and the symbol represents the deciphered meaning of the token, given its context.
 *
 * <p>Many symbols, but not all, represent XPath operators.</p>
 */
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

    final static HashMap<OperatorSymbol, Integer> operatorPrecedenceTable = new HashMap<>(30);
    static {
        initializeOperatorPrecedenceTable();
    }

    /**
     * Initialize the static operator precedence table
     */

    private static void initializeOperatorPrecedenceTable() {
        HashMap<OperatorSymbol, Integer> m = OperatorSymbol.operatorPrecedenceTable;
        m.put(OperatorSymbol.OR, 4);
        //m.put(Token.OR_ELSE, 4);
        m.put(OperatorSymbol.AND, 5);
        //m.put(Token.AND_ALSO, 5);
        m.put(OperatorSymbol.FEQ, 6);
        m.put(OperatorSymbol.FNE, 6);
        m.put(OperatorSymbol.FLT, 6);
        m.put(OperatorSymbol.FGT, 6);
        m.put(OperatorSymbol.FLE, 6);
        m.put(OperatorSymbol.FGE, 6);
        m.put(OperatorSymbol.EQUALS, 6);
        m.put(OperatorSymbol.NE, 6);
        m.put(OperatorSymbol.LT, 6);
        m.put(OperatorSymbol.LE, 6);
        m.put(OperatorSymbol.GT, 6);
        m.put(OperatorSymbol.GE, 6);
        m.put(OperatorSymbol.IS, 6);
        m.put(OperatorSymbol.PRECEDES, 6);
        m.put(OperatorSymbol.FOLLOWS, 6);
        m.put(OperatorSymbol.OTHERWISE, 7);
        m.put(OperatorSymbol.CONCAT, 8);
        m.put(OperatorSymbol.TO, 9);
        m.put(OperatorSymbol.PLUS, 10);
        m.put(OperatorSymbol.MINUS, 10);
        m.put(OperatorSymbol.TIMES, 11);
        m.put(OperatorSymbol.DIV, 11);
        m.put(OperatorSymbol.MOD, 11);
        m.put(OperatorSymbol.UNION, 13);
        m.put(OperatorSymbol.INTERSECT, 14);
        m.put(OperatorSymbol.EXCEPT, 14);
        m.put(OperatorSymbol.INSTANCE_OF, 15);
        m.put(OperatorSymbol.TREAT_AS, 16);
        m.put(OperatorSymbol.CASTABLE_AS, 17);
        m.put(OperatorSymbol.CAST_AS, 18);
        m.put(OperatorSymbol.FAT_ARROW, 19);
        m.put(OperatorSymbol.MAPPING_ARROW, 19);

        // remainder commented out because not used in precedence parsing (but perhaps they could be)
//            case BANG:
//                return 20;
//            case SLASH:
//                return 21;
//            case SLASH_SLASH:
//                return 22;
//            case QMARK:
//                return 23;

    }



    /**
     * Return the inverse of a relational operator, so that "a op b" can be
     * rewritten as "b inverse(op) a"
     *
     * @param operator the operator whose inverse is required
     * @return the inverse operator
     */

    public static OperatorSymbol inverse(OperatorSymbol operator) {
        switch (operator) {
            case LT:
                return GT;
            case LE:
                return GE;
            case GT:
                return LT;
            case GE:
                return LE;
            case FLT:
                return FGT;
            case FLE:
                return FGE;
            case FGT:
                return FLT;
            case FGE:
                return FLE;
            default:
                return operator;
        }
    }

    /**
     * Return the negation of a relational operator, so that "a op b" can be
     * rewritten as not(b op' a)
     *
     * @param operator the operator to be negated
     * @return the negated operator
     */

    public static OperatorSymbol negate(OperatorSymbol operator) {
        switch (operator) {
            case FEQ:
                return FNE;
            case FNE:
                return FEQ;
            case FLT:
                return FGE;
            case FLE:
                return FGT;
            case FGT:
                return FLE;
            case FGE:
                return FLT;
            default:
                throw new IllegalArgumentException("Invalid operator for negate()");
        }
    }

    public static boolean isOrderedOperator(OperatorSymbol operator) {
        return operator != FEQ && operator != FNE;
    }

    /**
     * Get the precedence associated with a given operator
     * @param operator the operator in question
     * @return a higher number for higher precedence (closer binding)
     */

    public static int operatorPrecedence(OperatorSymbol operator) {
        Integer p = operatorPrecedenceTable.get(operator);
        return p==null ? -1 : p;
    }

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

