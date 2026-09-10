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
import org.graalvm.nativeimage.c.type.*;
import org.graalvm.word.WordFactory;

import java.net.URI;
import java.net.URISyntaxException;
import java.nio.charset.StandardCharsets;
import java.util.*;

/**
 * * This utility class consists generic functions for use with XDM data model.
 * For the use of Saxon on C++
 */
public class XdmUtils {


    public XdmUtils() {

    }

    private enum XdmType {
        XDM_VALUE(0), XDM_ATOMIC_VALUE(1), XDM_NODE(2),  XDM_ARRAY_VALUE(3), XDM_MAP_VALUE(4), XDM_FUNCTION_ITEM(5), XDM_EMPTY_SEQUENCE(6);

        private final int value;

        XdmType(int value) {
            this.value = value;
        }

        public int getValue() {
            return value;
        }
    }

    /**
     * Get the type name as a QName
     * @param typeStr  - supplied type string
     * @return
     */
    public static QName getTypeName(String typeStr) {

        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);

        BuiltInAtomicType typei = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        StructuredQName sname = typei.getTypeName();
        return new QName(sname);

    }


    /**
     * Get the base URI for an XdmNode object
     * @param thread - Graalvm runtime data structure for the thread
     * @param fn - Allocator function to create C memory space passed from the C++ application
     * @param value - the XdmNode object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space
     */
    @CEntryPoint(name = "getBaseURIForXdmNode")
    public static CCharPointer getBaseURIForXdmNode(IsolateThread thread, SaxonCAPI.AllocatorCCharFn fn, ObjectHandle value) {
        if(value == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmNode node = SaxonCAPI.objectHandles.get(value);
        URI uri = node.getBaseURI();
        return SaxonCAPI.stringToNative2(fn, uri.toString());

    }

    /**
     * Get parent node of the given XdmNode object
     * @param thread - Graalvm runtime data structure for the thread
     * @param value - the XdmNode object given as a reference into the ObjectHandles pool
     * @return  - long value to the XdmNode object in the ObjectHandles pool
     */
    @CEntryPoint(name = "getParentForXdmNode")
    public static long getParentForXdmNode(IsolateThread thread, ObjectHandle value) {
        if(value == WordFactory.nullPointer()) {
            return SaxonCAPI.STATUS_FAIL;
        }
        XdmNode node = SaxonCAPI.objectHandles.get(value);
        XdmNode parent = node.getParent();

        ObjectHandle objectHandle = SaxonCAPI.createHandle(parent);
        return objectHandle.rawValue();
    }

    /**
     * Get the string value of the XdmValue object
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param value - the XdmValue object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space
     */
    @CEntryPoint(name = "getStringValueForXdmValue")
    public static CCharPointer getStringValue_XdmValue(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle value) {
        if(value == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmValue item = SaxonCAPI.objectHandles.get(value);
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(item.toString());
        return holder.get();
    }

    /**
     * call to the XdmItem.getStringValue() method
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param value - the XdmItem object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space.  Exceptions are handled and returned to the C program as null pointer
     */
    @CEntryPoint(name = "getStringValueForXdmItem", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer getStringValue_XdmItem(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle value)  throws SaxonApiException {
        if(value == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmItem item = SaxonCAPI.objectHandles.get(value);
        if(item == null) {
            throw new SaxonApiException("XdmItem is null cannot getStringValue");
        }
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, item.getStringValue());
        return a;
    }

    /**
     * call to the XdmItem.toString() method
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param value - the XdmItem object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space. Exceptions are handled and returned to the C program as null pointer
     */
    @CEntryPoint(name = "xdmItemToString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer toString_XdmItem(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle value) {
        if(value == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmItem item = SaxonCAPI.objectHandles.get(value);

        CCharPointer a = SaxonCAPI.stringToNative2(alloc, item.toString());
        return a;
    }


    /**
         * convert EQName to ClarkName
         * @param thread - Graalvm runtime data structure for the thread
         * @param alloc - Allocator function to create C memory space passed from the C++ application
         * @param c_eqname - the the eqname string to convert to Clarkname
         * @return C Pointer to the string value in the C memory space. Exceptions are handled and returned to the C program as null pointer
         */
        @CEntryPoint(name = "j_EQNameToClarkName", exceptionHandler = SaxonCExceptionCCharHandler.class)
        public static CCharPointer j_EQNameToClarkName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, CCharPointer c_eqname) {
            if(c_eqname == WordFactory.nullPointer()) {
                return WordFactory.nullPointer();
            }
            String eqname = CTypeConversion.toJavaString(c_eqname);
            QName qname = QName.fromEQName(eqname);
            String clarkname_result = qname.getClarkName();
            CCharPointer a = SaxonCAPI.stringToNative2(alloc, clarkname_result);
            return a;
        }

    /**
         * convert ClarkName to EQName
         * @param thread - Graalvm runtime data structure for the thread
         * @param alloc - Allocator function to create C memory space passed from the C++ application
         * @param c_clarkname - the the clarkname string to convert to EQName
         * @return C Pointer to the string value in the C memory space. Exceptions are handled and returned to the C program as null pointer
         */
        @CEntryPoint(name = "j_clarkNameToEQName", exceptionHandler = SaxonCExceptionCCharHandler.class)
        public static CCharPointer j_clarkNameToEQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, CCharPointer c_clarkname) {
            if(c_clarkname == WordFactory.nullPointer()) {
                return WordFactory.nullPointer();
            }
            String clarkname = CTypeConversion.toJavaString(c_clarkname);
            QName qname = QName.fromClarkName(clarkname);
            String eqname_result = qname.getEQName();
            CCharPointer a = SaxonCAPI.stringToNative2(alloc, eqname_result);
            return a;
        }

    /**
     * call to the XdmNode.toString() method
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param value - the XdmNode object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space. Exceptions are handled and returned to the C program as null pointer
     */
    @CEntryPoint(name = "xdmNodeToString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer toString_XdmNode(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle value) {
        if(value == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmNode node = SaxonCAPI.objectHandles.get(value);

        CCharPointer a = SaxonCAPI.stringToNative2(alloc, node.toString());
        return a;
    }

    /**
     * call to the XdmNMap.toString() method
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param value - the XdmMap object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space.  Exceptions are handled and returned to the C program as null pointer
     */
    @CEntryPoint(name = "xdmMapToString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer toString_XdmMap(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle value){
        if(value == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        XdmMap map = SaxonCAPI.objectHandles.get(value);

        CCharPointer a = SaxonCAPI.stringToNative2(alloc, map.toString());
        return a;
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

    /**
     * Get the primitive type of this atomic value, as a QName. The primitive types for this purpose are
     * the 19 primitive types of XML Schema, plus xs:integer, xs:dayTimeDuration and xs:yearMonthDuration,
     * and xs:untypedAtomic. For external objects, the result is xs:anyAtomicType.
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param valueRef - the XdmAtomicValue object given as a reference into the ObjectHandles pool
     * @return C Pointer to the primitive type name in the C memory space.
     *         A QName naming the primitive type of this atomic value. This will always be an atomic type.
     *          Exceptions are handled and returned to the C program as null pointer.
     */
    @CEntryPoint(name = "j_getPrimitiveTypeName", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer getPrimitiveTypeName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle valueRef) {
        XdmAtomicValue value = SaxonCAPI.objectHandles.get(valueRef);

        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(getEQName(value.getPrimitiveTypeName()));
        return holder.get();
    }

    /**
     * Get the name of the function
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param itemRef - the XdmFunctionItem object given as a reference into the ObjectHandles pool
     * @return  the function name, as a QName, or null for an anonymous inline function item. This is returned as
     *          C Pointer to the primitive type name in the C memory space. Exceptions are handled and returned to the C program as null pointer
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getFunctionName", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer j_getFunctionName(IsolateThread thread,  SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle itemRef) throws SaxonApiException {

        if(itemRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmFunctionItem is null");
        }
        XdmFunctionItem item = SaxonCAPI.objectHandles.get(itemRef);

        String eqname = getEQName(item.getName());
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, eqname);

        return a;

    }

    /**
     * Get the arity of the function
     * @param thread  - Graalvm runtime data structure for the thread
     * @param valueRef - the XdmFunctionItem object given as a reference into the ObjectHandles pool
     * @return the arity of the function, that is, the number of arguments in the function's signature
     */
    @CEntryPoint(name = "j_xdmFunctionItem_getArity")
    public static int xdmFunctionItem_getArity(IsolateThread thread, ObjectHandle valueRef){
        XdmFunctionItem value = SaxonCAPI.objectHandles.get(valueRef);
        int arity = value.getArity();
        return arity;
    }

    /**
     *  Call the function
     * @param thread  - Graalvm runtime data structure for the thread
     * @param procRef - the Processor object given as a reference into the ObjectHandles pool
     * @param xdmFuncValueRef - the XdmFunctionItem object given as a reference into the ObjectHandles pool
     * @param procDataRef the values to be supplied as arguments to the function are supplied as a ProcessorDataAccumulator object reference. The "function
     *      *                  conversion rules" will be applied to convert the arguments to the required
     *      *                  type when necessary.
     * @return   the result of calling the function is converted to a long reference in the ObjectHandles pool.
     *          Exceptions are handled and returned to the C program as -2 value
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmFunctionItem_call", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmFunctionItem_call(IsolateThread thread, ObjectHandle procRef, ObjectHandle xdmFuncValueRef, ObjectHandle procDataRef) throws SaxonApiException {
        if(procRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        if(xdmFuncValueRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmFunctionItem object is null");
        }
        Processor proc = SaxonCAPI.objectHandles.get(procRef);

        XdmFunctionItem functionItem = SaxonCAPI.objectHandles.get(xdmFuncValueRef);

        ProcessorDataAccumulator processorDataAccumulator = null;
        XdmValue arguments [];
        if(procDataRef != WordFactory.nullPointer()){
            processorDataAccumulator = SaxonCAPI.objectHandles.get(procDataRef);

            arguments = Xslt30Processor.getArguments(processorDataAccumulator.getValues());

        } else {
            arguments = new XdmValue[0];
        }
        XdmValue value = functionItem.call(proc, arguments);
        ObjectHandle handle = SaxonCAPI.objectHandles.create(value);
        return handle.rawValue();
    }


    /**
     * Get a system function. This can be any function defined in XPath 3.1 functions and operators,
     * including functions in the math, map, and array namespaces. It can also be a Saxon extension
     * function, provided a licensed Processor is used.
     * @param thread  - Graalvm runtime data structure for the thread
     * @param procRef - the Processor object given as a reference into the ObjectHandles pool
     * @param c_eqname the name of the requested function given as a C char pointer array. Both Clarkname and EQName and notation accepted
     * @param arity - the arity of the function
     * @return the requested function, or null if there is no such function. Note that some functions
     *      * (those with particular context dependencies) may be unsuitable for dynamic calling.
     * @throws SaxonApiException no longer thrown in the getSystemFunction call. But thrown if the eqname or procRef is null. Exception
     * is handled and returned as -2 to the call C program
     */
    @CEntryPoint(name = "j_getSystemFunction", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_getSystemFunction(IsolateThread thread, ObjectHandle procRef, CCharPointer c_eqname, int arity) throws SaxonApiException {

        if(procRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        if(c_eqname == WordFactory.nullPointer()) {
            throw new SaxonApiException("SystemFunction name is null");
        }
        Processor proc = SaxonCAPI.objectHandles.get(procRef);
        String eqname = CTypeConversion.toJavaString(c_eqname);
        QName qname = null;
        if(eqname.startsWith("Q")){
            qname =  QName.fromEQName(eqname);
        } else {
            qname = QName.fromClarkName(eqname);
        }
        XdmFunctionItem function = XdmFunctionItem.getSystemFunction(proc, qname, arity);
        if(function == null) {
            return -1;
        } else {
            ObjectHandle handle = SaxonCAPI.objectHandles.create(function);
            return handle.rawValue();
        }

    }

    /**
     *  Get the number of members in the array
     * @param thread  - Graalvm runtime data structure for the thread
     * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
     * @return  the length of the array
     * Exceptions are handled and returned as -2
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmArray_arrayLength", exceptionHandler = SaxonCExceptionIntHandler.class)
    public static int j_xdmArray_arrayLength(IsolateThread thread, ObjectHandle arrayRef) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmArray object is null");
        }
        XdmArray xdmArray = SaxonCAPI.objectHandles.get(arrayRef);

        return xdmArray.arrayLength();

    }

    /**
     *  Get the number of entries in the map
     * @param thread  - Graalvm runtime data structure for the thread
     * @param mapRef - the XdmMap object given as a reference into the ObjectHandles pool
     * @return  the length of the map
     * Exceptions are handled and returned as -2
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_size", exceptionHandler = SaxonCExceptionIntHandler.class)
    public static int j_xdmMap_size(IsolateThread thread, ObjectHandle mapRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);

        return xdmMap.mapSize();

    }


    /**
     * Create a new array in which one member is replaced with a new value.
     * @param thread  - Graalvm runtime data structure for the thread
     * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
     * @param n the member that is required, counting the first member in the array as member zero
     * @return the n'th member in the sequence making up the array, counting from zero.
     * Exceptions are handled and returned as -2 indicating exception thrown
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmArray_get", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmArray_get(IsolateThread thread, ObjectHandle arrayRef, int n) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        XdmArray xdmArray = SaxonCAPI.objectHandles.get(arrayRef);
        XdmValue result = null;
        try {
            result = xdmArray.get(n);
        }catch (IndexOutOfBoundsException e) {
            throw new SaxonApiException(e);
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     * Create a new array in which one member is replaced with a new value.
     * @param thread  - Graalvm runtime data structure for the thread
     * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
     * @param n the position of the member that is to be replaced, counting the first member
     *          in the array as member zero
     * @param valueRef - the new value for this member given as a reference into the ObjectHandles pool
     * @return a new array returned as a reference in the ObjectHandles pool,
     * the same length as the original, with one member replaced by a new value.
     * Exceptions are handled and returned as -2 indicating exception thrown
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmArray_put", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmArray_put(IsolateThread thread, ObjectHandle arrayRef, int n, ObjectHandle valueRef) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Array object is null");
        }
        if(valueRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Value object is null");
        }
        XdmArray xdmArray = SaxonCAPI.objectHandles.get(arrayRef);
        XdmValue value = SaxonCAPI.objectHandles.get(valueRef);
        XdmArray result;
        try {
            result = xdmArray.put(n, value);
        } catch(IndexOutOfBoundsException ex) {
            throw new SaxonApiException(ex);
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }


    /**
     * Create a new map in which the entry for a given key has been removed.
     * @param thread  - Graalvm runtime data structure for the thread
     * @param mapRef - the XdmMap object given as a reference into the ObjectHandles pool
     * @param keyRef - the key as an XdmAtomicValue reference into the ObjectHnaldes pool
     * @return a map without the specified entry given as a reference in the ObjectHandles pool. The original map is unchanged.
     * Exceptions are handled and returned as -2 indicating exception thrown
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_remove", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_put(IsolateThread thread, ObjectHandle mapRef, ObjectHandle keyRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Array object is null");
        }
        if(keyRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Value object is null");
        }
        XdmMap map = SaxonCAPI.objectHandles.get(mapRef);
        XdmAtomicValue key = SaxonCAPI.objectHandles.get(keyRef);
        XdmMap result = map.remove(key);
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     *
     * @param thread  - Graalvm runtime data structure for the thread
     * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
     * @param valueRef  the XdmValue object given as a reference into the ObjectHandles pool
     * @return a new array, one item longer than the original given as a reference in the ObjectHandles pool.
     * Exceptions are handled and returned as -2 indicating exception thrown
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmArray_addMember", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmArray_addMember(IsolateThread thread, ObjectHandle arrayRef, ObjectHandle valueRef) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Array object is null");
        }
        if(valueRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Value object is null");
        }
        XdmArray xdmArray = SaxonCAPI.objectHandles.get(arrayRef);
        XdmValue value = SaxonCAPI.objectHandles.get(valueRef);
        XdmArray result = xdmArray.addMember(value);
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     * Concatenate another array
     * @param thread  - Graalvm runtime data structure for the thread
      * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
      * @param valueRef  the XdmValue object given as a reference into the ObjectHandles pool
     * @return a list of the members of this array given as a reference in the ObjectHandles pool.
     * Exceptions are handled and returned as -2 indicating exception thrown
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmArray_concat", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmArray_concat(IsolateThread thread, ObjectHandle arrayRef, ObjectHandle valueRef) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Array object is null");
        }
        if(valueRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Value object is null");
        }
        XdmArray xdmArray = SaxonCAPI.objectHandles.get(arrayRef);
        XdmArray value = SaxonCAPI.objectHandles.get(valueRef);
        XdmArray result = xdmArray.concat(value);
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }


    /**
     * call to the XdmArray.toString() method
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
     * @return C Pointer to the string value in the C memory space. Exceptions are handled and returned to the C program as null pointer
     */
    @CEntryPoint(name = "j_xdmArray_toString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer j_xdmArray_toString(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle arrayRef) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Array object is null");
        }
        XdmArray xdmArray = SaxonCAPI.objectHandles.get(arrayRef);
        String result = xdmArray.toString();
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, result);
        return a;

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

    /**
     * Convert the XdmArray to a pointer array of references to the array items
     * @param thread - Graalvm runtime data structure for the thread
     * @param arrayRef - the XdmArray object given as a reference into the ObjectHandles pool
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @return C Pointer to the the array of long references to array of XdmValue objects in the C memory space.
     * Exceptions are handled and returned to the C program as null pointer.
     * Exceptions are handled and returned to the C program as null pointer
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_convertXdmArrayToArrayObject", exceptionHandler =  SaxonCExceptionCLongPointerHandler.class)
    public static CLongPointer convertXdmArrayToArrayObject(IsolateThread thread, ObjectHandle arrayRef, SaxonCAPI.AllocatorFn alloc) throws SaxonApiException {
        if(arrayRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Array object is null");
        }
         XdmArray value = SaxonCAPI.objectHandles.get(arrayRef);
        if(value.arrayLength() == 0) {
            return WordFactory.nullPointer();
        }
        long[] attrRefs = null;
        attrRefs = value.asList().stream().mapToLong(i -> {
                        ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
                        return objectHandle.rawValue();
                    }).toArray();

        CLongPointer addr = SaxonCAPI.longArrayToNative(alloc, attrRefs);
        return addr;
    }

    /**
     * make an XdmArray from individual XdmValue objects held in a ProcessorDataAccumulator
     * @param thread  - Graalvm runtime data structure for the thread
     * @param processorDataAccumulatorRef - the ProcessorDataAccumulator object is passed as a reference in the ObjectHandle's pool. This wraps a set of data items
     * @return long value as reference to the created XdmArray object held in the ObjectHandle's pool
     * Exceptions are handled and returned to the C program as null pointer
     * @throws IllegalArgumentException
     */
    @CEntryPoint(name = "j_makeXdmArray", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long makeXdmArray(IsolateThread thread, ObjectHandle processorDataAccumulatorRef)  throws IllegalArgumentException {
         ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
        XdmArray array = null;
         if(processorDataAccumulator.size() == 0) {
             array = new XdmArray();
         }  else {
             Object[] objValues = processorDataAccumulator.getValues();
             array = XdmArray.makeArray(objValues);
         }

         ObjectHandle arrayHandle = SaxonCAPI.createHandle(array);

         return arrayHandle.rawValue();

    }

    /**
     * make an XdmArray from a pointer array of short values
     * @param thread  - Graalvm runtime data structure for the thread
     * @param shortPointer - the C short pointer array of items
     * @param length - the number of items in the pointer array
     * @return long value as reference to the created XdmArray object held in the ObjectHandle's pool
     * Exceptions are handled and returned to the C program as null pointer
     * @throws IllegalArgumentException
     */
    @CEntryPoint(name = "j_makeXdmArrayFromShort", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long makeXdmArrayFromShort(IsolateThread thread, CShortPointer shortPointer, int length) {

        XdmArray array = null;
        short [] shortValues = new short[length];

         if(length == 0) {
             array = new XdmArray();
         }  else {

             for(int i =0; i< length; i++) {
                 shortValues[i] = shortPointer.read(i);

             }
             array = XdmArray.makeArray(shortValues);
         }
         ObjectHandle arrayHandle = SaxonCAPI.createHandle(array);
         return arrayHandle.rawValue();
    }

    /**
     * make an XdmArray from a C pointer array of strings
     * @param thread  - Graalvm runtime data structure for the thread
     * @param ccharPointer - the C pointer array to strings
     * @param length - the number of items in the pointer array
     * @return long value as reference to the created XdmArray object held in the ObjectHandle's pool
     * Exceptions are handled and returned to the C program as null pointer
     * @throws IllegalArgumentException
     */
    @CEntryPoint(name = "j_makeXdmArrayFromStrings", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long makeXdmArrayFromStrings(IsolateThread thread, CCharPointer ccharPointer, int length) {

        XdmArray array = null;
        String [] values = new String[length];

         if(length == 0) {
             array = new XdmArray();
         }  else {

             for(int i =0; i< length; i++) {
                 values[i] = CTypeConversion.toJavaString(ccharPointer.addressOf(i));

             }
             array = XdmArray.makeArray(values);
         }
         ObjectHandle arrayHandle = SaxonCAPI.createHandle(array);
         return arrayHandle.rawValue();
    }

    /**
     * make an XdmArray from a pointer array of int values
     * @param thread  - Graalvm runtime data structure for the thread
     * @param shortPointer - the C int pointer array of items
     * @param length - the number of items in the pointer array
     * @return long value as reference to the created XdmArray object held in the ObjectHandle's pool
     * Exceptions are handled and returned to the C program as null pointer
     * @throws IllegalArgumentException
     */
    @CEntryPoint(name = "j_makeXdmArrayFromInt", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long makeXdmArrayFromInt(IsolateThread thread, CIntPointer intPointer, int length) {

        XdmArray array = null;
        int [] intValues = new int[length];

         if(length == 0) {
             array = new XdmArray();
         }  else {

             for(int i =0; i< length; i++) {
                 intValues[i] = intPointer.read(i);

             }
             array = XdmArray.makeArray(intValues);
         }
         ObjectHandle arrayHandle = SaxonCAPI.createHandle(array);
         return arrayHandle.rawValue();
    }


    /**
     * make an XdmArray from a pointer array of long values
     * @param thread  - Graalvm runtime data structure for the thread
     * @param shortPointer - the C long pointer array of items
     * @param length - the number of items in the pointer array
     * @return long value as reference to the created XdmArray object held in the ObjectHandle's pool
     * Exceptions are handled and returned to the C program as null pointer
     * @throws IllegalArgumentException
     */
    @CEntryPoint(name = "j_makeXdmArrayFromLong", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long makeXdmArrayFromLong(IsolateThread thread, CLongPointer longPointer, int length) {

        XdmArray array = null;
        long [] values = new long[length];

         if(length == 0) {
             array = new XdmArray();
         }  else {

             for(int i =0; i< length; i++) {
                 values[i] = longPointer.read(i);

             }
             array = XdmArray.makeArray(values);
         }
         ObjectHandle arrayHandle = SaxonCAPI.createHandle(array);
         return arrayHandle.rawValue();
    }


    /**
     * make an XdmArray from a pointer array of bool values
     * @param thread  - Graalvm runtime data structure for the thread
     * @param boolPointer - the C bool pointer array of items. We pass these as int pointer array
     * @param length - the number of items in the pointer array
     * @return long value as reference to the created XdmArray object held in the ObjectHandle's pool
     * Exceptions are handled and returned to the C program as null pointer
     * @throws IllegalArgumentException
     */
    @CEntryPoint(name = "j_makeXdmArrayFromBool", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long makeXdmArrayFromBoolean(IsolateThread thread, CIntPointer boolPointer, int length) {

        XdmArray array = null;
        boolean [] values = new boolean[length];

         if(length == 0) {
             array = new XdmArray();
         }  else {

             for(int i =0; i< length; i++) {
                 values[i] = CTypeConversion.toBoolean(boolPointer.read(i));

             }
             array = XdmArray.makeArray(values);
         }
         ObjectHandle arrayHandle = SaxonCAPI.createHandle(array);
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

    /**
     *
     * @param thread
     * @param alloc
     * @param mapRef
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_values", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static CLongPointer j_xdmMap_values(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle mapRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        if(xdmMap.isEmpty()) {
            return WordFactory.nullPointer();
        }
        XdmValue [] values = getXdmMapValues(xdmMap);


        long [] valueRefs = Arrays.stream(values).mapToLong(i -> {
            ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
            return objectHandle.rawValue();
        }).toArray();

        CLongPointer addr = SaxonCAPI.longArrayToNative(alloc, valueRefs);
        return addr;

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param atomicKeyRef
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_containsKey")
    public static byte j_xdmMap_containsKey(IsolateThread thread, ObjectHandle mapRef, ObjectHandle atomicKeyRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        if(atomicKeyRef == WordFactory.nullPointer()) {
            return  CTypeConversion.toCBoolean(false);
        }
        XdmAtomicValue atomicKey = SaxonCAPI.objectHandles.get(atomicKeyRef);

        boolean b = xdmMap.containsKey(atomicKey);
        return  CTypeConversion.toCBoolean(b);

    }


    /**
     *
     * @param thread
     * @param mapRef
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_isEmpty")
    public static byte j_xdmMap_isEmpty(IsolateThread thread, ObjectHandle mapRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);

        boolean b = xdmMap.isEmpty();
        return  CTypeConversion.toCBoolean(b);

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param atomicKeyRef
     * @param valueRef
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_put", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_put(IsolateThread thread, ObjectHandle mapRef, ObjectHandle atomicKeyRef, ObjectHandle valueRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        XdmAtomicValue atomicKey = SaxonCAPI.objectHandles.get(atomicKeyRef);
        XdmValue value = SaxonCAPI.objectHandles.get(valueRef);
        XdmMap result = xdmMap.put(atomicKey,value);
        if(result == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param atomicKeyRef
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_get", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_get(IsolateThread thread, ObjectHandle mapRef, ObjectHandle atomicKeyRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        XdmAtomicValue atomicKey = SaxonCAPI.objectHandles.get(atomicKeyRef);
        XdmValue result = xdmMap.get(atomicKey);
        if(result == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param atomicKeyStr
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_get_with_key_as_string", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_get_with_key_as_string(IsolateThread thread, ObjectHandle mapRef, CCharPointer atomicKeyStr) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        if(atomicKeyStr == WordFactory.nullPointer()) {
            throw new SaxonApiException("Atomic key object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        String atomicKey = CTypeConversion.toJavaString(atomicKeyStr);
        XdmValue result = xdmMap.get(atomicKey);
        if(result == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param key
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_get_with_key_as_int", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_get_with_key_as_int(IsolateThread thread, ObjectHandle mapRef, int key) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        
        XdmValue result = xdmMap.get(key);
        if(result == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param key
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_get_with_key_as_double", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_get_with_key_as_double(IsolateThread thread, ObjectHandle mapRef, double key) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        
        XdmValue result = xdmMap.get(key);
        if(result == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }

    /**
     *
     * @param thread
     * @param mapRef
     * @param key
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_get_with_key_as_long", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_xdmMap_get_with_key_as_long(IsolateThread thread, ObjectHandle mapRef, long key) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        
        XdmValue result = xdmMap.get(key);
        if(result == null) {
            return SaxonCAPI.STATUS_FAIL;
        }
        ObjectHandle handle = SaxonCAPI.objectHandles.create(result);
        return handle.rawValue();

    }
    


    public static XdmAtomicValue [] getXdmMapKeys(XdmMap map) {

        XdmAtomicValue [] arr = map.keySet().toArray(new XdmAtomicValue[0]);
        return arr;

    }

    /**
     *  Get the keys from the XdmMap
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param mapRef - reference to an XdmMap object in the ObjectHandles pool
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_xdmMap_keys", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static CLongPointer j_xdmMap_keys(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle mapRef) throws SaxonApiException {
        if(mapRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmMap object is null");
        }
        XdmMap xdmMap = SaxonCAPI.objectHandles.get(mapRef);
        if(xdmMap.isEmpty()) {
            return WordFactory.nullPointer();
        }
        XdmValue [] values = getXdmMapKeys(xdmMap);

        long [] valueRefs = Arrays.stream(values).mapToLong(i -> {
            ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
            return objectHandle.rawValue();
        }).toArray();

        CLongPointer addr = SaxonCAPI.longArrayToNative(alloc, valueRefs);
        return addr;

    }

    /**
     * Convert the nodeKind enum type to a integer
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return int value of the NodeKind
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

    /**
     * The expanded name, as a string using the notation devised by EQName.
     * If the name is in a namespace, the resulting string takes the form <code>Q{uri}local</code>.
     * Otherwise, the value is the local part of the name.
     * @param qname
     * @return the String value
     */
    public static String getEQName(QName qname) {
        if(qname == null) {
            return null;
        }
        String uri = qname.getNamespaceUri().toString();
        if (uri.length() == 0) {
            return qname.getLocalName();
        } else {
            return "Q{" + uri + "}" + qname.getLocalName();
        }
    }

    /**
     *  Get the local name for the XdmNode object
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return C string array pointer
     */
    @CEntryPoint(name = "j_getLocalName", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer getLocalName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle nodeRef) {
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        QName qname = node.getNodeName();
        /*if (qname == null) {
            return null;
        }  */
        return SaxonCAPI.stringToNative2(alloc, qname.getLocalName());

    }

    /**
     * Get the name of the node, as a EQName
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return C string pointer to the name of the node in the C memory space. In the case of unnamed nodes (for example, text and comment nodes)
     * return null.
     */
    @CEntryPoint(name = "j_getNodeName", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer getNodeName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle nodeRef) {
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        QName qname = node.getNodeName();
        if (qname == null) {
            return WordFactory.nullPointer();
        }
        return SaxonCAPI.stringToNative2(alloc, XdmUtils.getEQName(qname));

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
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param processorDataAccumulatorRef - ObjectHandle to array of XdmValue held as ProcessorDataAccumulator
     * @return C string pointer to the string representation of the XdmValue array in the C memory space. Return null pointer if XdmValue is null
     */
    @CEntryPoint(name = "xdmValueArrayToString")
    public static CCharPointer xdmValueArrayToString(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle processorDataAccumulatorRef){
        if(processorDataAccumulatorRef == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataAccumulatorRef);
        Object objs [] = processorDataAccumulator.getValues();
        XdmValue [] values = new XdmValue[objs.length];
        for(int i = 0; i< objs.length;i++) {
             values[i]  = (XdmValue) objs[i];
        }

        CCharPointer a = SaxonCAPI.stringToNative2(alloc, buildXdmValue(values).toString());
        return a;
    }


    
    /**
     * Get the string value of a named attribute of the element passed
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param nodeRef  - reference to an XdmNode object in the ObjectHandles pool
     * @param c_eqname - the name of the required attribute
     * @return null if this node is not an element, or if this element has no
     * attribute with the specified name. Otherwise return the string value of the
     * selected attribute node.
     */
    @CEntryPoint(name = "j_getAttributeValue")
    public static CCharPointer getAttributeValue(IsolateThread thread, ObjectHandle nodeRef, CCharPointer c_eqname) {
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        String eqname = CTypeConversion.toJavaString(c_eqname);
        QName name = QName.fromEQName(eqname);
        if (SaxonCAPI.debug) {
            System.err.println("EQName= " + eqname);
            System.err.println("Java-Call Att-name=" + name.getClarkName() + " Value=" + node.getAttributeValue(name));
        }

        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(node.getAttributeValue(name));
        return holder.get();
    }

    /**
     * Get the attribute nodes at the given node
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return  long references held as a C long pointer array
     */
    @CEntryPoint(name = "j_getAttributeNodes")
    public static CLongPointer getAttributeNodes(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle nodeRef) {
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);

        XdmSequenceIterator<XdmNode> iter = node.axisIterator(Axis.ATTRIBUTE);

        long[] attrRefs = null;
        if(iter.hasNext()) {
            attrRefs = iter.stream().mapToLong(i -> {
                ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
                return objectHandle.rawValue();
            }).toArray();
        }else {
            return WordFactory.nullPointer();
        }

        CLongPointer addr = SaxonCAPI.longArrayToNative(alloc, attrRefs);
        return addr;
    }

    /**
     * Get the child nodes at the given node
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc - Allocator function to create C memory space passed from the C++ application
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return  C long pointer array for the references to the attribute nodes
     */
    @CEntryPoint(name = "j_getChildren", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static CLongPointer getChildren(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle nodeRef) {

        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        XdmSequenceIterator<XdmNode> iter = node.axisIterator(Axis.CHILD);
        long[] childRefs = null;
        if(iter.hasNext()) {
            childRefs = iter.stream().mapToLong(i -> {
                ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
                return objectHandle.rawValue();
            }).toArray();
        }else {
            return WordFactory.nullPointer();
        }
        CLongPointer addr = SaxonCAPI.longArrayToNative(alloc, childRefs);
        return addr;
    }

    /**
     *   Get the XdmValue subclass for an Object
     * @param thread - Graalvm runtime data structure for the thread
     * @param valueRef - reference to the XdmValue object in the ObjectHandles pool
     * @return  int value representing the XdmValue subclass
     */
    @CEntryPoint(name = "j_getXdmObjectType", exceptionHandler = SaxonCExceptionIntHandler.class)
    public  static int getXdmObjectType(IsolateThread thread, ObjectHandle valueRef){
        Object value = SaxonCAPI.objectHandles.get(valueRef);
        if(SaxonCAPI.debug){
            System.err.println("call j_getXdmObjectType="+value.getClass().getName() + value.getClass());
        }
        if(value instanceof XdmAtomicValue) {
            return XdmType.XDM_ATOMIC_VALUE.value;
        } else if (value instanceof XdmNode) {
            return XdmType.XDM_NODE.value;
        } else if(value instanceof XdmMap) {
            return XdmType.XDM_MAP_VALUE.value;
        } else if(value instanceof XdmArray) {
          return XdmType.XDM_ARRAY_VALUE.value;
        } else if (value instanceof  XdmFunctionItem) {
          return XdmType.XDM_FUNCTION_ITEM.value;
        }  else if (value instanceof XdmEmptySequence) {
            return XdmType.XDM_EMPTY_SEQUENCE.value;
        }
        return XdmType.XDM_VALUE.value;
    }


    /**
     * Get the i-th child node at a given node
     * @param thread - Graalvm runtime data structure for the thread
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @param i int value for the ith child node
     * @return  long value for the xdmNode object as reference in the ObjectHandle's pool
     */
    @CEntryPoint(name = "j_getChild")
    public static long getChild(IsolateThread thread, ObjectHandle nodeRef, int i) {
        
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);


        XdmSequenceIterator iter = node.axisIterator(Axis.CHILD);
        int pos = 0;
        while (iter.hasNext()) {
            if(pos == i) {
                 XdmNode childi = (XdmNode) iter.next();
                 ObjectHandle handle = SaxonCAPI.createHandle(childi);
                 return handle.rawValue();
            }
            pos++ ;
        }
        return -1;
    }

    /**
     * Get the number of children belonging to the node
     * @param thread  - Graalvm runtime data structure for the thread
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return int value for the number of children. Exception thrown are handled and -2 returned
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getChildCount", exceptionHandler = SaxonCExceptionIntHandler.class)
    public static int getChildCount(IsolateThread thread, ObjectHandle nodeRef) throws SaxonApiException {
        int num = 0;
        if(nodeRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("XdmNode object is null");
        }
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        XdmSequenceIterator iter = node.axisIterator(Axis.CHILD);
        while (iter.hasNext()) {
            iter.next();
            num++;
        }

        return num;
    }

    /**
     * Get the number of attribute nodes belonging to a node
     * @param thread  - Graalvm runtime data structure for the thread
     * @param nodeRef - reference to an XdmNode object in the ObjectHandles pool
     * @return int value for the number of attribute nodes. Exception thrown are handled and -2 returned
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_getAttributeCount")
    public static int getAttributeCount(IsolateThread thread, ObjectHandle nodeRef) {
        int num = 0;
        XdmNode node = SaxonCAPI.objectHandles.get(nodeRef);
        XdmSequenceIterator iter = node.axisIterator(Axis.ATTRIBUTE);
        while (iter.hasNext()) {
            iter.next();
            num++;
        }

        return num;
    }


    /**
     * Set the base URI of a document loaded using this <code>DocumentBuilder</code>.
     * <p>This is used for resolving any relative URIs appearing
     * within the document, for example in references to DTDs and external entities.</p>
     * <p>This information is required when the document is loaded from a source that does not
     * provide an intrinsic URI, notably when loading from a Stream or a DOMSource. The value is
     * ignored when loading from a source that does have an intrinsic base URI.</p>
     *
     * @param builder  DocumentBuilder to set the baseURI
     * @param baseStr  the base URI of documents loaded using this <code>DocumentBuilder</code>. This
     *                  must be an absolute URI.
     * @throws SaxonApiException
     */
    public static void setDocBuilderBaseURI(DocumentBuilder builder, String baseStr) throws SaxonApiException {
        URI baseURI = null;
        try {
            baseURI = new URI(baseStr);
        } catch (URISyntaxException e) {
            throw new SaxonApiException(e.getMessage());
        }
        builder.setBaseURI(baseURI);


    }

    /**
     * Make XdmAtomicValue from string
     * @param thread  - Graalvm runtime data structure for the thread
     * @param str - string value as a C char pointer array
     * @return  the long value reference to the XdmAtomicValue object in the ObjectHandle's pool
     */
    @CEntryPoint(name = "j_makeStringValue")
    public static long makeStringValue(IsolateThread thread,CCharPointer str){
        String valueStr = CTypeConversion.toJavaString(str);

        XdmAtomicValue value = new XdmAtomicValue(valueStr);
        ObjectHandle handle = SaxonCAPI.createHandle(value);
        return handle.rawValue();

    }



     /**
      * Factory method: makes either an Int64Value or a BigIntegerValue depending on the value supplied
      *
      * @param thread  - Graalvm runtime data structure for the thread
      * @param i the supplied primitive integer value
      * @return the value as a XdmAtomicValue which is a BigIntegerValue or Int64Value as appropriate
      */
     @CEntryPoint(name = "j_makeIntegerValue")
     public static long makeIntegerValue(IsolateThread thread, int i){

         XdmAtomicValue value = new XdmAtomicValue(i);
         ObjectHandle handle = SaxonCAPI.createHandle(value);
         return handle.rawValue();

     }


     /** Factory method. make an XdmAtomicValue from a primitive double value
      *
      * @param thread  - Graalvm runtime data structure for the thread
      * @param d the value of the double
      * @return a new XdmAtomicValue
      */
     @CEntryPoint(name = "j_makeDoubleValue")
     public static long j_makeDoubleValue(IsolateThread thread, double d){

         XdmAtomicValue value = new XdmAtomicValue(d);
         ObjectHandle handle = SaxonCAPI.createHandle(value);
         return handle.rawValue();
     }

     /**
      *  Factory method. make an XdmAtomicValue from a primitive float value
      *
      * @param thread  - Graalvm runtime data structure for the thread
      * @param f the value of the float
      * @return a new XdmAtomicValue
      */
     @CEntryPoint(name = "j_makeFloatValue")
     public static long j_makeFloatValue(IsolateThread thread, float f){

         XdmAtomicValue value = new XdmAtomicValue(f);
         ObjectHandle handle = SaxonCAPI.createHandle(value);
         return handle.rawValue();
     }

     /**
      * Factory method: makes either an Int64Value or a BigIntegerValue depending on the value supplied
      *
      * @param thread  - Graalvm runtime data structure for the thread
      * @param l the supplied primitive long value
      * @return the value as a XdmAtomicValue which is a BigIntegerValue or Int64Value as appropriate
      */
     @CEntryPoint(name = "j_makeLongValue")
     public static long makeLongValue(IsolateThread thread, long l){
         XdmAtomicValue value = new XdmAtomicValue(l);
         ObjectHandle handle = SaxonCAPI.createHandle(value);
         return handle.rawValue();
     }

     /**
      * Factory method: makes a XdmAtomicValue representing a boolean Value
      *
      * @param thread  - Graalvm runtime data structure for the thread
      * @param b true or false, to determine which boolean value is
      *              required
      * @return the XdmAtomicValue requested
      */
     @CEntryPoint(name = "j_makeBooleanValue")
     public static long makeBooleanValue(IsolateThread thread, int b){
         boolean bvalue = CTypeConversion.toBoolean(b);
         XdmAtomicValue value = new XdmAtomicValue(bvalue);
         ObjectHandle handle = SaxonCAPI.createHandle(value);
         return handle.rawValue();
     }

     /** Create an QName Xdm value from string representation in clark notation
      *
      * @param thread  - Graalvm runtime data structure for the thread
      * @param str - The value given in a string form in clark notation. {uri}local
      * @return XdmAtomicValue - value
     */
     @CEntryPoint(name = "j_makeQNameValue")
     public static long makeQNameValue(IsolateThread thread, CCharPointer str) throws SaxonApiException {

         XdmValue value = SaxonCAPI.createXdmAtomicItem("QName", CTypeConversion.toJavaString(str));
         ObjectHandle handle = SaxonCAPI.createHandle(value);
         return handle.rawValue();
     }



}
