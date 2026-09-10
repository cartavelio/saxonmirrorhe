using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class CollatingFunctionFixed : Saxon.Hej.functions.SystemFunction, Saxon.Hej.functions.StatefulSystemFunction {
        private string collationName;
        private Saxon.Hej.lib.StringCollator stringCollator = null;
        private Saxon.Hej.expr.sort.AtomicComparer atomicComparer = null;
        public virtual bool isSubstringMatchingFunction() {
            return false;
        }
        public Saxon.Hej.lib.StringCollator getStringCollator() {
            return stringCollator;
        }
        public override void setRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext) {
            base.setRetainedStaticContext(retainedStaticContext);
            if ((collationName == null)) {
                collationName = retainedStaticContext.getDefaultCollationName();
                try {
                    allocateCollator();
                } catch (Saxon.Hej.trans.XPathException e) {
                }
            }
        }
        public void setCollationName(string collationName) {
            this.collationName = collationName;
            allocateCollator();
        }
        private void allocateCollator() {
            stringCollator = getRetainedStaticContext().getConfiguration().getCollation(collationName);
            if ((stringCollator == null)) {
                throw new Saxon.Hej.trans.XPathException(("Unknown collation "+collationName), "FOCH0002");
            }
            if (isSubstringMatchingFunction()) {
                if ((stringCollator is Saxon.Hej.expr.sort.SimpleCollation)) {
                    stringCollator = ((Saxon.Hej.expr.sort.SimpleCollation)(stringCollator)).getSubstringMatcher();
                }
                if (!(((stringCollator is Saxon.Hej.lib.SubstringMatcher)))) {
                    throw new Saxon.Hej.trans.XPathException((("The collation requested for "+getFunctionName().getDisplayName())+" does not support substring matching"), "FOCH0004");
                }
            }
        }
        protected internal void preAllocateComparer(Saxon.Hej.type.AtomicType type0, Saxon.Hej.type.AtomicType type1, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.lib.StringCollator collation = getStringCollator();
            if (((type0 == Saxon.Hej.type.ErrorType.getInstance()) || (type1 == Saxon.Hej.type.ErrorType.getInstance()))) {
                atomicComparer = Saxon.Hej.expr.sort.EqualityComparer.getInstance();
                return ;
            }
            atomicComparer = Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer((Saxon.Hej.type.BuiltInAtomicType)(type0.getBuiltInBaseType()), (Saxon.Hej.type.BuiltInAtomicType)(type1.getBuiltInBaseType()), stringCollator, env.makeEarlyEvaluationContext());
        }
        public Saxon.Hej.expr.sort.AtomicComparer getPreAllocatedAtomicComparer() {
            return atomicComparer;
        }
        public virtual Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer(Saxon.Hej.expr.XPathContext context) {
            if ((atomicComparer != null)) {
                return atomicComparer.provideContext(context);
            } else {
                return new Saxon.Hej.expr.sort.GenericAtomicComparer(getStringCollator(), context);
            }
        }
        public override void exportAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            if (!(collationName.Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI))) {
                @out.emitAttribute("collation", collationName);
            }
        }
        public override void importAttributes(Saxon.Ejava.util.Properties attributes) {
            string collationName = attributes.getProperty("collation");
            if ((collationName != null)) {
                setCollationName(collationName);
            }
        }
        public Saxon.Hej.functions.SystemFunction copy() {
            Saxon.Hej.functions.SystemFunction copy = Saxon.Hej.functions.SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity());
            if ((copy is Saxon.Hej.functions.CollatingFunctionFree)) {
                try {
                    copy = ((Saxon.Hej.functions.CollatingFunctionFree)(copy)).bindCollation(collationName);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            }
            if ((copy is Saxon.Hej.functions.CollatingFunctionFixed)) {
                ((Saxon.Hej.functions.CollatingFunctionFixed)(copy)).collationName = collationName;
                ((Saxon.Hej.functions.CollatingFunctionFixed)(copy)).atomicComparer = atomicComparer;
                ((Saxon.Hej.functions.CollatingFunctionFixed)(copy)).stringCollator = stringCollator;
                return (Saxon.Hej.functions.CollatingFunctionFixed)(copy);
            }
            throw new Saxon.Ejava.lang.IllegalStateException();
        }
    
    }
}