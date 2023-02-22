using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class SystemFunction : Saxon.Hej.functions.AbstractFunction {
        private int arity;
        private Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry details;
        private Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext;
        public static Saxon.Hej.expr.Expression makeCall(string name, Saxon.Hej.expr.parser.RetainedStaticContext rsc, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.functions.SystemFunction f = makeFunction(name, rsc, arguments.Length);
            Saxon.Hej.expr.Expression expr = f.makeFunctionCall(arguments);
            expr.setRetainedStaticContext(rsc);
            return expr;
        }
        public static Saxon.Hej.functions.SystemFunction makeFunction(string name, Saxon.Hej.expr.parser.RetainedStaticContext rsc, int arity) {
            Saxon.Ejava.util.Objects.requireNonNull(rsc);
            Saxon.Hej.functions.SystemFunction fn = rsc.getConfiguration().makeSystemFunction(name, arity);
            if ((fn == null)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(((name+"#")+arity));
            }
            fn.setRetainedStaticContext(rsc);
            return fn;
        }
        public virtual Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments.Length>getArity()) && isSequenceVariadic())) {
                if ((getArity() != 1)) {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException("Not implemented: sequence-variadic function with arity>1");
                }
                arguments = new Saxon.Hej.expr.Expression[]{new Saxon.Hej.expr.instruct.Block(arguments)};
            }
            Saxon.Hej.expr.Expression e = new Saxon.Hej.expr.SystemFunctionCall(this, arguments);
            e.setRetainedStaticContext(getRetainedStaticContext());
            return e;
        }
        public void setArity(int arity) {
            this.arity = arity;
        }
        public override bool isSequenceVariadic() {
            return (((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.SEQV)) != 0);
        }
        public virtual int getNetCost() {
            return 1;
        }
        public virtual Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)) {
                return fixArguments(arguments);
            } else {
                return null;
            }
        }
        public virtual Saxon.Hej.expr.Expression fixArguments(params Saxon.Hej.expr.Expression[]  arguments) {
            for (int i = 0; (i<getArity()); i++) {
                if ((Saxon.Hej.expr.Literal.isEmptySequence(arguments[i]) && (resultIfEmpty(i) != null))) {
                    return Saxon.Hej.expr.Literal.makeLiteral(details.resultIfEmpty[i].materialize());
                }
            }
            return null;
        }
        protected internal virtual Saxon.Hej.om.Sequence resultIfEmpty(int arg) {
            return details.resultIfEmpty[arg];
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext getRetainedStaticContext() {
            return retainedStaticContext;
        }
        public virtual void setRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext) {
            this.retainedStaticContext = retainedStaticContext;
        }
        public bool dependsOnContextItem() {
            return (((details.properties&((Saxon.Hej.functions.registry.BuiltInFunctionSet.CITEM|Saxon.Hej.functions.registry.BuiltInFunctionSet.CDOC)))) != 0);
        }
        public virtual void setDetails(Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry) {
            details = entry;
        }
        public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry getDetails() {
            return details;
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return details.name;
        }
        public override string getDescription() {
            return details.name.getDisplayName();
        }
        public override int getArity() {
            return arity;
        }
        public override Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            Saxon.Hej.expr.OperandRole[] roles = new Saxon.Hej.expr.OperandRole[getArity()];
            Saxon.Hej.expr.OperandUsage[] usages;
            if (isSequenceVariadic()) {
                usages = new Saxon.Hej.expr.OperandUsage[getArity()];
                Saxon.Ejava.util.Arrays.fill(usages, details.usage[0]);
            } else {
                usages = details.usage;
            }
            try {
                for (int i = 0; (i<usages.Length); i++) {
                    roles[i] = new Saxon.Hej.expr.OperandRole(0, usages[i], getRequiredType(i));
                }
            } catch (System.IndexOutOfRangeException e) {
                System.Console.Error.WriteLine(e);
            }
            return roles;
        }
        public virtual Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return null;
        }
        public virtual void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression[] arguments) {
        }
        public override bool Equals(System.Object o) {
            return (((o is Saxon.Hej.functions.SystemFunction)) && base.Equals(o));
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public virtual string getErrorCodeForTypeErrors() {
            return "XPTY0004";
        }
        public virtual Saxon.Hej.value.SequenceType getRequiredType(int arg) {
            if ((details == null)) {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            }
            return details.argumentTypes[arg];
        }
        public virtual Saxon.Hej.type.ItemType getResultItemType() {
            return details.itemType;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.makeSequenceType(getResultItemType(), details.cardinality);
            return new Saxon.Hej.type.SpecificFunctionType(details.argumentTypes, resultType);
        }
        public virtual Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            if ((((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.AS_ARG0)) != 0)) {
                return args[0].getItemType();
            } else {
                if ((((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.AS_PRIM_ARG0)) != 0)) {
                    return args[0].getItemType().getPrimitiveItemType();
                } else {
                    return details.itemType;
                }
            }
        }
        public virtual int getCardinality(Saxon.Hej.expr.Expression[] args) {
            int c = details.cardinality;
            if ((((c == Saxon.Hej.functions.registry.BuiltInFunctionSet.OPT) && (((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.CARD0)) != 0)) && !(Saxon.Hej.value.Cardinality.allowsZero(args[0].getCardinality())))) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            } else {
                return c;
            }
        }
        public virtual int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            if ((((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.NEW)) != 0)) {
                return Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED;
            }
            int p = Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
            if ((((details.properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.SIDE)) != 0)) {
                p |= Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS;
            }
            return p;
        }
        protected internal Saxon.Hej.om.NodeInfo getContextNode(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Context item for "+getFunctionName())+"() is absent"), "XPDY0002");
                err.maybeSetContext(context);
                throw err;
            } else {
                if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Context item for "+getFunctionName())+"() is not a node"), "XPTY0004");
                    err.maybeSetContext(context);
                    throw err;
                } else {
                    return (Saxon.Hej.om.NodeInfo)(item);
                }
            }
        }
        public static Saxon.Hej.om.Sequence dynamicCall(Saxon.Hej.om.Function f, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            context = f.makeNewContext(context, null);
            context.setCurrentOutputUri(null);
            if ((context is Saxon.Hej.expr.XPathContextMajor)) {
                ((Saxon.Hej.expr.XPathContextMajor)(context)).setCurrentRegexIterator(null);
            }
            return f.call(context, args);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("fnRef");
            Saxon.Hej.om.StructuredQName qName = getFunctionName();
            string name = (qName.hasURI(Saxon.Hej.lib.NamespaceConstant.FN) ? qName.getLocalPart() : qName.getEQName());
            @out.emitAttribute("name", name);
            @out.emitAttribute("arity", (getArity()+""));
            if ((((getDetails().properties&Saxon.Hej.functions.registry.BuiltInFunctionSet.DEPENDS_ON_STATIC_CONTEXT)) != 0)) {
                @out.emitRetainedStaticContext(getRetainedStaticContext(), null);
            }
            @out.endElement();
        }
        public virtual Saxon.Hej.expr.Expression typeCheckCaller(Saxon.Hej.expr.FunctionCall caller, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return caller;
        }
        public override bool isTrustedResultType() {
            return true;
        }
        public string getStaticBaseUriString() {
            return getRetainedStaticContext().getStaticBaseUriString();
        }
        public virtual void exportAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        public virtual void exportAdditionalArguments(Saxon.Hej.expr.SystemFunctionCall call, Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        public virtual void importAttributes(Saxon.Ejava.util.Properties attributes) {
        }
        public virtual string getCompilerName() {
            return null;
        }
        public virtual string getStreamerName() {
            return null;
        }
        public override string toShortString() {
            return ((getFunctionName().getDisplayName()+'#')+getArity());
        }
        public override string ToString() {
            return ((getFunctionName().getDisplayName()+'#')+getArity());
        }
        protected internal Saxon.Hej.str.UnicodeString getUniStringArg(Saxon.Hej.om.Sequence supplied) {
            Saxon.Hej.value.StringValue item = (Saxon.Hej.value.StringValue)(supplied.head());
            return ((item == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : item.getUnicodeStringValue());
        }
    
    }
}