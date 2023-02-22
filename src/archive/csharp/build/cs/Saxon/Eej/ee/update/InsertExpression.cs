using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class InsertExpression : Saxon.Eej.ee.update.BasicUpdatingExpression {
        private readonly int position;
        private readonly int constructionMode;
        private readonly bool inheritNamespaces;
        private readonly bool preserveNamespaces;
        private readonly Saxon.Hej.expr.Operand sourceOp;
        private readonly Saxon.Hej.expr.Operand targetOp;
        public InsertExpression(Saxon.Hej.expr.Expression source, Saxon.Hej.expr.Expression target, int position, int constructionMode, bool inherit, bool preserve)  {
            sourceOp = new Saxon.Hej.expr.Operand(this, source, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            targetOp = new Saxon.Hej.expr.Operand(this, target, Saxon.Hej.expr.OperandRole.INSPECT);
            this.position = position;
            this.constructionMode = constructionMode;
            this.inheritNamespaces = inherit;
            this.preserveNamespaces = preserve;
        }
        public Saxon.Hej.expr.Expression getSourceExpression() {
            return sourceOp.getChildExpression();
        }
        public void setSourceExpression(Saxon.Hej.expr.Expression sourceExpression) {
            sourceOp.setChildExpression(sourceExpression);
        }
        public Saxon.Hej.expr.Expression getTargetExpression() {
            return targetOp.getChildExpression();
        }
        public void setTargetExpression(Saxon.Hej.expr.Expression targetExpression) {
            targetOp.setChildExpression(targetExpression);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(sourceOp, targetOp);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier0 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "insert", 1);
                role0.setErrorCode("XUDY0027");
                return role0;
            } ;
            setTargetExpression(tc.staticTypeCheck(getTargetExpression(), Saxon.Hej.value.SequenceType.NON_EMPTY_SEQUENCE, roleSupplier0, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier1 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "insert", 1);
                role0.setErrorCode((((position == Saxon.Hej.expr.parser.Token.AFTER) || (position == Saxon.Hej.expr.parser.Token.BEFORE)) ? "XUTY0006" : "XUTY0005"));
                return role0;
            } ;
            setTargetExpression(tc.staticTypeCheck(getTargetExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_NODE, roleSupplier1, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.update.InsertExpression exp = new Saxon.Eej.ee.update.InsertExpression(getSourceExpression().copy(rebindings), getTargetExpression().copy(rebindings), position, constructionMode, inheritNamespaces, preserveNamespaces);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override string getExpressionName() {
            return "insert";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("insert", this);
            @out.emitAttribute("position", /*134*/Saxon.Eej.ee.xtupdate.SaxonInsert.encodePosition(position));
            @out.emitAttribute("constructionMode", (constructionMode+""));
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(2);
            if (inheritNamespaces) {
                fsb.append("i");
            }
            if (preserveNamespaces) {
                fsb.append("p");
            }
            @out.emitAttribute("flags", fsb.ToString());
            getSourceExpression().export(@out);
            getTargetExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.update.InsertExpression.InsertExpressionElaborator();
        }
        private class InsertExpressionElaborator : Saxon.Hej.expr.elab.UpdateElaborator {
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Eej.ee.update.InsertExpression expr = (Saxon.Eej.ee.update.InsertExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator targetEval = expr.getTargetExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.PullEvaluator sourceEval = expr.getSourceExpression().makeElaborator().elaborateForPull();
                return (context, pul) => {
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> content = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    Saxon.Hej.om.NodeInfo target = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/targetEval(context));
                    if ((target == null)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Target of insert expression must not be empty", "XUDY0027");
                        e.setLocation(expr.getLocation());
                        e.setXPathContext(context);
                        throw e;
                    }
                    int kind = target.getNodeKind();
                    if (((expr.position == Saxon.Hej.expr.parser.Token.AFTER) || (expr.position == Saxon.Hej.expr.parser.Token.BEFORE))) {
                        if (!((((((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.TEXT)) || (kind == Saxon.Hej.type.Type.COMMENT)) || (kind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION))))) {
                            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("When inserting before or after a node, that node must "+"be an element, text, comment, or processing-instruction node"), "XUTY0006");
                            e.setLocation(expr.getLocation());
                            e.setXPathContext(context);
                            throw e;
                        }
                    } else {
                        if (!((((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.DOCUMENT))))) {
                            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("When inserting into a target node, that node must be an element or document node", "XUTY0005");
                            e.setLocation(expr.getLocation());
                            e.setXPathContext(context);
                            throw e;
                        }
                    }
                    Saxon.Hej.om.NodeInfo parent = (((expr.position == Saxon.Hej.expr.parser.Token.AFTER) || (expr.position == Saxon.Hej.expr.parser.Token.BEFORE)) ? target.getParent() : target);
                    if ((parent == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("When inserting before or after a node, that node must have a parent", "XUDY0029");
                        err.setLocation(expr.getLocation());
                        err.setXPathContext(context);
                        throw err;
                    }
                    if (!(((parent is Saxon.Hej.om.MutableNodeInfo)))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Target node of insert expression is not updatable", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
                        err.setLocation(expr.getLocation());
                        err.setXPathContext(context);
                        throw err;
                    }
                    bool foundNonAttribute = false;
                    Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/sourceEval(context);
                    for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                        if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE))) {
                            if (foundNonAttribute) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Attributes must precede non-attributes in the list of nodes to be inserted", "XUTY0004");
                                err.setLocation(expr.getLocation());
                                err.setXPathContext(context);
                                throw err;
                            }
                            if ((parent.getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                                if (((expr.position == Saxon.Hej.expr.parser.Token.AFTER) || (expr.position == Saxon.Hej.expr.parser.Token.BEFORE))) {
                                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("When inserting an attribute before or after a node, that node must "+"have a parent and the parent must be an element"), "XUDY0030");
                                    err.setLocation(expr.getLocation());
                                    err.setXPathContext(context);
                                    throw err;
                                } else {
                                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("When inserting an attribute into a target node, the target node must be an element", "XUTY0022");
                                    err.setLocation(expr.getLocation());
                                    err.setXPathContext(context);
                                    throw err;
                                }
                            }
                            Saxon.Hej.om.NodeInfo copiedNode = (Saxon.Hej.om.NodeInfo)(item);
                            Saxon.Hej.type.SimpleType attributeType;
                            int properties = Saxon.Hej.@event.ReceiverOption.NONE;
                            if (((expr.constructionMode == Saxon.Hej.lib.Validation.SKIP) || Saxon.Hej.type.Untyped.getInstance().Equals(parent.getSchemaType()))) {
                                attributeType = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                            } else {
                                attributeType = (Saxon.Hej.type.SimpleType)(copiedNode.getSchemaType());
                                if ((copiedNode.isId() || (("id".Equals(copiedNode.getLocalPart()) && Saxon.Hej.om.NamespaceUri.XML.Equals(copiedNode.getNamespaceUri()))))) {
                                    properties |= (Saxon.Hej.@event.ReceiverOption.IS_ID);
                                }
                                if (copiedNode.isIdref()) {
                                    properties |= (Saxon.Hej.@event.ReceiverOption.IS_IDREF);
                                }
                            }
                            try {
                                Saxon.Eej.ee.update.InsertAttributeAction action = new Saxon.Eej.ee.update.InsertAttributeAction((Saxon.Hej.om.MutableNodeInfo)(parent), Saxon.Hej.om.NameOfNode.makeName(copiedNode), attributeType, copiedNode.getStringValue(), properties, expr.inheritNamespaces);
                                action.setOriginator(expr);
                                ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                            } catch (Saxon.Hej.trans.XPathException e) {
                                e.maybeSetLocation(expr.getLocation());
                                e.maybeSetContext(context);
                                throw;
                            }
                        } else {
                            foundNonAttribute = true;
                            content.Add(item);
                        }
                    }
                    if (!(content.Count==0)) {
                        Saxon.Hej.@event.Builder builder = ((Saxon.Hej.om.MutableNodeInfo)(parent)).newBuilder();
                        Saxon.Hej.Controller controller = context.getController();
                        builder.setLineNumbering(controller.getConfiguration().isLineNumbering());
                        Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                        pipe.setHostLanguage(expr.getPackageData().getHostLanguage());
                        pipe.setXPathContext(context);
                        builder.setPipelineConfiguration(pipe);
                        Saxon.Hej.@event.Receiver r = builder;
                        if (((expr.constructionMode == Saxon.Hej.lib.Validation.STRIP) || Saxon.Hej.type.Untyped.getInstance().Equals(parent.getSchemaType()))) {
                            r = new Saxon.Eej.ee.validate.SkipValidator(r);
                        }
                        Saxon.Hej.@event.TreeReceiver @out = new Saxon.Hej.@event.TreeReceiver(r);
                        @out.setPipelineConfiguration(pipe);
                        @out.open();
                        @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                        int copyNs = (expr.preserveNamespaces ? Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES : Saxon.Hej.@event.ReceiverOption.NONE);
                        foreach (Saxon.Hej.om.Item n in content) {
                            @out.append(n, Saxon.Hej.expr.parser.Loc.NONE, copyNs);
                        }
                        @out.endDocument();
                        @out.close();
                        Saxon.Hej.om.NodeInfo root = builder.getCurrentRoot();
                        Saxon.Hej.tree.iter.AxisIterator top = root.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                        Saxon.Hej.value.SequenceExtent extent = Saxon.Hej.value.SequenceExtent.from(top);
                        int len = extent.getLength();
                        Saxon.Hej.om.NodeInfo[] array = new Saxon.Hej.om.NodeInfo[len];
                        for (int i = 0; (i<len); i++) {
                            array[i] = (Saxon.Hej.om.NodeInfo)(extent.itemAt(i));
                        }
                        Saxon.Eej.ee.update.InsertAction action = new Saxon.Eej.ee.update.InsertAction(array, (Saxon.Hej.om.MutableNodeInfo)(target), expr.position, expr.inheritNamespaces);
                        action.setOriginator(expr);
                        ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                    }
                } ;
            }
        
        }
    
    }
}