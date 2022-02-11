using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class IntegratedFunctionCall : Saxon.Hej.expr.FunctionCall, Saxon.Hej.expr.Callable {
        private readonly Saxon.Hej.om.StructuredQName name;
        private readonly Saxon.Hej.lib.ExtensionFunctionCall function;
        private Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        private int state = 0;
        public IntegratedFunctionCall(Saxon.Hej.om.StructuredQName name, Saxon.Hej.lib.ExtensionFunctionCall function)  {
            this.name = name;
            this.function = function;
        }
        public void setResultType(Saxon.Hej.value.SequenceType resultType) {
            this.resultType = resultType;
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return name;
        }
        public override Saxon.Hej.om.Function getTargetFunction(Saxon.Hej.expr.XPathContext context) {
            return null;
        }
        public Saxon.Hej.lib.ExtensionFunctionCall getFunction() {
            return function;
        }
        protected internal override void checkArguments(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.lib.ExtensionFunctionDefinition definition = function.getDefinition();
            checkArgumentCount(definition.getMinimumNumberOfArguments(), definition.getMaximumNumberOfArguments());
            int args = getArity();
            Saxon.Hej.value.SequenceType[] declaredArgumentTypes = definition.getArgumentTypes();
            if (((declaredArgumentTypes == null) || (((args != 0) && (declaredArgumentTypes.Length == 0))))) {
                throw new Saxon.Hej.trans.XPathException((("Integrated function "+getDisplayName())+" failed to declare its argument types"));
            }
            Saxon.Hej.value.SequenceType[] actualArgumentTypes = new Saxon.Hej.value.SequenceType[args];
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            for (int i = 0; (i<args); i++) {
                setArg(i, tc.staticTypeCheck(getArg(i), ((i<declaredArgumentTypes.Length) ? declaredArgumentTypes[i] : declaredArgumentTypes[(declaredArgumentTypes.Length-1)]), new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, getFunctionName().getDisplayName(), i), visitor));
                actualArgumentTypes[i] = Saxon.Hej.value.SequenceType.makeSequenceType(getArg(i).getItemType(), getArg(i).getCardinality());
            }
            resultType = definition.getResultType(actualArgumentTypes);
            if ((state == 0)) {
                function.supplyStaticContext(visitor.getStaticContext(), 0, getArguments());
            }
            state++;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression exp = base.typeCheck(visitor, contextInfo);
            if ((exp is Saxon.Hej.functions.IntegratedFunctionCall)) {
                Saxon.Hej.expr.Expression exp2 = ((Saxon.Hej.functions.IntegratedFunctionCall)(exp)).function.rewrite(visitor.getStaticContext(), getArguments());
                if ((exp2 == null)) {
                    return exp;
                } else {
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp2);
                    return exp2.simplify().typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                }
            }
            return exp;
        }
        public override Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return resultType.getPrimaryType();
        }
        protected internal override int computeCardinality() {
            return resultType.getCardinality();
        }
        public override int getIntrinsicDependencies() {
            Saxon.Hej.lib.ExtensionFunctionDefinition definition = function.getDefinition();
            return (definition.dependsOnFocus() ? Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS : 0);
        }
        protected internal override int computeSpecialProperties() {
            Saxon.Hej.lib.ExtensionFunctionDefinition definition = function.getDefinition();
            return (definition.hasSideEffects() ? Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS : Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.lib.ExtensionFunctionCall newCall = function.getDefinition().makeCallExpression();
            newCall.setDefinition(function.getDefinition());
            function.copyLocalData(newCall);
            Saxon.Hej.functions.IntegratedFunctionCall copy = new Saxon.Hej.functions.IntegratedFunctionCall(getFunctionName(), newCall);
            Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[getArity()];
            for (int i = 0; (i<args.Length); i++) {
                args[i] = getArg(i).copy(rebindings);
            }
            copy.setArguments(args);
            copy.resultType = resultType;
            copy.state = state;
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, copy);
            return copy;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("ifCall", this);
            @out.emitAttribute("name", getFunctionName());
            @out.emitAttribute("type", resultType.toAlphaCode());
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            @out.endElement();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ExtensionFunctionDefinition definition = function.getDefinition();
            Saxon.Hej.om.Sequence[] argValues = new Saxon.Hej.om.Sequence[getArity()];
            for (int i = 0; (i<argValues.Length); i++) {
                argValues[i] = Saxon.Hej.om.SequenceTool.toLazySequence(getArg(i).iterate(context));
            }
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION_RESULT, getFunctionName().getDisplayName(), 0);
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.om.SequenceIterator result;
            try {
                result = function.call(context, argValues).iterate();
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                throw;
            }
            if (!(definition.trustResultType())) {
                int card = resultType.getCardinality();
                if ((card != Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                    result = new Saxon.Hej.expr.CardinalityCheckingIterator(result, card, role, getLocation());
                }
                Saxon.Hej.type.ItemType type = resultType.getPrimaryType();
                if ((type != Saxon.Hej.type.AnyItemType.getInstance())) {
                    result = new Saxon.Hej.expr.ItemMappingIterator(result, Saxon.Hej.expr.ItemMapper.of(item => {
                        if (!(type.matches(item, th))) {
                            string msg = role.composeErrorMessage(type, item, th);
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, "XPTY0004");
                            err.setLocation(getLocation());
                            throw err;
                        }
                        return item;
                    } ), true);
                }
                if ((th.relationship(type, Saxon.Hej.pattern.AnyNodeTest.getInstance()) != Saxon.Hej.type.Affinity.DISJOINT)) {
                    result = new Saxon.Hej.expr.ItemMappingIterator(result, new Saxon.Hej.functions.IntegratedFunctionCall.ConfigurationCheckingFunction(context.getConfiguration()), true);
                }
            }
            return result;
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence[] argValues = new Saxon.Hej.om.Sequence[getArity()];
            for (int i = 0; (i<argValues.Length); i++) {
                argValues[i] = Saxon.Hej.om.SequenceTool.toLazySequence(getArg(i).iterate(context));
            }
            try {
                return function.effectiveBooleanValue(context, argValues);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                throw;
            }
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return function.call(context, arguments);
        }
        public class ConfigurationCheckingFunction : Saxon.Hej.expr.ItemMappingFunction {
            private readonly Saxon.Hej.Configuration config;
            public ConfigurationCheckingFunction(Saxon.Hej.Configuration config)  {
                this.config = config;
            }
            public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
                if (((item is Saxon.Hej.om.NodeInfo) && !(config.isCompatible(((Saxon.Hej.om.NodeInfo)(item)).getConfiguration())))) {
                    throw new Saxon.Hej.trans.XPathException("Node returned by extension function was created with an incompatible Configuration", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
                }
                return item;
            }
        
        }
    
    }
}