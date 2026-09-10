package com.saxonica.ee.optim;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.expr.instruct.ConditionalInstruction;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.instruct.TailCallReturner;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.CalendarValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;

import java.util.*;

/**
 * An optimized Choose expression in which all the branches are tests of the same expression for equality
 * with some value. Note this is not quite the same as the XQuery 3.1 switch expression; this switch expression
 * is usually generated as the result of optimizing an xsl:choose or extended if/then/else expression.
 */
public class SwitchExpression extends Expression implements TailCallReturner, ConditionalInstruction {

    private final Operand subjectOp;
    private final List<Operand> actions;
    private final Map<AtomicMatchKey, Integer> map;
    private Map<AtomicMatchKey, AtomicValue> originalValues;
    private final Operand defaultActionOp;
    private final StringCollator collation;
    private boolean _isInstruction;

    public final static OperandRole SUBJECT =
            new OperandRole(0, OperandUsage.ABSORPTION, SequenceType.SINGLE_ATOMIC);

    /**
     * Create a switch expression
     *
     * @param switchCaseInfo details of the expression to be constructed
     */

    public SwitchExpression(SwitchCaseInfo switchCaseInfo) {
        this.originalValues = switchCaseInfo.originalValues;
        this.subjectOp = new Operand(this, switchCaseInfo.commonlhs, SUBJECT);
        this.map = new HashMap<>(switchCaseInfo.index.size());
        this.actions = new ArrayList<>(switchCaseInfo.index.size());
        for (Map.Entry<AtomicMatchKey, Expression> entry : switchCaseInfo.index.entrySet()) {
            Expression action = entry.getValue();
            int actionNumber = -1;
            for (int i=0; i<actions.size(); i++) {
                if (actions.get(i).getChildExpression() == action) {
                    actionNumber = i;
                }
            }
            if (actionNumber == -1) {
                actionNumber = actions.size();
                actions.add(new Operand(this, action, Choose.CHOICE_ACTION));
            }
            this.map.put(entry.getKey(), actionNumber);
        }
        this.defaultActionOp = new Operand(this, switchCaseInfo.defaultAction, Choose.CHOICE_ACTION);
        this.collation = switchCaseInfo.collation;
    }

    private SwitchExpression(Expression commonlhs, Map<AtomicMatchKey,
            Integer> cases, List<Operand> actions, Expression otherwise, StringCollator collation) {
        this.subjectOp = new Operand(this, commonlhs, SUBJECT);
        this.map = new HashMap<>(cases);
        this.actions = new ArrayList<>(actions.size());
        for (Operand o : actions) {
            this.actions.add(
                         new Operand(this, o.getChildExpression().copy(new RebindingMap()), OperandRole.SAME_FOCUS_ACTION));
        }
        this.defaultActionOp = new Operand(this, otherwise, OperandRole.SAME_FOCUS_ACTION);
        this.collation = collation;
    }

    /**
     * Say whether this switch expression originates as an XSLT instruction
     *
     * @param inst true if this started life as an xsl:choose instruction
     */

    public void setInstruction(boolean inst) {
        _isInstruction = inst;
    }

    /**
     * Ask whether this expression is an instruction. In XSLT streamability analysis this
     * is used to distinguish constructs corresponding to XSLT instructions from other constructs.
     *
     * @return true if this construct originates as an XSLT instruction
     */

    @Override
    public boolean isInstruction() {
        return _isInstruction;
    }


    /**
     * Get the subject expression (the expression that computes the switch value)
     * @return the expression that computes the switch value
     */

    public Expression getSubjectExpression() {
        return subjectOp.getChildExpression();
    }

    /**
     * Get the map from XPathComparable objects representing the values to the expressions
     * to be executed for each of these values
     * @return the map of case clauses
     */

    public Map<AtomicMatchKey, Integer> getCaseMap() {
        return map;
    }

    /**
     * Get the default ("otherwise") expression
     * @return the "otherwise" expression giving the value to be returned
     * if none of the cases match
     */

    public Expression getOtherwiseExpression() {
        return defaultActionOp.getChildExpression();
    }

    public Operand getAction(int index) {
        return actions.get(index);
    }

    public List<Operand> getActions() {
        return actions;
    }

    public Operand getOtherwiseOperand() {
        return defaultActionOp;
    }

    @Override
    public Iterable<Operand> operands() {
        List<Operand> list = new ArrayList<>();
        list.add(subjectOp);
        list.add(defaultActionOp);
        list.addAll(actions);
        return list;
    }

