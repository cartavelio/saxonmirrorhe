using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardDiagnostics {
        public StandardDiagnostics()  {
        }
        public string getLocationMessageText(Saxon.Ejavax.xml.transform.SourceLocator loc) {
            string locMessage = "";
            string systemId = null;
            Saxon.Hej.om.NodeInfo node = null;
            string path;
            string nodeMessage = null;
            int lineNumber = -1;
            if ((loc == null)) {
                loc = Saxon.Hej.expr.parser.Loc.NONE;
            }
            if ((loc is Saxon.Hej.expr.parser.XPathParser.NestedLocation)) {
                loc = ((Saxon.Hej.expr.parser.XPathParser.NestedLocation)(loc)).getContainingLocation();
            }
            if ((loc is Saxon.Hej.tree.AttributeLocation)) {
                Saxon.Hej.tree.AttributeLocation saLoc = (Saxon.Hej.tree.AttributeLocation)(loc);
                nodeMessage = ("in "+saLoc.getElementName().getDisplayName());
                if ((saLoc.getAttributeName() != null)) {
                    nodeMessage += ("/@"+saLoc.getAttributeName());
                }
                nodeMessage += ' ';
            } else {
                if ((loc is Saxon.Hej.om.NodeInfo)) {
                    node = (Saxon.Hej.om.NodeInfo)(loc);
                    nodeMessage = (("at "+node.getDisplayName())+' ');
                } else {
                    if (((loc is Saxon.Hej.type.ValidationException) && ((node = ((Saxon.Hej.type.ValidationException)(loc)).getNode()) != null))) {
                        nodeMessage = (("at "+node.getDisplayName())+' ');
                    } else {
                        if ((((loc is Saxon.Hej.type.ValidationException) && (loc.getLineNumber() == -1)) && ((path = ((Saxon.Hej.type.ValidationException)(loc)).getPath()) != null))) {
                            nodeMessage = (("at "+path)+' ');
                        } else {
                            if ((loc is Saxon.Hej.expr.instruct.Instruction)) {
                                string instructionName = getInstructionName((Saxon.Hej.expr.instruct.Instruction)(loc));
                                if (!("".Equals(instructionName))) {
                                    nodeMessage = (("at "+instructionName)+' ');
                                }
                                systemId = loc.getSystemId();
                                lineNumber = loc.getLineNumber();
                            } else {
                                if ((loc is Saxon.Hej.expr.instruct.Actor)) {
                                    string kind = "procedure";
                                    if ((loc is Saxon.Hej.expr.instruct.UserFunction)) {
                                        kind = "function";
                                    } else {
                                        if ((loc is Saxon.Hej.expr.instruct.NamedTemplate)) {
                                            kind = "template";
                                        } else {
                                            if ((loc is Saxon.Hej.expr.instruct.AttributeSet)) {
                                                kind = "attribute-set";
                                            } else {
                                                if ((loc is Saxon.Hej.trans.KeyDefinition)) {
                                                    kind = "key";
                                                } else {
                                                    if ((loc is Saxon.Hej.expr.instruct.GlobalParam)) {
                                                        kind = "parameter";
                                                    } else {
                                                        if ((loc is Saxon.Hej.expr.instruct.GlobalVariable)) {
                                                            kind = "variable";
                                                        } else {
                                                            if ((loc is Saxon.Hej.trans.Mode)) {
                                                                kind = "mode";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    systemId = loc.getSystemId();
                                    lineNumber = loc.getLineNumber();
                                    nodeMessage = (("at "+kind)+" ");
                                    Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.expr.instruct.Actor)(loc)).getComponentName();
                                    if ((name != null)) {
                                        string n = name.ToString();
                                        if (n.Equals("xsl:unnamed")) {
                                            n = "(unnamed)";
                                        }
                                        nodeMessage += n;
                                        nodeMessage += " ";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((lineNumber == -1)) {
                lineNumber = loc.getLineNumber();
            }
            bool containsLineNumber = (lineNumber>0);
            if (((node != null) && !(containsLineNumber))) {
                nodeMessage = (("at "+Saxon.Hej.tree.util.Navigator.getPath(node))+' ');
            }
            if ((nodeMessage != null)) {
                locMessage += nodeMessage;
            }
            if (containsLineNumber) {
                locMessage += (("on line "+lineNumber)+' ');
                if ((loc.getColumnNumber() != -1)) {
                    locMessage += (("column "+loc.getColumnNumber())+' ');
                }
            }
            if (((systemId != null) && systemId.Length==0)) {
                systemId = null;
            }
            if ((systemId == null)) {
                try {
                    systemId = loc.getSystemId();
                } catch (System.Exception err) {
                    System.Console.Error.WriteLine(err);
                }
            }
            if (((systemId != null) && !(systemId.Length==0))) {
                locMessage += ((((containsLineNumber ? "of " : "in "))+abbreviateLocationURI(systemId))+':');
            }
            return locMessage;
        }
        public string getInstructionName(Saxon.Hej.expr.instruct.Instruction inst) {
            return getInstructionNameDefault(inst);
        }
        public static string getInstructionNameDefault(Saxon.Hej.expr.instruct.Instruction inst) {
            try {
                if ((inst is Saxon.Hej.expr.instruct.FixedElement)) {
                    Saxon.Hej.om.StructuredQName qName = inst.getObjectName();
                    return (("element constructor <"+qName.getDisplayName())+'>');
                } else {
                    if ((inst is Saxon.Hej.expr.instruct.FixedAttribute)) {
                        Saxon.Hej.om.StructuredQName qName = inst.getObjectName();
                        return (("attribute constructor "+qName.getDisplayName())+"=\"{...}\"");
                    }
                }
                int construct = inst.getInstructionNameCode();
                if ((construct<0)) {
                    return "";
                }
                if ((((construct<1024) && (construct != Saxon.Hej.om.StandardNames.XSL_FUNCTION)) && (construct != Saxon.Hej.om.StandardNames.XSL_TEMPLATE))) {
                    if (inst.getPackageData().isXSLT()) {
                        return Saxon.Hej.om.StandardNames.getDisplayName(construct);
                    } else {
                        string s = Saxon.Hej.om.StandardNames.getDisplayName(construct);
                        int colon = s.IndexOf(':');
                        if ((colon>0)) {
                            string local = s.Substring((colon+1));
                            if (local.Equals("document")) {
                                return "document node constructor";
                            } else {
                                if ((local.Equals("text") || s.Equals("value-of"))) {
                                    return "text node constructor";
                                } else {
                                    if (local.Equals("element")) {
                                        return "computed element constructor";
                                    } else {
                                        if (local.Equals("attribute")) {
                                            return "computed attribute constructor";
                                        } else {
                                            if (local.Equals("variable")) {
                                                return "variable declaration";
                                            } else {
                                                if (local.Equals("param")) {
                                                    return "external variable declaration";
                                                } else {
                                                    if (local.Equals("comment")) {
                                                        return "comment constructor";
                                                    } else {
                                                        if (local.Equals("processing-instruction")) {
                                                            return "processing-instruction constructor";
                                                        } else {
                                                            if (local.Equals("namespace")) {
                                                                return "namespace node constructor";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return s;
                    }
                } else {
                    return "";
                }
            } catch (System.Exception err) {
                return "";
            }
        }
        public void logStackTrace(Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.Logger @out, int level) {
            if ((level>0)) {
                int depth = 20;
                while ((depth-->0)) {
                    Saxon.Hej.expr.Component component = context.getCurrentComponent();
                    if ((component != null)) {
                        if ((component.getActor() is Saxon.Hej.trans.Mode)) {
                            Saxon.Hej.trans.rules.Rule rule = context.getCurrentTemplateRule();
                            if ((rule != null)) {
                                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
                                Saxon.Hej.s9api.Location loc = rule.getPattern().getLocation();
                                sb.append("  In template rule with match=\"").append(rule.getPattern().toShortString()).append("\" ");
                                if (((loc != null) && (loc.getLineNumber() != -1))) {
                                    sb.append("on line ").append(loc.getLineNumber()).append(" ");
                                }
                                if (((loc != null) && (loc.getSystemId() != null))) {
                                    sb.append("of ").append(abbreviateLocationURI(loc.getSystemId()));
                                }
                                @out.error(sb.ToString());
                            }
                        } else {
                            @out.error(getLocationMessageText(component.getActor()).Replace("$at ", "In "));
                        }
                    }
                    try {
                        context.getStackFrame().getStackFrameMap().showStackFrame(context, @out);
                    } catch (System.Exception e) {
                    }
                    while (!(((context is Saxon.Hej.expr.XPathContextMajor)))) {
                        context = context.getCaller();
                    }
                    Saxon.Hej.expr.ContextOriginator originator = ((Saxon.Hej.expr.XPathContextMajor)(context)).getOrigin();
                    if (((originator == null) || (originator is Saxon.Hej.Controller))) {
                        return ;
                    } else {
                        @out.error(("     invoked by "+showOriginator(originator)));
                    }
                    context = context.getCaller();
                }
            }
        }
        protected internal string showOriginator(Saxon.Hej.expr.ContextOriginator originator) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            if ((originator == null)) {
                sb.append("unknown caller (null)");
            } else {
                if ((originator is Saxon.Hej.expr.instruct.Instruction)) {
                    sb.append(getInstructionName((Saxon.Hej.expr.instruct.Instruction)(originator)));
                } else {
                    if ((originator is Saxon.Hej.expr.UserFunctionCall)) {
                        sb.append("function call");
                    } else {
                        if ((originator is Saxon.Hej.Controller)) {
                            sb.append("external application");
                        } else {
                            if ((originator is Saxon.Hej.trans.rules.BuiltInRuleSet)) {
                                sb.append("built-in template rule (").append(((Saxon.Hej.trans.rules.BuiltInRuleSet)(originator)).getName()).append(")");
                            } else {
                                if ((originator is Saxon.Hej.trans.KeyDefinition)) {
                                    sb.append("xsl:key definition");
                                } else {
                                    if ((originator is Saxon.Hej.expr.instruct.GlobalParam)) {
                                        sb.append("global parameter ").append(((Saxon.Hej.expr.instruct.GlobalParam)(originator)).getVariableQName().getDisplayName());
                                    } else {
                                        if ((originator is Saxon.Hej.expr.instruct.GlobalVariable)) {
                                            sb.append(((Saxon.Hej.expr.instruct.GlobalVariable)(originator)).getDescription());
                                        } else {
                                            sb.append("unknown caller (").append(originator.GetType()).append(")");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((originator is Saxon.Hej.expr.Locatable)) {
                Saxon.Hej.s9api.Location loc = ((Saxon.Hej.expr.Locatable)(originator)).getLocation();
                if ((loc.getLineNumber() != -1)) {
                    sb.append(" at ").append(((loc.getSystemId() == null) ? "line " : ((loc.getSystemId()+"#"))));
                    sb.append(loc.getLineNumber());
                }
            }
            return sb.ToString();
        }
        protected internal string formatListOfOffendingNodes(Saxon.Hej.type.ValidationFailure failure) {
            Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder();
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> offendingNodes = failure.getOffendingNodes();
            if (!(offendingNodes.Count==0)) {
                message.append("\n  Nodes for which the assertion fails:");
                foreach (Saxon.Hej.om.NodeInfo offender in offendingNodes) {
                    string nodeDesc = Saxon.Hej.type.Type.displayTypeName(offender);
                    if ((offender.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        nodeDesc += (" "+Saxon.Hej.trans.Err.wrap(offender.getUnicodeStringValue(), Saxon.Hej.trans.Err.VALUE));
                    }
                    if ((offender.getLineNumber() != -1)) {
                        nodeDesc += (" on line "+offender.getLineNumber());
                        if ((offender.getColumnNumber() != -1)) {
                            nodeDesc += (" column "+offender.getColumnNumber());
                        }
                        if ((offender.getSystemId() != null)) {
                            nodeDesc += (" of "+offender.getSystemId());
                        }
                    } else {
                        nodeDesc += (" at "+Saxon.Hej.tree.util.Navigator.getPath(offender));
                    }
                    message.append("\n  * ").append(nodeDesc);
                }
            }
            return message.ToString();
        }
        public string abbreviateLocationURI(string uri) {
            return abbreviateLocationURIDefault(uri);
        }
        public static string abbreviateLocationURIDefault(string uri) {
            if ((uri == null)) {
                return "*unknown*";
            }
            int slash = uri.LastIndexOf('/');
            if (((slash>=0) && (slash<(uri.Length-1)))) {
                return uri.Substring((slash+1));
            } else {
                return uri;
            }
        }
        public int MAX_MESSAGE_LENGTH = 2000;
        public int MAX_MESSAGE_LINE_LENGTH = 100;
        public int MIN_MESSAGE_LINE_LENGTH = 10;
        public int TARGET_MESSAGE_LINE_LENGTH = 90;
        public string wordWrap(string message) {
            if ((message.Length>MAX_MESSAGE_LENGTH)) {
                message = message.Substring(0, MAX_MESSAGE_LENGTH - 0);
            }
            int nl = message.IndexOf('\n');
            if ((nl<0)) {
                nl = message.Length;
            }
            if ((nl>MAX_MESSAGE_LINE_LENGTH)) {
                int i = TARGET_MESSAGE_LINE_LENGTH;
                while (((message[i] != ' ') && (i>0))) {
                    i--;
                }
                if ((i>MIN_MESSAGE_LINE_LENGTH)) {
                    return ((message.Substring(0, i - 0)+"\n  ")+wordWrap(message.Substring((i+1))));
                } else {
                    return message;
                }
            } else {
                if ((nl<message.Length)) {
                    return ((message.Substring(0, nl - 0)+'\n')+wordWrap(message.Substring((nl+1))));
                } else {
                    return message;
                }
            }
        }
        public string expandSpecialCharacters(string @in, int threshold) {
            if ((threshold>=Saxon.Hej.serialize.charcode.UTF16CharacterSet.NONBMP_MAX)) {
                return @in;
            }
            Saxon.Hej.value.StringValue str = new Saxon.Hej.value.StringValue(@in);
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder((str.length32()*2));
            Saxon.Hej.z.IntIterator iter = str.codePoints();
            while (iter.hasNext()) {
                int ch = iter.next();
                fsb.appendCodePoint(ch);
                if ((ch>threshold)) {
                    fsb.append("[x");
                    fsb.append(Saxon.Impl.Helpers.Utils.intToHexString(ch));
                    fsb.append("]");
                }
            }
            return fsb.ToString();
        }
    
    }
}