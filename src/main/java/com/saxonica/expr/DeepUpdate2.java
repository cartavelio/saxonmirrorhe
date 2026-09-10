package com.saxonica.expr;

import com.saxonica.functions.qt4.pin.LabelTool;
import com.saxonica.functions.extfn.PinFn;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.InsertBefore;
import net.sf.saxon.functions.hof.FilterFn;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceType;

import java.util.IdentityHashMap;
import java.util.function.Supplier;


/**
 * Implements the proposed update map/array expression in XQuery 4.0 (class should be moved...).
 * Derived from the saxon:deep-update extension instruction
 * @since 13
 */

public class DeepUpdate2 extends Instruction {

    private final UpdateTarget operation;
    private final Operand rootOp;
    private final Operand selectOp;
    private final Operand actionOp;



    public DeepUpdate2(UpdateTarget operation, Expression root, Expression select, Expression action) {
        this.operation = operation;
        rootOp = new Operand(this, root, OperandRole.INSPECT);
        selectOp = new Operand(this, select, OperandRole.FOCUS_CONTROLLED_ACTION);
        actionOp = new Operand(this, action, OperandRole.FOCUS_CONTROLLED_ACTION);
    }


    public Expression getRootExpression() {
        return rootOp.getChildExpression();
    }

    public Expression getSelectExpression() {
        return selectOp.getChildExpression();
    }

    public Expression getActionExpression() {
        return actionOp.getChildExpression();
    }

    public void setRootExpression(Expression expr) {
        rootOp.setChildExpression(expr);
    }

    public void setSelectExpression(Expression expr) {
        selectOp.setChildExpression(expr);
    }

    public void setActionExpression(Expression expr) {
        actionOp.setChildExpression(expr);
    }


    @Override
    public Iterable<Operand> operands() {
        return operandList(rootOp, selectOp, actionOp);
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     *
     * @return the string "saxon:while"
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.SAXON_DEEP_UPDATE;
    }

    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {

        rootOp.typeCheck(visitor, contextInfo);
        ContextItemStaticInfo ciso2 = new ContextItemStaticInfo(getRootExpression().getItemType(), false);
        selectOp.typeCheck(visitor, ciso2);
        ContextItemStaticInfo ciso3 = new ContextItemStaticInfo(getSelectExpression().getItemType(), false);
        actionOp.typeCheck(visitor, ciso3);

        TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);

        if (operation != UpdateTarget.UNKNOWN) {
            Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.SUBEXPRESSION, "update/root", 0);
            SequenceType required = operation == UpdateTarget.MAP ? SequenceType.SINGLE_MAP : ArrayItemType.SINGLE_ARRAY;
            setRootExpression(tc.staticTypeCheck(getRootExpression(), required, role, visitor));
        }

