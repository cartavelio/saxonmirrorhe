using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class XSLTTraceListener : Saxon.Hej.trace.AbstractTraceListener {
        protected internal override string getOpeningAttributes() {
            return (("xmlns:xsl=\""+Saxon.Hej.lib.NamespaceConstant.XSLT)+'\"');
        }
        protected internal override string tag(Saxon.Hej.trace.Traceable info) {
            return tagName(info);
        }
        public static string tagName(Saxon.Hej.trace.Traceable info) {
            if ((info is Saxon.Hej.expr.Expression)) {
                Saxon.Hej.expr.Expression expr = (Saxon.Hej.expr.Expression)(info);
                if ((expr is Saxon.Hej.expr.instruct.FixedElement)) {
                    return "LRE";
                } else {
                    if ((expr is Saxon.Hej.expr.instruct.FixedAttribute)) {
                        return "ATTR";
                    } else {
                        if ((expr is Saxon.Hej.expr.LetExpression)) {
                            return "xsl:variable";
                        } else {
                            if (expr.isCallOn(typeof(Saxon.Hej.functions.Trace))) {
                                return "fn:trace";
                            } else {
                                return expr.getExpressionName();
                            }
                        }
                    }
                }
            } else {
                if ((info is Saxon.Hej.expr.instruct.UserFunction)) {
                    return "xsl:function";
                } else {
                    if ((info is Saxon.Hej.expr.instruct.TemplateRule)) {
                        return "xsl:template";
                    } else {
                        if ((info is Saxon.Hej.expr.instruct.NamedTemplate)) {
                            return "xsl:template";
                        } else {
                            if ((info is Saxon.Hej.expr.instruct.GlobalParam)) {
                                return "xsl:param";
                            } else {
                                if ((info is Saxon.Hej.expr.instruct.GlobalVariable)) {
                                    return "xsl:variable";
                                } else {
                                    if ((info is Saxon.Hej.functions.Trace)) {
                                        return "fn:trace";
                                    } else {
                                        return "misc";
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