////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.ItemEvaluator;
import net.sf.saxon.expr.elab.PullElaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.jnode.AnyJNodeType;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.function.Supplier;


/**
 * A lookup expression is an expression of the form A?B. Here A must be a sequence of maps or arrays.
 * In the general case B is an expression that computes a key/index into the map or array; the case where
 * B is constant needs to be handled efficiently. The class also implements the unary lookup expression
 * ?B, which is interpreted as .?B. It does not handle the case A?* - that is handled as a LookupAllExpression.
 */

public class LookupByKey extends BinaryExpression {

    private boolean isClassified = false;
    protected boolean isArrayLookup = false;
    protected boolean isMapLookup = false;
    protected boolean isSingleContainer = false;
    protected boolean isSingleEntry = false;

    protected final int axis;

    /**
     * Constructor
     *
     * @param start The left hand operand (which must always select a sequence of maps or arrays).
     * @param step  The step to be followed from each map/array in the start expression to yield a new
     *              sequence
     */

    public LookupByKey(Expression start, int axis, Expression step) {
        super(start, OperatorSymbol.LOOKUP, step);
        this.axis = axis;
    }

    @Override
    protected OperandRole getOperandRole(int arg) {
        return arg == 0 ? OperandRole.INSPECT : OperandRole.ABSORB;
    }

    @Override
    public String getExpressionName() {
        return "lookupExp";
    }


    /**
     * Determine the data type of the items returned by this expression
     *
     * @return the type of the expression, as far as this is known. Prior to type-checking,
     * the method returns {@link AnyItemType}
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        if (isClassified) {
            if (isArrayLookup) {
                ItemType arrayType = getLhsExpression().getItemType();
                if (arrayType instanceof ArrayItemType) {
                    return ((ArrayItemType) arrayType).getMemberType().getPrimaryType();
                }
            } else if (isMapLookup) {
                ItemType mapType = getLhsExpression().getItemType();
                if (mapType instanceof MapType) {
                    return ((MapType) mapType).getValueType().getPrimaryType();
                }
            }
        }
        return AnyItemType.getInstance();
    }


    /**
     * Get the static type of the expression as a UType, following precisely the type
     * inference rules defined in the XSLT 3.0 specification.
     *
     * @param contextItemType not used
     * @return the static item type of the expression according to the XSLT 3.0 defined rules
     */
    @Override
    public UType getStaticUType(UType contextItemType) {
        return getItemType().getUType();
    }

    /**
     * Type-check the expression
     */

    /*@NotNull*/
    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {

        Configuration config = visitor.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();

        if (Literal.isEmptySequence(getLhsExpression())) {
            return getLhsExpression();
        }
        // Running typeCheck on the first operand can lose static type information if it's declared
        // with a tuple type. So check this first.
        ItemType originalType = getLhsExpression().getItemType();
        // Check the first operand
        getLhs().typeCheck(visitor, contextInfo);

        ItemType containerType = getLhsExpression().getItemType();
        isArrayLookup = containerType instanceof ArrayItemType;
        isMapLookup = containerType instanceof MapType;
        //#if CSHARP==false
        if (th.isSubType(containerType, AnyExternalObjectType.THE_INSTANCE)) {
            config.checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION, "use of lookup expressions on external objects", -1);
            return config.makeObjectLookupExpression(getLhsExpression(), getRhsExpression())
                    .typeCheck(visitor, contextInfo);
        }
        //#endif
        isSingleContainer = getLhsExpression().getCardinality() == StaticProperty.EXACTLY_ONE;
        
