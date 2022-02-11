using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public abstract class RangeIterator : Saxon.Hej.tree.iter.GroundedIterator {
        public abstract Saxon.Hej.om.GroundedValue getResidue();
        public abstract Saxon.Hej.value.IntegerValue getFirst();
        public abstract Saxon.Hej.value.IntegerValue getLast();
        public abstract Saxon.Hej.value.IntegerValue getMin();
        public abstract Saxon.Hej.value.IntegerValue getMax();
        public abstract Saxon.Hej.value.IntegerValue getStep();
        public bool containsEq(Saxon.Hej.value.NumericValue val) {
            Saxon.Hej.value.IntegerValue intVal;
            if ((val is Saxon.Hej.value.IntegerValue)) {
                intVal = (Saxon.Hej.value.IntegerValue)(val);
            } else {
                if (!(val.isWholeNumber())) {
                    return false;
                }
                intVal = Saxon.Hej.type.Converter.DecimalToInteger.INSTANCE.convert(val);
            }
            try {
                return (((intVal.compareTo(getMin())>=0) && (intVal.compareTo(getMax())<=0)) && intVal.minus(getFirst()).mod(getStep()).Equals(Saxon.Hej.value.Int64Value.ZERO));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method materialize from interface GroundedIterator */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.value.SequenceExtent.from(this).reduce();
        }

        /* Creating stub method next to satisfy interface SequenceIterator */
        public virtual Saxon.Hej.om.Item next() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isActuallyGrounded to satisfy interface GroundedIterator */
        public virtual bool isActuallyGrounded() {
            throw new System.NotImplementedException();
        }

    }
}