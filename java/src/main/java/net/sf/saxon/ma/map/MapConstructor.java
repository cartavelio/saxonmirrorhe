////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.map;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.oper.OperandArray;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;

import java.util.ArrayList;
import java.util.List;


/**
 * An expression corresponding to a map constructor, for example <code>{'a':1, 'b':2}</code>.
 *
 * <p>Previously, map constructors were compiled to a map:merge() function call. This is
 * no longer possible, because the treatment of JNodes in the input is different in the two cases.
 * And in any case, map:merge has a different usage profile, so a different implementation
 * strategy is appropriate. Using a custom expression also enables better diagnostics.</p>
 */

public class MapConstructor extends Expression implements Pingable {


    private OperandArray operanda;

    // Data used for learning about the dynamic usage of the constructed map
    private double numberOfCalls = 0;
    private double numberOfConversions = 0;

    /**
     * Create a map constructor with defined members
     * @param children expressions defining the members (comma-separated subexpressions).
     *                 For this purpose, an entry written as <code>X : Y</code>
     *                 is represented as a call on the function map:entry(X, Y).
     */

    public MapConstructor(List<Expression> children) {
        Expression[] kids = children.toArray(new Expression[0]);
        for (Expression e : children) {
            adoptChildExpression(e);
        }
        setOperanda(new OperandArray(this, kids, OperandRole.NAVIGATE));
    }

    /**
     * Set the data structure for the operands of this expression. This must be created during initialisation of the
     * expression and must not be subsequently changed
     *
     * @param operanda the data structure for expression operands
     */

    protected void setOperanda(OperandArray operanda) {
        this.operanda = operanda;
    }

    /**
     * Get the data structure holding the operands of this expression.
     *
     * @return the data structure holding expression operands
     */

    public OperandArray getOperanda() {
        return operanda;
    }

    public Operand getOperand(int i) {
        return operanda.getOperand(i);
    }

    @Override
    public Iterable<Operand> operands() {
        return operanda;
    }


    @Override
    public String getExpressionName() {
        return "MapConstructor";
    }

    @Override
    public String getStreamerName() {
        return "MapConstructor";
    }


    /**
     * Is this expression the same as another expression?
     *
     * @param other the expression to be compared with this one
     * @return true if the two expressions are statically equivalent
     */
    @Override
    public boolean equals(Object other) {
        if (other instanceof MapConstructor mc2) {
            if (mc2.getOperanda().getNumberOfOperands() != getOperanda().getNumberOfOperands()) {
                return false;
            }
            for (int i = 0; i < getOperanda().getNumberOfOperands(); i++) {
                if (!getOperanda().getOperand(i).equals(mc2.getOperanda().getOperand(i))) {
                    return false;
                }
            }
            return true;
        } else {
            return false;
        }
    }

    /**
     * Hashcode supporting equals()
     */

