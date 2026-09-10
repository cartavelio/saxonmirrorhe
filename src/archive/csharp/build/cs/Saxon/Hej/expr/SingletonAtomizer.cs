using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class SingletonAtomizer : Saxon.Hej.expr.UnaryExpression {
        private readonly bool allowEmpty;
        private readonly Saxon.Hej.expr.parser.RoleDiagnostic roleDiagnostic;
        public SingletonAtomizer(Saxon.Hej.expr.Expression sequence, Saxon.Hej.expr.parser.RoleDiagnostic role, bool allowEmpty) : base (sequence) {
            this.allowEmpty = allowEmpty;
            this.roleDiagnostic = role;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC;
        }
        public bool isAllowEmpty() {
            return allowEmpty;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.Expression operand = getBaseExpression().simplify();
            if (((operand is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(operand)).getGroundedValue() is Saxon.Hej.value.AtomicValue))) {
                return operand;
            }
            setBaseExpression(operand);
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(this);
            if (Saxon.Hej.expr.Literal.isEmptySequence(operand)) {
                if (!(allowEmpty)) {
                    typeError(("An empty sequence is not allowed as the "+roleDiagnostic.getMessage()), roleDiagnostic.getErrorCode(), null);
                }
                return operand;
            }
            Saxon.Hej.type.ItemType operandType = operand.getItemType();
            if (operandType.isPlainType()) {
                return operand;
            }
            if (!(operandType.isAtomizable(visitor.getConfiguration().getTypeHierarchy()))) {
                Saxon.Hej.trans.XPathException err;
                if ((operandType is Saxon.Hej.ma.map.MapType)) {
                    err = new Saxon.Hej.trans.XPathException((("Cannot atomize a map ("+toShortString())+")"), "FOTY0013");
                } else {
                    if ((operandType is Saxon.Hej.type.FunctionItemType)) {
                        err = new Saxon.Hej.trans.XPathException("Cannot atomize a function item", "FOTY0013");
                    } else {
                        err = new Saxon.Hej.trans.XPathException("Cannot atomize an element that is defined in the schema to have element-only content", "FOTY0012");
                    }
                }
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                err.setFailingExpression(getParentExpression());
                throw err;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression exp = base.optimize(visitor, contextInfo);
            if ((exp == this)) {
                setBaseExpression(getBaseExpression().unordered(true, false));
                if ((getBaseExpression().getItemType().isPlainType() && !(Saxon.Hej.value.Cardinality.allowsMany(getBaseExpression().getCardinality())))) {
                    return getBaseExpression();
                }
                return this;
            } else {
                return exp;
            }
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.SingletonAtomizer(getBaseExpression().copy(rebindings), roleDiagnostic, allowEmpty);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
            return e2;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override string getStreamerName() {
            return "SingletonAtomizer";
        }
        public Saxon.Hej.expr.parser.RoleDiagnostic getRole() {
            return roleDiagnostic;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = getBaseExpression().addToPathMap(pathMap, pathMapNodeSet);
            if ((result != null)) {
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                Saxon.Hej.type.ItemType operandItemType = getBaseExpression().getItemType();
                if (((th.relationship(Saxon.Hej.pattern.NodeKindTest.ELEMENT, operandItemType) != Saxon.Hej.type.Affinity.DISJOINT) || (th.relationship(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, operandItemType) != Saxon.Hej.type.Affinity.DISJOINT))) {
                    result.setAtomized();
                }
            }
            return null;
        }
        public override Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            int found = 0;
            Saxon.Hej.value.AtomicValue result = null;
            Saxon.Hej.om.SequenceIterator iter = getBaseExpression().iterate(context);
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                Saxon.Hej.om.AtomicSequence seq;
                try {
                    seq = item.atomize();
                } catch (Exception e) when (e is Saxon.Hej.expr.instruct.TerminationException || e is Saxon.Hej.functions.Error.UserDefinedXPathException) {
                    throw;
                } catch (Saxon.Hej.trans.XPathException e) {
                    if ((roleDiagnostic == null)) {
                        throw;
                    } else {
                        string message = ((e.Message+". Failed while atomizing the ")+roleDiagnostic.getMessage());
                        Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(message, e.getErrorCodeLocalPart(), e.getLocator());
                        e2.setXPathContext(context);
                        e2.maybeSetLocation(getLocation());
                        throw e2;
                    }
                }
                found += seq.getLength();
                if ((found>1)) {
                    typeError((("A sequence of more than one item is not allowed as the "+roleDiagnostic.getMessage())+Saxon.Hej.expr.CardinalityChecker.depictSequenceStart(getBaseExpression().iterate(context), 3)), roleDiagnostic.getErrorCode(), context);
                }
                if ((found == 1)) {
                    result = ((/*1430*/Saxon.Hej.value.AtomicValue)seq.head());
                }
            }
            if (((found == 0) && !(allowEmpty))) {
                typeError(("An empty sequence is not allowed as the "+roleDiagnostic.getMessage()), roleDiagnostic.getErrorCode(), null);
            }
            return result;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            bool isSchemaAware = true;
            try {
                isSchemaAware = getPackageData().isSchemaAware();
            } catch (Saxon.Ejava.lang.NullPointerException err) {
                if (!(getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION))) {
                    isSchemaAware = false;
                }
            }
            Saxon.Hej.type.ItemType @in = getBaseExpression().getItemType();
            if (@in.isPlainType()) {
                return @in;
            } else {
                if ((@in is Saxon.Hej.pattern.NodeTest)) {
                    Saxon.Hej.type.UType kinds = @in.getUType();
                    if (!(isSchemaAware)) {
                        if (Saxon.Hej.expr.Atomizer.STRING_KINDS.subsumes(kinds)) {
                            return Saxon.Hej.type.BuiltInAtomicType.STRING;
                        }
                        if (Saxon.Hej.expr.Atomizer.UNTYPED_IF_UNTYPED_KINDS.subsumes(kinds)) {
                            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                        }
                    } else {
                        if (Saxon.Hej.expr.Atomizer.UNTYPED_KINDS.subsumes(kinds)) {
                            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                        }
                    }
                    return @in.getAtomizedItemType();
                } else {
                    if ((@in is Saxon.Hej.type.JavaExternalObjectType)) {
                        return @in.getAtomizedItemType();
                    }
                }
            }
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        protected internal override int computeCardinality() {
            if (allowEmpty) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            } else {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            }
        }
        public override string getExpressionName() {
            return "atomizeSingleton";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("atomSing", this);
            if (allowEmpty) {
                @out.emitAttribute("card", "?");
            }
            @out.emitAttribute("diag", getRole().save());
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            return getBaseExpression().toShortString();
        }
    
    }
}