        if (!isArrayLookup && !isMapLookup) {
            // TODO: improve error handling here
            if (th.relationship(containerType, MapType.ANY_MAP_TYPE) == Affinity.DISJOINT &&
                    th.relationship(containerType, ArrayItemType.getInstance()) == Affinity.DISJOINT &&
                    th.relationship(containerType, AnyJNodeType.getInstance()) == Affinity.DISJOINT &&
                    th.relationship(containerType, AnyExternalObjectType.THE_INSTANCE) == Affinity.DISJOINT) {
                if (Cardinality.allowsZero(getLhsExpression().getCardinality())) {
                    visitor.issueWarning("The left-hand operand of '?' must be a map or an array; the expression can succeed only if the operand is an empty sequence " +
                                                 containerType, SaxonErrorCode.SXWN9026, getLocation());
                } else {
                    throw new XPathException("The left-hand operand of '?' must be a map or an array; "
                                                     + "the supplied expression is of type " + containerType, "XPTY0004")
                            .withLocation(getLocation())
                            .asTypeError()
                            .withFailingExpression(this);
                }
            } else {
                final TypeChecker tc = config.getTypeChecker(false);
                Supplier<RoleDiagnostic> role = () ->
                        new RoleDiagnostic(RoleDiagnostic.BINARY_EXPR, "?", 0);
                setLhsExpression(tc.staticTypeCheck(
                        getLhsExpression(),
                        SequenceType.zeroOrMore(
                                ChoiceItemType.of(MapType.ANY_MAP_TYPE,
                                                  ArrayItemType.ANY_ARRAY_TYPE,
                                                  AnyJNodeType.getInstance(),
                                                  AnyExternalObjectType.THE_INSTANCE)),
                        role,
                        visitor));
            }
        }

        // Now check the second operand

        getRhs().typeCheck(visitor, contextInfo);
        Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.BINARY_EXPR, "?", 1);
        TypeChecker tc = config.getTypeChecker(false);
        SequenceType req = BuiltInAtomicType.ANY_ATOMIC.zeroOrMore();
