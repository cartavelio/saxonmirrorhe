using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class ElementCreator : Saxon.Hej.expr.instruct.ParentNodeConstructor {
        public abstract class ElementCreationDetails {
            public abstract Saxon.Hej.om.NodeName getNodeName(Saxon.Hej.expr.XPathContext context);
            public abstract string getSystemId(Saxon.Hej.expr.XPathContext context);
            public abstract void processContent(Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context);
        
        }
        internal bool bequeathNamespacesToChildren = true;
        internal bool inheritNamespacesFromParent = true;
        public ElementCreator()  {
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.ELEMENT;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public void setBequeathNamespacesToChildren(bool inherit) {
            bequeathNamespacesToChildren = inherit;
        }
        public bool isBequeathNamespacesToChildren() {
            return bequeathNamespacesToChildren;
        }
        public void setInheritNamespacesFromParent(bool inherit) {
            inheritNamespacesFromParent = inherit;
        }
        public bool isInheritNamespacesFromParent() {
            return inheritNamespacesFromParent;
        }
        protected internal override int computeSpecialProperties() {
            int p = (base.computeSpecialProperties()|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
            if ((getValidationAction() == Saxon.Hej.lib.Validation.STRIP)) {
                p |= Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED;
            }
            return p;
        }
        public override void suppressValidation(int parentValidationMode) {
            if (((getValidationAction() == parentValidationMode) && (getSchemaType() == null))) {
                setValidationAction(Saxon.Hej.lib.Validation.PRESERVE, null);
            }
        }
        protected internal override void checkContentSequence(Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.Operand[] components;
            if ((getContentExpression() is Saxon.Hej.expr.instruct.Block)) {
                components = ((Saxon.Hej.expr.instruct.Block)(getContentExpression())).getOperanda();
            } else {
                components = new Saxon.Hej.expr.Operand[]{contentOp};
            }
            bool foundChild = false;
            bool foundPossibleChild = false;
            foreach (Saxon.Hej.expr.Operand o in components) {
                Saxon.Hej.expr.Expression component = o.getChildExpression();
                Saxon.Hej.type.ItemType it = component.getItemType();
                if (it.isAtomicType()) {
                    foundChild = true;
                } else {
                    if (((it is Saxon.Hej.type.FunctionItemType) && !(((it is Saxon.Hej.ma.arrays.ArrayItemType))))) {
                        string which = ((it is Saxon.Hej.ma.map.MapType) ? "map" : "function");
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Cannot add a "+which)+" as a child of a constructed element"));
                        de.setErrorCode((isXSLT() ? "XTDE0450" : "XQTY0105"));
                        de.setLocator(component.getLocation());
                        de.setIsTypeError(true);
                        throw de;
                    } else {
                        if ((it is Saxon.Hej.pattern.NodeTest)) {
                            bool maybeEmpty = Saxon.Hej.value.Cardinality.allowsZero(component.getCardinality());
                            Saxon.Hej.type.UType possibleNodeKinds = it.getUType();
                            if (possibleNodeKinds.overlaps(Saxon.Hej.type.UType.TEXT)) {
                                if (((component is Saxon.Hej.expr.instruct.ValueOf) && (((Saxon.Hej.expr.instruct.ValueOf)(component)).getSelect() is Saxon.Hej.expr.StringLiteral))) {
                                    string value = ((Saxon.Hej.expr.StringLiteral)(((Saxon.Hej.expr.instruct.ValueOf)(component)).getSelect())).stringify();
                                    if (value.Length==0) {
                                    } else {
                                        foundChild = true;
                                    }
                                } else {
                                    foundPossibleChild = true;
                                }
                            } else {
                                if (!(possibleNodeKinds.overlaps(Saxon.Hej.type.UType.CHILD_NODE_KINDS))) {
                                    if (maybeEmpty) {
                                        foundPossibleChild = true;
                                    } else {
                                        foundChild = true;
                                    }
                                } else {
                                    if (((foundChild && (possibleNodeKinds == Saxon.Hej.type.UType.ATTRIBUTE)) && !(maybeEmpty))) {
                                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Cannot create an attribute node after creating a child of the containing element");
                                        de.setErrorCode((isXSLT() ? "XTDE0410" : "XQTY0024"));
                                        de.setLocator(component.getLocation());
                                        throw de;
                                    } else {
                                        if (((foundChild && (possibleNodeKinds == Saxon.Hej.type.UType.NAMESPACE)) && !(maybeEmpty))) {
                                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Cannot create a namespace node after creating a child of the containing element");
                                            de.setErrorCode((isXSLT() ? "XTDE0410" : "XQTY0024"));
                                            de.setLocator(component.getLocation());
                                            throw de;
                                        } else {
                                            if ((((foundChild || foundPossibleChild)) && (possibleNodeKinds == Saxon.Hej.type.UType.ATTRIBUTE))) {
                                                env.issueWarning("Creating an attribute here will fail if previous instructions create any children", Saxon.Hej.trans.SaxonErrorCode.SXWN9030, component.getLocation());
                                            } else {
                                                if ((((foundChild || foundPossibleChild)) && (possibleNodeKinds == Saxon.Hej.type.UType.NAMESPACE))) {
                                                    env.issueWarning("Creating a namespace node here will fail if previous instructions create any children", Saxon.Hej.trans.SaxonErrorCode.SXWN9030, component.getLocation());
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
        }
        public abstract void outputNamespaceNodes(Saxon.Hej.@event.Outputter receiver, Saxon.Hej.om.NodeName nodeName, Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails details);
        public override int getImplementationMethod() {
            return Saxon.Hej.expr.Expression.PROCESS_METHOD;
        }
        internal void exportValidationAndType(Saxon.Hej.trace.ExpressionPresenter @out) {
            if (((getValidationAction() != Saxon.Hej.lib.Validation.SKIP) && (getValidationAction() != Saxon.Hej.lib.Validation.BY_TYPE))) {
                @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(getValidationAction()));
            }
            if ((getValidationAction() == Saxon.Hej.lib.Validation.BY_TYPE)) {
                Saxon.Hej.type.SchemaType type = getSchemaType();
                if ((type != null)) {
                    @out.emitAttribute("type", type.getStructuredQName());
                }
            }
        }
        internal string getInheritanceFlags() {
            string flags = "";
            if (!(inheritNamespacesFromParent)) {
                flags += "P";
            }
            if (!(bequeathNamespacesToChildren)) {
                flags += "C";
            }
            return flags;
        }
        public void setInheritanceFlags(string flags) {
            inheritNamespacesFromParent = !(flags.Contains("P"));
            bequeathNamespacesToChildren = !(flags.Contains("C"));
        }
        public virtual Saxon.Hej.expr.instruct.ElementCreator.ElementCreationDetails makeElementCreationDetails() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public override string getStreamerName() {
            return "ElementCreator";
        }
    
    }
}