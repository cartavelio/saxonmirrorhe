using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class XQueryTraceListener : Saxon.Hej.trace.AbstractTraceListener {
        protected internal override string getOpeningAttributes() {
            return "";
        }
        protected internal override string tag(Saxon.Hej.trace.Traceable info) {
            if ((info is Saxon.Hej.trace.TraceableComponent)) {
                if ((info is Saxon.Hej.expr.instruct.GlobalVariable)) {
                    return "variable";
                } else {
                    if ((info is Saxon.Hej.expr.instruct.UserFunction)) {
                        return "function";
                    } else {
                        if ((info is Saxon.Hej.query.XQueryExpression)) {
                            return "query";
                        } else {
                            return "misc";
                        }
                    }
                }
            } else {
                if ((info is Saxon.Hej.functions.Trace)) {
                    return "fn:trace";
                } else {
                    if ((info is Saxon.Hej.expr.flwor.ClauseInfo)) {
                        return ((Saxon.Hej.expr.flwor.ClauseInfo)(info)).getClause().getClauseKey().ToString();
                    } else {
                        if ((info is Saxon.Hej.expr.Expression)) {
                            string s = ((Saxon.Hej.expr.Expression)(info)).getExpressionName();
                            if (s.StartsWith("xsl:")) {
                                s = s.Substring(4);
                            }
                            switch (s) {
                              case "value-of":
                                return "text";
                              case "LRE":
                                return "element";
                              case "ATTR":
                                return "attribute";
                              default:
                                return s;
                            }
                        } else {
                            return null;
                        }
                    }
                }
            }
        }
        public override void startRuleSearch() {
        }
        public override void endRuleSearch(System.Object rule, Saxon.Hej.trans.Mode mode, Saxon.Hej.om.Item item) {
        }
    
    }
}