//        if (isArrayLookup) {
//            req = BuiltInAtomicType.INTEGER.zeroOrMore();
//        }
        setRhsExpression(tc.staticTypeCheck(getRhsExpression(), req, role, visitor));
        isSingleEntry = getRhsExpression().getCardinality() == StaticProperty.EXACTLY_ONE;

        isClassified = true;
        return this;
    }

    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        getLhs().optimize(visitor, contextInfo);
        getRhs().optimize(visitor, contextInfo);
        return this;
    }


    /**
     * Return the estimated cost of evaluating an expression. This is a very crude measure based
     * on the syntactic form of the expression (we have no knowledge of data values). We take
     * the cost of evaluating a simple scalar comparison or arithmetic expression as 1 (one),
     * and we assume that a sequence has length 5. The resulting estimates may be used, for
     * example, to reorder the predicates in a filter expression so cheaper predicates are
     * evaluated first.
     * @return a rough estimate of the cost of evaluation
     */
    @Override
    public double getCost() {
        return getLhsExpression().getCost() * getRhsExpression().getCost();
    }

    /**
     * An implementation of Expression must provide at least one of the methods evaluateItem(), iterate(), or process().
     * This method indicates which of these methods is provided directly. The other methods will always be available
     * indirectly, using an implementation that relies on one of the other methods.
     *
     * @return the implementation method, for example {@link #ITERATE_METHOD} or {@link #EVALUATE_METHOD} or
     * {@link #PROCESS_METHOD}
     */
    @Override
    public int getImplementationMethod() {
        return ITERATE_METHOD;
    }


    /**
     * Copy an expression. This makes a deep copy.
     *
     * @param rebindings a mutable list of (old binding, new binding) pairs
     *                   that is used to update the bindings held in any
     *                   local variable references that are copied.
     * @return the copy of the original expression
     */

    /*@NotNull*/
    @Override
    public LookupByKey copy(RebindingMap rebindings) {
        LookupByKey exp = new LookupByKey(getLhsExpression().copy(rebindings), axis, getRhsExpression().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        exp.isArrayLookup = isArrayLookup;
        exp.isMapLookup = isMapLookup;
        exp.isSingleEntry = isSingleEntry;
        exp.isSingleContainer = isSingleContainer;
        return exp;
    }


    /**
     * Determine the static cardinality of the expression
     */

    @Override
    protected int computeCardinality() {
        if (isSingleContainer && isSingleEntry) {
            if (isArrayLookup) {
                ItemType arrayType = getLhsExpression().getItemType();
                if (arrayType instanceof ArrayItemType) {
                    int memberCard = ((ArrayItemType) arrayType).getMemberType().getCardinality();
                    if (getRetainedStaticContext().getPackageData().getHostLanguageVersion() >= 40) {
                        return (Cardinality.union(memberCard,
                                                  StaticProperty.ALLOWS_ZERO));
                    } else {
                        return memberCard;
                    }
                }
            } else if (isMapLookup) {
                ItemType mapType = getLhsExpression().getItemType();
                if (mapType instanceof RecordTest && getRhsExpression() instanceof StringLiteral) {
                    String fieldName = ((StringLiteral) getRhsExpression()).stringify();
                    SequenceType fieldType = ((RecordTest) mapType).getFieldType(fieldName);
                    if (fieldType == null) {
                        return ((RecordTest) mapType).isExtensible() ? StaticProperty.ALLOWS_ZERO_OR_MORE : StaticProperty.ALLOWS_ZERO;
                    } else {
                        return fieldType.getCardinality();
                    }
                } else if (mapType instanceof MapType) {
                    return (Cardinality.union(((MapType) mapType).getValueType().getCardinality(),
                                              StaticProperty.ALLOWS_ZERO));
                }
            }
        }
        return StaticProperty.ALLOWS_ZERO_OR_MORE;
    }

    /**
     * Is this expression the same as another expression?
     */

    public boolean equals(Object other) {
        if (!(other instanceof LookupByKey)) {
            return false;
        }
        LookupByKey p = (LookupByKey) other;
        return getLhsExpression().isEqual(p.getLhsExpression())
                && getRhsExpression().isEqual(p.getRhsExpression())
                && axis == p.axis;
    }

    /**
     * get HashCode for comparing two expressions
     */

    @Override
    protected int computeHashCode() {
        return axis
                ^ getLhsExpression().hashCode()
                ^ getRhsExpression().hashCode();
    }

    /**
     * Iterate the lookup-expression in a given context
     *
     * @param context the evaluation context
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(final XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
    }

    protected static SequenceIterator optionalGroundedValueIterator(GroundedValue value) {
        if (value == null) {
            return EmptyIterator.getInstance();
        } else {
            return value.iterate();
        }
    }

    protected static void mustBeArrayOrMap(Expression exp, Item baseItem) throws XPathException {
        throw new XPathException("The items on the LHS of the '?' operator must be maps or arrays; but value (" +
                                                              baseItem.toShortString() + ") was supplied", "XPTY0004")
                .asTypeError()
                .withLocation(exp.getLocation())
                .withFailingExpression(exp);
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter destination) throws XPathException {
        destination.startElement("lookup", this);
        if (axis > 0) {
            destination.emitAttribute("axis", AxisInfo.axisName[axis]);
        }
        getLhsExpression().export(destination);
        getRhsExpression().export(destination);
        destination.endElement();
    }

    /**
     * The toString() method for an expression attempts to give a representation of the expression
     * in an XPath-like form, but there is no guarantee that the syntax will actually be true XPath.
     * In the case of XSLT instructions, the toString() method gives an abstracted view of the syntax
     *
     * @return a representation of the expression as a string
     */

    public String toString() {
        String rhs;
        if (getRhsExpression() instanceof Literal) {
            Literal lit = (Literal) getRhsExpression();
            if (lit instanceof StringLiteral && NameChecker.isValidNCName(((StringLiteral) lit).getGroundedValue().codePoints())) {
                rhs = ((StringLiteral) lit).stringify();
            } else if (lit.getGroundedValue() instanceof Int64Value) {
                rhs = lit.getGroundedValue().toString();
            } else {
                rhs = ExpressionTool.parenthesize(lit);
            }
        } else {
            rhs = ExpressionTool.parenthesize(getRhsExpression());
        }
        return ExpressionTool.parenthesize(getLhsExpression()) + "?" + rhs;
    }

    @Override
    public Elaborator getElaborator() {
        return new LookupElaborator();
    }

    public static class LookupElaborator extends PullElaborator {

        public PullEvaluator elaborateForPull() {
            LookupByKey expr = (LookupByKey) getExpression();
            if (expr.axis == -1) {
                if (expr.isArrayLookup) {
                    if (expr.isSingleContainer && expr.isSingleEntry) {
                        ItemEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForItem();
                        ItemEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForItem();

                        return context -> {
                            ArrayItem array = (ArrayItem) lhs.eval(context);
                            IntegerValue subscript = (IntegerValue) rhs.eval(context);
                            return array.getIfPresent(subscript.asSubscript() - 1).iterate();
                        };
                    } else if (expr.isSingleEntry) {
                        PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
                        ItemEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForItem();

                        return context -> {
                            IntegerValue subscriptValue = (IntegerValue) rhs.eval(context);
                            int subscript = subscriptValue.asSubscript() - 1;
                            return MappingIterator.map(lhs.iterate(context), baseItem -> {
                                ArrayItem array = (ArrayItem) baseItem;
                                return array.getIfPresent(subscript).iterate();
                            });
                        };

                    } else {
                        PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
                        PullEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForPull();
                        return context -> {
                            SequenceIterator baseIterator = lhs.iterate(context);
                            GroundedValue rhsValue;
                            try {
                                rhsValue = SequenceTool.toGroundedValue(rhs.iterate(context));
                            } catch (UncheckedXPathException e) {
                                throw e.getXPathException();
                            }
                            return MappingIterator.map(baseIterator, baseItem ->
                                    MappingIterator.map(rhsValue.iterate(), index -> {
                                        ArrayItem array = (ArrayItem) baseItem;
                                        return array.get(((IntegerValue) index).asSubscript() - 1).iterate();
                                    })
                            );
                        };

                    }

                } else if (expr.isMapLookup) {

                    if (expr.isSingleContainer && expr.isSingleEntry) {
                        ItemEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForItem();
                        ItemEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForItem();

                        return context -> {
                            MapItem map = (MapItem) lhs.eval(context);
                            AtomicValue key = (AtomicValue) rhs.eval(context);
                            return optionalGroundedValueIterator(map.get(key));
                        };

                    } else if (expr.isSingleEntry) {
                        PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
                        ItemEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForItem();
                        return context -> {
                            SequenceIterator baseIterator = lhs.iterate(context);
                            AtomicValue key = (AtomicValue) rhs.eval(context);
                            return MappingIterator.map(baseIterator, baseItem ->
                                    optionalGroundedValueIterator(((MapItem) baseItem).get(key)));
                        };

                    } else {
                        PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
                        PullEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForPull();
                        return context -> {
                            SequenceIterator baseIterator = lhs.iterate(context);
                            GroundedValue rhsVal;
                            try {
                                rhsVal = SequenceTool.toGroundedValue(rhs.iterate(context));
                            } catch (UncheckedXPathException e) {
                                throw e.getXPathException();
                            }
                            return MappingIterator.map(baseIterator, baseItem ->
                                    MappingIterator.map(rhsVal.iterate(), index ->
                                            optionalGroundedValueIterator(((MapItem) baseItem).get((AtomicValue) index))
                                    ));
                        };

                    }
                } else {
                    // No explicit axis; LHS is not known statically to be an array or map
                    PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
                    PullEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForPull();
                    return context -> {
                        SequenceIterator baseIterator = lhs.iterate(context);
                        GroundedValue rhsVal;
                        try {
                            rhsVal = SequenceTool.toGroundedValue(rhs.iterate(context));
                        } catch (UncheckedXPathException e) {
                            throw e.getXPathException();
                        }
                        MappingFunction mappingFunction = SequenceMapper.of(baseItem -> {
                            switch (baseItem.getGenre()) {
                                case ARRAY: {
                                    MappingFunction arrayAccess = SequenceMapper.of(index -> {
                                        if (index instanceof IntegerValue) {
                                            int i = ((IntegerValue) index).asSubscript();
                                            if (i > 0 && i <= ((ArrayItem) baseItem).arrayLength()) {
                                                GroundedValue member = ((ArrayItem) baseItem).get(i - 1);
                                                return member.iterate();
                                            }
                                        }
                                        return EmptyIterator.getInstance();
                                    });
                                    SequenceIterator rhsIter = rhsVal.iterate();
                                    return new MappingIterator(rhsIter, arrayAccess);
                                }
                                case MAP: {
                                    SequenceIterator rhsIter = rhsVal.iterate();
                                    return MappingIterator.map(rhsIter, key ->
                                            optionalGroundedValueIterator(((MapItem) baseItem).get((AtomicValue) key))
                                    );
                                }
                                case JNODE:
                                    SequenceIterator rhsIter = rhsVal.iterate();
                                    return MappingIterator.map(rhsIter, key -> {
                                                                   SequenceIterator all = ((JNode) baseItem).iterateChildAxis(AnyJNodeType.getInstance());
                                                                   return new ItemMappingIterator(all, jnode ->
                                                                           ((JNode) jnode).getSelector().equals(key) ? jnode : null);
                                                               }

                                    );

                                case EXTERNAL: {
                                    if (!(rhsVal instanceof StringValue)) {
                                        throw new XPathException(
                                                "An item on the LHS of the '?' operator is an external object, but a value on the RHS of the operator (" +
                                                        baseItem.toShortString() + ") is not a singleton string", "XPTY0004")
                                                .asTypeError()
                                                .withLocation(expr.getLocation())
                                                .withFailingExpression(expr);
                                    }
                                    String key = ((StringValue) rhsVal).getStringValue();
                                    final GroundedValue entry = context.getConfiguration().externalObjectAsMap(
                                            (ObjectValue<?>) baseItem, key).get((StringValue) rhsVal);
                                    if (entry == null) {
                                        throw new XPathException("There is no unique method named " + key +
                                                                         " in the external object of type " + ((ObjectValue<?>) baseItem).getObject().getClass().getName(), "XPTY0004");
                                    }
                                    return entry.iterate();
                                }
                                default: {
                                    mustBeArrayOrMap(expr, baseItem);
                                    return null;
                                }
                            }
                        });
                        return new MappingIterator(baseIterator, mappingFunction);

                    };
                }
            } else {
                // expression uses an explicit axis
                PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
                PullEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForPull();
                return context -> {
                    SequenceIterator baseIterator = lhs.iterate(context);
                    GroundedValue rhsVal;
                    try {
                        rhsVal = SequenceTool.toGroundedValue(rhs.iterate(context));
                    } catch (UncheckedXPathException e) {
                        throw e.getXPathException();
                    }
                    MappingFunction mappingFunction = SequenceMapper.of(baseItem -> {
                        switch (baseItem.getGenre()) {
                            case ARRAY: {
                                MappingFunction arrayAccess = SequenceMapper.of(index -> {
                                    if (index instanceof IntegerValue) {
                                        int i = ((IntegerValue)index).asSubscript();
                                        if (i > 0 && i <= ((ArrayItem) baseItem).arrayLength()) {
                                            GroundedValue member = ((ArrayItem) baseItem).get(i - 1);
                                            return member.iterate();
                                        }
                                    }
                                    return EmptyIterator.getInstance();
                                });
                                SequenceIterator rhsIter = rhsVal.iterate();
                                return new MappingIterator(rhsIter, arrayAccess);
                            }
                            case MAP: {
                                SequenceIterator rhsIter = rhsVal.iterate();
                                return MappingIterator.map(rhsIter, key ->
                                        optionalGroundedValueIterator(((MapItem) baseItem).get((AtomicValue) key))
                                );
                            }
                            case JNODE:
                                SequenceIterator rhsIter = rhsVal.iterate();
                                final int axis = expr.axis;
                                return MappingIterator.map(rhsIter, key -> {
                                    SequenceIterator all = ((JNode)baseItem).iterateChildAxis(AnyJNodeType.getInstance());
                                    return new ItemMappingIterator(all, jnode ->
                                            ((JNode)jnode).getSelector().equals(key) ? jnode : null);
                                }

                            );

                            case EXTERNAL: {
                                if (!(rhsVal instanceof StringValue)) {
                                    throw new XPathException(
                                            "An item on the LHS of the '?' operator is an external object, but a value on the RHS of the operator (" +
                                                    baseItem.toShortString() + ") is not a singleton string", "XPTY0004")
                                            .asTypeError()
                                            .withLocation(expr.getLocation())
                                            .withFailingExpression(expr);
                                }
                                String key = ((StringValue) rhsVal).getStringValue();
                                final GroundedValue entry = context.getConfiguration().externalObjectAsMap(
                                        (ObjectValue<?>) baseItem, key).get((StringValue) rhsVal);
                                if (entry == null) {
                                    throw new XPathException("There is no unique method named " + key +
                                                                     " in the external object of type " + ((ObjectValue<?>) baseItem).getObject().getClass().getName(), "XPTY0004");
                                }
                                return entry.iterate();
                            }
                            default: {
                                mustBeArrayOrMap(expr, baseItem);
                                return null;
                            }
                        }
                    });
                    return new MappingIterator(baseIterator, mappingFunction);

                };
            }
        }
    }




}

