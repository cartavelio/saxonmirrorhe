using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public abstract class AbstractTraceListener : Saxon.Hej.lib.StandardDiagnostics, Saxon.Hej.lib.TraceListener {
        protected internal int indent = 0;
        private int detail = 2;
        protected internal Saxon.Hej.lib.Logger @out = new Saxon.Callbacks.StandardLogger();
        private static readonly Saxon.Ejava.lang.StringBuilder spaceBuffer = new Saxon.Ejava.lang.StringBuilder("                ");
        public virtual Saxon.Hej.expr.parser.CodeInjector getCodeInjector() {
            return new Saxon.Hej.trace.TraceCodeInjector();
        }
        public void setLevelOfDetail(int level) {
            this.detail = level;
        }
        public void open(Saxon.Hej.Controller controller) {
            @out.info(((((("<trace "+"saxon-version=\"")+Saxon.Hej.Version.getProductVersion())+"\" ")+getOpeningAttributes())+'>'));
            indent++;
        }
        protected internal abstract string getOpeningAttributes();
        public void close() {
            indent--;
            @out.info("</trace>");
        }
        public void enter(Saxon.Hej.trace.Traceable info, System.Collections.Generic.IDictionary<string,System.Object> properties, Saxon.Hej.expr.XPathContext context) {
            if (isApplicable(info)) {
                Saxon.Hej.s9api.Location loc = info.getLocation();
                string file = abbreviateLocationURI(loc.getSystemId());
                Saxon.Ejava.lang.StringBuilder msg = new Saxon.Ejava.lang.StringBuilder(((Saxon.Hej.trace.AbstractTraceListener.spaces(indent)+'<')+tag(info)));
                foreach (System.Collections.Generic.KeyValuePair<string,System.Object> entry in properties) {
                    System.Object val = entry.Value;
                    if ((val is Saxon.Hej.om.StructuredQName)) {
                        val = ((Saxon.Hej.om.StructuredQName)(val)).getDisplayName();
                    } else {
                        if ((val is Saxon.Hej.value.StringValue)) {
                            val = ((Saxon.Hej.value.StringValue)(val)).getUnicodeStringValue();
                        }
                    }
                    if ((val != null)) {
                        msg.append(' ').append(entry.Key).append("=\"").append(escape(val.ToString())).append('"');
                    }
                }
                msg.append(" line=\"").append(loc.getLineNumber()).append('"');
                int col = loc.getColumnNumber();
                if ((col>=0)) {
                    msg.append(" column=\"").append(loc.getColumnNumber()).append('"');
                }
                msg.append(" module=\"").append(escape(file)).append('"');
                msg.append(">");
                @out.info(msg.ToString());
                indent++;
            }
        }
        public string escape(string @in) {
            if ((@in == null)) {
                return "";
            }
            string collapsed = Saxon.Hej.value.Whitespace.collapseWhitespace(@in);
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((collapsed.Length+10));
            for (int i = 0; (i<collapsed.Length); i++) {
                char c = collapsed[i];
                if ((c == '<')) {
                    sb.append("&lt;");
                } else {
                    if ((c == '>')) {
                        sb.append("&gt;");
                    } else {
                        if ((c == '&')) {
                            sb.append("&amp;");
                        } else {
                            if ((c == '\"')) {
                                sb.append("&#34;");
                            } else {
                                if ((c == '\n')) {
                                    sb.append("&#xA;");
                                } else {
                                    if ((c == '\r')) {
                                        sb.append("&#xD;");
                                    } else {
                                        if ((c == '\t')) {
                                            sb.append("&#x9;");
                                        } else {
                                            sb.append(c);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }
        public void leave(Saxon.Hej.trace.Traceable info) {
            if (isApplicable(info)) {
                indent--;
                @out.info((((Saxon.Hej.trace.AbstractTraceListener.spaces(indent)+"</")+tag(info))+'>'));
            }
        }
        protected internal bool isApplicable(Saxon.Hej.trace.Traceable info) {
            return (level(info)<=detail);
        }
        protected internal abstract string tag(Saxon.Hej.trace.Traceable info);
        protected internal int level(Saxon.Hej.trace.Traceable info) {
            if (((info is Saxon.Hej.trace.TraceableComponent) || (info is Saxon.Hej.expr.instruct.ApplyTemplates))) {
                return 1;
            }
            if ((info is Saxon.Hej.expr.instruct.Instruction)) {
                return 2;
            } else {
                return 3;
            }
        }
        public void startCurrentItem(Saxon.Hej.om.Item item) {
            if (((item is Saxon.Hej.om.NodeInfo) && (detail>0))) {
                Saxon.Hej.om.NodeInfo curr = (Saxon.Hej.om.NodeInfo)(item);
                @out.info((((((((Saxon.Hej.trace.AbstractTraceListener.spaces(indent)+"<source node=\"")+Saxon.Hej.tree.util.Navigator.getPath(curr))+"\" line=\"")+curr.getLineNumber())+"\" file=\"")+abbreviateLocationURI(curr.getSystemId()))+"\">"));
            }
            indent++;
        }
        public void endCurrentItem(Saxon.Hej.om.Item item) {
            indent--;
            if (((item is Saxon.Hej.om.NodeInfo) && (detail>0))) {
                Saxon.Hej.om.NodeInfo curr = (Saxon.Hej.om.NodeInfo)(item);
                @out.info((((Saxon.Hej.trace.AbstractTraceListener.spaces(indent)+"</source><!-- ")+Saxon.Hej.tree.util.Navigator.getPath(curr))+" -->"));
            }
        }
        protected internal static string spaces(int n) {
            while ((spaceBuffer.length()<n)) {
                spaceBuffer.append(Saxon.Hej.trace.AbstractTraceListener.spaceBuffer);
            }
            return spaceBuffer.substring(0, n);
        }
        public void setOutputDestination(Saxon.Hej.lib.Logger stream) {
            @out = stream;
        }
        public Saxon.Hej.lib.Logger getOutputDestination() {
            return @out;
        }
        public virtual void endRuleSearch(System.Object rule, Saxon.Hej.trans.Mode mode, Saxon.Hej.om.Item item) {
        }
        public virtual void startRuleSearch() {
        }
    
    }
}