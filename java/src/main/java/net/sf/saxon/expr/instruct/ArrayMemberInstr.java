////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.instruct;

import com.saxonica.trans.XSLArrayMember;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.ItemElaborator;
import net.sf.saxon.expr.elab.ItemEvaluator;
import net.sf.saxon.expr.elab.SequenceEvaluator;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.functions.ConstantFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.query.Annotation;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;


/**
 * An xsl:array-member instruction in the stylesheet (XSLT 4.0 addition)
 */

public class ArrayMemberInstr extends Instruction {

    private final Operand selectOp;

    /**
     * Create a compiled xsl:array-member instruction
     *
     * @param select the expression in the select attribute or sequence constructor
     */

    public ArrayMemberInstr(Expression select) {
        selectOp = new Operand(this, select, OperandRole.SAME_FOCUS_ACTION);
    }


    public Expression getSelectExp() {
        return selectOp.getChildExpression();
    }

    public void setSelectExp(Expression nameExp) {
        selectOp.setChildExpression(nameExp);
    }

    @Override
    public Iterable<Operand> operands() {
        return selectOp;
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     *
     * @return the string "xsl:processing-instruction"
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.XSL_ARRAY_MEMBER;
    }

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return AnyFunctionType.INSTANCE;
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
        ArrayMemberInstr exp = new ArrayMemberInstr(getSelectExp().copy(rebindings));
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
        out.startElement("xslArrayMember", this);
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
        return new ArrayMemberInstrElaborator();
    }


    private static class ArrayMemberInstrElaborator extends ItemElaborator {

        @Override
        public ItemEvaluator elaborateForItem() {
            ArrayMemberInstr expr = (ArrayMemberInstr) getExpression();
            Expression selectExp = expr.getSelectExp();
            SequenceEvaluator selectEval = selectExp.makeElaborator().eagerly();
            return cxt -> {
                GroundedValue member = selectEval.evaluate(cxt).materialize();
                return new ConstantFunction(member, SequenceType.ANY_SEQUENCE)
                        .withAnnotationList(AnnotationList.singleton(
                                new Annotation(XSLArrayMember.ANNOTATION_NAME), cxt.getConfiguration()));
            };

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




