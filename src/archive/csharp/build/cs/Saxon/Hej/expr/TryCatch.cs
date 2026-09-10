using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class TryCatch : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand tryOp;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.TryCatch.CatchClause> catchClauses = new System.Collections.Generic.List<Saxon.Hej.expr.TryCatch.CatchClause>();
        private bool rollbackOutput;
        public TryCatch(Saxon.Hej.expr.Expression tryExpr)  {
            this.tryOp = new Saxon.Hej.expr.Operand(this, tryExpr, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
        }
        public void addCatchExpression(Saxon.Hej.pattern.QNameTest test, Saxon.Hej.expr.Expression catchExpr) {
            Saxon.Hej.expr.TryCatch.CatchClause clause = new Saxon.Hej.expr.TryCatch.CatchClause();
            clause.catchOp = new Saxon.Hej.expr.Operand(this, catchExpr, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            clause.nameTest = test;
            catchClauses.Add(clause);
        }
        public void setRollbackOutput(bool rollback) {
            this.rollbackOutput = rollback;
        }
        public bool isRollbackOutput() {
            return this.rollbackOutput;
        }
        public Saxon.Hej.expr.Operand getTryOperand() {
            return tryOp;
        }
        public Saxon.Hej.expr.Expression getTryExpr() {
            return tryOp.getChildExpression();
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.TryCatch.CatchClause> getCatchClauses() {
            return catchClauses;
        }
        public override bool isInstruction() {
            return true;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        protected internal override int computeCardinality() {
            int card = getTryExpr().getCardinality();
            foreach (Saxon.Hej.expr.TryCatch.CatchClause catchClause in catchClauses) {
                card = Saxon.Hej.value.Cardinality.union(card, catchClause.catchOp.getChildExpression().getCardinality());
            }
            return card;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType type = getTryExpr().getItemType();
            foreach (Saxon.Hej.expr.TryCatch.CatchClause catchClause in catchClauses) {
                type = Saxon.Hej.type.Type.getCommonSuperType(type, catchClause.catchOp.getChildExpression().getItemType());
            }
            return type;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            list.Add(tryOp);
            foreach (Saxon.Hej.expr.TryCatch.CatchClause cc in catchClauses) {
                list.Add(cc.catchOp);
            }
            return list;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            optimizeChildren(visitor, contextInfo);
            Saxon.Hej.expr.Expression e = getParentExpression();
            while ((e != null)) {
                if (((e is Saxon.Hej.expr.LetExpression) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(getTryExpr(), new Saxon.Hej.expr.Binding[]{(Saxon.Hej.expr.LetExpression)(e)}))) {
                    ((Saxon.Hej.expr.LetExpression)(e)).setNeedsEagerEvaluation(true);
                }
                e = e.getParentExpression();
            }
            return this;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.TryCatch) && ((Saxon.Hej.expr.TryCatch)(other)).tryOp.getChildExpression().isEqual(tryOp.getChildExpression())) && ((Saxon.Hej.expr.TryCatch)(other)).catchClauses.Equals(catchClauses));
        }
        protected internal override int computeHashCode() {
            int h = 0x636b12a0;
            for (int i = 0; (i<catchClauses.Count); i++) {
                h ^= (catchClauses[i].GetHashCode()<<i);
            }
            return (h+tryOp.getChildExpression().GetHashCode());
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.TryCatch t2 = new Saxon.Hej.expr.TryCatch(tryOp.getChildExpression().copy(rebindings));
            foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in catchClauses) {
                t2.addCatchExpression(clause.nameTest, clause.catchOp.getChildExpression().copy(rebindings));
            }
            t2.setRollbackOutput(rollbackOutput);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, t2);
            return t2;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.expr.XPathContext c1 = c.newMinorContext();
            try {
                try {
                    return Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(tryOp.getChildExpression(), c1).head();
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.isGlobalError()) {
                    err.setIsGlobalError(false);
                } else {
                    Saxon.Hej.om.StructuredQName code = err.getErrorCodeQName();
                    if ((code == null)) {
                        code = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "XXXX9999");
                    }
                    foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in catchClauses) {
                        if (clause.nameTest.matches(code)) {
                            Saxon.Hej.expr.Expression caught = clause.catchOp.getChildExpression();
                            Saxon.Hej.expr.XPathContextMajor c2 = c.newContext();
                            c2.setCurrentException(err);
                            return caught.evaluateItem(c2);
                        }
                    }
                }
                err.setHasBeenReported(false);
                throw;
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.expr.XPathContextMajor c1 = c.newContext();
            c1.createThreadManager();
            c1.setErrorReporter(new Saxon.Hej.expr.TryCatch.FilteringErrorReporter(c.getErrorReporter(), catchClauses));
            try {
                Saxon.Hej.om.Sequence v = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(tryOp.getChildExpression(), c1);
                c1.waitForChildThreads();
                Saxon.Hej.expr.TailCallLoop.TailCallInfo tci = c1.getTailCallInfo();
                if ((tci is Saxon.Hej.expr.instruct.BreakInstr)) {
                    ((Saxon.Hej.expr.instruct.BreakInstr)(tci)).markContext(c);
                }
                return v.iterate();
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.isGlobalError()) {
                    err.setIsGlobalError(false);
                } else {
                    Saxon.Hej.om.StructuredQName code = err.getErrorCodeQName();
                    if ((code == null)) {
                        code = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "XXXX9999");
                    }
                    foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in catchClauses) {
                        if (clause.nameTest.matches(code)) {
                            Saxon.Hej.expr.Expression caught = clause.catchOp.getChildExpression();
                            Saxon.Hej.expr.XPathContextMajor c2 = c.newContext();
                            c2.setCurrentException(err);
                            Saxon.Hej.om.Sequence v = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(caught, c2);
                            Saxon.Hej.expr.TailCallLoop.TailCallInfo tci = c2.getTailCallInfo();
                            if ((tci is Saxon.Hej.expr.instruct.BreakInstr)) {
                                ((Saxon.Hej.expr.instruct.BreakInstr)(tci)).markContext(c);
                            }
                            return v.iterate();
                        }
                    }
                }
                err.setHasBeenReported(false);
                throw;
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.@event.PipelineConfiguration pipe = output.getPipelineConfiguration();
            Saxon.Hej.expr.XPathContextMajor c1 = context.newContext();
            c1.createThreadManager();
            c1.setErrorReporter(new Saxon.Hej.expr.TryCatch.FilteringErrorReporter(context.getErrorReporter(), catchClauses));
            Saxon.Hej.@event.Outputter o2;
            if (rollbackOutput) {
                o2 = new Saxon.Hej.@event.OutputterEventBuffer();
                o2.setPipelineConfiguration(pipe);
            } else {
                o2 = new Saxon.Hej.@event.EventMonitor(output);
                o2.setPipelineConfiguration(pipe);
            }
            try {
                tryOp.getChildExpression().process(o2, c1);
                c1.waitForChildThreads();
                Saxon.Hej.expr.TailCallLoop.TailCallInfo tci = c1.getTailCallInfo();
                if ((tci is Saxon.Hej.expr.instruct.BreakInstr)) {
                    ((Saxon.Hej.expr.instruct.BreakInstr)(tci)).markContext(context);
                }
                if (rollbackOutput) {
                    ((Saxon.Hej.@event.OutputterEventBuffer)(o2)).replay(output);
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.isGlobalError()) {
                    err.setIsGlobalError(false);
                } else {
                    Saxon.Hej.om.StructuredQName code = err.getErrorCodeQName();
                    if ((code == null)) {
                        code = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "XXXX9999");
                    }
                    foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in catchClauses) {
                        if (clause.nameTest.matches(code)) {
                            if (((o2 is Saxon.Hej.@event.EventMonitor) && ((Saxon.Hej.@event.EventMonitor)(o2)).hasBeenWrittenTo())) {
                                string message = (err.Message+". The error could not be caught, because rollback-output=no was specified, and output was already written to the result tree");
                                Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(message, "XTDE3530");
                                xe.setLocation(err.getLocator());
                                xe.setXPathContext(context);
                                throw xe;
                            }
                            Saxon.Hej.expr.Expression caught = clause.catchOp.getChildExpression();
                            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                            c2.setCurrentException(err);
                            Saxon.Hej.om.Sequence v = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(caught, c2);
                            Saxon.Hej.expr.TailCallLoop.TailCallInfo tci = c2.getTailCallInfo();
                            if ((tci is Saxon.Hej.expr.instruct.BreakInstr)) {
                                ((Saxon.Hej.expr.instruct.BreakInstr)(tci)).markContext(context);
                            }
                            Saxon.Hej.om.SequenceTool.supply(((Saxon.Hej.om.GroundedValue)(v)).iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => output.append(item))));
                            return ;
                        }
                    }
                }
                err.setHasBeenReported(false);
                throw;
            }
        }
        public override string getExpressionName() {
            return "tryCatch";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("try", this);
            if (rollbackOutput) {
                @out.emitAttribute("flags", "r");
            }
            tryOp.getChildExpression().export(@out);
            foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in catchClauses) {
                @out.startElement("catch");
                @out.emitAttribute("errors", clause.nameTest.exportQNameTest());
                clause.catchOp.getChildExpression().export(@out);
                @out.endElement();
            }
            @out.endElement();
        }
        public class CatchClause {
            public int slotNumber = -1;
            public Saxon.Hej.expr.Operand catchOp;
            public Saxon.Hej.pattern.QNameTest nameTest;
        
        }
        private class FilteringErrorReporter : Saxon.Hej.lib.ErrorReporter {
            private readonly Saxon.Hej.lib.ErrorReporter @base;
            private readonly System.Collections.Generic.IList<Saxon.Hej.expr.TryCatch.CatchClause> catchClauses;
            internal FilteringErrorReporter(Saxon.Hej.lib.ErrorReporter @base, System.Collections.Generic.IList<Saxon.Hej.expr.TryCatch.CatchClause> catchClauses)  {
                this.@base = @base;
                this.catchClauses = catchClauses;
            }
            private bool isCaught(Saxon.Hej.s9api.XmlProcessingError err) {
                Saxon.Hej.om.StructuredQName code = err.getErrorCode().GetStructuredQName();
                foreach (Saxon.Hej.expr.TryCatch.CatchClause clause in catchClauses) {
                    if (clause.nameTest.matches(code)) {
                        return true;
                    }
                }
                return false;
            }
            public void report(Saxon.Hej.s9api.XmlProcessingError error) {
                if ((error.isWarning() || !(isCaught(error)))) {
                    @base.report(error);
                }
            }
        
        }
        public override string getStreamerName() {
            return "TryCatch";
        }
    
    }
}