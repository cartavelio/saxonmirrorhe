using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class ReplaceValueExpression : Saxon.Eej.ee.update.BasicUpdatingExpression {
        private readonly Saxon.Hej.expr.Operand targetOp;
        private readonly Saxon.Hej.expr.Operand replacementOp;
        public ReplaceValueExpression(Saxon.Hej.expr.Expression target, Saxon.Hej.expr.Expression replacement)  {
            targetOp = new Saxon.Hej.expr.Operand(this, target, Saxon.Hej.expr.OperandRole.INSPECT);
            replacementOp = new Saxon.Hej.expr.Operand(this, replacement, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
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
            Saxon.Hej.expr.Expression result1 = getReplacementExp();
            setReplacementExp(Saxon.Hej.query.XQueryParser.stringify(result1, true, visitor.getStaticContext()));
            Saxon.Hej.expr.Expression result = getReplacementExp();
            setReplacementExp(new Saxon.Hej.expr.instruct.ValueOf(result, false, true));
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "replace", 0);
            role0.setErrorCode("XUDY0027");
            setTargetExp(tc.staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.NON_EMPTY_SEQUENCE, role0, visitor));
            role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "replace", 0);
            role0.setErrorCode("XUTY0008");
            setTargetExp(tc.staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.OPTIONAL_NODE, role0, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression result = getReplacementExp();
            Saxon.Eej.ee.update.ReplaceValueExpression exp = new Saxon.Eej.ee.update.ReplaceValueExpression(getTargetExp().copy(rebindings), result.copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("replaceValue", this);
            getTargetExp().export(@out);
            Saxon.Hej.expr.Expression result = getReplacementExp();
            result.export(@out);
            @out.endElement();
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            Saxon.Hej.expr.Expression result = getReplacementExp();
            Saxon.Hej.om.Item newText = result.evaluateItem(context);
            string newString = ((newText == null) ? "" : newText.getStringValue());
            Saxon.Hej.om.NodeInfo targetNode = (Saxon.Hej.om.NodeInfo)(getTargetExp().evaluateItem(context));
            Saxon.Hej.trans.XPathException err = null;
            switch (targetNode.getNodeKind()) {
              case Saxon.Hej.type.Type.COMMENT:
                if (newString.Contains("--")) {
                    err = new Saxon.Hej.trans.XPathException("Replacement comment value cannot contain two adjacent hyphens", "XQDY0072");
                } else {
                    if (newString.EndsWith("-")) {
                        err = new Saxon.Hej.trans.XPathException("Replacement comment value cannot end with a hyphen", "XQDY0072");
                    }
                }
                break ;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                if (newString.Contains("?>")) {
                    err = new Saxon.Hej.trans.XPathException("Replacement processing instruction value cannot contain '?>'", "XQDY0026");
                }
                break ;
              case Saxon.Hej.type.Type.DOCUMENT:
                err = new Saxon.Hej.trans.XPathException("Cannot replace the value of a document node", "XUTY0008");
                break ;
              case Saxon.Hej.type.Type.NAMESPACE:
                err = new Saxon.Hej.trans.XPathException("Cannot replace the value of a namespace node", "XUTY0008");
                break ;
              default:
                break ;
            }
            if ((err != null)) {
                err.setLocation(getLocation());
                err.setXPathContext(context);
                throw err;
            }
            if (!(((targetNode is Saxon.Hej.om.MutableNodeInfo)))) {
                Saxon.Hej.trans.XPathException er = new Saxon.Hej.trans.XPathException("Target node of replace expression is not updateable", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
                er.setLocation(getLocation());
                throw er;
            }
            try {
                Saxon.Eej.ee.update.ReplaceValueAction action = new Saxon.Eej.ee.update.ReplaceValueAction(Saxon.Hej.str.StringView.tidy(newString), (Saxon.Hej.om.MutableNodeInfo)(targetNode));
                action.setOriginator(this);
                ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw;
            }
        }
        public override string getExpressionName() {
            return "replaceValue";
        }
    
    }
}