    /**
     * Get the collation used
     * @return the collation
     */

    public StringCollator getCollation() {
        return collation;
    }


    /**
     * Compute the static cardinality of this expression
     *
     * @return the computed cardinality, as one of the values {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_ONE},
     *         {@link net.sf.saxon.expr.StaticProperty#EXACTLY_ONE}, {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ONE_OR_MORE},
     *         {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_MORE}
     */

    @Override
    protected int computeCardinality() {
        int card = getOtherwiseExpression().getCardinality();
        for (Operand action : actions) {
            card = Cardinality.union(card, action.getChildExpression().getCardinality());
            if (card == StaticProperty.ALLOWS_ZERO_OR_MORE) {
                return card;
            }
        }
        return card;
    }

    /**
     * Ask whether common subexpressions found in the operands of this expression can
     * be extracted and evaluated outside the expression itself. The result is irrelevant
     * in the case of operands evaluated with a different focus, which will never be
     * extracted in this way, even if they have no focus dependency.
     *
     * @return false for this kind of expression
     */
    @Override
    public boolean allowExtractingCommonSubexpressions() {
        return false;
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
        List<Operand> newActions = new ArrayList<>();
        for (Operand o : actions) {
            newActions.add(new Operand(this, o.getChildExpression().copy(rebindings), Choose.CHOICE_ACTION));
        }
        SwitchExpression s2 = new SwitchExpression(
                getSubjectExpression().copy(rebindings),
                map,
                newActions,
                getOtherwiseExpression().copy(rebindings),
                collation);
        s2.originalValues = originalValues;
        return s2;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     *
     * @param out the expression presenter used to display the structure
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        // Note: if several keys map to the same action, we are outputting the action repeatedly.
        // This is partly for simplicity, and partly to keep the SEF file compatible: see bug 3261
        out.startElement("switch", this);
        if (collation != CodepointCollator.getInstance()) {
            out.emitAttribute("coll", collation.getCollationURI());
        }
        out.setChildRole("value");
        getSubjectExpression().export(out);
        for (AtomicMatchKey key : map.keySet()) {
            out.startElement("case", this);
            Literal.exportAtomicValue(originalValues.get(key), out);
            int selection = map.get(key);
            actions.get(selection).getChildExpression().export(out);
            out.endElement();
        }
        out.setChildRole("default");
        getOtherwiseExpression().export(out);
        out.endElement();
    }


    /**
     * Determine the data type of the expression, if possible. All expression return
     * sequences, in general; this method determines the type of the items within the
     * sequence, assuming that (a) this is known in advance, and (b) it is the same for
     * all items in the sequence.
     * <p>This method should always return a result, though it may be the best approximation
     * that is available at the time.</p>
     *
     * @return a value such as Type.STRING, Type.BOOLEAN, Type.NUMBER,
     *         Type.NODE, or Type.ITEM (meaning not known at compile time)
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        ItemType type = getOtherwiseExpression().getItemType();
        TypeHierarchy th = getConfiguration().getTypeHierarchy();
        for (Operand operand : actions) {
            Expression action = operand.getChildExpression();
            type = Type.getCommonSuperType(type, action.getItemType(), th);
        }
        return type;
    }


    /**
     * Replace this expression by a simpler expression that delivers the results without regard
     * to order.
     *
     * @param retainAllNodes set to true if the result must contain exactly the same nodes as the
     *                       original; set to false if the result can eliminate (or introduce) duplicates.
     * @param forStreaming  set to true if optimizing for streaming
     */
    @Override
    public Expression unordered(boolean retainAllNodes, boolean forStreaming) throws XPathException {
        for (Operand o : actions) {
            Expression e2 = o.getChildExpression().unordered(retainAllNodes, forStreaming);
            if (e2 != o.getChildExpression()) {
                o.setChildExpression(e2);
            }
        }
        defaultActionOp.setChildExpression(getOtherwiseExpression().unordered(retainAllNodes, forStreaming));
        return this;
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
     *         expression; or null to indicate that the result is an empty
     *         sequence
     * @throws net.sf.saxon.trans.XPathException
     *          if any dynamic error occurs evaluating the
     *          expression
     */

    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        AtomicValue subjectValue = (AtomicValue) getSubjectExpression().evaluateItem(context);
        if (subjectValue == null) {
            return getOtherwiseExpression().evaluateItem(context);
        }
        AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
        int selection = getIndex(key);
        if (selection == -1) {
            return getOtherwiseExpression().evaluateItem(context);
        } else {
            return actions.get(selection).getChildExpression().evaluateItem(context);
        }
    }

