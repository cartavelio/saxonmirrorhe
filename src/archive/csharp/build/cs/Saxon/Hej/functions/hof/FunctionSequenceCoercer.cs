using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    #pragma warning disable 659
    public sealed class FunctionSequenceCoercer : Saxon.Hej.expr.UnaryExpression {
        private readonly Saxon.Hej.type.SpecificFunctionType requiredItemType;
        private readonly Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier;
        public FunctionSequenceCoercer(Saxon.Hej.expr.Expression sequence, Saxon.Hej.type.SpecificFunctionType requiredItemType, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role) : base (sequence) {
            this.requiredItemType = requiredItemType;
            this.roleSupplier = role;
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(sequence, this);
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.INSPECT;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            setBaseExpression(getBaseExpression().simplify());
            if ((getBaseExpression() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue val = Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(new Saxon.Hej.expr.EarlyEvaluationContext(getConfiguration())));
                return Saxon.Hej.expr.Literal.makeLiteral(val, this);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if (th.isSubType(getBaseExpression().getItemType(), requiredItemType)) {
                return getBaseExpression();
            } else {
                return this;
            }
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.functions.hof.FunctionSequenceCoercer fsc2 = new Saxon.Hej.functions.hof.FunctionSequenceCoercer(getBaseExpression().copy(rebindings), requiredItemType, roleSupplier);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, fsc2);
            return fsc2;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override Saxon.Hej.om.FunctionItem evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.om.FunctionItem)(/*Delegate.eval()*/makeElaborator().elaborateForItem()(context));
        }
        public override Saxon.Hej.type.SpecificFunctionType getItemType() {
            return requiredItemType;
        }
        protected internal override int computeCardinality() {
            return getBaseExpression().getCardinality();
        }
        public Saxon.Hej.expr.parser.RoleDiagnostic getRoleSupplier() {
            return roleSupplier();
        }
        public override bool Equals(System.Object other) {
            return (base.Equals(other) && requiredItemType.Equals(((Saxon.Hej.functions.hof.FunctionSequenceCoercer)(other)).requiredItemType));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^requiredItemType.GetHashCode());
        }
        public override string getExpressionName() {
            return "fnCoercer";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("fnCoercer", this);
            Saxon.Hej.value.SequenceType st = Saxon.Hej.value.SequenceType.makeSequenceType(requiredItemType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            destination.emitAttribute("to", st.toAlphaCode());
            destination.emitAttribute("diag", roleSupplier().save());
            getBaseExpression().export(destination);
            destination.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.hof.FunctionSequenceCoercer.FunctionSequenceCoercerElaborator();
        }
        private static void checkAnnotations(Saxon.Hej.om.FunctionItem item, Saxon.Hej.type.FunctionItemType requiredItemType, Saxon.Hej.Configuration config) {
            foreach (Saxon.Hej.query.Annotation ann in requiredItemType.getAnnotationAssertions()) {
                Saxon.Hej.lib.FunctionAnnotationHandler handler = config.getFunctionAnnotationHandler(ann.getAnnotationQName().getNamespaceUri());
                if (((handler != null) && !(handler.satisfiesAssertion(ann, item.getAnnotations())))) {
                    throw new Saxon.Hej.trans.XPathException("Supplied function does not satisfy the annotation assertions of the required function type", "XPTY0004");
                }
            }
        }
        public class Coercer : Saxon.Hej.expr.ItemMappingFunction {
            private readonly Saxon.Hej.type.SpecificFunctionType requiredItemType;
            private readonly Saxon.Hej.Configuration config;
            private readonly Saxon.Hej.s9api.Location locator;
            public Coercer(Saxon.Hej.type.SpecificFunctionType requiredItemType, Saxon.Hej.Configuration config, Saxon.Hej.s9api.Location locator)  {
                this.requiredItemType = requiredItemType;
                this.config = config;
                this.locator = locator;
            }
            public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
                if (!(((item is Saxon.Hej.om.FunctionItem)))) {
                    throw new Saxon.Hej.trans.XPathException((("Function coercion attempted on an item ("+item.toShortString())+") which is not a function"), "XPTY0004", locator);
                }
                try {
                    checkAnnotations((Saxon.Hej.om.FunctionItem)(item), requiredItemType, config);
                    return new Saxon.Hej.functions.hof.CoercedFunction((Saxon.Hej.om.FunctionItem)(item), requiredItemType);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(locator);
                    throw;
                }
            }
        
        }
        private class FunctionSequenceCoercerElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.functions.hof.FunctionSequenceCoercer expr = (Saxon.Hej.functions.hof.FunctionSequenceCoercer)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator @base = expr.getBaseExpression().makeElaborator().elaborateForPull();
                Saxon.Hej.functions.hof.FunctionSequenceCoercer.Coercer coercer = new Saxon.Hej.functions.hof.FunctionSequenceCoercer.Coercer(expr.requiredItemType, expr.getConfiguration(), expr.getLocation());
                return context => new Saxon.Hej.expr.ItemMappingIterator(/*Delegate.iterate()*/@base(context), coercer, true);
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.functions.hof.FunctionSequenceCoercer expr = (Saxon.Hej.functions.hof.FunctionSequenceCoercer)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator @base = expr.getBaseExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.functions.hof.FunctionSequenceCoercer.Coercer coercer = new Saxon.Hej.functions.hof.FunctionSequenceCoercer.Coercer(expr.requiredItemType, expr.getConfiguration(), expr.getLocation());
                return context => {
                    Saxon.Hej.om.Item item = /*Delegate.eval()*/@base(context);
                    if ((item == null)) {
                        return null;
                    }
                    return ((/*1430*/Saxon.Hej.om.FunctionItem)coercer.mapItem(item));
                } ;
            }
        
        }
    
    }
}