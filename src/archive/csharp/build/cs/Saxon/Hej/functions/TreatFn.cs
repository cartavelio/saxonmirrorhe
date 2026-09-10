using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class TreatFn : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public abstract override string getErrorCodeForTypeErrors();
        public abstract int getRequiredCardinality();
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator iterator = arguments[0].iterate();
            int card = getRequiredCardinality();
            iterator = new Saxon.Hej.expr.CardinalityCheckingIterator(iterator, card, () => makeRoleDiagnostic(), null);
            return new Saxon.Hej.om.LazySequence(iterator);
        }
        public Saxon.Hej.expr.parser.RoleDiagnostic makeRoleDiagnostic() {
            return new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, getFunctionName().getDisplayName(), 0, getErrorCodeForTypeErrors());
        }
        public override string getStreamerName() {
            return "TreatFn";
        }
        public class ExactlyOne : Saxon.Hej.functions.TreatFn {
            public override int getRequiredCardinality() {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            }
            public override string getErrorCodeForTypeErrors() {
                return "FORG0005";
            }
        
        }
        public class OneOrMore : Saxon.Hej.functions.TreatFn {
            public override int getRequiredCardinality() {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
            }
            public override string getErrorCodeForTypeErrors() {
                return "FORG0004";
            }
        
        }
        public class ZeroOrOne : Saxon.Hej.functions.TreatFn {
            public override int getRequiredCardinality() {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
            public override string getErrorCodeForTypeErrors() {
                return "FORG0003";
            }
        
        }
    
    }
}