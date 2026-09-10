package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SimpleLazyFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;

/**
 * This class supports the proposed QT4 function fn:schema-type.
 */

public class SchemaTypeFn extends SystemFunction {

    public final static RecordTest schemaTypeRecord = new RecordTest(new StructuredQName("fn", NamespaceUri.FN, "schema-type-record"));

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
            SimpleLazyFunction fn = new SimpleLazyFunction(() -> makeSchemaRecord(base),
                                                           SequenceType.one(schemaTypeRecord));
            result.initialPut("base-type", fn);
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
            AtomicType prim = ((AtomicType) st).getPrimitiveAtomicType();
            // This might not be truly primitive, for example it might be xs:integer
            while (prim.getBaseType() != BuiltInAtomicType.ANY_ATOMIC) {
                prim = (AtomicType)prim.getBaseType();
            }
            final AtomicType finalResult = prim;
            SimpleLazyFunction fn = new SimpleLazyFunction(() -> makeSchemaRecord(finalResult),
                                                           SequenceType.one(schemaTypeRecord));
            result.initialPut("primitive-type", fn);

        }

        if (st.isSimpleType() && ((SimpleType)st).isListType()) {
            final SimpleType itemType = ((ListType)st).getItemType();
            SimpleLazyFunction fn = new SimpleLazyFunction(() -> makeSchemaRecord(itemType),
                                                           SequenceType.one(schemaTypeRecord));
            result.initialPut("members", fn);
        }
        if (st.isSimpleType() && ((SimpleType) st).isUnionType()) {
            final List<? extends PlainType> memberTypes = ((UnionType) st).getPlainMemberTypes();
            // TODO: handle general XSD union types that are not "plain unions"
            SimpleLazyFunction fn = new SimpleLazyFunction(() -> {
                List<MapItem> memberTypeRecords = new ArrayList<>();
                for (PlainType m : memberTypes) {
                    memberTypeRecords.add(makeSchemaRecord((SimpleType)m));
                }
                return SequenceExtent.makeSequenceExtent(memberTypeRecords);
            }, SequenceType.zeroOrMore(schemaTypeRecord));
            result.initialPut("members", fn);
        }

        if (st.isAtomicType()) {
            CallableFunction matches = new CallableFunction((context, args) -> {
                TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
                return BooleanValue.get(((AtomicType) st).matches(args[0].head(), th));
            }, new SpecificFunctionType(SequenceType.SINGLE_ITEM, SequenceType.SINGLE_BOOLEAN));
            result.initialPut("matches", matches);
        }
        return result;
    }


}

// Copyright (c) 2024 Saxonica Limited
