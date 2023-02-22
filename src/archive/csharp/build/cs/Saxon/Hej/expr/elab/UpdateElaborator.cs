using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public abstract class UpdateElaborator : Saxon.Hej.expr.elab.Elaborator {
        private void notAllowed() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot evaluate updating expression");
        }
        public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
            notAllowed();
            return null;
        }
        public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
            notAllowed();
            return null;
        }
        public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
            notAllowed();
            return null;
        }
        public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
            notAllowed();
            return null;
        }
        public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
            notAllowed();
            return null;
        }
        public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
            notAllowed();
            return null;
        }
        public abstract override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate();
    
    }
}