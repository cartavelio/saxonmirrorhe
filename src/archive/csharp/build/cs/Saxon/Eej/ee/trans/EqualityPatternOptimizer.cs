using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class EqualityPatternOptimizer : Saxon.Eej.ee.trans.PatternOptimizer {
        private static readonly Saxon.Eej.ee.trans.EqualityPatternOptimizer THE_INSTANCE = new Saxon.Eej.ee.trans.EqualityPatternOptimizer();
        public static Saxon.Eej.ee.trans.EqualityPatternOptimizer getInstance() {
            return THE_INSTANCE;
        }
        public override bool appliesTo(Saxon.Hej.pattern.Pattern pattern) {
            if ((pattern is Saxon.Hej.pattern.PatternWithPredicate)) {
                Saxon.Hej.expr.Expression exp = ((Saxon.Hej.pattern.PatternWithPredicate)(pattern)).getPredicate();
                if ((exp is Saxon.Hej.expr.ComparisonExpression)) {
                    if ((((exp is Saxon.Hej.expr.GeneralComparison) && (((Saxon.Hej.expr.ComparisonExpression)(exp)).getRhsExpression() is Saxon.Hej.expr.StringLiteral)) && (((Saxon.Hej.expr.GeneralComparison)(exp)).getSingletonOperator() == Saxon.Hej.expr.parser.Token.FEQ))) {
                        return true;
                    }
                    if ((((exp is Saxon.Hej.expr.ValueComparison) && (((Saxon.Hej.expr.ComparisonExpression)(exp)).getRhsExpression() is Saxon.Hej.expr.StringLiteral)) && (((Saxon.Hej.expr.ValueComparison)(exp)).getSingletonOperator() == Saxon.Hej.expr.parser.Token.FEQ))) {
                        return true;
                    }
                }
            }
            return false;
        }
        public override void process(Saxon.Eej.ee.trans.RuleEE rule, Saxon.Hej.pattern.Pattern pattern, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            Saxon.Hej.expr.Expression exp = ((Saxon.Hej.pattern.PatternWithPredicate)(pattern)).getPredicate();
            Saxon.Hej.expr.Expression lhs = ((Saxon.Hej.expr.ComparisonExpression)(exp)).getLhsExpression();
            Saxon.Hej.lib.StringCollator collator = ((Saxon.Hej.expr.ComparisonExpression)(exp)).getAtomicComparer().getCollator();
            Saxon.Eej.ee.trans.GeneralComparisonOperand chic = new Saxon.Eej.ee.trans.GeneralComparisonOperand(collator, lhs);
            chic = (Saxon.Eej.ee.trans.GeneralComparisonOperand)(allocateCharacteristic(characteristics, chic));
            Saxon.Hej.expr.sort.AtomicMatchKey key = collator.getCollationKey(((Saxon.Hej.expr.StringLiteral)(((Saxon.Hej.expr.ComparisonExpression)(exp)).getRhsExpression())).getString());
            Saxon.Eej.ee.trans.Precondition.StringHashPrecondition p = new Saxon.Eej.ee.trans.Precondition.StringHashPrecondition(chic, key, collator);
            addPrecondition(rule, preconditions, p);
            if ((pattern is Saxon.Hej.pattern.BasePatternWithPredicate)) {
                Saxon.Hej.pattern.Pattern @base = ((Saxon.Hej.pattern.BasePatternWithPredicate)(pattern)).getBasePattern();
                /*134*/Saxon.Eej.ee.trans.GeneralPatternOptimizer.getInstance().process(rule, @base, preconditions, characteristics);
            }
        }
    
    }
}