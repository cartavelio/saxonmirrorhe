using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class AtomizingIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private Saxon.Hej.om.AtomicSequence currentValue = null;
        private int currentValuePosition = 1;
        private int currentValueSize = 1;
        private Saxon.Hej.expr.parser.RoleDiagnostic roleDiagnostic;
        public AtomizingIterator(Saxon.Hej.om.SequenceIterator @base)  {
            this.@base = @base;
        }
        public void setRoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic role) {
            this.roleDiagnostic = role;
        }
        public Saxon.Hej.om.Item next() {
            while (true) {
                if ((currentValue != null)) {
                    if ((currentValuePosition<currentValueSize)) {
                        return ((/*1430*/Saxon.Hej.value.AtomicValue)currentValue.itemAt(currentValuePosition++));
                    } else {
                        currentValue = null;
                    }
                }
                Saxon.Hej.om.Item nextSource = @base.next();
                if ((nextSource != null)) {
                    try {
                        Saxon.Hej.om.AtomicSequence v = nextSource.atomize();
                        if ((v is Saxon.Hej.value.AtomicValue)) {
                            return (Saxon.Hej.value.AtomicValue)(v);
                        } else {
                            currentValue = v;
                            currentValuePosition = 0;
                            currentValueSize = currentValue.getLength();
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        if ((roleDiagnostic == null)) {
                            throw new Saxon.Hej.trans.UncheckedXPathException(e);
                        } else {
                            string message = ((e.Message+". Failed while atomizing the ")+roleDiagnostic.getMessage());
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, e.getErrorCodeLocalPart(), e.getLocator());
                            throw new Saxon.Hej.trans.UncheckedXPathException(err);
                        }
                    }
                } else {
                    currentValue = null;
                    return null;
                }
            }
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
    
    }
}