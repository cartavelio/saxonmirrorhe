using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public abstract class PatternOptimizer {
        public abstract bool appliesTo(Saxon.Hej.pattern.Pattern pattern);
        public abstract void process(Saxon.Eej.ee.trans.RuleEE rule, Saxon.Hej.pattern.Pattern pattern, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics);
        protected internal Saxon.Eej.ee.trans.Characteristic allocateCharacteristic(System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Characteristic,Saxon.Eej.ee.trans.Characteristic> characteristics, Saxon.Eej.ee.trans.Characteristic newChic) {
            Saxon.Eej.ee.trans.Characteristic chic = Saxon.Impl.Helpers.MapUtils.Get(characteristics, newChic);
            if ((chic == null)) {
                Saxon.Impl.Helpers.MapUtils.Put(characteristics, newChic, newChic);
                return newChic;
            } else {
                return chic;
            }
        }
        protected internal void addPrecondition(Saxon.Eej.ee.trans.RuleEE rule, System.Collections.Generic.IDictionary<Saxon.Eej.ee.trans.Precondition,System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE>> preconditions, Saxon.Eej.ee.trans.Precondition p) {
            if (preconditions.ContainsKey(p)) {
                Saxon.Impl.Helpers.MapUtils.Get(preconditions, p).Add(rule);
            } else {
                System.Collections.Generic.IList<Saxon.Eej.ee.trans.RuleEE> applicableRules = new System.Collections.Generic.List<Saxon.Eej.ee.trans.RuleEE>();
                applicableRules.Add(rule);
                Saxon.Impl.Helpers.MapUtils.Put(preconditions, p, applicableRules);
            }
        }
    
    }
}