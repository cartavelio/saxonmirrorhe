////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.Type;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * * This utility class consists generic functions for use with XDM data model.
 * For the use of Saxon on C++
 */
public class XdmUtils {


    public XdmUtils() {

    }

    public static QName getTypeName(String typeStr) {

        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);

        BuiltInAtomicType typei = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        StructuredQName sname = typei.getTypeName();
        return new QName(sname);

    }

    @CEntryPoint(name = "getStringValueForXdmValue")
    public static CCharPointer getStringValue_XdmValue(IsolateThread thread, ObjectHandle value) {
        XdmValue item = SaxonCAPI.objectHandles.get(value);
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(item.toString());
        return holder.get();
    }

    @CEntryPoint(name = "getStringValueForXdmItem")
    public static CCharPointer getStringValue_XdmItem(IsolateThread thread, ObjectHandle value) {
        XdmItem item = SaxonCAPI.objectHandles.get(value);
        if(item == null) {
            return null;
        }
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(item.getStringValue());
        return holder.get();
    }

    @CEntryPoint(name = "xdmItemToString")
    public static CCharPointer toString_XdmItem(IsolateThread thread, ObjectHandle value) {
        XdmItem item = SaxonCAPI.objectHandles.get(value);
        if(item == null) {
            return null;
        }
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(item.toString());
        return holder.get();
    }

    /**
     * Attempt to Downcast XdmItem to XdmNode. If this is not possible then return null
     */
    public static XdmNode castToXdmNode(XdmItem item) {
        if (item instanceof XdmNode) {
            return (XdmNode) item;
        }
        return null;
    }

    public static String getPrimitiveTypeName(XdmAtomicValue value) {
        return getEQName(value.getPrimitiveTypeName());
    }

    public static String getFunctionName(XdmFunctionItem item) {
        return getEQName(item.getName());
    }

    public static XdmFunctionItem getSystemFunction(Processor proc, String eqname, int arity) throws SaxonApiException {
        return  XdmFunctionItem.getSystemFunction(proc, QName.fromClarkName(eqname), arity);
    }

    /**
     * Attempt to Downcast XdmItem to XdmAtomicValue. If this is not possible then return null
     */
    public static XdmAtomicValue castToXdmAtomicValue(XdmItem item) {
        if (item.isAtomicValue()) {
            return (XdmAtomicValue) item;
        }
        return null;
    }

    /**
     * Attempt to Downcast XdmItem to XdmFunctionItem. If this is not possible then return null
     */
    public static XdmFunctionItem castToXdmFunctionItem(XdmItem item) {
        if (item instanceof XdmFunctionItem) {
            return (XdmFunctionItem) item;
        }
        return null;
    }

    public static XdmValue [] convertXdmArrayToArrayObject(XdmArray value) {
        XdmValue arr [] = new XdmValue[value.arrayLength()];
        value.asList().toArray(arr);
        return arr;

    }

    @CEntryPoint(name = "j_makeXdmArray")
    public static long makeXdmArray(IsolateThread thread, ObjectHandle processorDataAccumulatorRef) {
         ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);

         XdmValue [] xdmValues = (XdmValue[]) processorDataAccumulator.getValues();
         XdmArray array = new XdmArray(xdmValues);

         ObjectHandle arrayHandle = SaxonCAPI.objectHandles.create(array);

         return arrayHandle.rawValue();

    }


    public static XdmMap makeXdmMap(XdmAtomicValue [] keyArr, XdmValue [] valueArray) {
        if(keyArr == null || valueArray == null || keyArr.length ==0 || valueArray.length == 0) {
            if(SaxonCAPI.debug){
                    System.err.println("DEBUG: makeXdmMap has null arrays");
                }

            return null;
        }
        Map<XdmAtomicValue, XdmValue> jmap = new HashMap<>();


        XdmMap map;
        if(SaxonCAPI.debug){
            if(keyArr != null && valueArray != null) {
                System.err.println("DEBUG: makeXdmMap: key length=" + keyArr.length + " value length=" + valueArray.length);
            } else {
                System.err.println("DEBUG: makeXdmMap has null arrays");
            }

        }
        for(int i=0; i< keyArr.length;i++){
            jmap.put(keyArr[i], valueArray[i]);
        }

        map = new XdmMap(jmap);
        if(SaxonCAPI.debug){
                System.err.println("DEBUG: makeXdmMap: map size=" + map.mapSize() );


        }
        return map;

    }


    public static XdmValue [] getXdmMapValues(XdmMap map) {

        XdmValue [] arr = map.values().toArray(new XdmValue[0]);
        return arr;

    }


    public static XdmAtomicValue [] getXdmMapKeys(XdmMap map) {

        XdmAtomicValue [] arr = map.keySet().toArray(new XdmAtomicValue[0]);
        return arr;

    }

    /**
     * Convert the nodeKind enum type to a integer
     *
     * @param thread
     * @param nodeRef
     */
    @CEntryPoint(name = "j_getNodeKind")
    public static int convertNodeKindType(IsolateThread thread, ObjectHandle nodeRef) {
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        if(node == null) {
            return 0;
        }

        XdmNodeKind nodeKind = node.getNodeKind();
        switch (nodeKind) {
            case DOCUMENT:
                return Type.DOCUMENT;
            case ELEMENT:
                return Type.ELEMENT;
            case ATTRIBUTE:
                return Type.ATTRIBUTE;
            case TEXT:
                return Type.TEXT;
            case COMMENT:
                return Type.COMMENT;
            case PROCESSING_INSTRUCTION:
                return Type.PROCESSING_INSTRUCTION;
            case NAMESPACE:
                return Type.NAMESPACE;
        }

        return 0;

    }

    /*
     * The expanded name, as a string using the notation devised by EQName.
     * If the name is in a namespace, the resulting string takes the form <code>Q{uri}local</code>.
     * Otherwise, the value is the local part of the name.
     * @param qname
     *
     */
    public static String getEQName(QName qname) {
        String uri = qname.getNamespaceUri().toString();
        if (uri.length() == 0) {
            return qname.getLocalName();
        } else {
            return "Q{" + uri + "}" + qname.getLocalName();
        }
    }



    public static String getLocalName(XdmNode node) {
        QName qname = node.getNodeName();
        if (qname == null) {
            return null;
        }
        return qname.getLocalName();
    }

    /**
     * Get the name of the node, as a EQName
     *
     * @param node
     * @return the name of the node. In the case of unnamed nodes (for example, text and comment nodes)
     * return null.
     */
    public static String getNodeName(XdmNode node) {
        QName qname = node.getNodeName();
        if (qname == null) {
            return null;
        }
        return XdmUtils.getEQName(qname);

    }

    /**
     * Convert an array of xdmValues (Which we expect to be XdmItem objects) to a XdmValue
     * @param values   - given as an array of XdmValue
     * @return   XdmValue
     */
    public static XdmValue buildXdmValue(XdmValue [] values){

        if(values == null) {
            return null;
        }
        List<XdmItem> items = new ArrayList<>();

        for(int i = 0; i< values.length; i++) {
                items.add((XdmItem) values[i]);
        }
        return new XdmValue(items);
    }


    /**
     * Create a string representation of the array of values. The is the result of serializing
     * the value using the adaptive serialization method.
     * @param processorDataAccumulatorRef - ObjectHandle to array of XdmValue held as ProcessorDataAccumulator
     * @return a string representation of the value
     */
    @CEntryPoint(name = "xdmValueArrayToString")
    public static CCharPointer xdmValueArrayToString(IsolateThread thread, ObjectHandle processorDataAccumulatorRef){
        ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
        XdmValue [] values = (XdmValue[]) processorDataAccumulator.getValues();
        if(values == null) {
            return null;
        }
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(buildXdmValue(values).toString());
        return holder.get();
    }






    /**
     * Get the string value of a named attribute of the element passed
     *
     * @param node   the element in question
     * @param eqname the name of the required attribute
     * @return null if this node is not an element, or if this element has no
     * attribute with the specified name. Otherwise return the string value of the
     * selected attribute node.
     */
    public static String getAttributeValue(XdmNode node, String eqname) {

        QName name = QName.fromEQName(eqname);
        if (SaxonCAPI.debug) {
            System.err.println("EQName= " + eqname);
            System.err.println("Java-Call Att-name=" + name.getClarkName() + " Value=" + node.getAttributeValue(name));
        }
        return node.getAttributeValue(name);
    }

    public static XdmNode[] getAttributeNodes(XdmNode node) {
        List<XdmNode> children = new ArrayList<XdmNode>();
        XdmSequenceIterator iter = node.axisIterator(Axis.ATTRIBUTE);
        while (iter.hasNext()) {
            children.add((XdmNode) iter.next());
        }
        if (children.size() == 0) {
            return null;
        }
        return children.toArray(new XdmNode[0]);
    }

    public static XdmNode[] getChildren(XdmNode node) {
        List<XdmNode> children = new ArrayList<XdmNode>();
        XdmSequenceIterator iter = node.axisIterator(Axis.CHILD);
        while (iter.hasNext()) {
            children.add((XdmNode) iter.next());
        }
        if (children.size() == 0) {
            return null;
        }
        return children.toArray(new XdmNode[0]);
    }



    public static XdmNode getChild(XdmNode node, int i) {
        XdmSequenceIterator iter = node.axisIterator(Axis.CHILD);
        int pos = 0;
        while (iter.hasNext()) {
            if(pos == i) {
                 return (XdmNode) iter.next();
            }
            pos++ ;
        }
        return null;
    }

    public static int getChildCount(XdmNode node) {
        int num = 0;
        XdmSequenceIterator iter = node.axisIterator(Axis.CHILD);
        while (iter.hasNext()) {
            iter.next();
            num++;
        }

        return num;
    }

    public static int getAttributeCount(XdmNode node) {
        int num = 0;
        XdmSequenceIterator iter = node.axisIterator(Axis.ATTRIBUTE);
        while (iter.hasNext()) {
            iter.next();
            num++;
        }

        return num;
    }



    public static void setDocBuilderBaseURI(DocumentBuilder builder, String baseStr) throws SaxonApiException {
        URI baseURI = null;
        try {
            baseURI = new URI(baseStr);
        } catch (URISyntaxException e) {
            throw new SaxonApiException(e.getMessage());
        }
        builder.setBaseURI(baseURI);


    }


    @CEntryPoint(name = "j_makeStringValue")
    public static long makeStringValue(IsolateThread thread,CCharPointer str){
        String valueStr = CTypeConversion.toJavaString(str);

        XdmAtomicValue value = new XdmAtomicValue(valueStr);
        ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
        return handle.rawValue();

    }



     /*!
      * Factory method: makes either an Int64Value or a BigIntegerValue depending on the value supplied
      *
      * @param i the supplied primitive integer value
      * @return the value as a XdmAtomicValue which is a BigIntegerValue or Int64Value as appropriate
      */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long makeIntegerValue(IsolateThread thread, int i){

         XdmAtomicValue value = new XdmAtomicValue(i);
         ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
         return handle.rawValue();

     }


     //! Factory method. make an XdmAtomicValue from a primitive double value
     /*!
      *
      * @param d the value of the double
      * @return a new XdmAtomicValue
      */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long j_makeDoubleValue(IsolateThread thread, double d){

         XdmAtomicValue value = new XdmAtomicValue(d);
         ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
         return handle.rawValue();
     }

     //! Factory method. make an XdmAtomicValue from a primitive float value
     /*!
      *
      *
      * @param f the value of the foat
      * @return a new XdmAtomicValue
      */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long j_makeFloatValue(IsolateThread thread, float f){

         XdmAtomicValue value = new XdmAtomicValue(f);
         ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
         return handle.rawValue();
     }

     //! Factory method: makes either an Int64Value or a BigIntegerValue depending on the value supplied
     /*!
      *
      *
      * @param l the supplied primitive long value
      * @return the value as a XdmAtomicValue which is a BigIntegerValue or Int64Value as appropriate
      */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long makeLongValue(IsolateThread thread, long l){
         XdmAtomicValue value = new XdmAtomicValue(l);
         ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
         return handle.rawValue();
     }

     /*
      * Factory method: makes a XdmAtomicValue representing a boolean Value
      *
      * @param b true or false, to determine which boolean value is
      *              required
      * @return the XdmAtomicValue requested
      */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long makeBooleanValue(IsolateThread thread, int b){
         boolean bvalue = CTypeConversion.toBoolean(b);
         XdmAtomicValue value = new XdmAtomicValue(bvalue);
         ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
         return handle.rawValue();
     }

     //! Create an QName Xdm value from string representation in clark notation
     /**
      *
      * @param str - The value given in a string form in clark notation. {uri}local
      * @return XdmAtomicValue - value
     */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long makeQNameValue(IsolateThread thread, CCharPointer str) throws SaxonApiException {

         XdmValue value = SaxonCAPI.createXdmAtomicItem("QName", CTypeConversion.toJavaString(str));
         ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
         return handle.rawValue();
     }



}
