package com.saxonica.xsltextn.instruct;

import com.saxonica.xsltextn.pedigree.Pedigree;
import com.saxonica.xsltextn.pedigree.PedigreeArrayItem;
import com.saxonica.xsltextn.pedigree.PedigreeMapItem;
import com.saxonica.xsltextn.pedigree.PedigreeValue;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceType;

import java.util.IdentityHashMap;
import java.util.function.Supplier;


/**
 * Handler for saxon:deep-update elements in stylesheet.
 * @since 9.9
 */

public class DeepUpdate extends Instruction {

    private final Operand rootOp;
    private final Operand selectOp;
    private final Operand actionOp;


    public DeepUpdate(Expression root, Expression select, Expression action) {
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

        Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.INSTRUCTION, "deep-update/root", 0);
        SequenceType required = SequenceType.FUNCTION_ITEM_SEQUENCE;
        setRootExpression(tc.staticTypeCheck(getRootExpression(), required, role, visitor));

        role = () -> new RoleDiagnostic(RoleDiagnostic.INSTRUCTION, "deep-update/select", 0);
        setSelectExpression(tc.staticTypeCheck(getSelectExpression(), required, role, visitor));

        role = () -> new RoleDiagnostic(RoleDiagnostic.INSTRUCTION, "deep-update/action", 0);
        setActionExpression(tc.staticTypeCheck(getActionExpression(), SequenceType.ANY_SEQUENCE, role, visitor));

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
        DeepUpdate exp = new DeepUpdate(
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

    /*@Nullable*/
    @Override
    public TailCall processLeavingTail(Outputter output, XPathContext context) throws XPathException {
        Item root;
        SequenceIterator rootIterator = getRootExpression().iterate(context);
        SequenceIterator mappedRootIterator =
                MappingIterator.map(rootIterator,
                                    item -> SingletonIterator.makeIterator(makeRootWithPedigree(item)));

        XPathContext c2 = context.newMinorContext();
        FocusIterator trackingIterator = c2.trackFocus(mappedRootIterator);
        while ((root = trackingIterator.next()) != null) {
            Item replacedRoot = processRootItem(root, c2);
            output.append(replacedRoot);
        }
        return null;
    }

    private static Item makeRootWithPedigree(Item root) {
        if (root instanceof MapItem) {
            return new PedigreeMapItem((MapItem) root);
        } else if (root instanceof ArrayItem) {
            return new PedigreeArrayItem((ArrayItem) root);
        } else {
            assert false;
            return null;
        }
    }

    private Item processRootItem(Item root, XPathContext c2) throws XPathException {
        IdentityHashMap<Item, Item> substitutions = new IdentityHashMap<>();
        XPathContext c3 = c2.newMinorContext();
        FocusIterator selectIterator = c3.trackFocus(getSelectExpression().iterate(c2));
        Item selected;
        while ((selected = selectIterator.next()) != null) {
            if (!(selected instanceof PedigreeValue)) {
                if (selected instanceof MapItem) {
                    throw new XPathException("saxon:deep-update: selected item is a map with no pedigree");
                } else if (selected instanceof ArrayItem) {
                    throw new XPathException("saxon:deep-update: selected item is an array with no pedigree");
                } else {
                    throw new XPathException(
                            "saxon:deep-update: selected value " + Err.depict(selected) +
                                    " is an instance of "
                                    + SequenceTool.getItemType(selected, c2.getConfiguration().getTypeHierarchy())
                                    + " (expected a map/array with pedigree)");
                }
            }
            SequenceIterator actionIter = getActionExpression().iterate(c3);
            GroundedValue actionVal = SequenceTool.toGroundedValue(actionIter);
            Pedigree pedigree = ((PedigreeValue)selected).getPedigree();
            PedigreeValue parent = pedigree.getContainer();
            GroundedValue val = actionVal;
            while (parent != null) {
                Item latestParent = (Item) parent;
                // If two updates affect the same parent/ancestor at some level in the
                // tree, then the updates need to be cumulative, so we keep track of a mapping
                // from items in the original tree to the items that replace them in the new tree
                if (substitutions.containsKey((Item)parent)) {
                    latestParent = substitutions.get((Item)parent);
                }
                if (latestParent instanceof ArrayItem) {
                    val = ((ArrayItem) latestParent).put(pedigree.getIndex(), val);
                } else if (latestParent instanceof MapItem) {
                    val = ((MapItem) latestParent).addEntry(pedigree.getKey(), val);
                } else {
                    assert false;
                }
                substitutions.put((Item) parent, (Item) val);
                substitutions.put(latestParent, (Item) val);
                pedigree = parent.getPedigree();
                parent = pedigree.getContainer();
            }
            root = (Item)val;
        }
        return root;
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

}

// Copyright (c) 2011-2022 Saxonica Limited
