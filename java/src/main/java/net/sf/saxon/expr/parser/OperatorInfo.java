package net.sf.saxon.expr.parser;

public class OperatorInfo {

    /**
     * Get a string representation of an operator
     */

    public String toString(OperatorSymbol symbol) {
        switch (symbol) {
            case AND:
                return "and";
            case OR:
                return "or";
            case PLUS:
                return "+";
            case TIMES:
                return "*";
            case MINUS:
                return "-";
            case NEGATE:
                return "-";
            case DIV:
                return "div";
            case IDIV:
                return "idiv";
            case MOD:
                return "mod";
            case EQUALS:
                return "=";
            case NE:
                return "!=";
            case LT:
                return "<";
            case LE:
                return "<=";
            case GT:
                return ">";
            case GE:
                return ">=";
            case FEQ:
                return "eq";
            case FNE:
                return "ne";
            case FLT:
                return "lt";
            case FLE:
                return "le";
            case FGT:
                return "gt";
            case FGE:
                return "ge";
            case IS:
                return "is";
            case PRECEDES:
                return "<<";
            case FOLLOWS:
                return ">>";
            case UNION:
                return "union";
            case INTERSECT:
                return "intersect";
            case EXCEPT:
                return "except";
            case OTHERWISE:
                return "otherwise";
            case CONCAT:
                return "||";
            case TO:
                return "to";
            case CAST_AS:
                return "cast as";
            case CASTABLE_AS:
                return "castable as";
            case INSTANCE_OF:
                return "instance of";
            case TREAT_AS:
                return "treat as";
            case FAT_ARROW:
                return "=>";
            case MAPPING_ARROW:
                return "=!>";
            case SLASH:
                return "/";
            case SLASH_SLASH:
                return "//";
            case BANG:
                return "!";
            case LOOKUP:
                return "?";
            case DEEP_LOOKUP:
                return "??";
            case AM_FILTER:
                return "?[]";
            default:
                return "<unknown op>";
        }
    }

    /**
     * Get the precedence associated with a given operator
     *
     * @param symbol the operator in question
     * @return a higher number for higher precedence (closer binding)
     */

    public static int operatorPrecedence(OperatorSymbol symbol) {
        switch (symbol) {
            case OR: return ( 4);
            //m.put(Token.OR_ELSE: return ( 4);
            case AND: return ( 5);
            //m.put(Token.AND_ALSO: return ( 5);
            case FEQ: return ( 6);
            case FNE: return ( 6);
            case FLT: return ( 6);
            case FGT: return ( 6);
            case FLE: return ( 6);
            case FGE: return ( 6);
            case EQUALS: return ( 6);
            case NE: return ( 6);
            case LT: return ( 6);
            case LE: return ( 6);
            case GT: return ( 6);
            case GE: return ( 6);
            case IS: return ( 6);
            case PRECEDES: return ( 6);
            case FOLLOWS: return ( 6);
            case OTHERWISE: return ( 7);
            case CONCAT: return ( 8);
            case TO: return ( 9);
            case PLUS: return ( 10);
            case MINUS: return ( 10);
            case TIMES: return ( 11);
            case DIV: return ( 11);
            case IDIV: return ( 11);
            case MOD: return ( 11);
            case UNION: return ( 13);
            case INTERSECT: return ( 14);
            case EXCEPT: return ( 14);
            case INSTANCE_OF: return ( 15);
            case TREAT_AS: return ( 16);
            case CASTABLE_AS: return ( 17);
            case CAST_AS: return ( 18);
            case FAT_ARROW: return ( 19);
            case MAPPING_ARROW: return ( 19);
            default: return -1;

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
                return OperatorSymbol.GT;
            case LE:
                return OperatorSymbol.GE;
            case GT:
                return OperatorSymbol.LT;
            case GE:
                return OperatorSymbol.LE;
            case FLT:
                return OperatorSymbol.FGT;
            case FLE:
                return OperatorSymbol.FGE;
            case FGT:
                return OperatorSymbol.FLT;
            case FGE:
                return OperatorSymbol.FLE;
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
                return OperatorSymbol.FNE;
            case FNE:
                return OperatorSymbol.FEQ;
            case FLT:
                return OperatorSymbol.FGE;
            case FLE:
                return OperatorSymbol.FGT;
            case FGT:
                return OperatorSymbol.FLE;
            case FGE:
                return OperatorSymbol.FLT;
            default:
                throw new IllegalArgumentException("Invalid operator for negate()");
        }
    }

    public static boolean isOrderedOperator(OperatorSymbol operator) {
        return operator != OperatorSymbol.FEQ && operator != OperatorSymbol.FNE;
    }
}

