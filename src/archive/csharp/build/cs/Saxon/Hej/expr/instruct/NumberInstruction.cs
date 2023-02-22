using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class NumberInstruction : Saxon.Hej.expr.Expression {
        public const int SINGLE = 0;
        public const int MULTI = 1;
        public const int ANY = 2;
        public const int SIMPLE = 3;
        public static readonly string[] LEVEL_NAMES = new string[]{"single", "multi", "any", "simple"};
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly int level;
        private Saxon.Hej.expr.Operand countOp;
        private Saxon.Hej.expr.Operand fromOp;
        private bool hasVariablesInPatterns = false;
        public NumberInstruction(Saxon.Hej.expr.Expression select, int level, Saxon.Hej.pattern.Pattern count, Saxon.Hej.pattern.Pattern from)  {
            System.Diagnostics.Debug.Assert((select != null));
            selectOp = new Saxon.Hej.expr.Operand(this, select, new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.SINGLE_NODE));
            this.level = level;
            if ((count != null)) {
                countOp = new Saxon.Hej.expr.Operand(this, count, Saxon.Hej.expr.OperandRole.INSPECT);
            }
            if ((from != null)) {
                fromOp = new Saxon.Hej.expr.Operand(this, from, Saxon.Hej.expr.OperandRole.INSPECT);
            }
            this.hasVariablesInPatterns = (Saxon.Hej.pattern.Pattern.patternContainsVariable(count) || Saxon.Hej.pattern.Pattern.patternContainsVariable(from));
        }
        public override bool isInstruction() {
            return true;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(selectOp, countOp, fromOp);
        }
        public int getLevel() {
            return level;
        }
        public Saxon.Hej.pattern.Pattern getCount() {
            return ((countOp == null) ? null : (Saxon.Hej.pattern.Pattern)(countOp.getChildExpression()));
        }
        public Saxon.Hej.pattern.Pattern getFrom() {
            return ((fromOp == null) ? null : (Saxon.Hej.pattern.Pattern)(fromOp.getChildExpression()));
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.NumberInstruction exp = new Saxon.Hej.expr.instruct.NumberInstruction(copy(selectOp, rebindings), level, copy(getCount(), rebindings), copy(getFrom(), rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        private Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.Operand op, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return ((op == null) ? null : op.getChildExpression().copy(rebindings));
        }
        private Saxon.Hej.pattern.Pattern copy(Saxon.Hej.pattern.Pattern op, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return ((op == null) ? null : op.copy(rebindings));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
        }
        protected internal override int computeCardinality() {
            switch (level) {
              case SIMPLE:
              case SINGLE:
              case ANY:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
              case MULTI:
              default:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            }
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            if ("EE".Equals(getPackageData().getTargetEdition())) {
                e = visitor.obtainOptimizer().optimizeNumberInstruction(this, contextInfo);
                if ((e != null)) {
                    return e;
                }
            }
            return this;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> vec = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(1);
            Saxon.Hej.om.NodeInfo source = (Saxon.Hej.om.NodeInfo)(selectOp.getChildExpression().evaluateItem(context));
            switch (level) {
              case SIMPLE:
                {
                    long value = Saxon.Hej.tree.util.Navigator.getNumberSimple(source, context);
                    if ((value != 0)) {
                        vec.Add(Saxon.Hej.value.Int64Value.makeIntegerValue(value));
                    }
                    break ;
                }
              case SINGLE:
                {
                    long value = Saxon.Hej.tree.util.Navigator.getNumberSingle(source, getCount(), getFrom(), context);
                    if ((value != 0)) {
                        vec.Add(Saxon.Hej.value.Int64Value.makeIntegerValue(value));
                    }
                    break ;
                }
              case ANY:
                {
                    long value = Saxon.Hej.tree.util.Navigator.getNumberAny(this, source, getCount(), getFrom(), context, hasVariablesInPatterns);
                    if ((value != 0)) {
                        vec.Add(Saxon.Hej.value.Int64Value.makeIntegerValue(value));
                    }
                    break ;
                }
              case MULTI:
                {
                    foreach (long n in Saxon.Hej.tree.util.Navigator.getNumberMulti(source, getCount(), getFrom(), context)) {
                        vec.Add(Saxon.Hej.value.Int64Value.makeIntegerValue(n));
                    }
                    break ;
                }
            }
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.value.AtomicValue>(vec);
        }
        public override string getExpressionName() {
            return "xsl:number";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("nodeNum", this);
            @out.emitAttribute("level", LEVEL_NAMES[level]);
            @out.setChildRole("select");
            selectOp.getChildExpression().export(@out);
            if ((countOp != null)) {
                @out.setChildRole("count");
                getCount().export(@out);
            }
            if ((fromOp != null)) {
                @out.setChildRole("from");
                getFrom().export(@out);
            }
            @out.endElement();
        }
    
    }
}