    public int getIndex(AtomicMatchKey key) {
        return map.getOrDefault(key, -1);
    }

    /**
     * Return an Iterator to iterate over the values of a sequence. The value of every
     * expression can be regarded as a sequence, so this method is supported for all
     * expressions. This default implementation handles iteration for expressions that
     * return singleton values: for non-singleton expressions, the subclass must
     * provide its own implementation.
     *
     * @param context supplies the context for evaluation
     * @return a SequenceIterator that can be used to iterate over the result
     *         of the expression
     * @throws net.sf.saxon.trans.XPathException
     *          if any dynamic error occurs evaluating the
     *          expression
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        AtomicValue subjectValue = (AtomicValue) getSubjectExpression().evaluateItem(context);
        if (subjectValue == null) {
            return getOtherwiseExpression().iterate(context);
        }
        AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
        int selection = getIndex(key);
        if (selection == -1) {
            return getOtherwiseExpression().iterate(context);
        } else {
            return actions.get(selection).getChildExpression().iterate(context);
        }
    }

    /**
     * Process the instruction, without returning any tail calls
     *
     * @param output the destination for the result
     * @param context The dynamic context, giving access to the current node,
     */

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        AtomicValue subjectValue = (AtomicValue) getSubjectExpression().evaluateItem(context);
        if (subjectValue == null) {
            getOtherwiseExpression().process(output, context);
            return;
        }
        AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
        int selection = getIndex(key);
        if (selection == -1) {
            getOtherwiseExpression().process(output, context);
        } else {
            actions.get(selection).getChildExpression().process(output, context);
        }
    }

    @Override
    public TailCall processLeavingTail(Outputter output, XPathContext context) throws XPathException {
        AtomicValue subjectValue = (AtomicValue) getSubjectExpression().evaluateItem(context);
        if (subjectValue == null) {
            return doTailCall(getOtherwiseExpression(), output, context);
        }
        AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
        int selection = map.getOrDefault(key, -1);
        if (selection == -1) {
            return doTailCall(getOtherwiseExpression(), output, context);
        } else {
            return doTailCall(actions.get(selection).getChildExpression(), output, context);
        }
    }

    private TailCall doTailCall(Expression o, Outputter output, XPathContext context) throws XPathException {
        if (o instanceof TailCallReturner) {
            return ((TailCallReturner) o).processLeavingTail(output, context);
        } else {
            o.process(output, context);
            return null;
        }
    }


    @Override
    public int getImplementationMethod() {
        return PROCESS_METHOD | ITERATE_METHOD | EVALUATE_METHOD;
    }

    /**
     * Get a name identifying the kind of expression, in terms meaningful to a user.
     *
     * @return a name identifying the kind of expression, in terms meaningful to a user.
     * The name will always be in the form of a lexical XML QName, and should match the name used
     * in export() output displaying the expression.
     */
    @Override
    public String getExpressionName() {
        return "switch";
    }

    /**
     * Data structure used for holding details of a switch expression
     */

    public static class SwitchCaseInfo {
        public HashMap<AtomicMatchKey, Expression> index = new HashMap<>();
        public HashMap<AtomicMatchKey, AtomicValue> originalValues = new HashMap<>();
        public Expression commonlhs = null;
        public BuiltInAtomicType commontype = null;
        public StringCollator collation = null;
        public List<AtomicValue> values = new ArrayList<>();
        public List<Expression> actions = new ArrayList<>();
        public Expression defaultAction = null;

        /**
         * Attempt to make the index from atomic values to expressions
         * @return true if successful, false if there is an atomic value that is not comparable
         */

        public boolean makeIndex() {
            for (int i=0; i<values.size(); i++) {
                AtomicValue value = values.get(i);
                Expression action = actions.get(i);
                try {
                    AtomicMatchKey key = value.getXPathMatchKey(
                            collation, CalendarValue.MISSING_TIMEZONE);
                    if (key == null) {
                        return false;
                    } else if (index.get(key) == null) {
                        index.put(key, action);
                        originalValues.put(key, value);
                        // otherwise the value is a duplicate and will never be reached, ignore it
                    }
                } catch (NoDynamicContextException e) {
                    return false;
                }
            }
            return true;
        }
    }

    @Override
    public String getStreamerName() {
        return "SwitchExpression";
    }


}

// Copyright (c) 2009-2022 Saxonica Limited
