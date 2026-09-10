////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullElaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomizingIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;
import java.util.function.Supplier;


/**
 * A deep lookup expression is an expression of the form A??B.
 */

public class DeepLookupExpression extends BinaryExpression {

    /**
     * Constructor
     *
     * @param start The left hand operand.
     * @param step  The right hand operand
     */

    public DeepLookupExpression(Expression start, Expression step) {
        super(start, Token.QMARK_QMARK, step);
    }

    @Override
    protected OperandRole getOperandRole(int arg) {
        return arg == 0 ? OperandRole.INSPECT : OperandRole.ABSORB;
    }

    @Override
    public String getExpressionName() {
        return "deepLookupExp";
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
        resetLocalStaticProperties();
        getLhs().typeCheck(visitor, contextInfo);
        getRhs().typeCheck(visitor, contextInfo);

        Configuration config = visitor.getConfiguration();
        final TypeChecker tc = config.getTypeChecker(false);

        SequenceType atomicType = SequenceType.ATOMIC_SEQUENCE;

        Supplier<RoleDiagnostic> role1 = () -> new RoleDiagnostic(RoleDiagnostic.BINARY_EXPR, Token.tokens[operator], 1);
        setRhsExpression(tc.staticTypeCheck(getRhsExpression(), atomicType, role1, visitor));
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
        return getLhsExpression().getCost() + getRhsExpression().getCost();
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
    public DeepLookupExpression copy(RebindingMap rebindings) {
        DeepLookupExpression exp = new DeepLookupExpression(getLhsExpression().copy(rebindings), getRhsExpression().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
    }


    /**
     * Determine the static cardinality of the expression
     */

    @Override
    protected int computeCardinality() {
        return StaticProperty.ALLOWS_ZERO_OR_MORE;
    }

    /**
     * Is this expression the same as another expression?
     */

    public boolean equals(Object other) {
        if (!(other instanceof DeepLookupExpression)) {
            return false;
        }
        DeepLookupExpression p = (DeepLookupExpression) other;
        return getLhsExpression().isEqual(p.getLhsExpression()) && getRhsExpression().isEqual(p.getRhsExpression());
    }

    /**
     * get HashCode for comparing two expressions
     */

    @Override
    protected int computeHashCode() {
        return "DeepLookupExpression".hashCode() ^ getLhsExpression().hashCode() ^ getRhsExpression().hashCode();
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


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter destination) throws XPathException {
        destination.startElement("deepLookup", this);
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
        return new DeepLookupElaborator();
    }

    public static class DeepLookupElaborator extends PullElaborator {

        public PullEvaluator elaborateForPull() {
            DeepLookupExpression expr = (DeepLookupExpression) getExpression();

            PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();
            PullEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForPull();
            return context -> {
                // Follow the spec exactly for now
                List<Item> content = new ArrayList<>();
                SequenceIterator lhsIter = lhs.iterate(context);
                Item lhsItem;
                while ((lhsItem = lhsIter.next()) != null) {
                    gatherRecursiveContent(lhsItem, content);
                }
                MapItem keySet = new HashTrieMap();
                SequenceIterator keyIter = new AtomizingIterator(rhs.iterate(context));
                Item key;
                while ((key = keyIter.next()) != null) {
                    keySet = keySet.addEntry((AtomicValue)key, BooleanValue.TRUE);
                }
                ZenoSequence result = new ZenoSequence();
                for (Item it : content) {
                    SingleEntryMap sem = (SingleEntryMap)it;
                    if (keySet.get(sem.getKey()) != null) {
                        result = result.appendSequence(sem.getValue());
                    }
                }
                return result.iterate();
            };
//            return context -> {
//                // Initial implementation is eager for simplicity
//                SequenceIterator keyIter = new AtomizingIterator(
//                        rhs.iterate(context));
//                MapItem keySet = new HashTrieMap();
//                Item key;
//                while ((key = keyIter.next()) != null) {
//                    keySet = keySet.addEntry((AtomicValue)key, BooleanValue.TRUE);
//                }
//                final MapItem finalKeySet = keySet;
//                List<Item> result = new ArrayList<>();
//                gatherContent(lhs.iterate(context), false, k -> finalKeySet.get(k) != null, result, 0);
//                return SequenceExtent.makeSequenceExtent(result).iterate();
//            };

        }
    }

    private static GroundedValue immediateContent(Item item) throws XPathException {
        if (item instanceof MapItem) {
            return MapFunctionSet.MapEntries.getMapEntries((MapItem)item);
        } else if (item instanceof ArrayItem) {
            ZenoSequence result = new ZenoSequence();
            long pos = 1;
            for (GroundedValue member : ((ArrayItem)item).members()) {
                result = result.append(new SingleEntryMap(Int64Value.makeIntegerValue(pos++), member));
            }
            return result;
        } else {
            return EmptySequence.getInstance();
        }
    }

    public static void gatherRecursiveContent(Item item, List<Item> result) throws XPathException {
        for (Item child : immediateContent(item).asIterable()) {
            result.add(child);
            for (Item childItem : ((SingleEntryMap)child).getValue().asIterable()) {
                gatherRecursiveContent(childItem, result);
            }
        }
    }

    public static void gatherContent(SequenceIterator lhs,
                                     boolean selectAll,
                                     Predicate<AtomicValue> keys,
                                     List<Item> result,
                                     int level) throws XPathException {
        Item it;
        while ((it = lhs.next()) != null) {
            if (selectAll) {
                result.add(it);
            }
            if (it instanceof ArrayItem) {
                int pos = 1;
                for (GroundedValue val : ((ArrayItem) it).members()) {
                    gatherContent(val.iterate(), selectAll, keys, result, level+1);
                    if (keys != null && keys.test(Int64Value.makeIntegerValue(pos++))) {
                        for (Item valItem : val.asIterable()) {
                            result.add(valItem);
                        }
                    }
                }
            } else if (it instanceof MapItem) {
                for (KeyValuePair pair : ((MapItem) it).keyValuePairs()) {
                    gatherContent(pair.value.iterate(), selectAll, keys, result, level+1);
                    if (keys != null && keys.test(pair.key)) {
                        for (Item valItem : pair.value.asIterable()) {
                            result.add(valItem);
                        }
                    }
                }
            } else {
                if (level > 0) {
                    result.add(it);
                }
            }
        }
    }





}

