package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

/**
 * This class supports the proposed QT4 function fn:schema-type.
 */

public class SchemaTypeFn extends SystemFunction {

    public final static RecordTest schemaTypeRecord = new RecordTest();

    private final static SequenceType[] NO_ARGS = new SequenceType[]{};

    static {
        schemaTypeRecord.setDetails(
                true,
                f("name", SequenceType.SINGLE_QNAME, false),
                f("is-simple", SequenceType.SINGLE_BOOLEAN, false),
                f("base-type", SequenceType.one(
                        new SpecificFunctionType(SequenceType.optional(schemaTypeRecord))), false),
                f("primitive-type", SequenceType.one(
                        new SpecificFunctionType(SequenceType.one(schemaTypeRecord))), true),
                f("variety", SequenceType.one(
                        EnumerationUnionType.of("atomic", "list", "union", "empty", "simple", "element-only", "mixed")), false),
                f("members", SequenceType.one(
                        new SpecificFunctionType(SequenceType.zeroOrMore(schemaTypeRecord))), true),
                f("simple-content-type", SequenceType.one(
                        new SpecificFunctionType(SequenceType.one(schemaTypeRecord))), false),
                f("matches", SequenceType.one(
                        new SpecificFunctionType(SequenceType.SINGLE_ATOMIC,
                                                 SequenceType.SINGLE_BOOLEAN)), true),
                f("constructor", SequenceType.one(
                        new SpecificFunctionType(SequenceType.SINGLE_ATOMIC,
                                                 SequenceType.SINGLE_ATOMIC)), true)
        );
    }
    


    private static RecordTest.Field f(String name, SequenceType type, boolean optional) {
        return new RecordTest.Field(name, type, optional);
    }


    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call.
     * @return the results of the function, as a sequence of arrays
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        QNameValue name = (QNameValue) arguments[0].head();
        SchemaType st = getRetainedStaticContext().getPackageData()
                .getImportedSchema().getSchemaType(name.getStructuredQName());
        if (st == null) {
            return EmptySequence.getInstance();
        }
        return makeSchemaRecord(st);
    }

    public static MapItem makeSchemaRecord(SchemaType st) {
        DictionaryMap result = new DictionaryMap(10);
        result.initialPut("is-atomic", BooleanValue.get(st.isSimpleType()));
        if (!st.isAnonymousType()) {
            result.initialPut("name", new QNameValue(st.getStructuredQName(), BuiltInAtomicType.QNAME));
        }
        SchemaType base = st.getBaseType();
        if (base != null) {
            result.initialPut("base-type", makeSchemaRecord(base));
        }
        String variety = "*unknown*";
        if (st.isSimpleType()) {
            if (st.isAtomicType()) {
                variety = "atomic";
            } else if (((SimpleType) st).isUnionType()) {
                variety = "union";
            } else if (((SimpleType) st).isListType()) {
                variety = "list";
            }
        } else {
            if (((ComplexType) st).isSimpleContent()) {
                variety = "simple";
            } else if (((ComplexType) st).isEmptyContent()) {
                variety = "empty";
            } else if (((ComplexType) st).isMixedContent()) {
                variety = "mixed";
            } else {
                variety = "element-only";
            }
        }
        result.initialPut("variety", new StringValue(variety));  // TODO: should be an enum type

        if (st.isAtomicType() && !st.isSameType(BuiltInAtomicType.ANY_ATOMIC)) {
            Callable callable = new CallableDelegate((context, args) -> makeSchemaRecord(((AtomicType) st).getPrimitiveAtomicType()));
            CallableFunction fn = new CallableFunction(0, callable,
                                                       new SpecificFunctionType(SequenceType.one(schemaTypeRecord)));
            result.initialPut("primitive-type", fn);

        }
        return result;
    }


}

// Copyright (c) 2024 Saxonica Limited
