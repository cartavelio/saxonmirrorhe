using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class RefSelectorWatch : Saxon.Eej.ee.validate.SelectorWatch {
        internal Saxon.Hej.s9api.Location lastLocation = null;
        public RefSelectorWatch(Saxon.Eej.ee.validate.ConstraintChecker checker, Saxon.Eej.ee.schema.KeyRef constraint) : base (checker, constraint) {
        }
        public void notifyActualKey(Saxon.Hej.om.AtomicSequence[] keyValue, Saxon.Hej.s9api.Location locationId) {
            checkRow(keyValue, true, locationId);
            lastLocation = locationId.saveLocation();
        }
        public override void close() {
            int errorCount = 0;
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Eej.ee.validate.SelectorWatch.ValueEntry,int> e in table) {
                Saxon.Eej.ee.validate.SelectorWatch.ValueEntry entry = e.Key;
                int info = e.Value;
                if ((((info&1)) != 0)) {
                    int count = (((info>>16))&unchecked((int)0xffff));
                    if ((count != 1)) {
                        string card = ((count == 0) ? "No" : "More than one");
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((((card+" referenced value found for keyRef ")+Saxon.Hej.trans.Err.wrap(identityConstraint.getName()))+": ")+entry));
                        ve.setConstraintReference(1, "cvc-identity-constraint", "4.3");
                        ve.setErrorCode((getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027"));
                        reportValidationError(ve, lastLocation);
                        if ((errorCount++>100)) {
                            break ;
                        }
                    }
                }
            }
        }
    
    }
}