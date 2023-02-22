using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public abstract class SelectorWatch : Saxon.Eej.ee.stream.watch.PatternWatch {
        internal Saxon.Eej.ee.schema.IdentityConstraint identityConstraint;
        private readonly Saxon.Eej.ee.validate.ConstraintChecker constraintChecker;
        private readonly int numberOfColumns;
        private Saxon.Hej.om.AtomicSequence[] currentRow;
        private System.Collections.Generic.Stack<Saxon.Hej.om.AtomicSequence[]> activationStack = null;
        internal System.Collections.Generic.Dictionary<Saxon.Eej.ee.validate.SelectorWatch.ValueEntry,int> table = new System.Collections.Generic.Dictionary<Saxon.Eej.ee.validate.SelectorWatch.ValueEntry, int>(100);
        internal SelectorWatch(Saxon.Eej.ee.validate.ConstraintChecker checker, Saxon.Eej.ee.schema.IdentityConstraint uniqueConstraint)  {
            identityConstraint = uniqueConstraint;
            constraintChecker = checker;
            setSelection(uniqueConstraint.getSelector().getSelection());
            numberOfColumns = uniqueConstraint.getFields().Count;
        }
        internal Saxon.Eej.ee.schema.IdentityConstraint getIdentityConstraint() {
            return identityConstraint;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            if ((currentRow != null)) {
                if ((activationStack == null)) {
                    activationStack = new System.Collections.Generic.Stack<Saxon.Hej.om.AtomicSequence[]>();
                }
                activationStack.Push(currentRow);
            }
            currentRow = new Saxon.Hej.om.AtomicSequence[numberOfColumns];
            int column = 0;
            Saxon.Eej.ee.stream.watch.WatchManager fieldHandler = constraintChecker;
            foreach (Saxon.Eej.ee.schema.IdentityField field in identityConstraint.getFields()) {
                Saxon.Hej.pattern.Pattern fieldSelection = field.getSelection();
                Saxon.Eej.ee.validate.FieldWatch fieldWatch = new Saxon.Eej.ee.validate.FieldWatch(this, fieldSelection, currentRow, column++);
                fieldWatch.setAnchorNode(node);
                fieldWatch.setNamespaceResolver(getNamespaceResolver());
                fieldHandler.addWatch(fieldWatch, true);
            }
            return null;
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            checkRow(currentRow, false, locationId);
            if (((activationStack != null) && !((activationStack.Count==0)))) {
                currentRow = activationStack.Pop();
            } else {
                currentRow = null;
            }
        }
        public override void close() {
        }
        internal void addFieldValue(Saxon.Hej.om.AtomicSequence[] currentRow, int column, Saxon.Hej.om.AtomicSequence value, bool nillableElement, Saxon.Hej.s9api.Location locationId) {
            if ((currentRow[column] != null)) {
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure((((("Identity constraint "+Saxon.Hej.trans.Err.wrap(identityConstraint.getName()))+": the ")+identifyField(column))+" selects more than one node"));
                ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                reportValidationError(ve, locationId);
            }
            if ((nillableElement && (identityConstraint is Saxon.Eej.ee.schema.Key))) {
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("The element declaration of a field in an xs:key constraint must not be nillable");
                ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                ve.setConstraintReference(1, "cvc-identity-constraint", "4.2.3");
                reportValidationError(ve, locationId);
            }
            currentRow[column] = value;
        }
        private string identifyField(int column) {
            if ((numberOfColumns == 1)) {
                return "field";
            } else {
                if ((column == 0)) {
                    return "first field";
                } else {
                    if ((column == 1)) {
                        return "second field";
                    } else {
                        if ((column == 2)) {
                            return "third field";
                        } else {
                            if ((column<20)) {
                                return (((column+1))+"th field");
                            } else {
                                return ("field "+((column+1)));
                            }
                        }
                    }
                }
            }
        }
        protected internal virtual void checkRow(Saxon.Hej.om.AtomicSequence[] currentRow, bool isTarget, Saxon.Hej.s9api.Location locationId) {
            for (int i = 0; (i<numberOfColumns); i++) {
                if ((currentRow[i] == null)) {
                    if (isTarget) {
                        return ;
                    } else {
                        if ((identityConstraint is Saxon.Eej.ee.schema.Unique)) {
                            return ;
                        } else {
                            if ((identityConstraint is Saxon.Eej.ee.schema.Key)) {
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure((((("The "+identifyField(i))+" in constraint ")+Saxon.Hej.trans.Err.wrap(identityConstraint.getName()))+" has no value"));
                                ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                                ve.setConstraintReference(1, "cvc-identity-constraint", "4.2.1");
                                reportValidationError(ve, locationId);
                            } else {
                                return ;
                            }
                        }
                    }
                }
            }
            Saxon.Eej.ee.validate.SelectorWatch.ValueEntry entry = /*134*/Saxon.Eej.ee.validate.SelectorWatch.ValueEntry.makeValueEntry(currentRow);
            if (table.ContainsKey(entry)) {
                int flags = Saxon.Impl.Helpers.MapUtils.Get(table, entry);
                if (isTarget) {
                    int count = (flags>>16);
                    count++;
                    flags = (((count<<16))|((flags&unchecked((int)0xffff))));
                    if (((count>1) && ((((flags&1)) != 0)))) {
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("More than one referenced value found for keyRef "+Saxon.Hej.trans.Err.wrap(identityConstraint.getName()))+"\n    ")+entry));
                        ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                        ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                        reportValidationError(ve, locationId);
                    }
                    Saxon.Impl.Helpers.MapUtils.Put(table, entry, flags);
                } else {
                    if ((identityConstraint is Saxon.Eej.ee.schema.KeyRef)) {
                        flags |= 1;
                        if ((((flags>>16))>1)) {
                            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("More than one referenced value found for keyRef "+Saxon.Hej.trans.Err.wrap(identityConstraint.getName()))+"\n    ")+entry));
                            ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                            ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                            reportValidationError(ve, locationId);
                        }
                        Saxon.Impl.Helpers.MapUtils.Put(table, entry, flags);
                    } else {
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Non-unique value found for constraint "+identityConstraint.getName())+": ")+entry));
                        string err = constraintChecker.getValidationContext().getErrorCode();
                        ve.setErrorCode(err);
                        ve.setConstraintReference(1, "cvc-identity-constraint", ((identityConstraint is Saxon.Eej.ee.schema.Unique) ? "4.1" : "4.2.2"));
                        reportValidationError(ve, locationId);
                    }
                }
            } else {
                if (identityConstraint.isOrdered()) {
                    if (table.Count==0) {
                        Saxon.Impl.Helpers.MapUtils.Put(table, entry, PRESENT_VALUE);
                    } else {
                        Saxon.Eej.ee.validate.SelectorWatch.ValueEntry latest = null;
                        foreach (Saxon.Eej.ee.validate.SelectorWatch.ValueEntry ve in Saxon.Impl.Helpers.MapUtils.KeySet(table)) {
                            latest = ve;
                        }
                        System.Diagnostics.Debug.Assert((latest != null));
                        if (orderedOK(latest, entry)) {
                            table.Clear();
                            Saxon.Impl.Helpers.MapUtils.Put(table, entry, PRESENT_VALUE);
                        } else {
                            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Value out of sequence for ordered constraint "+identityConstraint.getName())+": ")+entry));
                            ve.setErrorCode(Saxon.Hej.trans.SaxonErrorCode.SXOR0001);
                            ve.setConstraintReference(1, "saxon-identity-constraint", "0");
                            reportValidationError(ve, locationId);
                        }
                    }
                } else {
                    int details = (isTarget ? REFERENCED_VALUE : PRESENT_VALUE);
                    Saxon.Impl.Helpers.MapUtils.Put(table, entry, details);
                }
            }
        }
        private bool orderedOK(Saxon.Eej.ee.validate.SelectorWatch.ValueEntry first, Saxon.Eej.ee.validate.SelectorWatch.ValueEntry second) {
            Saxon.Hej.om.AtomicSequence[] s1 = first.getValues();
            Saxon.Hej.om.AtomicSequence[] s2 = second.getValues();
            if ((s1.Length != s2.Length)) {
                throw new Saxon.Ejava.lang.AssertionError();
            }
            for (int i = 0; (i<s1.Length); i++) {
                int c = Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareSequences(s1[i], s2[i]);
                if ((c != 0)) {
                    return (identityConstraint.getFields()[i].isAscending() ? (c<0) : (c>0));
                }
            }
            return false;
        }
        private const int REFERENCED_VALUE = 0x10000;
        private const int PRESENT_VALUE = 1;
        internal abstract class ValueEntry {
            protected internal static Saxon.Eej.ee.validate.SelectorWatch.ValueEntry makeValueEntry(Saxon.Hej.om.AtomicSequence[] values) {
                switch (values.Length) {
                  case 1:
                    return new Saxon.Eej.ee.validate.SelectorWatch.ValueEntry1(values[0]);
                  case 2:
                    return new Saxon.Eej.ee.validate.SelectorWatch.ValueEntry2(values[0], values[1]);
                  case 3:
                    return new Saxon.Eej.ee.validate.SelectorWatch.ValueEntry3(values[0], values[1], values[2]);
                  case 4:
                    return new Saxon.Eej.ee.validate.SelectorWatch.ValueEntry4(values[0], values[1], values[2], values[3]);
                  default:
                    return new Saxon.Eej.ee.validate.SelectorWatch.MultiValueEntry(values);
                }
            }
            internal bool equalValues(Saxon.Hej.om.AtomicSequence val0, Saxon.Hej.om.AtomicSequence val1) {
                if ((val0 == null)) {
                    return (val1 == null);
                } else {
                    return ((val1 != null) && Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equalOrIdentical(val0, val1));
                }
            }
            internal string displayValue(Saxon.Hej.om.AtomicSequence val) {
                if ((val == null)) {
                    return "()";
                } else {
                    return val.ToString();
                }
            }
            protected internal Saxon.Hej.om.AtomicSequence compact(Saxon.Hej.om.AtomicSequence value) {
                if ((value is Saxon.Hej.value.StringValue)) {
                    return new Saxon.Eej.ee.validate.CompactStringValue(value.getUnicodeStringValue());
                } else {
                    return value;
                }
            }
            protected internal abstract Saxon.Hej.om.AtomicSequence[] getValues();
            protected internal int hash(Saxon.Hej.om.AtomicSequence value) {
                return ((value == null) ? 0 : Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().hash(value));
            }
        
        }
        private class MultiValueEntry : Saxon.Eej.ee.validate.SelectorWatch.ValueEntry {
            internal Saxon.Hej.om.AtomicSequence[] values;
            internal MultiValueEntry(Saxon.Hej.om.AtomicSequence[] values)  {
                this.values = values;
                for (int i = 0; (i<values.Length); i++) {
                    values[i] = compact(values[i]);
                }
            }
            protected internal override Saxon.Hej.om.AtomicSequence[] getValues() {
                return values;
            }
            public override bool Equals(System.Object obj) {
                if (((obj is Saxon.Eej.ee.validate.SelectorWatch.MultiValueEntry) && (((Saxon.Eej.ee.validate.SelectorWatch.MultiValueEntry)(obj)).values.Length == values.Length))) {
                    for (int i = 0; (i<values.Length); i++) {
                        if (!(equalValues(values[i], ((Saxon.Eej.ee.validate.SelectorWatch.MultiValueEntry)(obj)).values[i]))) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    return false;
                }
            }
            public override int GetHashCode() {
                int h = unchecked((int)0xa012b876);
                foreach (Saxon.Hej.om.AtomicSequence a in values) {
                    h ^= hash(a);
                }
                return h;
            }
            public override string ToString() {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
                sb.append('(');
                for (int i = 0; (i<values.Length); i++) {
                    if ((i != 0)) {
                        sb.append(", ");
                    }
                    sb.append(displayValue(values[i]));
                }
                sb.append(')');
                return sb.ToString();
            }
        
        }
        private class ValueEntry1 : Saxon.Eej.ee.validate.SelectorWatch.ValueEntry {
            internal Saxon.Hej.om.AtomicSequence value;
            internal ValueEntry1(Saxon.Hej.om.AtomicSequence value)  {
                this.value = compact(value);
            }
            protected internal override Saxon.Hej.om.AtomicSequence[] getValues() {
                return new Saxon.Hej.om.AtomicSequence[]{value};
            }
            public override bool Equals(System.Object obj) {
                return ((obj is Saxon.Eej.ee.validate.SelectorWatch.ValueEntry1) && equalValues(value, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry1)(obj)).value));
            }
            public override int GetHashCode() {
                return (unchecked((int)0xa012b876)^hash(value));
            }
            public override string ToString() {
                return displayValue(value);
            }
        
        }
        private class ValueEntry2 : Saxon.Eej.ee.validate.SelectorWatch.ValueEntry {
            internal Saxon.Hej.om.AtomicSequence value0;
            internal Saxon.Hej.om.AtomicSequence value1;
            internal ValueEntry2(Saxon.Hej.om.AtomicSequence value0, Saxon.Hej.om.AtomicSequence value1)  {
                this.value0 = compact(value0);
                this.value1 = compact(value1);
            }
            protected internal override Saxon.Hej.om.AtomicSequence[] getValues() {
                return new Saxon.Hej.om.AtomicSequence[]{value0, value1};
            }
            public override bool Equals(System.Object obj) {
                return (((obj is Saxon.Eej.ee.validate.SelectorWatch.ValueEntry2) && equalValues(value0, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry2)(obj)).value0)) && equalValues(value1, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry2)(obj)).value1));
            }
            public override int GetHashCode() {
                return ((unchecked((int)0xa012b876)^hash(value0))^((hash(value1)*31)));
            }
            public override string ToString() {
                return (((("("+displayValue(value0))+", ")+displayValue(value1))+")");
            }
        
        }
        private class ValueEntry3 : Saxon.Eej.ee.validate.SelectorWatch.ValueEntry {
            internal Saxon.Hej.om.AtomicSequence value0;
            internal Saxon.Hej.om.AtomicSequence value1;
            internal Saxon.Hej.om.AtomicSequence value2;
            internal ValueEntry3(Saxon.Hej.om.AtomicSequence value0, Saxon.Hej.om.AtomicSequence value1, Saxon.Hej.om.AtomicSequence value2)  {
                this.value0 = compact(value0);
                this.value1 = compact(value1);
                this.value2 = compact(value2);
            }
            protected internal override Saxon.Hej.om.AtomicSequence[] getValues() {
                return new Saxon.Hej.om.AtomicSequence[]{value0, value1, value2};
            }
            public override bool Equals(System.Object obj) {
                return ((((obj is Saxon.Eej.ee.validate.SelectorWatch.ValueEntry3) && equalValues(value0, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry3)(obj)).value0)) && equalValues(value1, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry3)(obj)).value1)) && equalValues(value2, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry3)(obj)).value2));
            }
            public override int GetHashCode() {
                return (((unchecked((int)0xa012b876)^hash(value0))^((hash(value1)*31)))^((hash(value2)*63)));
            }
            public override string ToString() {
                return (((((("("+displayValue(value0))+", ")+displayValue(value1))+", ")+displayValue(value2))+")");
            }
        
        }
        private class ValueEntry4 : Saxon.Eej.ee.validate.SelectorWatch.ValueEntry {
            internal Saxon.Hej.om.AtomicSequence value0;
            internal Saxon.Hej.om.AtomicSequence value1;
            internal Saxon.Hej.om.AtomicSequence value2;
            internal Saxon.Hej.om.AtomicSequence value3;
            internal ValueEntry4(Saxon.Hej.om.AtomicSequence value0, Saxon.Hej.om.AtomicSequence value1, Saxon.Hej.om.AtomicSequence value2, Saxon.Hej.om.AtomicSequence value3)  {
                this.value0 = compact(value0);
                this.value1 = compact(value1);
                this.value2 = compact(value2);
                this.value3 = compact(value3);
            }
            protected internal override Saxon.Hej.om.AtomicSequence[] getValues() {
                return new Saxon.Hej.om.AtomicSequence[]{value0, value1, value2, value3};
            }
            public override bool Equals(System.Object obj) {
                return (((((obj is Saxon.Eej.ee.validate.SelectorWatch.ValueEntry4) && equalValues(value0, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry4)(obj)).value0)) && equalValues(value1, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry4)(obj)).value1)) && equalValues(value2, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry4)(obj)).value2)) && equalValues(value3, ((Saxon.Eej.ee.validate.SelectorWatch.ValueEntry4)(obj)).value3));
            }
            public override int GetHashCode() {
                return ((((unchecked((int)0xa012b876)^hash(value0))^((hash(value1)*31)))^((hash(value2)*63)))^((hash(value3)*127)));
            }
            public override string ToString() {
                return (((((((("("+displayValue(value0))+", ")+displayValue(value1))+", ")+displayValue(value2))+", ")+displayValue(value3))+")");
            }
        
        }
        protected internal void reportValidationError(Saxon.Hej.type.ValidationFailure err, Saxon.Hej.s9api.Location locator) {
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            Saxon.Eej.ee.validate.ValidationContext vc = constraintChecker.getValidationContext();
            Saxon.Hej.lib.InvalidityHandler handler = vc.getInvalidityHandler();
            if ((handler == null)) {
                handler = new Saxon.Hej.lib.InvalidityHandlerWrappingErrorReporter(pipe.getErrorReporter());
            }
            err.setHasBeenReported(true);
            err.setSourceLocator(locator);
            handler.reportInvalidity(err);
            if ((vc.getErrorCount() == 0)) {
                vc.setErrorCode(err.getErrorCode());
            }
            if (vc.incrementErrorCount(err)) {
                throw err.makeException();
            }
        }
    
    }
}