using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class RoleDiagnostic {
        private readonly int kind;
        private readonly string operation;
        private readonly int operand;
        private string errorCode = "XPTY0004";
        public const int FUNCTION = 0;
        public const int BINARY_EXPR = 1;
        public const int TYPE_OP = 2;
        public const int VARIABLE = 3;
        public const int INSTRUCTION = 4;
        public const int FUNCTION_RESULT = 5;
        public const int ORDER_BY = 6;
        public const int TEMPLATE_RESULT = 7;
        public const int PARAM = 8;
        public const int UNARY_EXPR = 9;
        public const int UPDATING_EXPR = 10;
        public const int EVALUATE_RESULT = 12;
        public const int CONTEXT_ITEM = 13;
        public const int AXIS_STEP = 14;
        public const int OPTION = 15;
        public const int CHARACTER_MAP_EXPANSION = 16;
        public const int FOR_MEMBER = 17;
        public const int MATCH_PATTERN = 19;
        public const int MISC = 20;
        public RoleDiagnostic(int kind, string operation, int operand)  {
            this.kind = kind;
            this.operation = operation;
            this.operand = operand;
        }
        public void setErrorCode(string code) {
            if ((code != null)) {
                this.errorCode = code;
            }
        }
        public string getErrorCode() {
            return errorCode;
        }
        public bool isTypeError() {
            return (!(errorCode.StartsWith("FORG")) && !(errorCode.Equals("XPDY0050")));
        }
        public string getMessage() {
            string name = operation;
            switch (kind) {
              case FUNCTION:
                if ((name.Equals("saxon:call") || name.Equals("saxon:apply"))) {
                    if ((operand == 0)) {
                        return "target of the dynamic function call";
                    } else {
                        return (ordinal(operand)+" argument of the dynamic function call");
                    }
                } else {
                    return ((ordinal((operand+1))+" argument of ")+((name.Length==0 ? "the anonymous function" : (name+"()"))));
                }
              case BINARY_EXPR:
                return (((ordinal((operand+1))+" operand of '")+name)+'\'');
              case UNARY_EXPR:
                return "operand of '-'";
              case TYPE_OP:
                return (("value in '"+name)+"' expression");
              case VARIABLE:
                if (name.Equals("saxon:context-item")) {
                    return "context item";
                } else {
                    return ("value of variable $"+name);
                }
              case INSTRUCTION:
                int slash = name.IndexOf('/');
                string attributeName = "";
                if ((slash>=0)) {
                    attributeName = name.Substring((slash+1));
                    name = name.Substring(0, slash - 0);
                }
                return ((("@"+attributeName)+" attribute of ")+((name.Equals("LRE") ? "a literal result element" : name)));
              case FUNCTION_RESULT:
                if (name.Length==0) {
                    return "result of the anonymous function";
                } else {
                    return ("result of a call to "+name);
                }
              case TEMPLATE_RESULT:
                return ("result of template "+name);
              case ORDER_BY:
                return (ordinal((operand+1))+" sort key");
              case PARAM:
                return ("value of parameter $"+name);
              case UPDATING_EXPR:
                return (((("value of the "+ordinal((operand+1)))+" operand of ")+name)+" expression");
              case EVALUATE_RESULT:
                return (("result of the expression {"+name)+"} evaluated by xsl:evaluate");
              case CONTEXT_ITEM:
                return "context item";
              case AXIS_STEP:
                return (("context item for the "+Saxon.Hej.om.AxisInfo.axisName[operand])+" axis");
              case OPTION:
                return (("value of the "+name)+" option");
              case CHARACTER_MAP_EXPANSION:
                return (("substitute value for character '"+name)+"' in the character map");
              case FOR_MEMBER:
                return (("'for member $"+name)+"' expression");
              case MATCH_PATTERN:
                return "match pattern";
              case MISC:
                return operation;
              default:
                return "";
            }
        }
        public string composeRequiredMessage(Saxon.Hej.type.ItemType requiredItemType) {
            return ((("The required item type of the "+getMessage())+" is ")+requiredItemType);
        }
        public string composeErrorMessage(Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.type.ItemType suppliedItemType) {
            return ((composeRequiredMessage(requiredItemType)+"; supplied value has item type ")+suppliedItemType);
        }
        public string composeErrorMessage(Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.expr.Expression supplied, Saxon.Hej.type.TypeHierarchy th) {
            if ((supplied is Saxon.Hej.expr.Literal)) {
                string s = composeRequiredMessage(requiredItemType);
                Saxon.Ejava.util.Optional<string> more = Saxon.Hej.value.SequenceType.makeSequenceType(requiredItemType, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE).explainMismatch(((Saxon.Hej.expr.Literal)(supplied)).getGroundedValue(), th);
                if (more.isPresent()) {
                    s = ((s+". ")+more.get());
                }
                return s;
            }
            return ((((composeRequiredMessage(requiredItemType)+", but the supplied expression {")+supplied.toShortString())+"} has item type ")+supplied.getItemType());
        }
        public string composeErrorMessage(Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder(256);
            message.append(composeRequiredMessage(requiredItemType));
            message.append("; the supplied value ");
            message.append(Saxon.Hej.trans.Err.depict(item));
            if ((requiredItemType.getGenre() != item.getGenre())) {
                message.append(" is ");
                message.append(Saxon.Hej.trans.Err.describeGenre(item.getGenre()));
            } else {
                message.append(" does not match. ");
                if ((th != null)) {
                    Saxon.Ejava.util.Optional<string> more = requiredItemType.explainMismatch(item, th);
                    if (more.isPresent()) {
                        message.append(more.get());
                    }
                }
            }
            return message.ToString();
        }
        public string composeErrorMessage(Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.type.UType suppliedItemType) {
            return ((composeRequiredMessage(requiredItemType)+"; supplied value has item type ")+suppliedItemType);
        }
        public string save() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            fsb.append((kind+"|"));
            fsb.append((operand+"|"));
            fsb.append((errorCode.Equals("XPTY0004") ? "" : errorCode));
            fsb.append("|");
            fsb.append(operation);
            return fsb.ToString();
        }
        public static Saxon.Hej.expr.parser.RoleDiagnostic reconstruct(string @in) {
            int v = @in.IndexOf('|');
            int kind = Saxon.Impl.Helpers.Utils.ParseInt(@in.Substring(0, v - 0));
            int w = @in.IndexOf('|', (v+1));
            int operand = Saxon.Impl.Helpers.Utils.ParseInt(@in.Substring((v+1), w - (v+1)));
            int x = @in.IndexOf('|', (w+1));
            string errorCode = @in.Substring((w+1), x - (w+1));
            string operation = @in.Substring((x+1));
            Saxon.Hej.expr.parser.RoleDiagnostic cd = new Saxon.Hej.expr.parser.RoleDiagnostic(kind, operation, operand);
            if (!(errorCode.Length==0)) {
                cd.setErrorCode(errorCode);
            }
            return cd;
        }
        public static string ordinal(int n) {
            switch (n) {
              case 1:
                return "first";
              case 2:
                return "second";
              case 3:
                return "third";
              default:
                if ((n>=21)) {
                    switch ((n%10)) {
                      case 1:
                        return (n+"st");
                      case 2:
                        return (n+"nd");
                      case 3:
                        return (n+"rd");
                    }
                }
                return (n+"th");
            }
        }
    
    }
}