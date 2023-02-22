using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class OriginalFunction : Saxon.Hej.functions.AbstractFunction, Saxon.Hej.om.FunctionItem, Saxon.Hej.expr.ContextOriginator {
        private readonly Saxon.Hej.expr.instruct.UserFunction userFunction;
        private readonly Saxon.Hej.expr.Component component;
        public OriginalFunction(Saxon.Hej.expr.Component component)  {
            this.component = component;
            this.userFunction = (Saxon.Hej.expr.instruct.UserFunction)(component.getActor());
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.expr.XPathContextMajor c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)userFunction.makeNewContext(context, this));
            c2.setCurrentComponent(component);
            return userFunction.call(c2, args);
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return userFunction.getFunctionItemType();
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return userFunction.getFunctionName();
        }
        public override int getArity() {
            return userFunction.getArity();
        }
        public override string getDescription() {
            return userFunction.getDescription();
        }
        public string getContainingPackageName() {
            return component.getContainingPackage().getPackageName();
        }
        public Saxon.Hej.expr.Component getComponent() {
            return component;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = @out.getOptions();
            @out.startElement("origF");
            @out.emitAttribute("name", getFunctionName());
            @out.emitAttribute("arity", (""+getArity()));
            @out.emitAttribute("pack", (Saxon.Impl.Helpers.MapUtils.Get(options.packageMap, component.getContainingPackage())+""));
            @out.endElement();
        }
    
    }
}