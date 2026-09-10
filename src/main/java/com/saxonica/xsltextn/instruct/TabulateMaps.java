package com.saxonica.xsltextn.instruct;

import com.saxonica.functions.extfn.WithPedigree;
import com.saxonica.xsltextn.pedigree.Pedigree;
import com.saxonica.xsltextn.pedigree.PedigreeMapItem;
import com.saxonica.xsltextn.pedigree.PedigreeValue;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;
import java.util.function.Supplier;


/**
 * Handler for a saxon:tabulate-maps instruction in an XSLT stylesheet.
 * @since 9.9
 */

public class TabulateMaps extends Instruction {

    private final Operand rootOp;
    private final Operand selectOp;

    /**
     * Create the instruction
     * @param root the expression in the saxon:tabulate-maps/@root attribute
     * @param select the expression in the saxon:tabulate-maps/@select attribute
     */

    public TabulateMaps(Expression root, Expression select) {
        rootOp = new Operand(this, root, OperandRole.INSPECT);
        selectOp = new Operand(this, select, OperandRole.FOCUS_CONTROLLED_ACTION);
    }

    /**
     * Get the expression in the @root attribute
     * @return the root expression
     */

    public Expression getRootExpression() {
        return rootOp.getChildExpression();
    }

    /**
     * Get the expression in the @select attribute
     * @return the select expression
     */

    public Expression getSelectExpression() {
        return selectOp.getChildExpression();
    }

    /**
     * Set the expression in the @root attribute
     * @param expr the root expression
     */

    public void setRootExpression(Expression expr) {
        rootOp.setChildExpression(expr);
    }

    /**
     * Set the expression in the @select attribute
     * @param expr the select expression
     */

    public void setSelectExpression(Expression expr) {
        selectOp.setChildExpression(expr);
    }


    @Override
    public Iterable<Operand> operands() {
        return operandList(rootOp, selectOp);
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     *
     * @return the integer namecode of the QName saxon:tabulate-maps
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.SAXON_TABULATE_MAPS;
    }

    @Override
    public Expression typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {

        rootOp.typeCheck(visitor, contextInfo);
        ContextItemStaticInfo ciso2 = new ContextItemStaticInfo(getRootExpression().getItemType(), false);
        selectOp.typeCheck(visitor, ciso2);

        TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);

        Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.INSTRUCTION, "saxon:tabulate-maps/root", 0);
        SequenceType required = SequenceType.FUNCTION_ITEM_SEQUENCE;
        setRootExpression(tc.staticTypeCheck(getRootExpression(), required, role, visitor));

        required = MapType.SEQUENCE_OF_MAPS;
        role = () -> new RoleDiagnostic(RoleDiagnostic.INSTRUCTION, "saxon:tabulate-maps/select", 0);
        setSelectExpression(tc.staticTypeCheck(getSelectExpression(), required, role, visitor));

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
        TabulateMaps exp = new TabulateMaps(
                getRootExpression().copy(rebindings),
                getSelectExpression().copy(rebindings));
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
        return MapType.ANY_MAP_TYPE;
    }

    /*@Nullable*/
    @Override
    public TailCall processLeavingTail(Outputter output, XPathContext context) throws XPathException {
        Item root;
        SequenceIterator rootIterator = getRootExpression().iterate(context);
        SequenceIterator mappedRootIterator =
                MappingIterator.map(rootIterator,
                                    item -> SingletonIterator.makeIterator(WithPedigree.makeRootWithPedigree(item)));

        XPathContext c2 = context.newMinorContext();
        FocusIterator trackingIterator = c2.trackFocus(mappedRootIterator);
        while ((root = trackingIterator.next()) != null) {
            processRootItem(output, root, c2);
        }
        return null;
    }

    private void processRootItem(Outputter output, Item root, XPathContext c2) throws XPathException {
        FocusIterator selectIterator = SequenceTool.focusTracker(
                getSelectExpression().iterate(c2));
        Item selected;
        while ((selected = selectIterator.next()) != null) {
            if (!(selected instanceof PedigreeMapItem)) {
                if (selected instanceof MapItem) {
                    throw new XPathException("saxon:tabulate: selected item is a map with no pedigree", SaxonErrorCode.SXTM0001);
                } else {
                    throw new XPathException("saxon:tabulate: selected item is not a map", SaxonErrorCode.SXTM0001);
                    // This shouldn't happen, as a result of standard type checking
                }
            }
            MapItem selectedMap = (MapItem)selected;

            Pedigree pedigree = ((PedigreeValue)selected).getPedigree();
            PedigreeValue parent = pedigree.getContainer();
            int level = 0;
            LinkedList<AtomicValue> keys = new LinkedList<>();
            while (parent != null) {
                level++;
                Item latestParent = (Item) parent;
                if (latestParent instanceof ArrayItem) {
                    keys.addFirst(new Int64Value(pedigree.getIndex() + 1));
                } else if (latestParent instanceof MapItem) {
                    keys.addFirst(pedigree.getKey());
                    for (KeyValuePair kvp : ((MapItem) latestParent).keyValuePairs()) {
                        AtomicValue k = kvp.key;
                        if (!k.isIdentical(pedigree.getKey())) {
                            GroundedValue value = selectedMap.get(k);
                            if (value == null) {
                                selectedMap = selectedMap.addEntry(k, kvp.value);
                            } else if (k instanceof StringValue) {
                                String ks = k.getStringValue();
                                for (int suffix = 1; suffix < Integer.MAX_VALUE; suffix++) {
                                    ks = ks + (suffix + "");
                                    StringValue sv = new StringValue(ks);
                                    if (selectedMap.get(sv) == null) {
                                        selectedMap = selectedMap.addEntry(sv, kvp.value);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                } else {
                    assert false;
                }
                pedigree = parent.getPedigree();
                parent = pedigree.getContainer();
            }
            String keysKey = "_keys";
            while (selectedMap.get(new StringValue(keysKey)) != null) {
                keysKey = keysKey + "_";
            }
            List<GroundedValue> copy = new ArrayList<>(keys);
            selectedMap = selectedMap.addEntry(new StringValue(keysKey),
                                               new SimpleArrayItem(copy));
            output.append(selectedMap);
        }
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("tabulate", this);
        getRootExpression().export(out);
        getSelectExpression().export(out);
        out.endElement();
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
