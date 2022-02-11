using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class KeySelectorWatch : Saxon.Eej.ee.validate.SelectorWatch {
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.validate.RefSelectorWatch> keyrefs = new System.Collections.Generic.List<Saxon.Eej.ee.validate.RefSelectorWatch>(4);
        public KeySelectorWatch(Saxon.Eej.ee.validate.ConstraintChecker checker, Saxon.Eej.ee.schema.IdentityConstraint uniqueConstraint) : base (checker, uniqueConstraint) {
        }
        public void addKeyRef(Saxon.Eej.ee.validate.RefSelectorWatch watch) {
            keyrefs.Add(watch);
        }
        protected internal override void checkRow(Saxon.Hej.om.AtomicSequence[] currentRow, bool isTarget, Saxon.Hej.s9api.Location locationId) {
            base.checkRow(currentRow, isTarget, locationId);
            foreach (Saxon.Eej.ee.validate.RefSelectorWatch watch in keyrefs) {
                watch.notifyActualKey(currentRow, locationId);
            }
        }
    
    }
}