using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class Err {
        public const int ELEMENT = 1;
        public const int ATTRIBUTE = 2;
        public const int FUNCTION = 3;
        public const int VALUE = 4;
        public const int VARIABLE = 5;
        public const int GENERAL = 6;
        public const int URI = 7;
        public const int EQNAME = 8;
        public static string wrap(Saxon.Hej.str.UnicodeString cs) {
            return wrap(cs, GENERAL);
        }
        public static string wrap(string cs) {
            return wrap(cs, GENERAL);
        }
        public static string wrap(string cs, int valueType) {
            return wrap(Saxon.Hej.str.StringView.of(cs), valueType);
        }
        public static string wrap(Saxon.Hej.str.UnicodeString cs, int valueType) {
            if ((cs == null)) {
                return "(NULL)";
            }
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            Saxon.Hej.z.IntIterator iter = cs.codePoints();
            int len = 0;
            while (iter.hasNext()) {
                int c = iter.next();
                len++;
                switch (c) {
                  case '\n':
                    sb.append("\\n");
                    break ;
                  case '\t':
                    sb.append("\\t");
                    break ;
                  case '\r':
                    sb.append("\\r");
                    break ;
                  default:
                    if ((c<32)) {
                        sb.append("\\x");
                        sb.append(Saxon.Impl.Helpers.Utils.intToHexString(c));
                    } else {
                        sb.appendCodePoint(c);
                    }
                    break ;
                }
            }
            string s;
            if (((valueType == ELEMENT) || (valueType == ATTRIBUTE))) {
                s = sb.ToString();
                if (s.StartsWith("{")) {
                    s = ("Q"+s);
                }
                if (s.StartsWith("Q{")) {
                    try {
                        Saxon.Hej.om.StructuredQName qn = Saxon.Hej.om.StructuredQName.fromEQName(sb.ToString());
                        string uri = abbreviateURI(qn.getURI());
                        s = ((("Q{"+uri)+"}")+qn.getLocalPart());
                    } catch (System.Exception e) {
                        s = sb.ToString();
                    }
                }
            } else {
                if ((valueType == URI)) {
                    s = abbreviateURI(sb.ToString());
                } else {
                    if ((valueType == EQNAME)) {
                        s = abbreviateEQName(sb.ToString());
                    } else {
                        s = ((len>30) ? (sb.ToString().Substring(0, 30 - 0)+"...") : sb.ToString());
                    }
                }
            }
            switch (valueType) {
              case ELEMENT:
                return (("<"+s)+">");
              case ATTRIBUTE:
                return ("@"+s);
              case FUNCTION:
                return (s+"()");
              case VARIABLE:
                return ("$"+s);
              case VALUE:
                return (("\""+s)+"\"");
              case EQNAME:
                return s;
              default:
                return (("{"+s)+"}");
            }
        }
        public static string depict(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                    return (("doc("+abbreviateURI(node.getSystemId()))+')');
                  case Saxon.Hej.type.Type.ELEMENT:
                    return (('<'+node.getDisplayName())+'>');
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    return (((('@'+node.getDisplayName())+"=\"")+node.getUnicodeStringValue())+'"');
                  case Saxon.Hej.type.Type.TEXT:
                    return (("text{"+truncate30(node.getUnicodeStringValue()))+"}");
                  case Saxon.Hej.type.Type.COMMENT:
                    return "<!--...-->";
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    return (("<?"+node.getLocalPart())+"...?>");
                  case Saxon.Hej.type.Type.NAMESPACE:
                    return ((("xmlns:"+node.getLocalPart())+"=")+abbreviateURI(node.getStringValue()));
                  default:
                    return "";
                }
            } else {
                return item.toShortString();
            }
        }
        public static string depictSequence(Saxon.Hej.om.Sequence seq) {
            if ((seq == null)) {
                return "(*null*)";
            }
            try {
                Saxon.Hej.om.GroundedValue val = seq.materialize();
                if ((val.getLength() == 0)) {
                    return "()";
                } else {
                    if ((val.getLength() == 1)) {
                        return depict(seq.head());
                    } else {
                        return depictSequenceStart(val.iterate(), 3, val.getLength());
                    }
                }
            } catch (System.Exception e) {
                return "(*unreadable*)";
            }
        }
        public static string depictSequenceStart(Saxon.Hej.om.SequenceIterator seq, int max, int actual) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            int count = 0;
            sb.append(" (");
            Saxon.Hej.om.Item next;
            while (((next = seq.next()) != null)) {
                if ((count++>0)) {
                    sb.append(", ");
                }
                if ((count>max)) {
                    sb.append((("... ["+actual)+"])"));
                    return sb.ToString();
                }
                sb.append(Saxon.Hej.trans.Err.depict(next));
            }
            sb.append(") ");
            return sb.ToString();
        }
        public static Saxon.Hej.str.UnicodeString truncate30(Saxon.Hej.str.UnicodeString cs) {
            if ((cs.length()<=30)) {
                return Saxon.Hej.value.Whitespace.collapseWhitespace(cs);
            } else {
                return Saxon.Hej.value.Whitespace.collapseWhitespace(cs.substring(0, 30)).concat(Saxon.Hej.str.BMPString.of("..."));
            }
        }
        public static string abbreviateURI(string uri) {
            if ((uri == null)) {
                return "";
            }
            int lastSlash = ((uri.EndsWith("/") ? uri.Substring(0, (uri.Length-1) - 0) : uri)).LastIndexOf('/');
            if ((lastSlash<0)) {
                if ((uri.Length>15)) {
                    uri = ("..."+uri.Substring((uri.Length-15)));
                }
                return uri;
            } else {
                return ("..."+uri.Substring(lastSlash));
            }
        }
        public static string abbreviateEQName(string eqName) {
            try {
                if (eqName.StartsWith("{")) {
                    eqName = ("Q"+eqName);
                }
                Saxon.Hej.om.StructuredQName sq = Saxon.Hej.om.StructuredQName.fromEQName(eqName);
                return ((("Q{"+abbreviateURI(sq.getURI()))+"}")+sq.getLocalPart());
            } catch (System.Exception e) {
                return eqName;
            }
        }
        public static string wrap(Saxon.Hej.expr.Expression exp) {
            if (((Saxon.Hej.expr.parser.ExpressionTool.expressionSize(exp)<10) && !(((exp is Saxon.Hej.expr.instruct.Instruction))))) {
                return (("{"+exp)+"}");
            } else {
                return exp.getExpressionName();
            }
        }
        public static string describeGenre(Saxon.Hej.om.Genre genre) {
            switch (genre) {
              case Saxon.Hej.om.Genre.ANY:
                return "any item";
              case Saxon.Hej.om.Genre.ATOMIC:
                return "an atomic value";
              case Saxon.Hej.om.Genre.NODE:
                return "a node";
              case Saxon.Hej.om.Genre.FUNCTION:
                return "a function";
              case Saxon.Hej.om.Genre.MAP:
                return "a map";
              case Saxon.Hej.om.Genre.ARRAY:
                return "an array";
              case Saxon.Hej.om.Genre.EXTERNAL:
              default:
                return "an external object";
            }
        }
        public static string describeVisibility(Saxon.Hej.trans.Visibility vis) {
            return vis.ToString().ToLower();
        }
        public static string show(Saxon.Hej.s9api.Location loc) {
            return ((abbreviateURI(loc.getSystemId())+"#")+loc.getLineNumber());
        }
    
    }
}