using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class ApplyNextMatchingTemplate : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ITemplateCall {
        private Saxon.Hej.expr.instruct.WithParam[] actualParams;
        private Saxon.Hej.expr.instruct.WithParam[] tunnelParams;
        public ApplyNextMatchingTemplate()  {
        }
        public override int getImplementationMethod() {
            return (base.getImplementationMethod()|Saxon.Hej.expr.Expression.WATCH_METHOD);
        }
        public Saxon.Hej.expr.instruct.WithParam[] getActualParams() {
            return actualParams;
        }
        public Saxon.Hej.expr.instruct.WithParam[] getTunnelParams() {
            return tunnelParams;
        }
        public void setActualParams(Saxon.Hej.expr.instruct.WithParam[] @params) {
            this.actualParams = @params;
        }
        public void setTunnelParams(Saxon.Hej.expr.instruct.WithParam[] @params) {
            this.tunnelParams = @params;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operanda = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>((actualParams.Length+tunnelParams.Length));
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, actualParams, operanda);
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, tunnelParams, operanda);
            return operanda;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.instruct.WithParam.simplify(getActualParams());
            Saxon.Hej.expr.instruct.WithParam.simplify(getTunnelParams());
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.instruct.WithParam.typeCheck(actualParams, visitor, contextInfo);
            Saxon.Hej.expr.instruct.WithParam.typeCheck(tunnelParams, visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.instruct.WithParam.optimize(visitor, actualParams, contextInfo);
            Saxon.Hej.expr.instruct.WithParam.optimize(visitor, tunnelParams, contextInfo);
            return this;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM;
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if ((pathMapNodeSet == null)) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                pathMapNodeSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(cie));
            }
            pathMapNodeSet.addDescendants();
            return new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
        }
    
    }
}