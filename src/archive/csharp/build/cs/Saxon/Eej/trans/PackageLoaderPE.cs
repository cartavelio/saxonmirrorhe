using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.trans {
    public class PackageLoaderPE : Saxon.Hej.trans.PackageLoaderHE {

        static PackageLoaderPE() {
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "acFnRef", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.type.SchemaType type = loader.getConfiguration().getSchemaType(name);
    if (!(((type is Saxon.Hej.type.AtomicType)))) {
        throw new Saxon.Hej.trans.XPathException((("No atomic type "+name.getEQName())+" found"));
    }
    Saxon.Hej.functions.hof.AtomicConstructorFunction fn = new Saxon.Hej.functions.hof.AtomicConstructorFunction((Saxon.Hej.type.AtomicType)(type), null);
    return new Saxon.Hej.functions.hof.FunctionLiteral(fn);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "assign", (loader, element) => {
    loader.needsPELicense("saxon:assign");
    Saxon.Hej.om.StructuredQName varName = loader.getQNameAttribute(element, "name");
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    Saxon.Eej.xsltextn.instruct.Assign inst = new Saxon.Eej.xsltextn.instruct.Assign(select);
    loader.completionActions.Add(() => {
    Saxon.Hej.trans.SymbolicName sn = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, varName);
    Saxon.Hej.expr.instruct.GlobalVariable var = (Saxon.Hej.expr.instruct.GlobalVariable)(loader.getTopLevelPackage().getComponent(sn).getActor());
    inst.fixup(var);
} );
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "deepUpdate", (loader, element) => {
    loader.needsPELicense("saxon:deep-update");
    Saxon.Hej.expr.Expression root = loader.getExpressionWithRole(element, "root");
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    Saxon.Hej.expr.Expression action = loader.getExpressionWithRole(element, "action");
    return new Saxon.Eej.xsltextn.instruct.DeepUpdate(root, select, action);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "do", (loader, element) => {
    Saxon.Hej.expr.Expression arg = loader.getFirstChildExpression(element);
    return new Saxon.Eej.xsltextn.instruct.DoInstr(arg);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "forMember", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    int slot = loader.getIntegerAttribute(element, "slot");
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "var");
    Saxon.Hej.expr.ForMemberExpression forEx = new Saxon.Hej.expr.ForMemberExpression();
    forEx.setSequence(select);
    forEx.setSlotNumber(slot);
    forEx.setVariableQName(name);
    loader.localBindings.Push(forEx);
    Saxon.Hej.expr.Expression action = loader.getSecondChildExpression(element);
    loader.localBindings.Pop();
    forEx.setAction(action);
    return forEx;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "while", (loader, element) => {
    loader.needsPELicense("saxon:while");
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Eej.xsltextn.instruct.While(lhs, rhs);
} );
        }
        public PackageLoaderPE(Saxon.Eej.config.ProfessionalConfiguration config) : base (config) {
        }
        protected internal override void readSchemaNamespaces(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.Configuration config = getConfiguration();
            foreach (Saxon.Hej.om.NodeInfo varElement in packageElement.children(Saxon.Hej.pattern.NodeSelector.of(n => n.getLocalPart().Equals("schema")))) {
                string ns = varElement.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "ns");
                if ((!(config.isSchemaAvailable(Saxon.Hej.om.NamespaceUri.of(ns))) && !(ns.Equals(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS)))) {
                    throw new Saxon.Hej.trans.XPathException((("The XSLT package requires access to "+((ns.Length==0 ? "a no-namespace schema" : ("a schema for namespace "+ns))))+", but this is not available in this Configuration"), Saxon.Hej.trans.SaxonErrorCode.SXPK0003);
                }
                packStack.Peek().getSchemaNamespaces().Add(Saxon.Hej.om.NamespaceUri.of(ns));
            }
        }
        protected internal override Saxon.Hej.expr.instruct.UserFunction makeFunction(string flags) {
            return (flags.Contains("m") ? new Saxon.Hej.expr.instruct.MemoFunction() : new Saxon.Hej.expr.instruct.UserFunction());
        }
        protected internal override bool processComponentReference(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.expr.ComponentInvocation call) {
            Saxon.Hej.trans.SymbolicName sn = call.getSymbolicName();
            if ((call is Saxon.Hej.functions.hof.UserFunctionReference)) {
                Saxon.Hej.expr.Component c = pack.getComponent(sn);
                if ((c == null)) {
                    return base.processComponentReference(pack, call);
                }
                ((Saxon.Hej.functions.hof.UserFunctionReference)(call)).setFunction((Saxon.Hej.expr.instruct.UserFunction)(c.getActor()));
            } else {
                return base.processComponentReference(pack, call);
            }
            return false;
        }
        protected internal override void addVendorFunctionLibrary(Saxon.Hej.functions.FunctionLibraryList targetList, Saxon.Hej.Configuration config) {
            targetList.addFunctionLibrary(((Saxon.Eej.config.ProfessionalConfiguration)(config)).getVendorFunctionSet());
        }
    
    }
}