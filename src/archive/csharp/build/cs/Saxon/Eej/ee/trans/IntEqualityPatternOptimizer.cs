using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class IntEqualityPatternOptimizer : Saxon.Eej.ee.trans.PatternOptimizer {
        private static readonly Saxon.Eej.ee.trans.IntEqualityPatternOptimizer THE_INSTANCE = new Saxon.Eej.ee.trans.IntEqualityPatternOptimizer();
        public static Saxon.Eej.ee.trans.IntEqualityPatternOptimizer getInstance() {
            return THE_INSTANCE;
        }
        public override bool appliesTo(Saxon.Hej.pattern.Pattern pattern) {
            if ((pattern is Saxon.Hej.pattern.PatternWithPredicate)) {
                Saxon.Hej.expr.Expression exp = ((Saxon.Hej.pattern.PatternWithPredicate)(pattern)).getPredicate();
                return (exp is Saxon.Hej.expr.CompareToIntegerConstant);
            }
            return false;
        }
        public override void process(Saxon.Eej.ee.trans.RuleEE rule, Saxon.Hej.pattern.Pattern pattern, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            Saxon.Hej.expr.Expression exp = ((Saxon.Hej.pattern.PatternWithPredicate)(pattern)).getPredicate();
            Saxon.Hej.expr.Expression lhs = ((Saxon.Hej.expr.CompareToIntegerConstant)(exp)).getLhsExpression();
            Saxon.Eej.ee.trans.NumericComparisonOperand chic = new Saxon.Eej.ee.trans.NumericComparisonOperand(lhs);
            chic = (Saxon.Eej.ee.trans.NumericComparisonOperand)(allocateCharacteristic(characteristics, chic));
            long key = ((Saxon.Hej.expr.CompareToIntegerConstant)(exp)).getComparand();
            Saxon.Eej.ee.trans.Precondition.NumericComparisonPrecondition p = new Saxon.Eej.ee.trans.Precondition.NumericComparisonPrecondition(chic, key);
            addPrecondition(rule, preconditions, p);
            if ((pattern is Saxon.Hej.pattern.BasePatternWithPredicate)) {
                Saxon.Hej.pattern.Pattern @base = ((Saxon.Hej.pattern.BasePatternWithPredicate)(pattern)).getBasePattern();
                /*134*/Saxon.Eej.ee.trans.GeneralPatternOptimizer.getInstance().process(rule, @base, preconditions, characteristics);
            }
        }
    
    }
}