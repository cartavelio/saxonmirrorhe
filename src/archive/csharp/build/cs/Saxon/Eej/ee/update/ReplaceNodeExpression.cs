using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class ReplaceNodeExpression : Saxon.Eej.ee.update.BasicUpdatingExpression {
        private readonly Saxon.Hej.expr.Operand targetOp;
        private readonly Saxon.Hej.expr.Operand replacementOp;
        private readonly int constructionMode;
        private readonly bool inheritNamespaces;
        public ReplaceNodeExpression(Saxon.Hej.expr.Expression target, Saxon.Hej.expr.Expression replacement, int constructionMode, bool inheritNamespaces)  {
            targetOp = new Saxon.Hej.expr.Operand(this, target, Saxon.Hej.expr.OperandRole.INSPECT);
            replacementOp = new Saxon.Hej.expr.Operand(this, replacement, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            this.constructionMode = constructionMode;
            this.inheritNamespaces = inheritNamespaces;
        }
        public Saxon.Hej.expr.Expression getTargetExp() {
            return targetOp.getChildExpression();
        }
        public void setTargetExp(Saxon.Hej.expr.Expression targetExpression) {
            targetOp.setChildExpression(targetExpression);
        }
        public Saxon.Hej.expr.Expression getReplacementExp() {
            return replacementOp.getChildExpression();
        }
        public void setReplacementExp(Saxon.Hej.expr.Expression replacement) {
            replacementOp.setChildExpression(replacement);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(targetOp, replacementOp);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier0 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "replace", 0);
                role0.setErrorCode("XUDY0027");
                return role0;
            } ;
            setTargetExp(tc.staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.NON_EMPTY_SEQUENCE, roleSupplier0, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier1 = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "replace", 0);
                role0.setErrorCode("XUTY0008");
                return role0;
            } ;
            setTargetExp(tc.staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.OPTIONAL_NODE, roleSupplier1, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.update.ReplaceNodeExpression exp = new Saxon.Eej.ee.update.ReplaceNodeExpression(getTargetExp().copy(rebindings), getReplacementExp().copy(rebindings), constructionMode, inheritNamespaces);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("replaceNodes", this);
            @out.emitAttribute("constructionMode", (constructionMode+""));
            if (inheritNamespaces) {
                @out.emitAttribute("flags", "i");
            }
            getTargetExp().export(@out);
            getReplacementExp().export(@out);
            @out.endElement();
        }
        public override string getExpressionName() {
            return "replaceNode";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.update.ReplaceNodeExpression.ReplaceNodeExpressionElaborator();
        }
        private class ReplaceNodeExpressionElaborator : Saxon.Hej.expr.elab.UpdateElaborator {
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Eej.ee.update.ReplaceNodeExpression expr = (Saxon.Eej.ee.update.ReplaceNodeExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator targetEval = expr.getTargetExp().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.PullEvaluator replacementEval = expr.getReplacementExp().makeElaborator().elaborateForPull();
                return (context, pul) => {
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> content = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    Saxon.Hej.om.NodeInfo target = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/targetEval(context));
                    if ((target == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Target of replace expression must not be empty", "XUDY0027");
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    if (!(target.getTreeInfo().isMutable())) {
                        throw new Saxon.Hej.trans.XPathException("Attempting to update a non-updatable tree", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
                    }
                    int kind = target.getNodeKind();
                    if ((kind == Saxon.Hej.type.Type.NAMESPACE)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Target of replace expression must not be a namespace node", "XUTY0008");
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    if ((kind == Saxon.Hej.type.Type.DOCUMENT)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Target of replace expression must not be a document node", "XUTY0008");
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    Saxon.Hej.om.NodeInfo parent = target.getParent();
                    if ((parent == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Target node of replace expression has no parent", "XUDY0009");
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    if (!(((parent is Saxon.Hej.om.MutableNodeInfo)))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Target node of replace expression is not updatable", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/replacementEval(context);
                        Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                        for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                            bool isAttribute = ((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE));
                            if (!(isAttribute)) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The replacement nodes for an attribute must all be attributes", "XUTY0011");
                                err.setLocation(expr.getLocation());
                                throw err;
                            }
                            Saxon.Hej.om.NodeInfo att = (Saxon.Hej.om.NodeInfo)(item);
                            Saxon.Hej.type.SimpleType attributeType;
                            int properties = Saxon.Hej.@event.ReceiverOption.NONE;
                            if (((expr.constructionMode == Saxon.Hej.lib.Validation.SKIP) || Saxon.Hej.type.Untyped.getInstance().Equals(parent.getSchemaType()))) {
                                attributeType = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                            } else {
                                attributeType = (Saxon.Hej.type.SimpleType)(att.getSchemaType());
                                if ((att.isId() || ((att.getLocalPart().Equals("id") && att.getNamespaceUri().Equals(Saxon.Hej.om.NamespaceUri.XML))))) {
                                    properties |= (Saxon.Hej.@event.ReceiverOption.IS_ID);
                                }
                                if (att.isIdref()) {
                                    properties |= (Saxon.Hej.@event.ReceiverOption.IS_IDREF);
                                }
                            }
                            atts = atts.put(new Saxon.Hej.om.AttributeInfo(Saxon.Hej.om.NameOfNode.makeName(att), attributeType, att.getStringValue(), Saxon.Hej.expr.parser.Loc.NONE, properties));
                        }
                        Saxon.Eej.ee.update.ReplaceAttributeAction action = new Saxon.Eej.ee.update.ReplaceAttributeAction(parent, target, atts, expr.inheritNamespaces);
                        action.setOriginator(expr);
                        ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                    } else {
                        Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/replacementEval(context);
                        for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                            if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE))) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("An attribute node cannot replace a node that is not itself an attribute", "XUTY0010");
                                err.setLocation(expr.getLocation());
                                throw err;
                            } else {
                                content.Add(item);
                            }
                        }
                        if (content.Count==0) {
                            try {
                                Saxon.Eej.ee.update.ReplaceNodeAction action = new Saxon.Eej.ee.update.ReplaceNodeAction(new Saxon.Hej.tree.linked.NodeImpl[]{}, (Saxon.Hej.om.MutableNodeInfo)(target), expr.inheritNamespaces);
                                action.setOriginator(expr);
                                ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                            } catch (Saxon.Hej.trans.XPathException e) {
                                e.maybeSetLocation(expr.getLocation());
                                e.maybeSetContext(context);
                                throw;
                            }
                        } else {
                            Saxon.Hej.@event.Builder builder = ((Saxon.Hej.om.MutableNodeInfo)(parent)).newBuilder();
                            Saxon.Hej.Controller controller = context.getController();
                            builder.setLineNumbering(controller.getConfiguration().isLineNumbering());
                            Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                            pipe.setXPathContext(context);
                            pipe.setHostLanguage(expr.getPackageData().getHostLanguage());
                            builder.setPipelineConfiguration(pipe);
                            Saxon.Hej.@event.Receiver r = builder;
                            if (((expr.constructionMode == Saxon.Hej.lib.Validation.STRIP) || Saxon.Hej.type.Untyped.getInstance().Equals(parent.getSchemaType()))) {
                                r = new Saxon.Eej.ee.validate.SkipValidator(r);
                            }
                            Saxon.Hej.@event.TreeReceiver @out = new Saxon.Hej.@event.TreeReceiver(r);
                            @out.setPipelineConfiguration(pipe);
                            @out.open();
                            @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                            foreach (System.Object aContent in content) {
                                Saxon.Hej.om.Item n = (Saxon.Hej.om.Item)(aContent);
                                @out.append(n, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                            }
                            @out.endDocument();
                            @out.close();
                            Saxon.Hej.om.NodeInfo root = builder.getCurrentRoot();
                            Saxon.Hej.tree.iter.AxisIterator top = root.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                            Saxon.Hej.om.GroundedValue extent = Saxon.Hej.om.SequenceTool.toGroundedValue(top);
                            int len = extent.getLength();
                            Saxon.Hej.om.NodeInfo[] array = new Saxon.Hej.om.NodeInfo[len];
                            for (int i = 0; (i<len); i++) {
                                array[i] = (Saxon.Hej.om.NodeInfo)(extent.itemAt(i));
                            }
                            try {
                                Saxon.Eej.ee.update.ReplaceNodeAction action = new Saxon.Eej.ee.update.ReplaceNodeAction(array, (Saxon.Hej.om.MutableNodeInfo)(target), expr.inheritNamespaces);
                                action.setOriginator(expr);
                                ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                            } catch (Saxon.Hej.trans.XPathException e) {
                                e.maybeSetLocation(expr.getLocation());
                                e.maybeSetContext(context);
                                throw;
                            }
                        }
                    }
                } ;
            }
        
        }
    
    }
}