package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.LocalUnionType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;


/**
 * Binds an {@link UnboundFieldGetter} to a specific set of fields and acts as an XPath function so it can be called by
 * the user.
 */
class FieldGetter implements Callable {
    static SequenceType FIELD_FN_ARG_TYPE = SequenceType.makeSequenceType(
            new LocalUnionType(BuiltInAtomicType.STRING, BuiltInAtomicType.INTEGER),
            StaticProperty.EXACTLY_ONE);
    static FunctionItemType FIELD_FN_TYPE = new SpecificFunctionType(
            new SequenceType[]{FIELD_FN_ARG_TYPE},
            SequenceType.OPTIONAL_STRING);


    private final UnboundFieldGetter fieldGetter;
    private final List<GroundedValue> fields;

    static CallableFunction createBoundFunction(UnboundFieldGetter fieldGetter, Iterable<GroundedValue> fields) {
        return new CallableFunction(1, new FieldGetter(fieldGetter, fields), FIELD_FN_TYPE);
    }

    /**
     * Create a new instance by binding the UnboundFieldGetter to a row.
     *
     * @param fieldGetter the field getter for the CSV the row is part of.
     * @param iterable    the fields of the row.
     */
    public FieldGetter(UnboundFieldGetter fieldGetter, Iterable<GroundedValue> iterable) {
        this.fieldGetter = fieldGetter;
        fields = new ArrayList<>();
        iterable.forEach(fields::add);
    }

    /**
     * Call the Callable.
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences.
     *                  <p>Generally it is advisable, if calling iterate() to process a supplied sequence, to
     *                  call it only once; if the value is required more than once, it should first be converted to a
     *                  {@link GroundedValue} by calling the utility methd SequenceTool.toGroundedValue().</p>
     *                  <p>If the expected value is a single item, the item should be obtained by calling
     *                  Sequence.head(): it cannot be assumed that the item will be passed as an instance of
     *                  {@link net.sf.saxon.om.Item} or {@link net.sf.saxon.value.AtomicValue}.</p>
     *                  <p>It is the caller's responsibility to perform any type conversions required
     *                  to convert arguments to the type expected by the callee. An exception is where this Callable is
     *                  explicitly an argument-converting wrapper around the original Callable.</p>
     * @return the result of the evaluation, in the form of a Sequence. It is the responsibility of the callee to ensure
     *         that the type of result conforms to the expected result type.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        AtomicValue key = arguments[0].head().atomize().head();
        if (key.getPrimitiveType() == BuiltInAtomicType.INTEGER) {
            return fieldGetter.get((Int64Value) key, fields);
        } else {
            return fieldGetter.get((StringValue) key, fields);
        }
    }

    /**
     * The value of toString() may be used to identify this function in diagnostics
     */
    public String toString() {
        return "csv-to-xdm.field";
    }
}
