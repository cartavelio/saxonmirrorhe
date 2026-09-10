////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.s9api.*;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import java.math.BigInteger;

/**
 * * This class is to use with Saxon on C++
 */
public class XdmValueForCpp {
    private BuiltInAtomicType type;
    private XdmValue value;
    private static QName errorCode = null;

    /**
     * Constructor
     */
    public XdmValueForCpp() {
        value = XdmEmptySequence.getInstance();
        type = (BuiltInAtomicType) BuiltInType.getSchemaType(StandardNames.XS_ERROR);
    }

    public String getErrorCode() {
        if (errorCode != null) {
            errorCode.toString();
        }
        return "";
    }


    public XdmValueForCpp(String typeStr, String valueStr) throws Exception {

        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);

        type = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        ConversionRules rules = new ConversionRules();
        Converter converter = rules.getConverter(BuiltInAtomicType.STRING, type);

        value = XdmValue.wrap(converter.convert(new StringValue(valueStr)).asAtomic());


    }


    public XdmValueForCpp(boolean b) {
        value = XdmValue.wrap(BooleanValue.get(b));
        type = BuiltInAtomicType.BOOLEAN;
    }

    public XdmValueForCpp(int i) {
            BigInteger bigInt = BigInteger.valueOf(i);
            value = XdmValue.wrap(IntegerValue.makeIntegerValue(bigInt).asAtomic());
            type = BuiltInAtomicType.INT;
    }

    public XdmValueForCpp(XdmNode node) {
        value = XdmValue.wrap(node.getUnderlyingValue());
        type = null;
    }

    /**
     * convert XdmValue to items in a ProcessorDataAccumulator object
     * @param value   XdmValue
     * @return  ProcessorDataAccumulator
     */
    public static ProcessorDataAccumulator makeArrayFromXdmValue(XdmValue value) {
        XdmItem[] items = new XdmItem[value.size()];

        for(int i=0;i<value.size();i++) {
            XdmItem item = value.itemAt(i);
            items[i] = item;
        }
        ProcessorDataAccumulator dataAccumulator = new ProcessorDataAccumulator(items);
        return dataAccumulator;

    }

    @CEntryPoint(name = "makeArrayFromXdmValue2")
    public static CLongPointer makeArrayFromXdmValue2(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle valueRef) {
        if(valueRef == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmValue value = SaxonCAPI.objectHandles.get(valueRef);
        if(value.size() == 0) {
            return WordFactory.nullPointer();
        }
        int size = value.size()+1;
        long[] valueRefs = new long[size];
        valueRefs[0] = value.size();
        int i = 1;
        for (XdmItem item : value) {
            ObjectHandle objectHandle = SaxonCAPI.createHandle(item);
            valueRefs[i] = objectHandle.rawValue();
            i++;
        }

        CLongPointer addr = SaxonCAPI.longArrayToNative(alloc, valueRefs);
        return addr;

    }

    /**
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param valueRef - reference to XdmValue held in the ObjectHandle's pool
     * @return long value reference to the XdmValue
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getTypedValue")
    public static long getTypedValue(IsolateThread thread, ObjectHandle valueRef) throws SaxonApiException {
        XdmNode value = SaxonCAPI.objectHandles.get(valueRef);
        XdmValue resultValue = value.getTypedValue();
        ObjectHandle objectHandle = SaxonCAPI.createHandle(resultValue);
        return objectHandle.rawValue();
    }

    /**
     * Get the value converted to a boolean using the XPath casting rules
     * @param thread - Graalvm runtime data structure for the thread
     * @param valueRef - reference to XdmValue held in the ObjectHandle's pool
     * @return byte value representing the boolean
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getBooleanValue")
    public static byte getBooleanValue(IsolateThread thread, ObjectHandle valueRef) throws SaxonApiException {
        XdmAtomicValue value = SaxonCAPI.objectHandles.get(valueRef);

        byte result =CTypeConversion.toCBoolean(value.getBooleanValue());
        return result;
    }

    /**
     * Get the value converted to a double using the XPath casting rules.
     * <p>If the value is a string, the XSD 1.1 rules are used, which means that the string
     * "+INF" is recognised.</p>
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param valueRef - reference to XdmValue held in the ObjectHandle's pool
     * @return  the result of converting to a double - reference to XdmAtomicValue given as long
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getDoubleValue")
    public static double getDoubleValue(IsolateThread thread, ObjectHandle valueRef) throws SaxonApiException {
        XdmAtomicValue value = SaxonCAPI.objectHandles.get(valueRef);
        return value.getDoubleValue();
    }

    /**
     * Get the value converted to an integer using the XPath casting rules
     * @param thread - Graalvm runtime data structure for the thread
     * @param valueRef - reference to XdmValue held in the ObjectHandle's pool
     * @return  long value as reference for the XdmAtomicValue - the result of converting to an integer
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getLongValue")
    public static long getLongValue(IsolateThread thread, ObjectHandle valueRef) throws SaxonApiException {
        XdmAtomicValue value = SaxonCAPI.objectHandles.get(valueRef);
        return value.getLongValue();
    }

    /**
     * Get a hashcode that reflects the rules for equality matching
     * @param thread - Graalvm runtime data structure for the thread
     * @param valueRef - reference to XdmValue held in the ObjectHandle's pool
     * @return a suitable hash code
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getHashCode")
    public static int getHashCode(IsolateThread thread, ObjectHandle valueRef) throws SaxonApiException {
        XdmAtomicValue value = SaxonCAPI.objectHandles.get(valueRef);
        return value.hashCode();
    }


    public static Processor getProcessor(Object node){
        return (Processor) ((XdmNode)node).getUnderlyingNode().getConfiguration().getProcessor();
    }


    public XdmValue getXdmValue() {
        return value;
    }

    public BuiltInAtomicType getAtomicType() {
        return type;
    }

    public QName getTypeName() {
        return new QName(type.getTypeName());
    }

    public static QName getTypeName(String typeStr) {

        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);

        BuiltInAtomicType typei = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        return new QName(typei.getTypeName());

    }

    public String getStringValue() {
        return value.toString();
    }
}
