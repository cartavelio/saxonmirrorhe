using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class AbstractFunction : Saxon.Hej.om.Function {
        public virtual Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            Saxon.Hej.expr.OperandRole[] roles = new Saxon.Hej.expr.OperandRole[getArity()];
            Saxon.Ejava.util.Arrays.fill(roles, new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.NAVIGATION));
            return roles;
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            throw new Saxon.Hej.trans.XPathException("Function items (other than arrays) cannot be atomized", "FOTY0013");
        }
        public bool isArray() {
            return false;
        }
        public bool isMap() {
            return false;
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("The string value of a function is not defined");
        }
        public virtual Saxon.Hej.query.AnnotationList getAnnotations() {
            return Saxon.Hej.query.AnnotationList.EMPTY;
        }
        public bool effectiveBooleanValue() {
            throw new Saxon.Hej.trans.XPathException("A function has no effective boolean value", "XPTY0004");
        }
        public void simplify() {
        }
        public virtual void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
        }
        public virtual Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext callingContext, Saxon.Hej.expr.ContextOriginator originator) {
            return callingContext;
        }
        public bool deepEquals(Saxon.Hej.om.Function other, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer comparer, int flags) {
            throw new Saxon.Hej.trans.XPathException("Argument to deep-equal() contains a function item", "FOTY0015");
        }
        public virtual void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException(("export() not implemented for "+this.GetType()));
        }
        public virtual bool isTrustedResultType() {
            return false;
        }
        public virtual string toShortString() {
            return getDescription();
        }
    
        /* Expanding default method isSequenceVariadic from interface Function */
        public virtual bool isSequenceVariadic() {
            return false;
        }

        /* Expanding default method getGenre from interface Function */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.FUNCTION;
        }

        /* Expanding default method reduce from interface Item */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method head from interface Item */
        public virtual Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method itemAt from interface Item */
        public virtual Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? head() : null);
        }

        /* Expanding default method subsequence from interface Item */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public virtual int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
            return false;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

        /* Creating stub method call to satisfy interface Callable */
        public virtual Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext arg1, Saxon.Hej.om.Sequence[] arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getFunctionItemType to satisfy interface Function */
        public virtual Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getFunctionName to satisfy interface Function */
        public virtual Saxon.Hej.om.StructuredQName getFunctionName() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getArity to satisfy interface Function */
        public virtual int getArity() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getDescription to satisfy interface Function */
        public virtual string getDescription() {
            throw new System.NotImplementedException();
        }

    }
}