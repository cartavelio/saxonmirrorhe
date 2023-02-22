using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class AncestorPatternOptimizer : Saxon.Eej.ee.trans.PatternOptimizer {
        private static readonly Saxon.Eej.ee.trans.AncestorPatternOptimizer THE_INSTANCE = new Saxon.Eej.ee.trans.AncestorPatternOptimizer();
        public static Saxon.Eej.ee.trans.AncestorPatternOptimizer getInstance() {
            return THE_INSTANCE;
        }
        public override bool appliesTo(Saxon.Hej.pattern.Pattern pattern) {
            return (pattern is Saxon.Hej.pattern.AncestorQualifiedPattern);
        }
        public override void process(Saxon.Eej.ee.trans.RuleEE rule, Saxon.Hej.pattern.Pattern pattern, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            Saxon.Hej.pattern.Pattern anc = ((Saxon.Hej.pattern.AncestorQualifiedPattern)(pattern)).getUpperPattern();
            bool isParent = (((Saxon.Hej.pattern.AncestorQualifiedPattern)(pattern)).getUpwardsAxis() == Saxon.Hej.om.AxisInfo.PARENT);
            Saxon.Eej.ee.trans.AncestorCharacteristic chic = new Saxon.Eej.ee.trans.AncestorCharacteristic(anc, isParent);
            Saxon.Eej.ee.trans.AncestorCharacteristic chic2 = (Saxon.Eej.ee.trans.AncestorCharacteristic)(allocateCharacteristic(characteristics, chic));
            Saxon.Eej.ee.trans.Precondition.BooleanPrecondition p = new Saxon.Eej.ee.trans.Precondition.BooleanPrecondition(chic2);
            addPrecondition(rule, preconditions, p);
            Saxon.Hej.pattern.Pattern @base = ((Saxon.Hej.pattern.AncestorQualifiedPattern)(pattern)).getBasePattern();
            /*134*/Saxon.Eej.ee.trans.GeneralPatternOptimizer.getInstance().process(rule, @base, preconditions, characteristics);
        }
    
    }
}