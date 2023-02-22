using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public abstract class Token {

        static Token() {
            tokens[EOF] = "<eof>";
            tokens[UNION] = "|";
            tokens[SLASH] = "/";
            tokens[AT] = "@";
            tokens[LSQB] = "[";
            tokens[LPAR] = "(";
            tokens[EQUALS] = "=";
            tokens[COMMA] = ",";
            tokens[SLASH_SLASH] = "//";
            tokens[OR] = "or";
            tokens[AND] = "and";
            tokens[GT] = ">";
            tokens[LT] = "<";
            tokens[GE] = ">=";
            tokens[LE] = "<=";
            tokens[PLUS] = "+";
            tokens[MINUS] = "-";
            tokens[MULT] = "*";
            tokens[DIV] = "div";
            tokens[MOD] = "mod";
            tokens[IS] = "is";
            tokens[DOLLAR] = "$";
            tokens[NE] = "!=";
            tokens[BANG] = "!";
            tokens[CONCAT] = "||";
            tokens[INTERSECT] = "intersect";
            tokens[EXCEPT] = "except";
            tokens[RETURN] = "return";
            tokens[THEN] = "then";
            tokens[ELSE] = "else";
            tokens[TO] = "to";
            tokens[IN] = "in";
            tokens[SOME] = "some";
            tokens[EVERY] = "every";
            tokens[SATISFIES] = "satisfies";
            tokens[FUNCTION] = "<function>(";
            tokens[AXIS] = "<axis>";
            tokens[IF] = "if(";
            tokens[PRECEDES] = "<<";
            tokens[FOLLOWS] = ">>";
            tokens[COLONCOLON] = "::";
            tokens[COLONSTAR] = ":*";
            tokens[HASH] = "#";
            tokens[INSTANCE_OF] = "instance of";
            tokens[CAST_AS] = "cast as";
            tokens[TREAT_AS] = "treat as";
            tokens[QMARK_QMARK] = "??";
            tokens[BANG_BANG] = "!!";
            tokens[FEQ] = "eq";
            tokens[FNE] = "ne";
            tokens[FGT] = "gt";
            tokens[FGE] = "ge";
            tokens[FLT] = "lt";
            tokens[FLE] = "le";
            tokens[IDIV] = "idiv";
            tokens[CASTABLE_AS] = "castable as";
            tokens[ASSIGN] = ":=";
            tokens[SWITCH] = "switch";
            tokens[TYPESWITCH] = "typeswitch";
            tokens[CASE] = "case";
            tokens[DEFAULT] = "default";
            tokens[AFTER] = "after";
            tokens[BEFORE] = "before";
            tokens[INTO] = "into";
            tokens[WITH] = "with";
            tokens[MODIFY] = "modify";
            tokens[AS] = "as";
            tokens[COLON] = ":";
            tokens[FAT_ARROW] = "=>";
            tokens[THIN_ARROW] = "->";
            tokens[AND_ALSO] = "andAlso";
            tokens[OR_ELSE] = "orElse";
            tokens[BY] = "by";
            tokens[STRING_CONSTRUCTOR_INITIAL] = "``[<string>`{";
            tokens[STRING_LITERAL_BACKTICKED] = "``[<string>]``";
            tokens[OTHERWISE] = "otherwise";
            tokens[NAME] = "<name>";
            tokens[STRING_LITERAL] = "<string-literal>";
            tokens[RSQB] = "]";
            tokens[RPAR] = ")";
            tokens[DOT] = ".";
            tokens[DOTDOT] = "..";
            tokens[STAR] = "*";
            tokens[PREFIX] = "<prefix:*>";
            tokens[NUMBER] = "<numeric-literal>";
            tokens[NODEKIND] = "<node-type>()";
            tokens[FOR] = "for";
            tokens[SUFFIX] = "<*:local-name>";
            tokens[QMARK] = "?";
            tokens[LCURLY] = "{";
            tokens[KEYWORD_CURLY] = "<keyword> {";
            tokens[RCURLY] = "}";
            tokens[LET] = "let";
            tokens[VALIDATE] = "validate {";
            tokens[TAG] = "<element>";
            tokens[PRAGMA] = "(# ... #)";
            tokens[SEMICOLON] = ";";
            tokens[COPY] = "copy";
            tokens[NEGATE] = "-";
            tokens[PERCENT] = "%";
            mapDouble("instance of", INSTANCE_OF);
            mapDouble("cast as", CAST_AS);
            mapDouble("treat as", TREAT_AS);
            mapDouble("castable as", CASTABLE_AS);
            mapDouble("group by", GROUP_BY);
            mapDouble("for tumbling", FOR_TUMBLING);
            mapDouble("for sliding", FOR_SLIDING);
            mapDouble("for member", FOR_MEMBER);
            mapDouble("xquery version", XQUERY_VERSION);
            mapDouble("xquery encoding", XQUERY_ENCODING);
            mapDouble("declare namespace", DECLARE_NAMESPACE);
            mapDouble("declare default", DECLARE_DEFAULT);
            mapDouble("declare construction", DECLARE_CONSTRUCTION);
            mapDouble("declare base-uri", DECLARE_BASEURI);
            mapDouble("declare boundary-space", DECLARE_BOUNDARY_SPACE);
            mapDouble("declare decimal-format", DECLARE_DECIMAL_FORMAT);
            mapDouble("declare ordering", DECLARE_ORDERING);
            mapDouble("declare copy-namespaces", DECLARE_COPY_NAMESPACES);
            mapDouble("declare option", DECLARE_OPTION);
            mapDouble("declare revalidation", DECLARE_REVALIDATION);
            mapDouble("declare type", DECLARE_TYPE);
            mapDouble("import schema", IMPORT_SCHEMA);
            mapDouble("import module", IMPORT_MODULE);
            mapDouble("declare variable", DECLARE_VARIABLE);
            mapDouble("declare context", DECLARE_CONTEXT);
            mapDouble("declare function", DECLARE_FUNCTION);
            mapDouble("declare updating", DECLARE_UPDATING);
            mapDouble("module namespace", MODULE_NAMESPACE);
            mapDouble("validate strict", VALIDATE_STRICT);
            mapDouble("validate lax", VALIDATE_LAX);
            mapDouble("validate type", VALIDATE_TYPE);
            mapDouble("insert node", INSERT_NODE);
            mapDouble("insert nodes", INSERT_NODE);
            mapDouble("delete node", DELETE_NODE);
            mapDouble("delete nodes", DELETE_NODE);
            mapDouble("replace node", REPLACE_NODE);
            mapDouble("replace value", REPLACE_VALUE);
            mapDouble("rename node", RENAME_NODE);
            mapDouble("rename nodes", RENAME_NODE);
            mapDouble("first into", FIRST_INTO);
            mapDouble("last into", LAST_INTO);
        }
        public const int IMPLICIT_EOF = -1;
        public const int EOF = 0;
        public const int UNION = 1;
        public const int SLASH = 2;
        public const int AT = 3;
        public const int LSQB = 4;
        public const int LPAR = 5;
        public const int EQUALS = 6;
        public const int COMMA = 7;
        public const int SLASH_SLASH = 8;
        public const int OR = 9;
        public const int AND = 10;
        public const int GT = 11;
        public const int LT = 12;
        public const int GE = 13;
        public const int LE = 14;
        public const int PLUS = 15;
        public const int MINUS = 16;
        public const int MULT = 17;
        public const int DIV = 18;
        public const int MOD = 19;
        public const int IS = 20;
        public const int DOLLAR = 21;
        public const int NE = 22;
        public const int INTERSECT = 23;
        public const int EXCEPT = 24;
        public const int RETURN = 25;
        public const int THEN = 26;
        public const int ELSE = 27;
        public const int WHERE = 28;
        public const int TO = 29;
        public const int CONCAT = 30;
        public const int IN = 31;
        public const int SOME = 32;
        public const int EVERY = 33;
        public const int SATISFIES = 34;
        public const int FUNCTION = 35;
        public const int AXIS = 36;
        public const int IF = 37;
        public const int PRECEDES = 38;
        public const int FOLLOWS = 39;
        public const int BANG = 40;
        public const int COLONCOLON = 41;
        public const int COLONSTAR = 42;
        public const int NAMED_FUNCTION_REF = 43;
        public const int HASH = 44;
        public const int INSTANCE_OF = 45;
        public const int CAST_AS = 46;
        public const int TREAT_AS = 47;
        public const int QMARK_QMARK = 48;
        public const int BANG_BANG = 49;
        public const int FEQ = 50;
        public const int FNE = 51;
        public const int FGT = 52;
        public const int FLT = 53;
        public const int FGE = 54;
        public const int FLE = 55;
        public const int IDIV = 56;
        public const int CASTABLE_AS = 57;
        public const int ASSIGN = 58;
        public const int LCURLY = 59;
        public const int KEYWORD_CURLY = 60;
        public const int ELEMENT_QNAME = 61;
        public const int ATTRIBUTE_QNAME = 62;
        public const int PI_QNAME = 63;
        public const int NAMESPACE_QNAME = 64;
        public const int TYPESWITCH = 65;
        public const int SWITCH = 66;
        public const int CASE = 67;
        public const int MODIFY = 68;
        public const int NODEKIND = 69;
        public const int SUFFIX = 70;
        public const int AS = 71;
        public const int GROUP_BY = 72;
        public const int FOR_TUMBLING = 73;
        public const int FOR_SLIDING = 74;
        public const int FOR_MEMBER = 75;
        public const int COLON = 76;
        public const int FAT_ARROW = 77;
        public const int STRING_CONSTRUCTOR_INITIAL = 78;
        public const int OTHERWISE = 79;
        public const int AND_ALSO = 80;
        public const int OR_ELSE = 81;
        public const int BY = 82;
        public const int THIN_ARROW = 83;
        public const int XQUERY_VERSION = 88;
        public const int XQUERY_ENCODING = 89;
        public const int DECLARE_NAMESPACE = 90;
        public const int DECLARE_DEFAULT = 91;
        public const int DECLARE_CONSTRUCTION = 92;
        public const int DECLARE_BASEURI = 93;
        public const int DECLARE_BOUNDARY_SPACE = 94;
        public const int DECLARE_DECIMAL_FORMAT = 95;
        public const int IMPORT_SCHEMA = 96;
        public const int IMPORT_MODULE = 97;
        public const int DECLARE_VARIABLE = 98;
        public const int DECLARE_CONTEXT = 99;
        public const int DECLARE_FUNCTION = 100;
        public const int MODULE_NAMESPACE = 101;
        public const int VALIDATE = 102;
        public const int VALIDATE_STRICT = 103;
        public const int VALIDATE_LAX = 104;
        public const int VALIDATE_TYPE = 105;
        public const int PERCENT = 106;
        public const int DECLARE_ORDERING = 107;
        public const int DECLARE_COPY_NAMESPACES = 108;
        public const int DECLARE_OPTION = 109;
        public const int DECLARE_REVALIDATION = 110;
        public const int INSERT_NODE = 111;
        public const int DELETE_NODE = 112;
        public const int REPLACE_NODE = 113;
        public const int REPLACE_VALUE = 114;
        public const int RENAME_NODE = 115;
        public const int FIRST_INTO = 116;
        public const int LAST_INTO = 117;
        public const int AFTER = 118;
        public const int BEFORE = 119;
        public const int INTO = 120;
        public const int WITH = 121;
        public const int DECLARE_UPDATING = 122;
        public const int DECLARE_ANNOTATED = 123;
        public const int DECLARE_TYPE = 124;
        public const int SEMICOLON = 149;
        public const int LAST_OPERATOR = 150;
        public const int NAME = 201;
        public const int STRING_LITERAL = 202;
        public const int RSQB = 203;
        public const int RPAR = 204;
        public const int DOT = 205;
        public const int DOTDOT = 206;
        public const int STAR = 207;
        public const int PREFIX = 208;
        public const int NUMBER = 209;
        public const int FOR = 211;
        public const int DEFAULT = 212;
        public const int QMARK = 213;
        public const int RCURLY = 215;
        public const int LET = 216;
        public const int TAG = 217;
        public const int PRAGMA = 218;
        public const int COPY = 219;
        public const int COUNT = 220;
        public const int STRING_LITERAL_BACKTICKED = 222;
        public const int NEGATE = 299;
        public readonly static string[] tokens = new string[300];
        public static System.Collections.Generic.Dictionary<string,int> doubleKeywords = new System.Collections.Generic.Dictionary<string, int>(30);
        public const int UNKNOWN = -1;
        private Token()  {
        }
        private static void mapDouble(string doubleKeyword, int token) {
            Saxon.Impl.Helpers.MapUtils.Put(doubleKeywords, doubleKeyword, token);
            tokens[token] = doubleKeyword;
        }
        public static int inverse(int @operator) {
            switch (@operator) {
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
                return @operator;
            }
        }
        public static int negate(int @operator) {
            switch (@operator) {
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
                throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid operator for negate()");
            }
        }
        public static bool isOrderedOperator(int @operator) {
            return ((@operator != FEQ) && (@operator != FNE));
        }
    
    }
}