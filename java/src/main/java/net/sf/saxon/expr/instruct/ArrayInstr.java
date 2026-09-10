////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.instruct;

import com.saxonica.trans.XSLArrayMember;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;


/**
 * An xsl:array instruction in the stylesheet (XSLT 4.0 addition)
 */

public class ArrayInstr extends Instruction {

    private final Operand forEachOp;
    private final Operand selectOp;

    /**
     * Create a compiled xsl:array instruction
     *
     * @param select the expression in the select attribute or sequence constructor
     */

    public ArrayInstr(Expression forEach, Expression select) {
        forEachOp = new Operand(this, forEach, OperandRole.FOCUS_CONTROLLING_SELECT);
        selectOp = new Operand(this, select, OperandRole.FOCUS_CONTROLLED_ACTION);
    }


    public Expression getSelectExp() {
        return selectOp.getChildExpression();
    }

    public void setSelectExp(Expression nameExp) {
        selectOp.setChildExpression(nameExp);
    }

    public Expression getForEachExp() {
        return forEachOp.getChildExpression();
    }

    @Override
    public Iterable<Operand> operands() {
        return operandList(forEachOp, selectOp);
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     *
     * @return the string "xsl:processing-instruction"
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.XSL_ARRAY;
    }

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return ArrayItemType.ANY_ARRAY_TYPE;
    }

    @Override
    public int getCardinality() {
        return StaticProperty.EXACTLY_ONE;
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables to be re-bound
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        ArrayInstr exp = new ArrayInstr(getForEachExp().copy(rebindings), getSelectExp().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
    }

    @Override
    public int getDependencies() {
        return getSelectExp().getDependencies() | super.getDependencies();
    }



    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("xslArray", this);
        getForEachExp().export(out);
        getSelectExp().export(out);
        out.endElement();
    }


    /**
     * Make an elaborator for this expression
     *
     * @return a suitable elaborator
     */

    @Override
    public Elaborator getElaborator() {
        return new ArrayInstrElaborator();
    }

    private static boolean isParcel(Item item) {
        return item instanceof FunctionItem fit && fit.getArity() == 0 &&
                fit.getAnnotations().includes(XSLArrayMember.ANNOTATION_NAME);
    }

    private static class ArrayInstrElaborator extends ItemElaborator {

        @Override
        public ItemEvaluator elaborateForItem() {
            ArrayInstr expr = (ArrayInstr) getExpression();
            PullEvaluator forEach = expr.getForEachExp().makeElaborator().elaborateForPull();
            if (expr.getSelectExp() instanceof ContextItemExpression) {
                return context -> {
                    List<GroundedValue> members = new ArrayList<>(8);
                    SequenceIterator iter = forEach.iterate(context);
                    Item it;
                    while ((it = iter.next()) != null) {
                        if (isParcel(it)) {
                            members.add(((FunctionItem)it).call(context, new GroundedValue[]{}).materialize());
                        } else {
                            members.add(it);
                        }
                    }
                    return new SimpleArrayItem(members);
                };
            } else {
                SequenceEvaluator selectEval = expr.getSelectExp().makeElaborator().eagerly();
                PullEvaluator pullEval = expr.getForEachExp().makeElaborator().elaborateForPull();
                return context -> {
                    List<GroundedValue> members = new ArrayList<>(8);
                    SequenceIterator iter = pullEval.iterate(context);
                    FocusIterator focusIterator = new FocusTrackingIterator(iter);
                    XPathContext c2 = context.newMinorContext();
                    c2.setCurrentIterator(focusIterator);
                    while (focusIterator.next() != null) {
                        GroundedValue gv = selectEval.evaluate(c2).materialize();
                        if (gv.getLength() == 1 && isParcel(gv.head())) {
                            members.add(((FunctionItem)gv.head()).call(context, new GroundedValue[]{}).materialize());
                        } else if (gv.getLength() >= 2) {
                            for (Item it : gv.asIterable()) {
                                if (isParcel(it)) {
                                    throw new XPathException("xsl:array-member cannot be used to create one "
                                                                     + "item in the array member, only to create the entire array member", "XTSE4060");
                                }
                            }
                        } else {
                            members.add(selectEval.evaluate(c2).materialize());
                        }
                    }
                    return new SimpleArrayItem(members);
                };
            }

            };
        }

        public static final StringValue valueField = new StringValue(new Twine8("value"));

        private GroundedValue memberFromValueRecord(Item valueRecord) throws XPathException {
            if (valueRecord instanceof MapItem) {
                GroundedValue val = ((MapItem)valueRecord).get(valueField);
                if (val != null) {
                    return val;
                }
            }
            throw new XPathException("In input to xsl:array, expected a value record. Found " + Err.depict(valueRecord));
        }


    }

    

