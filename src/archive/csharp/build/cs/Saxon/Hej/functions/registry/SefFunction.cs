using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class SefFunction : Saxon.Hej.functions.SystemFunction {
        private const string FN_ALIAS = "http://ns.saxonica.com/xpath-functions";
        protected internal Saxon.Hej.trans.SymbolicName.F functionAlias;
        public override void setDetails(Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry) {
            base.setDetails(entry);
            functionAlias = new Saxon.Hej.trans.SymbolicName.F(new Saxon.Hej.om.StructuredQName("", FN_ALIAS, getDetails().name.getLocalPart()), getDetails().arity);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression e = new Saxon.Hej.expr.SystemFunctionCall(this, arguments);
            e.setRetainedStaticContext(getRetainedStaticContext());
            return e;
        }
        protected internal Saxon.Hej.style.StylesheetPackage pack;
        protected internal Saxon.Hej.om.Sequence callFunction(Saxon.Hej.expr.XPathContext context, Saxon.Hej.trans.SymbolicName.F name, Saxon.Hej.om.Sequence[] args) {
            if ((pack == null)) {
                Saxon.Hej.Configuration config = context.getConfiguration();
                Saxon.Hej.trans.PackageLoaderHE loader = new Saxon.Hej.trans.PackageLoaderHE(config);
                System.Collections.Generic.IList<string> messages = new System.Collections.Generic.List<string>();
                System.IO.Stream @is = Saxon.Hej.Version.platform.locateResource("sef/function-library.sef.xml", messages);
                if ((@is == null)) {
                    Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder("Failed to load built-in function library: function-library.sef.xml");
                    foreach (string msg in messages) {
                        sb.append(": ").append(msg);
                    }
                    throw new Saxon.Hej.trans.XPathException(sb.ToString());
                }
                pack = loader.loadPackage(new Saxon.Ejavax.xml.transform.stream.StreamSource(@is));
            }
            Saxon.Hej.expr.instruct.UserFunction targetFunction;
            Saxon.Hej.expr.XPathContextMajor c2;
            Saxon.Hej.expr.Component target = pack.getComponent(name);
            if ((target == null)) {
                throw new Saxon.Hej.trans.XPathException((("Built-in XSLT function "+getDetails().name.getEQName())+" not found"));
            }
            if (target.isHiddenAbstractComponent()) {
                throw new Saxon.Hej.trans.XPathException((("Cannot call an abstract function ("+name.getComponentName().getDisplayName())+") with no implementation"), "XTDE3052");
            }
            targetFunction = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
            c2 = ((/*1430*/Saxon.Hej.expr.XPathContextMajor)targetFunction.makeNewContext(context, null));
            c2.setCurrentComponent(target);
            try {
                return targetFunction.call(c2, args);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            for (int i = 0; (i<args.Length); i++) {
                args[i] = args[i].materialize();
            }
            return callFunction(context, functionAlias, args);
        }
    
    }
}