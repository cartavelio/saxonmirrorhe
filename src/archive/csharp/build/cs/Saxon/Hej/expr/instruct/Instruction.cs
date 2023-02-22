using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class Instruction : Saxon.Hej.expr.Expression, Saxon.Hej.expr.instruct.TailCallReturner {
        public Instruction()  {
        }
        public override int getImplementationMethod() {
            return Saxon.Hej.expr.Expression.PROCESS_METHOD;
        }
        public override bool isInstruction() {
            return true;
        }
        public virtual int getInstructionNameCode() {
            return -1;
        }
        public override string getExpressionName() {
            int code = getInstructionNameCode();
            if (((code>=0)&(code<1024))) {
                return Saxon.Hej.om.StandardNames.getDisplayName(code);
            } else {
                return base.getExpressionName();
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.Type.ITEM_TYPE;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public abstract override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands();
        public abstract Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context);
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.expr.instruct.TailCall tc = processLeavingTail(output, context);
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetFailingExpression(this);
                err.maybeSetContext(context);
                throw;
            }
        }
        public Saxon.Ejavax.xml.transform.SourceLocator getSourceLocator() {
            return getLocation();
        }
        protected internal static Saxon.Hej.trans.XPathException dynamicError(Saxon.Hej.s9api.Location loc, Saxon.Hej.trans.XPathException error, Saxon.Hej.expr.XPathContext context) {
            if ((error is Saxon.Hej.expr.instruct.TerminationException)) {
                return error;
            }
            error.maybeSetLocation(loc);
            error.maybeSetContext(context);
            return error;
        }
        public static Saxon.Hej.expr.instruct.ParameterSet assembleParams(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.instruct.WithParam[] actualParams) {
            if (((actualParams == null) || (actualParams.Length == 0))) {
                return null;
            }
            Saxon.Hej.expr.instruct.ParameterSet @params = new Saxon.Hej.expr.instruct.ParameterSet(actualParams.Length);
            foreach (Saxon.Hej.expr.instruct.WithParam actualParam in actualParams) {
                @params.put(actualParam.getVariableQName(), actualParam.getSelectValue(context), actualParam.isTypeChecked());
            }
            return @params;
        }
        public static Saxon.Hej.expr.instruct.ParameterSet assembleTunnelParams(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.instruct.WithParam[] actualParams) {
            Saxon.Hej.expr.instruct.ParameterSet existingParams = context.getTunnelParameters();
            if ((existingParams == null)) {
                return assembleParams(context, actualParams);
            }
            if (((actualParams == null) || (actualParams.Length == 0))) {
                return existingParams;
            }
            Saxon.Hej.expr.instruct.ParameterSet newParams = new Saxon.Hej.expr.instruct.ParameterSet(existingParams, actualParams.Length);
            foreach (Saxon.Hej.expr.instruct.WithParam actualParam in actualParams) {
                newParams.put(actualParam.getVariableQName(), actualParam.getSelectValue(context), false);
            }
            return newParams;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            if (alwaysCreatesNewNodes()) {
                p |= Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED;
            }
            if (mayCreateNewNodes()) {
                return p;
            } else {
                return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            }
        }
        public override int getNetCost() {
            return 20;
        }
        public virtual bool mayCreateNewNodes() {
            return false;
        }
        public virtual bool alwaysCreatesNewNodes() {
            return false;
        }
        protected internal bool someOperandCreatesNewNodes() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                int props = child.getSpecialProperties();
                if ((((props&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0)) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            int m = getImplementationMethod();
            if ((((m&EVALUATE_METHOD)) != 0)) {
                throw new Saxon.Ejava.lang.AssertionError(("evaluateItem() is not implemented in the subclass "+this.GetType()));
            } else {
                if ((((m&ITERATE_METHOD)) != 0)) {
                    return iterate(context).next();
                } else {
                    return Saxon.Hej.expr.parser.ExpressionTool.getItemFromProcessMethod(this, context);
                }
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            int m = getImplementationMethod();
            if ((((m&EVALUATE_METHOD)) != 0)) {
                Saxon.Hej.om.Item item = evaluateItem(context);
                if ((item == null)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                } else {
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(item);
                }
            } else {
                if ((((m&ITERATE_METHOD)) != 0)) {
                    throw new Saxon.Ejava.lang.AssertionError(("iterate() is not implemented in the subclass "+this.GetType()));
                } else {
                    return Saxon.Hej.expr.parser.ExpressionTool.getIteratorFromProcessMethod(this, context);
                }
            }
        }
        public override Saxon.Hej.str.UnicodeString evaluateAsString(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = evaluateItem(context);
            if ((item == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                return item.getUnicodeStringValue();
            }
        }
        public bool isXSLT() {
            return getPackageData().isXSLT();
        }
    
    }
}