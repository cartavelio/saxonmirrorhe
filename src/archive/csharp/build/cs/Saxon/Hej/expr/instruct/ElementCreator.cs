using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class ElementCreator : Saxon.Hej.expr.instruct.ParentNodeConstructor {
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
                                                env.issueWarning("Creating an attribute here will fail if previous instructions create any children", component.getLocation());
                                            } else {
                                                if ((((foundChild || foundPossibleChild)) && (possibleNodeKinds == Saxon.Hej.type.UType.NAMESPACE))) {
                                                    env.issueWarning("Creating a namespace node here will fail if previous instructions create any children", component.getLocation());
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
        public abstract Saxon.Hej.om.NodeName getElementName(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.NodeInfo copiedNode);
        public abstract string getNewBaseURI(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.NodeInfo copiedNode);
        public abstract void outputNamespaceNodes(Saxon.Hej.@event.Outputter receiver, Saxon.Hej.om.NodeName nodeName, Saxon.Hej.om.NodeInfo copiedNode);
        public override int getImplementationMethod() {
            return Saxon.Hej.expr.Expression.PROCESS_METHOD;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return processLeavingTail(output, context, null);
        }
        public Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.NodeInfo copiedNode) {
            try {
                Saxon.Hej.om.NodeName elemName = getElementName(context, copiedNode);
                Saxon.Hej.type.SchemaType typeCode;
                if ((getValidationAction() == Saxon.Hej.lib.Validation.PRESERVE)) {
                    typeCode = Saxon.Hej.type.AnyType.getInstance();
                } else {
                    typeCode = Saxon.Hej.type.Untyped.getInstance();
                }
                Saxon.Hej.@event.Receiver elemOut = @out;
                if (!(preservingTypes)) {
                    Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(getValidationOptions());
                    options.setTopLevelElement(elemName.getStructuredQName());
                    context.getConfiguration().prepareValidationReporting(context, options);
                    Saxon.Hej.@event.Receiver validator = context.getConfiguration().getElementValidator(elemOut, options, getLocation());
                    if ((validator != elemOut)) {
                        @out = new Saxon.Hej.@event.ComplexContentOutputter(validator);
                    }
                }
                if ((@out.getSystemId() == null)) {
                    @out.setSystemId(getNewBaseURI(context, copiedNode));
                }
                int properties = Saxon.Hej.@event.ReceiverOption.NONE;
                if (!(bequeathNamespacesToChildren)) {
                    properties |= Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES;
                }
                if (!(inheritNamespacesFromParent)) {
                    properties |= Saxon.Hej.@event.ReceiverOption.REFUSE_NAMESPACES;
                }
                properties |= Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES;
                @out.startElement(elemName, typeCode, getLocation(), properties);
                outputNamespaceNodes(@out, elemName, copiedNode);
                getContentExpression().process(@out, context);
                @out.endElement();
                return null;
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw;
            }
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
        public override string getStreamerName() {
            return "ElementCreator";
        }
    
    }
}