using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CallableFunction : Saxon.Hej.functions.AbstractFunction {
        private Saxon.Hej.expr.Callable callable;
        private readonly Saxon.Hej.trans.SymbolicName.F name;
        private Saxon.Hej.type.FunctionItemType type;
        private Saxon.Hej.query.AnnotationList annotations;
        public CallableFunction(Saxon.Hej.trans.SymbolicName.F name, Saxon.Hej.expr.Callable callable, Saxon.Hej.type.FunctionItemType type)  {
            this.name = name;
            this.callable = callable;
            this.type = type;
        }
        public CallableFunction(int arity, Saxon.Hej.expr.Callable callable, Saxon.Hej.type.FunctionItemType type)  {
            this.name = new Saxon.Hej.trans.SymbolicName.F(new Saxon.Hej.om.StructuredQName("", "anon", "anon"), arity);
            this.callable = callable;
            this.type = type;
        }
        public Saxon.Hej.expr.Callable getCallable() {
            return callable;
        }
        public void setCallable(Saxon.Hej.expr.Callable callable) {
            this.callable = callable;
        }
        public void setType(Saxon.Hej.type.FunctionItemType type) {
            this.type = type;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            if (((type == Saxon.Hej.type.AnyFunctionType.getInstance()) && (callable is Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable))) {
                Saxon.Hej.expr.instruct.UserFunction uf = ((Saxon.Hej.query.XQueryFunctionLibrary.UnresolvedCallable)(callable)).getFunction();
                if ((uf != null)) {
                    type = uf.getFunctionItemType();
                }
            }
            return type;
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return name.getComponentName();
        }
        public override string getDescription() {
            return callable.ToString();
        }
        public override int getArity() {
            return name.getArity();
        }
        public void setAnnotations(Saxon.Hej.query.AnnotationList annotations) {
            this.annotations = annotations;
        }
        public override Saxon.Hej.query.AnnotationList getAnnotations() {
            return annotations;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            return callable.call(context, args);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("A CallableFunction is a transient value that cannot be exported");
        }
    
    }
}