    @Override
    protected int computeHashCode() {
        int h = 0xe82b91c0;
        for (Operand o : operands()) {
            h ^= o.getChildExpression().hashCode();
        }
        return h;
    }

    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        Expression e = super.typeCheck(visitor, contextInfo);
        if (e != this) {
            return e;
        }
        return preEvaluate(visitor);
    }

    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        Expression e = super.optimize(visitor, contextInfo);
        if (e != this) {
            return e;
        }
        return preEvaluate(visitor);
    }

    private Expression preEvaluate(ExpressionVisitor visitor) {
        for (Operand o : operands()) {
            if (!(o.getChildExpression() instanceof Literal)) {
                return this;
            }
        }
        try {
            return Literal.makeLiteral(evaluateItem(visitor.makeDynamicContext()), this);
        } catch (XPathException e) {
            return this;
        }
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables that need to be re-bound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        List<Expression> m2 = new ArrayList<>(getOperanda().getNumberOfOperands());
        for (Operand o : operands()) {
            m2.add(o.getChildExpression().copy(rebindings));
        }
        MapConstructor b2 = new MapConstructor(m2);
        ExpressionTool.copyLocationInfo(this, b2);
        return b2;
    }

    /**
     * Determine the data type of the items returned by this expression
     *
     * @return the data type
     */

    /*@NotNull*/
    @Override
    public final ItemType getItemType() {
        return MapType.ANY_MAP_TYPE;
//        ItemType contentType = null;
//        int contentCardinality = StaticProperty.EXACTLY_ONE;
//        TypeHierarchy th = getConfiguration().getTypeHierarchy();
//        for (Expression e : getOperanda().operandExpressions()) {
//            if (contentType == null) {
//                contentType = e.getItemType();
//                contentCardinality = e.getCardinality();
//            } else {
//                contentType = Type.getCommonSuperType(contentType, e.getItemType(), th);
//                contentCardinality = Cardinality.union(contentCardinality, e.getCardinality());
//            }
//        }
//        if (contentType == null) {
//            contentType = ErrorType.getInstance();
//        }
//        return new ArrayItemType(SequenceType.makeSequenceType(contentType, contentCardinality));
    }

    /**
     * Get the static type of the expression as a UType, following precisely the type
     * inference rules defined in the XSLT 3.0 specification.
     *
     * @param contextItemType the static type of the context item
     * @return the static item type of the expression according to the XSLT 3.0 defined rules
     */
    @Override
    public UType getStaticUType(UType contextItemType) {
        return UType.FUNCTION;
    }

    /**
     * Determine the cardinality of the expression
     */

    @Override
    protected final int computeCardinality() {
        // A map is an item!
        return StaticProperty.EXACTLY_ONE;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("mapC", this);
        for (Operand o : operands()) {
            o.getChildExpression().export(out);
        }
        out.endElement();
    }

    @Override
    public String toShortString() {
        int n = getOperanda().getNumberOfOperands();
        return switch (n) {
            case 0 -> "{}";
            case 1 -> "{" + getOperanda().getOperand(0).getChildExpression().toShortString() + "}";
            case 2 -> "{" + getOperanda().getOperand(0).getChildExpression().toShortString() + ", " +
                    getOperanda().getOperand(1).getChildExpression().toShortString() + "}";
            default -> "{" + getOperanda().getOperand(0).getChildExpression().toShortString() + ", ...}";
        };
    }

    @Override
    public String toString() {
        int n = getOperanda().getNumberOfOperands();
        return switch (n) {
            case 0 -> "{}";
            case 1 -> "{" + getOperanda().getOperand(0).getChildExpression().toString() + "}";
            case 2 -> "{" + getOperanda().getOperand(0).getChildExpression().toString() + ", " +
                    getOperanda().getOperand(1).getChildExpression().toString() + "}";
            default -> "{" + getOperanda().getOperand(0).getChildExpression().toString() + ", ...}";
        };
    }

    /**
     * An implementation of Expression must provide at least one of the methods evaluateItem(), iterate(), or process().
     * This method indicates which of these methods is provided. This implementation provides both iterate() and
     * process() methods natively.
     */

    @Override
    public int getImplementationMethod() {
        return EVALUATE_METHOD;
    }

    /**
     * Notify that a map created using this expression has been converted to an immutable map.
     */
    @Override
    public void ping() {
        numberOfConversions++;
    }

    /**
     * Evaluate an expression as a single item. This always returns either a single Item or
     * null (denoting the empty sequence). No conversion is done. This method should not be
     * used unless the static type of the expression is a subtype of "item" or "item?": that is,
     * it should not be called if the expression may return a sequence. There is no guarantee that
     * this condition will be detected.
     *
     * @param context The context in which the expression is to be evaluated
     * @return the node or atomic value that results from evaluating the
     * expression; or null to indicate that the result is an empty
     * sequence
     * @throws XPathException if any dynamic error occurs evaluating the
     *                                           expression
     */
    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForItem().eval(context);
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new MapConstructorElaborator();
    }

    private static class MapConstructorElaborator extends ItemElaborator {

        @Override
        public ItemEvaluator elaborateForItem() {
            MapConstructor expr = (MapConstructor)getExpression();
            int version = expr.getRetainedStaticContext().getPackageData().getHostLanguageVersion();
            List<PullEvaluator> entryEvaluators = new ArrayList<>(expr.getOperanda().getNumberOfOperands());
            for (Operand o : expr.operands()) {
                entryEvaluators.add(o.getChildExpression().makeElaborator().elaborateForPull());
            }
            return context -> {
                GeneralMapBuilder builder = new GeneralMapBuilder(version);
                builder.setDuplicatesAction(
                        (x,y, cxt) -> {
                            throw new UncheckedXPathException("Duplicate key in map constructor", "XQDY0137");
                        });
                for (PullEvaluator eval : entryEvaluators) {
                     SequenceIterator entryIter = eval.iterate(context);
                     Item entry;
                     while ((entry = entryIter.next()) != null) {
                         if (entry instanceof JNode jNode) {
                             if (jNode.getSelector() != null) {
                                 builder.put(jNode.getSelector(), jNode.getContent());
                             } else {
                                 SequenceIterator contentIter = jNode.getContent().iterate();
                                 Item childEntry;
                                 while ((childEntry = contentIter.next()) != null) {
                                     if (childEntry instanceof MapItem childMap) {
                                         for (KeyValuePair pair : childMap.keyValuePairs()) {
                                             builder.put(pair.key(), pair.value());
                                         }
                                     }
                                 }
                             }
                         } else if (entry instanceof SingleEntryMap singleEntryMap) {
                             // Fast path for a common case
                             builder.put(singleEntryMap.getKey(), singleEntryMap.getValue());
                         } else if (entry instanceof MapItem mapEntry) {
                              for (KeyValuePair kvp : mapEntry.keyValuePairs()) {
                                  builder.put(kvp.key(), kvp.value());
                              }
                         } else {
                             throw new XPathException("Invalid entry in map constructor (each entry must be a map or a JNode): " + entry)
                                     .withErrorCode("XPTY0004")
                                     .asTypeError();
                         }
                     }
                }
                return builder.getCompletedMap(context);
            };
        }

    }

}