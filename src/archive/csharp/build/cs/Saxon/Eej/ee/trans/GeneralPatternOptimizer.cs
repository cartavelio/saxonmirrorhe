using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class GeneralPatternOptimizer : Saxon.Eej.ee.trans.PatternOptimizer {

        static GeneralPatternOptimizer() {
            patternOptimizers.Add(/*134*/Saxon.Eej.ee.trans.EqualityPatternOptimizer.getInstance());
            patternOptimizers.Add(/*134*/Saxon.Eej.ee.trans.ContainsTokenPatternOptimizer.getInstance());
            patternOptimizers.Add(/*134*/Saxon.Eej.ee.trans.IntEqualityPatternOptimizer.getInstance());
            patternOptimizers.Add(/*134*/Saxon.Eej.ee.trans.AncestorPatternOptimizer.getInstance());
        }
        private static readonly System.Collections.Generic.IList<Saxon.Eej.ee.trans.PatternOptimizer> patternOptimizers = new System.Collections.Generic.List<Saxon.Eej.ee.trans.PatternOptimizer>();
        private static readonly Saxon.Eej.ee.trans.GeneralPatternOptimizer THE_INSTANCE = new Saxon.Eej.ee.trans.GeneralPatternOptimizer();
        public static Saxon.Eej.ee.trans.GeneralPatternOptimizer getInstance() {
            return THE_INSTANCE;
        }
        public override bool appliesTo(Saxon.Hej.pattern.Pattern pattern) {
            return true;
        }
        public override void process(Saxon.Eej.ee.trans.RuleEE rule, Saxon.Hej.pattern.Pattern pattern, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics) {
            foreach (Saxon.Eej.ee.trans.PatternOptimizer optimizer in patternOptimizers) {
                if (optimizer.appliesTo(pattern)) {
                    optimizer.process(rule, pattern, preconditions, characteristics);
                    break ;
                }
            }
        }
    
    }
}