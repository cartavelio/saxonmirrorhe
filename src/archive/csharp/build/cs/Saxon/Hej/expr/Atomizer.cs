using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class Atomizer : Saxon.Hej.expr.UnaryExpression {
        private bool untyped = false;
        private bool singleValued = false;
        private Saxon.Hej.type.ItemType operandItemType = null;
        private Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier = null;
        public Atomizer(Saxon.Hej.expr.Expression sequence, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role) : base (sequence) {
            this.roleSupplier = role;
            sequence.setFlattened(true);
        }
        public static Saxon.Hej.expr.Expression makeAtomizer(Saxon.Hej.expr.Expression sequence, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier) {
            if (((sequence is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(sequence)).getGroundedValue() is Saxon.Hej.om.AtomicSequence))) {
                return sequence;
            } else {
                return new Saxon.Hej.expr.Atomizer(sequence, roleSupplier);
            }
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE;
        }
        public override int getImplementationMethod() {
            return (ITERATE_METHOD|WATCH_METHOD);
        }
        public Saxon.Hej.type.ItemType getOperandItemType() {
            if ((operandItemType == null)) {
                operandItemType = getBaseExpression().getItemType();
            }
            return operandItemType;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            untyped = !(getPackageData().isSchemaAware());
            computeSingleValued(getConfiguration().getTypeHierarchy());
            Saxon.Hej.expr.Expression operand = getBaseExpression().simplify();
            if ((operand is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.expr.Literal)(operand)).getGroundedValue();
                if ((val is Saxon.Hej.value.AtomicValue)) {
                    return operand;
                }
                Saxon.Hej.om.SequenceIterator iter = val.iterate();
                Saxon.Hej.om.Item i;
                while (((i = iter.next()) != null)) {
                    if ((i is Saxon.Hej.om.NodeInfo)) {
                        return this;
                    }
                    if ((i is Saxon.Hej.om.FunctionItem)) {
                        if (((Saxon.Hej.om.FunctionItem)(i)).isArray()) {
                            return this;
                        } else {
                            if (((Saxon.Hej.om.FunctionItem)(i)).isMap()) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(expandMessage((("Cannot atomize a map ("+i.toShortString())+")")), "FOTY0013");
                                err.setIsTypeError(true);
                                err.setLocation(getLocation());
                                throw err;
                            } else {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(expandMessage("Cannot atomize a function item"), "FOTY0013");
                                err.setIsTypeError(true);
                                err.setLocation(getLocation());
                                throw err;
                            }
                        }
                    }
                }
                return operand;
            } else {
                if (((operand is Saxon.Hej.expr.instruct.ValueOf) && !(Saxon.Hej.@event.ReceiverOption.contains(((Saxon.Hej.expr.instruct.ValueOf)(operand)).getOptions(), Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING)))) {
                    return ((Saxon.Hej.expr.instruct.ValueOf)(operand)).convertToCastAsString();
                }
            }
            setBaseExpression(operand);
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            untyped = (untyped|!(visitor.getStaticContext().getPackageData().isSchemaAware()));
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            computeSingleValued(th);
            resetLocalStaticProperties();
            Saxon.Hej.type.ItemType operandType = getOperandItemType();
            if (th.isSubType(operandType, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                return getBaseExpression();
            }
            if (!(operandType.isAtomizable(th))) {
                Saxon.Hej.trans.XPathException err;
                if ((operandType is Saxon.Hej.type.FunctionItemType)) {
                    string thing = ((operandType is Saxon.Hej.ma.map.MapType) ? "map" : "function item");
                    err = new Saxon.Hej.trans.XPathException(expandMessage(("Cannot atomize a "+thing)), "FOTY0013");
                } else {
                    err = new Saxon.Hej.trans.XPathException(expandMessage("Cannot atomize an element that is defined in the schema to have element-only content"), "FOTY0012");
                }
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            }
            getBaseExpression().setFlattened(true);
            return this;
        }
        private void computeSingleValued(Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.type.ItemType operandType = getOperandItemType();
            if ((th.relationship(operandType, Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE) != Saxon.Hej.type.Affinity.DISJOINT)) {
                singleValued = false;
            } else {
                singleValued = untyped;
                if (!(singleValued)) {
                    Saxon.Hej.type.ItemType nodeType = getBaseExpression().getItemType();
                    if ((nodeType is Saxon.Hej.pattern.NodeTest)) {
                        if (!(nodeType.getUType().overlaps(Saxon.Hej.type.UType.ELEMENT.union(Saxon.Hej.type.UType.ATTRIBUTE)))) {
                            singleValued = true;
                        } else {
                            Saxon.Hej.type.SchemaType st = ((Saxon.Hej.pattern.NodeTest)(nodeType)).getContentType();
                            if (isSingleValuedSchemaType(st)) {
                                singleValued = true;
                            }
                        }
                    }
                }
            }
        }
        private bool isSingleValuedSchemaType(Saxon.Hej.type.SchemaType st) {
            if ((st == Saxon.Hej.type.Untyped.getInstance())) {
                return true;
            }
            if (st.isSimpleType()) {
                Saxon.Hej.type.SimpleType sim = (Saxon.Hej.type.SimpleType)(st);
                if (sim.isAtomicType()) {
                    return true;
                } else {
                    if (sim.isListType()) {
                        return false;
                    } else {
                        if (sim.isUnionType()) {
                            return ((Saxon.Hej.type.UnionType)(sim)).isPlainType();
                        } else {
                            return false;
                        }
                    }
                }
            }
            if (st.isComplexType()) {
                if ((st == Saxon.Hej.type.AnyType.getInstance())) {
                    return false;
                }
                if (((Saxon.Hej.type.ComplexType)(st)).isSimpleContent()) {
                    return isSingleValuedSchemaType(((Saxon.Hej.type.ComplexType)(st)).getSimpleContentType());
                }
            }
            return false;
        }
        private string expandMessage(string message) {
            if ((roleSupplier == null)) {
                return message;
            } else {
                return ((message+". Found while atomizing the ")+roleSupplier().getMessage());
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression exp = base.optimize(visitor, contextInfo);
            if ((exp == this)) {
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                Saxon.Hej.expr.Expression operand = getBaseExpression();
                if (th.isSubType(operand.getItemType(), Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                    return operand;
                }
                if (((operand is Saxon.Hej.expr.instruct.ValueOf) && !(Saxon.Hej.@event.ReceiverOption.contains(((Saxon.Hej.expr.instruct.ValueOf)(operand)).getOptions(), Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING)))) {
                    Saxon.Hej.expr.Expression cast = ((Saxon.Hej.expr.instruct.ValueOf)(operand)).convertToCastAsString();
                    return cast.optimize(visitor, contextInfo);
                }
                if (((operand is Saxon.Hej.expr.LetExpression) || (operand is Saxon.Hej.expr.ForExpression))) {
                    Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.Assignation)(operand)).getAction();
                    ((Saxon.Hej.expr.Assignation)(operand)).setAction(new Saxon.Hej.expr.Atomizer(action, roleSupplier));
                    return operand.optimize(visitor, contextInfo);
                }
                if ((operand is Saxon.Hej.expr.instruct.Choose)) {
                    ((Saxon.Hej.expr.instruct.Choose)(operand)).atomizeActions();
                    return operand.optimize(visitor, contextInfo);
                }
                if ((operand is Saxon.Hej.expr.instruct.Block)) {
                    Saxon.Hej.expr.Operand[] children = ((Saxon.Hej.expr.instruct.Block)(operand)).getOperanda();
                    Saxon.Hej.expr.Expression[] atomizedChildren = new Saxon.Hej.expr.Expression[children.Length];
                    for (int i = 0; (i<children.Length); i++) {
                        atomizedChildren[i] = new Saxon.Hej.expr.Atomizer(children[i].getChildExpression(), roleSupplier);
                    }
                    Saxon.Hej.expr.instruct.Block newBlock = new Saxon.Hej.expr.instruct.Block(atomizedChildren);
                    return newBlock.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                }
                if (((((untyped && (operand is Saxon.Hej.expr.AxisExpression)) && (((Saxon.Hej.expr.AxisExpression)(operand)).getAxis() == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) && (((Saxon.Hej.expr.AxisExpression)(operand)).getNodeTest() is Saxon.Hej.pattern.NameTest)) && !(((Saxon.Hej.expr.AxisExpression)(operand)).isContextPossiblyUndefined()))) {
                    Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.expr.AxisExpression)(operand)).getNodeTest().getMatchingNodeName();
                    Saxon.Hej.om.FingerprintedQName qName = new Saxon.Hej.om.FingerprintedQName(name, visitor.getConfiguration().getNamePool());
                    Saxon.Hej.expr.AttributeGetter ag = new Saxon.Hej.expr.AttributeGetter(qName);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ag);
                    return ag;
                }
                if ((((untyped && (operand is Saxon.Hej.expr.SimpleStepExpression)) && (((Saxon.Hej.expr.SimpleStepExpression)(operand)).getAxisExpression().getAxis() == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) && (((Saxon.Hej.expr.SimpleStepExpression)(operand)).getAxisExpression().getNodeTest() is Saxon.Hej.pattern.NameTest))) {
                    Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.expr.SimpleStepExpression)(operand)).getAxisExpression().getNodeTest().getMatchingNodeName();
                    Saxon.Hej.om.FingerprintedQName qName = new Saxon.Hej.om.FingerprintedQName(name, visitor.getConfiguration().getNamePool());
                    Saxon.Hej.expr.AttributeGetter ag = new Saxon.Hej.expr.AttributeGetter(qName);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ag);
                    return new Saxon.Hej.expr.SlashExpression(((Saxon.Hej.expr.SimpleStepExpression)(operand)).getStart(), ag);
                }
            }
            return exp;
        }
        public bool isUntyped() {
            return untyped;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            p &= ~(Saxon.Hej.expr.StaticProperty.NODESET_PROPERTIES);
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override void resetLocalStaticProperties() {
            base.resetLocalStaticProperties();
            operandItemType = null;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Atomizer copy = new Saxon.Hej.expr.Atomizer(getBaseExpression().copy(rebindings), roleSupplier);
            copy.untyped = untyped;
            copy.singleValued = singleValued;
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, copy);
            return copy;
        }
        public override string getStreamerName() {
            return "Atomizer";
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.om.SequenceIterator @base = getBaseExpression().iterate(context);
                return getAtomizingIterator(@base, (untyped && (operandItemType is Saxon.Hej.pattern.NodeTest)));
            } catch (Exception e) when (e is Saxon.Hej.expr.instruct.TerminationException || e is Saxon.Hej.functions.Error.UserDefinedXPathException) {
                throw;
            } catch (Saxon.Hej.trans.XPathException e) {
                if ((roleSupplier == null)) {
                    throw;
                } else {
                    string message = expandMessage(e.Message);
                    Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(message, e.getErrorCodeLocalPart(), e.getLocator());
                    e2.setXPathContext(context);
                    e2.maybeSetLocation(getLocation());
                    throw e2;
                }
            }
        }
        public override Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/makeElaborator().elaborateForItem()(context));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            operandItemType = getBaseExpression().getItemType();
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            return getAtomizedItemType(getBaseExpression(), untyped, th);
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.ANY_ATOMIC.intersection(getItemType().getUType());
        }
        public static Saxon.Hej.type.ItemType getAtomizedItemType(Saxon.Hej.expr.Expression operand, bool alwaysUntyped, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.type.ItemType @in = operand.getItemType();
            if (@in.isPlainType()) {
                return @in;
            } else {
                if ((@in is Saxon.Hej.pattern.NodeTest)) {
                    Saxon.Hej.type.UType kinds = @in.getUType();
                    if (alwaysUntyped) {
                        if (STRING_KINDS.subsumes(kinds)) {
                            return Saxon.Hej.type.BuiltInAtomicType.STRING;
                        }
                        if (UNTYPED_IF_UNTYPED_KINDS.subsumes(kinds)) {
                            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                        }
                    } else {
                        if (UNTYPED_KINDS.subsumes(kinds)) {
                            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                        }
                    }
                    return @in.getAtomizedItemType();
                } else {
                    if ((@in is Saxon.Hej.type.JavaExternalObjectType)) {
                        return @in.getAtomizedItemType();
                    } else {
                        if ((@in is Saxon.Hej.ma.arrays.ArrayItemType)) {
                            Saxon.Hej.type.PlainType ait = ((Saxon.Hej.ma.arrays.ArrayItemType)(@in)).getMemberType().getPrimaryType().getAtomizedItemType();
                            return ((ait == null) ? Saxon.Hej.type.ErrorType.getInstance() : ait);
                        } else {
                            if ((@in is Saxon.Hej.type.FunctionItemType)) {
                                return Saxon.Hej.type.ErrorType.getInstance();
                            }
                        }
                    }
                }
            }
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public static readonly Saxon.Hej.type.UType STRING_KINDS = Saxon.Hej.type.UType.NAMESPACE.union(Saxon.Hej.type.UType.COMMENT).union(Saxon.Hej.type.UType.PI);
        public static readonly Saxon.Hej.type.UType UNTYPED_KINDS = Saxon.Hej.type.UType.TEXT.union(Saxon.Hej.type.UType.DOCUMENT);
        public static readonly Saxon.Hej.type.UType UNTYPED_IF_UNTYPED_KINDS = Saxon.Hej.type.UType.TEXT.union(Saxon.Hej.type.UType.ELEMENT).union(Saxon.Hej.type.UType.DOCUMENT).union(Saxon.Hej.type.UType.ATTRIBUTE);
        protected internal override int computeCardinality() {
            Saxon.Hej.type.ItemType @in = getOperandItemType();
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            if (singleValued) {
                return operand.getCardinality();
            } else {
                if ((untyped && (@in is Saxon.Hej.pattern.NodeTest))) {
                    return operand.getCardinality();
                } else {
                    if (Saxon.Hej.value.Cardinality.allowsMany(operand.getCardinality())) {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                    } else {
                        if (@in.isPlainType()) {
                            return operand.getCardinality();
                        } else {
                            if ((@in is Saxon.Hej.pattern.NodeTest)) {
                                Saxon.Hej.type.SchemaType schemaType = ((Saxon.Hej.pattern.NodeTest)(@in)).getContentType();
                                if (schemaType.isAtomicType()) {
                                    return operand.getCardinality();
                                }
                            }
                        }
                    }
                }
            }
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
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
        public static Saxon.Hej.om.SequenceIterator getAtomizingIterator(Saxon.Hej.om.SequenceIterator @base, bool oneToOne) {
            if (Saxon.Hej.om.SequenceTool.supportsGetLength(@base)) {
                int count = Saxon.Hej.om.SequenceTool.getLength(@base);
                if ((count == 0)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                } else {
                    if ((count == 1)) {
                        Saxon.Hej.om.Item first = @base.next();
                        Saxon.Ejava.util.Objects.requireNonNull(first);
                        return ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)first.atomize().iterate());
                    }
                }
            } else {
                if ((@base is Saxon.Hej.om.AtomizedValueIterator)) {
                    return new Saxon.Hej.expr.AxisAtomizingIterator((Saxon.Hej.om.AtomizedValueIterator)(@base));
                }
            }
            if (oneToOne) {
                return new Saxon.Hej.tree.iter.UntypedAtomizingIterator(@base);
            } else {
                return new Saxon.Hej.tree.iter.AtomizingIterator(@base);
            }
        }
        public static Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.Sequence sequence) {
            if ((sequence is Saxon.Hej.om.AtomicSequence)) {
                return (Saxon.Hej.om.AtomicSequence)(sequence);
            } else {
                if ((sequence is Saxon.Hej.value.EmptySequence)) {
                    return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
                } else {
                    Saxon.Hej.om.SequenceIterator iter = getAtomizingIterator(sequence.iterate(), false);
                    return new Saxon.Hej.om.AtomicArray(iter);
                }
            }
        }
        public override string getExpressionName() {
            return "data";
        }
        public override string ToString() {
            return (("data("+getBaseExpression().ToString())+")");
        }
        public override string toShortString() {
            return getBaseExpression().toShortString();
        }
        protected internal override void emitExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            if ((roleSupplier != null)) {
                @out.emitAttribute("diag", roleSupplier().save());
            }
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.Atomizer.AtomizerElaborator();
        }
        public class AtomizerElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.Atomizer expr = (Saxon.Hej.expr.Atomizer)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBaseExpression().makeElaborator().elaborateForPull();
                bool oneToOne = (expr.isUntyped() && (expr.getBaseExpression().getItemType() is Saxon.Hej.pattern.NodeTest));
                return context => {
                    try {
                        Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/baseEval(context);
                        return getAtomizingIterator(@base, oneToOne);
                    } catch (Exception e) when (e is Saxon.Hej.expr.instruct.TerminationException || e is Saxon.Hej.functions.Error.UserDefinedXPathException) {
                        throw;
                    } catch (Saxon.Hej.trans.XPathException e) {
                        if ((expr.roleSupplier == null)) {
                            throw;
                        } else {
                            string message = expr.expandMessage(e.Message);
                            Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(message, e.getErrorCodeLocalPart(), e.getLocator());
                            e2.setXPathContext(context);
                            e2.maybeSetLocation(expr.getLocation());
                            throw e2;
                        }
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.Atomizer expr = (Saxon.Hej.expr.Atomizer)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator baseEval = expr.getBaseExpression().makeElaborator().elaborateForItem();
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(expr.getBaseExpression().getCardinality());
                if (nullable) {
                    return context => {
                        Saxon.Hej.om.Item it = /*Delegate.eval()*/baseEval(context);
                        if ((it == null)) {
                            return null;
                        }
                        return ((/*1430*/Saxon.Hej.value.AtomicValue)it.atomize().head());
                    } ;
                } else {
                    return context => /*Delegate.eval()*/baseEval(context).atomize().head();
                }
            }
        
        }
    
    }
}