using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public abstract class ContextStackFrame {
        protected internal Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.s9api.Location location;
        private Saxon.Hej.om.Item contextItem;
        private System.Object container;
        public void setLocation(Saxon.Hej.s9api.Location loc) {
            this.location = loc;
        }
        public string getSystemId() {
            return location.getSystemId();
        }
        public int getLineNumber() {
            return location.getLineNumber();
        }
        public void setComponent(System.Object container) {
            this.container = container;
        }
        public System.Object getContainer() {
            return container;
        }
        public void setContext(Saxon.Hej.expr.XPathContext context) {
            this.context = context;
        }
        public Saxon.Hej.expr.XPathContext getContext() {
            return context;
        }
        public void setContextItem(Saxon.Hej.om.Item contextItem) {
            this.contextItem = contextItem;
        }
        public Saxon.Hej.om.Item getContextItem() {
            return contextItem;
        }
        public abstract void print(Saxon.Hej.lib.Logger @out);
        protected internal string showLocation() {
            if ((getSystemId() == null)) {
                return "";
            }
            int line = getLineNumber();
            if (((line == -1) || (line == unchecked((int)0xfffff)))) {
                return (("("+getSystemId())+")");
            } else {
                return (((("("+getSystemId())+"#")+getLineNumber())+")");
            }
        }
        public class CallingApplication : Saxon.Hej.trace.ContextStackFrame {
            public override void print(Saxon.Hej.lib.Logger @out) {
            }
        
        }
        public class BuiltInTemplateRule : Saxon.Hej.trace.ContextStackFrame {
            public BuiltInTemplateRule(Saxon.Hej.expr.XPathContext context)  {
                this.context = context;
            }
            public override void print(Saxon.Hej.lib.Logger @out) {
                Saxon.Hej.om.Item contextItem = context.getContextItem();
                string diag;
                if ((contextItem is Saxon.Hej.om.NodeInfo)) {
                    diag = Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(contextItem));
                } else {
                    if ((contextItem is Saxon.Hej.value.AtomicValue)) {
                        diag = ("value "+contextItem.ToString());
                    } else {
                        if ((contextItem is Saxon.Hej.ma.map.MapItem)) {
                            diag = "map";
                        } else {
                            if ((contextItem is Saxon.Hej.ma.arrays.ArrayItem)) {
                                diag = "array";
                            } else {
                                if ((contextItem is Saxon.Hej.om.FunctionItem)) {
                                    diag = "function";
                                } else {
                                    diag = "item";
                                }
                            }
                        }
                    }
                }
                @out.error(((("  in built-in template rule for "+diag)+" in ")+context.getCurrentMode().getActor().getModeTitle(false)));
            }
        
        }
        public class FunctionCall : Saxon.Hej.trace.ContextStackFrame {
            internal Saxon.Hej.om.StructuredQName functionName;
            public Saxon.Hej.om.StructuredQName getFunctionName() {
                return functionName;
            }
            public void setFunctionName(Saxon.Hej.om.StructuredQName functionName) {
                this.functionName = functionName;
            }
            public override void print(Saxon.Hej.lib.Logger @out) {
                @out.error(((("  at "+(((functionName == null) ? "(anonymous)" : functionName.getDisplayName())))+"() ")+showLocation()));
            }
        
        }
        public class ApplyTemplates : Saxon.Hej.trace.ContextStackFrame {
            public override void print(Saxon.Hej.lib.Logger @out) {
                @out.error(("  at xsl:apply-templates "+showLocation()));
                Saxon.Hej.om.Item node = getContextItem();
                if ((node is Saxon.Hej.om.NodeInfo)) {
                    @out.error(("     processing "+Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(node))));
                }
            }
        
        }
        public class CallTemplate : Saxon.Hej.trace.ContextStackFrame {
            public Saxon.Hej.om.StructuredQName getTemplateName() {
                return templateName;
            }
            public void setTemplateName(Saxon.Hej.om.StructuredQName templateName) {
                this.templateName = templateName;
            }
            internal Saxon.Hej.om.StructuredQName templateName;
            public override void print(Saxon.Hej.lib.Logger @out) {
                string name = ((templateName == null) ? "??" : templateName.getDisplayName());
                @out.error(((("  at xsl:call-template name=\""+name)+"\" ")+showLocation()));
            }
        
        }
        public class VariableEvaluation : Saxon.Hej.trace.ContextStackFrame {
            public Saxon.Hej.om.StructuredQName getVariableName() {
                return variableName;
            }
            public void setVariableName(Saxon.Hej.om.StructuredQName variableName) {
                this.variableName = variableName;
            }
            internal Saxon.Hej.om.StructuredQName variableName;
            public override void print(Saxon.Hej.lib.Logger @out) {
                @out.error(((("  in "+displayContainer(getContainer()))+" ")+showLocation()));
            }
        
        }
        private static string displayContainer(System.Object container) {
            if ((container is Saxon.Hej.expr.instruct.Actor)) {
                Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.expr.instruct.Actor)(container)).getComponentName();
                string objectName = ((name == null) ? "" : name.getDisplayName());
                if ((container is Saxon.Hej.expr.instruct.NamedTemplate)) {
                    return (("template name=\""+objectName)+"\"");
                } else {
                    if ((container is Saxon.Hej.expr.instruct.UserFunction)) {
                        return (("function "+objectName)+"()");
                    } else {
                        if ((container is Saxon.Hej.expr.instruct.AttributeSet)) {
                            return ("attribute-set "+objectName);
                        } else {
                            if ((container is Saxon.Hej.trans.KeyDefinition)) {
                                return ("key "+objectName);
                            } else {
                                if ((container is Saxon.Hej.expr.instruct.GlobalVariable)) {
                                    Saxon.Hej.om.StructuredQName qName = ((Saxon.Hej.expr.instruct.GlobalVariable)(container)).getVariableQName();
                                    if (qName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE)) {
                                        return "optimizer-created global variable";
                                    } else {
                                        return ("global variable $"+qName.getDisplayName());
                                    }
                                }
                            }
                        }
                    }
                }
            } else {
                if ((container is Saxon.Hej.expr.instruct.TemplateRule)) {
                    return (("template match=\""+((Saxon.Hej.expr.instruct.TemplateRule)(container)).getMatchPattern().ToString())+"\"");
                }
            }
            return "";
        }
    
    }
}