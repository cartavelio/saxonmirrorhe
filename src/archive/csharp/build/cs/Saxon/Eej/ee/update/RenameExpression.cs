using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class RenameExpression : Saxon.Eej.ee.update.BasicUpdatingExpression {
        private readonly Saxon.Hej.om.NamespaceResolver nsContext;
        private readonly Saxon.Hej.om.NamespaceUri defaultNamespace;
        private readonly Saxon.Hej.expr.Operand nameOp;
        private readonly Saxon.Hej.expr.Operand targetOp;
        private readonly bool inheritNamespaces;
        public RenameExpression(Saxon.Hej.expr.Expression target, Saxon.Hej.expr.Expression name, Saxon.Hej.om.NamespaceResolver nsContext, Saxon.Hej.om.NamespaceUri defaultNamespace, bool inherit)  {
            nameOp = new Saxon.Hej.expr.Operand(this, name, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            targetOp = new Saxon.Hej.expr.Operand(this, target, Saxon.Hej.expr.OperandRole.INSPECT);
            this.nsContext = nsContext;
            this.defaultNamespace = ((defaultNamespace == null) ? Saxon.Hej.om.NamespaceUri.NULL : defaultNamespace);
            this.inheritNamespaces = inherit;
        }
        public Saxon.Hej.expr.Expression getTargetExp() {
            return targetOp.getChildExpression();
        }
        public void setTargetExp(Saxon.Hej.expr.Expression targetExpression) {
            targetOp.setChildExpression(targetExpression);
        }
        public Saxon.Hej.expr.Expression getNameExp() {
            return nameOp.getChildExpression();
        }
        public void setNameExp(Saxon.Hej.expr.Expression nameExpression) {
            nameOp.setChildExpression(nameExpression);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(targetOp, nameOp);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier0 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "insert", 1);
                role0.setErrorCode("XUDY0027");
                return role0;
            } ;
            setTargetExp(tc.staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.NON_EMPTY_SEQUENCE, roleSupplier0, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSUpplier1 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "rename", 0);
                role0.setErrorCode("XUTY0012");
                return role0;
            } ;
            setTargetExp(tc.staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.OPTIONAL_NODE, roleSUpplier1, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSUpplier2 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role1 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "rename", 1);
                role1.setErrorCode("XPTY0004");
                return role1;
            } ;
            setNameExp(tc.staticTypeCheck(getNameExp(), Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, roleSUpplier2, visitor));
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType supplied = getNameExp().getItemType();
            if ((((th.relationship(supplied, Saxon.Hej.type.BuiltInAtomicType.STRING) == Saxon.Hej.type.Affinity.DISJOINT) && (th.relationship(supplied, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) == Saxon.Hej.type.Affinity.DISJOINT)) && (th.relationship(supplied, Saxon.Hej.type.BuiltInAtomicType.QNAME) == Saxon.Hej.type.Affinity.DISJOINT))) {
                throw new Saxon.Hej.trans.XPathException("The new name must be a string, QName, or untypedAtomic").withErrorCode("XPTY0004").asTypeError().withLocation(getLocation());
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.update.RenameExpression exp = new Saxon.Eej.ee.update.RenameExpression(getTargetExp().copy(rebindings), getNameExp().copy(rebindings), nsContext, defaultNamespace, inheritNamespaces);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("rename", this);
            if (inheritNamespaces) {
                @out.emitAttribute("flags", "i");
            }
            getTargetExp().export(@out);
            getNameExp().export(@out);
            @out.endElement();
        }
        public override string getExpressionName() {
            return "rename";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.update.RenameExpression.RenameExpressionElaborator();
        }
        private class RenameExpressionElaborator : Saxon.Hej.expr.elab.UpdateElaborator {
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Eej.ee.update.RenameExpression expr = (Saxon.Eej.ee.update.RenameExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator targetEval = expr.getTargetExp().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator nameEval = expr.getNameExp().makeElaborator().elaborateForItem();
                return (context, pul) => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/targetEval(context));
                    int nodeKind = node.getNodeKind();
                    if (!(((((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (nodeKind == Saxon.Hej.type.Type.ATTRIBUTE)) || (nodeKind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION))))) {
                        throw new Saxon.Hej.trans.XPathException("Node to be renamed is not an element, attribute, or processing-instruction", "XUTY0012").withLocation(expr.getLocation());
                    }
                    if (!(node.getTreeInfo().isMutable())) {
                        throw new Saxon.Hej.trans.XPathException("Node to be renamed is not updatable", Saxon.Hej.trans.SaxonErrorCode.SXUP0081).withLocation(expr.getLocation());
                    }
                    Saxon.Hej.value.AtomicValue nameValue = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/nameEval(context));
                    string prefix;
                    string localName;
                    Saxon.Hej.om.NamespaceUri uri;
                    if ((nameValue is Saxon.Hej.value.StringValue)) {
                        string rawName = nameValue.getStringValue();
                        try {
                            string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(rawName);
                            prefix = parts[0];
                            localName = parts[1];
                        } catch (Saxon.Hej.om.QNameException err11) {
                            throw new Saxon.Hej.trans.XPathException(("Invalid node name. "+err11.Message), "XQDY0074", expr.getLocation()).withXPathContext(context);
                        }
                        if (prefix.Length==0) {
                            uri = ((nodeKind == Saxon.Hej.type.Type.ELEMENT) ? expr.defaultNamespace : Saxon.Hej.om.NamespaceUri.NULL);
                        } else {
                            uri = expr.nsContext.getURIForPrefix(prefix, true);
                            if ((uri == null)) {
                                throw new Saxon.Hej.trans.XPathException(("Undeclared prefix in new node name: "+prefix), "XQDY0074", expr.getLocation()).withXPathContext(context);
                            }
                        }
                    } else {
                        if ((nameValue is Saxon.Hej.value.QNameValue)) {
                            localName = ((Saxon.Hej.value.QNameValue)(nameValue)).getLocalName();
                            uri = ((Saxon.Hej.value.QNameValue)(nameValue)).getNamespaceURI();
                            prefix = ((Saxon.Hej.value.QNameValue)(nameValue)).getPrefix();
                        } else {
                            throw new Saxon.Hej.trans.XPathException("Computed element name has incorrect type").withErrorCode("XPTY0004").asTypeError().withXPathContext(context).withLocation(expr.getLocation());
                        }
                    }
                    if (uri.isEmpty()) {
                        prefix = "";
                    }
                    if (prefix.Equals("xmlns")) {
                        prefix = "x-xmlns";
                    }
                    if ((nodeKind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                        if (((prefix.Length>0) || !(uri.isEmpty()))) {
                            throw new Saxon.Hej.trans.XPathException("Processing instruction cannot be in a namespace").withErrorCode("XUDY0025").asTypeError().withXPathContext(context).withLocation(expr.getLocation());
                        }
                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(localName, "xml")) {
                            throw new Saxon.Hej.trans.XPathException("Processing instruction cannot be named 'xml'").withErrorCode("XQDY0064").withXPathContext(context).withLocation(expr.getLocation());
                        }
                    }
                    Saxon.Hej.om.NodeName name = new Saxon.Hej.om.FingerprintedQName(prefix, uri, localName);
                    Saxon.Hej.om.NodeInfo targetNode = node;
                    if ((nodeKind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        targetNode = node.getParent();
                    } else {
                        if ((nodeKind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                            targetNode = null;
                        }
                    }
                    if (((targetNode != null) && (((nodeKind == Saxon.Hej.type.Type.ELEMENT) || (prefix.Length != 0))))) {
                        Saxon.Hej.om.NamespaceResolver nr = targetNode.getAllNamespaces();
                        Saxon.Hej.om.NamespaceUri existingURI = nr.getURIForPrefix(prefix, true);
                        if ((((existingURI != null) && !(existingURI.isEmpty())) && !(existingURI.Equals(uri)))) {
                            throw new Saxon.Hej.trans.XPathException("New name conflicts with existing namespace binding").withErrorCode("XUDY0023").asTypeError().withXPathContext(context).withLocation(expr.getLocation());
                        }
                    }
                    try {
                        Saxon.Eej.ee.update.RenameAction action = new Saxon.Eej.ee.update.RenameAction(node, name, expr.inheritNamespaces);
                        action.setOriginator(expr);
                        ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw e.maybeWithLocation(expr.getLocation()).maybeWithContext(context);
                    }
                } ;
            }
        
        }
    
    }
}