//        setSelectExpression(getSelectExpression());
//
//        setActionExpression(getActionExpression());

        return this;
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
        DeepUpdate2 exp = new DeepUpdate2(
                operation,
                getRootExpression().copy(rebindings),
                getSelectExpression().copy(rebindings),
                getActionExpression().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
    }

    /**
     * Get the item type of the items returned by evaluating this instruction
     *
     * @return the static item type of the instruction
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return getRootExpression().getItemType();
    }

    private static Item makePinnedRoot(Item root) throws XPathException {
        return PinFn.makePinnedRoot(root);
    }



    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("deepUpdate", this);
        out.setChildRole("root");
        getRootExpression().export(out);
        out.setChildRole("select");
        getSelectExpression().export(out);
        out.setChildRole("action");
        getActionExpression().export(out);
        out.endElement();
    }

    public Elaborator getElaborator() {
        return new DeepUpdateElaborator();
    }

    private static class DeepUpdateElaborator extends PushElaborator {


        @Override
        public PushEvaluator elaborateForPush() {
            DeepUpdate2 expr = (DeepUpdate2) getExpression();
            ItemEvaluator rootEval = expr.getRootExpression().makeElaborator().elaborateForItem();
            PullEvaluator selectPuller = expr.getSelectExpression().makeElaborator().elaborateForPull();
            SequenceEvaluator actionEval = expr.getActionExpression().makeElaborator().eagerly();
            return (output, context) -> {
                try {
                    Item root = rootEval.eval(context);
                    Item pinnedRoot = makePinnedRoot(root);
                    XPathContext c2 = context.newMinorContext();
                    c2.setCurrentIterator(new ManualIterator(pinnedRoot));
                    // ContainerSubstitutions is a mapping from maps/arrays in the original tree to
                    // maps/arrays in the updated tree
                    IdentityHashMap<Item, Item> containerSubstitutions = new IdentityHashMap<>();
                    // SequenceSubstitutions is used to track multiple updates to the same sequence.
                    // The key is the object identity of the sequence in the original tree; the
                    // corresponding value is a list of pairs of integers; the first integer in a pair
                    // is the position of a replaced item in the input sequence, the second integer
                    // is the number of items in its replacement.
                    XPathContext c3 = c2.newMinorContext();
                    FocusIterator selectIterator = new FocusTrackingIterator(selectPuller.iterate(c2));
                    c3.setCurrentIterator(selectIterator);
                    Item selected;
                    Item finalRoot = null;
                    while ((selected = selectIterator.next()) != null) {
                        StringMap label = selected.getLabel();
                        if (label == null) {
                            throw new XPathException(
                                    "Update expression: selected value " + Err.depict(selected) +
                                            " is not a labeled item", "XXXX9999");
                        }
                        // TODO: test if it belongs to the right tree
                        GroundedValue replacementValue = actionEval.evaluate(c3).materialize();
                        Item parent = (Item)label.getU(LabelTool.PARENT_KEY);
                        GroundedValue val = replacementValue;
                        while (parent != null) {
                            Item latestParent = parent;
                            // If two updates affect the same parent/ancestor at some level in the
                            // tree, then the updates need to be cumulative, so we keep track of a mapping
                            // from items in the original tree to the items that replace them in the new tree
                            if (containerSubstitutions.containsKey(parent)) {
                                latestParent = containerSubstitutions.get(parent);
                            }
                            int position = ((IntegerValue) label.getU(LabelTool.POSITION_KEY)).asSubscript();
                            if (latestParent instanceof ArrayItem) {
                                int arrayPos = ((IntegerValue) label.getU(LabelTool.KEY_KEY)).asSubscript() - 1;
                                GroundedValue oldMember = ((ArrayItem) latestParent).get(arrayPos);
                                GroundedValue newMember = replaceItem(oldMember, position, val);
                                val = ((ArrayItem) latestParent).put(
                                        ((IntegerValue) label.getU(LabelTool.KEY_KEY)).asSubscript() - 1, newMember);

                            } else if (latestParent instanceof MapItem) {
                                AtomicValue keyVal = (AtomicValue) label.getU(LabelTool.KEY_KEY);
                                GroundedValue oldMember = ((MapItem) latestParent).get(keyVal);
                                GroundedValue newMember = replaceItem(oldMember, position, val);
                                val = ((MapItem) latestParent).addEntry(
                                        ((AtomicValue) label.getU(LabelTool.KEY_KEY)), newMember);
                            } else {
                                assert false;
                            }
                            containerSubstitutions.put(parent, (Item) val);
                            containerSubstitutions.put(latestParent, (Item) val);
                            label = parent.getLabel();
                            parent = (Item)label.getU(LabelTool.PARENT_KEY);
                            finalRoot = (Item)val;
                        }

                    }
                    assert finalRoot != null;
                    output.append(finalRoot);
                } catch (XPathException e) {
                    throw e.maybeWithLocation(expr.getLocation()).maybeWithContext(context);
                }
                return null;
            };
        }
    }

    private static GroundedValue replaceItem(GroundedValue input, int position, GroundedValue replacement) {
        if (input.getLength() == 1 && position == 0) {
            return replacement;
        }
        SequenceIterator iter = new FilterFn.PositionalFilterIterator(input.iterate(), (it, pos) -> pos != position);
        iter = new InsertBefore.InsertIterator(iter,replacement.iterate(), position);
        return SequenceTool.toGroundedValue(iter);
    }

}

// Copyright (c) 2011-2023 Saxonica Limited
