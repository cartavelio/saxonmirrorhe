using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class ContainsTokenPatternOptimizer : Saxon.Eej.ee.trans.PatternOptimizer {
        private static readonly Saxon.Eej.ee.trans.ContainsTokenPatternOptimizer THE_INSTANCE = new Saxon.Eej.ee.trans.ContainsTokenPatternOptimizer();
        public static Saxon.Eej.ee.trans.ContainsTokenPatternOptimizer getInstance() {
            return THE_INSTANCE;
        }
        public override bool appliesTo(Saxon.Hej.pattern.Pattern pattern) {
            if ((pattern is Saxon.Hej.pattern.PatternWithPredicate)) {
                Saxon.Hej.expr.Expression exp = ((Saxon.Hej.pattern.PatternWithPredicate)(pattern)).getPredicate();
                if (exp.isCallOn(typeof(Saxon.Hej.functions.Contains))) {
                    Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(exp);
                    if ((((call.getArity() == 2) && (call.getArg(1) is Saxon.Hej.expr.StringLiteral)) && exp.getRetainedStaticContext().getDefaultCollationName().Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI))) {
                        string contained = ((Saxon.Hej.expr.StringLiteral)(call.getArg(1))).stringify();
                        return System.Text.RegularExpressions.Regex.IsMatch(contained, " [^ ]+ ");
                    }
                }
            }
            return false;
        }
        public override void process(Saxon.Eej.ee.trans.RuleEE rule, Saxon.Hej.pattern.Pattern pattern, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            Saxon.Hej.expr.Expression exp = ((Saxon.Hej.pattern.BasePatternWithPredicate)(pattern)).getPredicate();
            Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(exp);
            Saxon.Hej.str.UnicodeString contained = ((Saxon.Hej.expr.StringLiteral)(call.getArg(1))).getString();
            Saxon.Hej.str.UnicodeString token = Saxon.Hej.value.Whitespace.trim(contained);
            Saxon.Eej.ee.trans.TokenMatchOperand chic = new Saxon.Eej.ee.trans.TokenMatchOperand(call.getArg(0));
            chic = (Saxon.Eej.ee.trans.TokenMatchOperand)(allocateCharacteristic(characteristics, chic));
            Saxon.Hej.lib.StringCollator collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            Saxon.Hej.expr.sort.AtomicMatchKey key = collator.getCollationKey(token);
            Saxon.Eej.ee.trans.Precondition.StringHashPrecondition p = new Saxon.Eej.ee.trans.Precondition.StringHashPrecondition(chic, key, collator);
            addPrecondition(rule, preconditions, p);
        }
    
    }
}