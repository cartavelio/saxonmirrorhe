////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import com.saxonica.ee.pin.pin.LabeledArrayItem;
import com.saxonica.ee.pin.pin.LabeledMapItem;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullElaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.ShapedMap;
import net.sf.saxon.ma.map.SingleEntryMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;
import java.util.function.Supplier;



/**
 * A deep lookup expression is an expression of the form A??KS. It handles all variants
 * of the syntax except {@code A??*}, which is handled by a {@link DeepLookupAllExpression}.
 * The key-specifier KS is treated as a general expression, so in a common case like $map?keyvalue,
 * the expression will be a string literal "keyvalue".
 */

public class DeepLookupExpression extends BinaryExpression {

    private final LookupExpression.Modifier modifier;

    /**
     * Constructor
     *
     * @param start The left hand operand.
     * @param step  The right hand operand
     */

    public DeepLookupExpression(Expression start, LookupExpression.Modifier modifier, Expression step) {
        super(start, OperatorSymbol.DEEP_LOOKUP, step);
        this.modifier = modifier;
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

        Supplier<RoleDiagnostic> role1 = () -> new RoleDiagnostic(RoleDiagnostic.BINARY_EXPR, operator.toString(), 1);
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
        DeepLookupExpression exp = new DeepLookupExpression(getLhsExpression().copy(rebindings), modifier, getRhsExpression().copy(rebindings));
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

        // The algorithm is similar to that in the spec, but slightly optimised.
        // The basic idea is to build a list of singleton maps representing the entire recursive
        // content of the tree of maps and arrays. But (a) we do this lazily as far as possible, and
        // (b) rather than building the entire list and then selecting the items that match the
        // requested key, we test items against the key before adding them to the result.
        // In addition, there is a fast path for the common case where the key is known
        // statically.

        public PullEvaluator elaborateForPull() {
            DeepLookupExpression expr = (DeepLookupExpression) getExpression();

            PullEvaluator lhs = expr.getLhsExpression().makeElaborator().elaborateForPull();

            if (expr.getRhsExpression() instanceof Literal) {
                GroundedValue keys = ((Literal)expr.getRhsExpression()).getGroundedValue();
                MapItem keySet;
                if (keys.getLength() == 1) {
                    keySet = new SingleEntryMap((AtomicValue)keys.head(), BooleanValue.TRUE);
                } else {
                    keySet = new HashTrieMap();
                    for (Item key : keys.asIterable()) {
                        keySet = keySet.addEntry((AtomicValue) key, BooleanValue.TRUE);
                    }
                }
                final MapItem finalKeySet = keySet;
                return context -> {
                    List<Item> content = new ArrayList<>();
                    SequenceIterator lhsIter = lhs.iterate(context);
                    Item lhsItem;
                    while ((lhsItem = lhsIter.next()) != null) {
                        if (lhsItem instanceof ArrayItem && !(lhsItem instanceof LabeledArrayItem)) {
                            lhsItem = LabeledArrayItem.makePinnedArrayItem((ArrayItem)lhsItem);
                        }
                        if (lhsItem instanceof MapItem && !(lhsItem instanceof LabeledMapItem)) {
                            lhsItem = LabeledMapItem.makePinnedMapItem((MapItem) lhsItem);
                        }
                        gatherRecursiveContent(lhsItem, content,
                                               item -> finalKeySet.get((AtomicValue)((ShapedMap) item).get("key")) != null);
                    }
                    SequenceIterator selectedEntries = new ListIterator.Of<>(content);
                    return DeepLookupAllExpression.modifiedIterator(selectedEntries, expr.modifier);
                };
            } else {
                PullEvaluator rhs = expr.getRhsExpression().makeElaborator().elaborateForPull();
                return context -> {

                    MapItem keySet = new HashTrieMap();
                    SequenceIterator keyIter = rhs.iterate(context);
                    Item key;
                    while ((key = keyIter.next()) != null) {
                        keySet = keySet.addEntry((AtomicValue) key, BooleanValue.TRUE);
                    }
                    final MapItem finalKeySet = keySet;

                    List<Item> content = new ArrayList<>();
                    SequenceIterator lhsIter = lhs.iterate(context);
                    Item lhsItem;
                    while ((lhsItem = lhsIter.next()) != null) {
                        gatherRecursiveContent(lhsItem, content,
                                               item -> finalKeySet.get((AtomicValue)((ShapedMap) item).get("key")) != null);
                    }
                    SequenceIterator selectedEntries = new ListIterator.Of<>(content);
                    return DeepLookupAllExpression.modifiedIterator(selectedEntries, expr.modifier);
                };
            }
        }
    }

    /**
     * Return a sequence of singleton maps representing the entries in a map or the members in an array
     * @param item typically a map or array
     * @return for a map, the result of map:entries(). For an array, a sequence of singleton maps, one
     * per member of the array, where the map key is the integer position of the member and the corresponding
     * value is the member value. Otherwise, an empty sequence.
     */
    private static SequenceIterator immediateContent(Item item)  {
        if (item instanceof MapItem) {
            return new LookupAllExpression.MapEntriesIterator((MapItem)item);
        } else if (item instanceof ArrayItem) {
            return new LookupAllExpression.ArrayEntriesIterator((ArrayItem)item);
        } else {
            return EmptyIterator.getInstance();
        }
    }

    /**
     * Gather the recursive content of a map or array
     * @param item typically a map or array; supplying any other item leaves the result array unchanged
     * @param result a mutable list to receive the results. The items in this list will be singleton maps;
     *               when expanding a map, these represent entries in the supplied map and its content,
     *               expanded recursively; when expanding an array, these represent members of the
     *               supplied array, mapped to their position in the array.
     */

    public static void gatherRecursiveContent(Item item, List<Item> result, Predicate<Item> matcher) {
        Item child;
        SequenceIterator iter = immediateContent(item);
        while ((child = iter.next()) != null) {
            if (matcher.test(child)) {
                result.add(child);
            }
            for (Item childItem : ((ShapedMap)child).get("value").asIterable()) {
                gatherRecursiveContent(childItem, result, matcher);
            }
        }
    }






}

