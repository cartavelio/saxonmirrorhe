using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class AccumulatorRegistry {
        protected internal System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.accum.Accumulator> accumulatorsByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.accum.Accumulator>();
        public AccumulatorRegistry()  {
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> getUsedAccumulators(string useAccumulatorsAtt, Saxon.Hej.style.StyleElement styleElement) {
            System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
            string attNames = Saxon.Hej.value.Whitespace.trim(useAccumulatorsAtt);
            string[] tokens = System.Text.RegularExpressions.Regex.Split(attNames, "[ \t\r\n]+");
            if (((tokens.Length == 1) && tokens[0].Equals("#all"))) {
                foreach (Saxon.Hej.expr.accum.Accumulator acc in getAllAccumulators()) {
                    accumulators.Add(acc);
                }
            } else {
                if (((tokens.Length == 1) && tokens[0].Length==0)) {
                } else {
                    System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> names = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>(tokens.Length);
                    foreach (string token in tokens) {
                        if (token.Equals("#all")) {
                            styleElement.compileErrorInAttribute("If use-accumulators contains the token '#all', it must be the only token", "XTSE3300", "use-accumulators");
                            break ;
                        }
                        Saxon.Hej.om.StructuredQName name = styleElement.makeQName(token, "XTSE3300", "use-accumulators");
                        if (names.Contains(name)) {
                            styleElement.compileErrorInAttribute(("Duplicate QName in use-accumulators attribute: "+token), "XTSE3300", "use-accumuators");
                            break ;
                        }
                        Saxon.Hej.expr.accum.Accumulator acc = getAccumulator(name);
                        if ((acc == null)) {
                            styleElement.compileErrorInAttribute(("Unknown accumulator name: "+token), "XTSE3300", "use-accumulators");
                            break ;
                        }
                        names.Add(name);
                        accumulators.Add(acc);
                    }
                }
            }
            return accumulators;
        }
        public void addAccumulator(Saxon.Hej.expr.accum.Accumulator acc) {
            if ((acc.getAccumulatorName() != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(accumulatorsByName, acc.getAccumulatorName(), acc);
            }
        }
        public Saxon.Hej.expr.accum.Accumulator getAccumulator(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(accumulatorsByName, name);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.accum.Accumulator> getAllAccumulators() {
            return accumulatorsByName.Values;
        }
        public virtual Saxon.Hej.om.Sequence getStreamingAccumulatorValue(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.accum.Accumulator accumulator, Saxon.Hej.functions.AccumulatorFn.Phase phase) {
            return null;
        }
    
    }
}