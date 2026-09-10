////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
package net.sf.saxon.option.cpp;

import com.saxonica.functions.extfn.cpp.NativeCall;
import net.sf.saxon.Configuration;

import net.sf.saxon.Gizmo;
import net.sf.saxon.Query;
import net.sf.saxon.Transform;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.lib.*;
import net.sf.saxon.s9api.SequenceType;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.CommandLineOptions;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.ObjectHandles;
import org.graalvm.nativeimage.VMRuntime;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.function.CFunctionPointer;
import org.graalvm.nativeimage.c.function.InvokeCFunctionPointer;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.UnsignedWord;
import org.graalvm.word.WordFactory;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.*;

import java.net.MalformedURLException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.concurrent.ConcurrentHashMap;


/**
 * This class holds common attributes and methods required in the XsltProcessor, XQueryEngine and XPathProcessor
 */

public class SaxonCAPI {

    protected Processor processor;  //TODO remove this variable - class should be stateless

    public static boolean debug = false;
    protected boolean schemaAware = false;
    public static String RESOURCES_DIR = null;


    public static final ObjectHandles objectHandles = ObjectHandles.create();
    public static final Map<Long , String> objectHandlesPool = new ConcurrentHashMap<>(); //for debugging

    public static int objHandlesCreated = 0;

    public static final long STATUS_OK = 0;
    public static final long STATUS_FAIL = -1;
    public static final long STATUS_EXCEPTION = -2;


    /**
     * Default Constructor. Creates a processor that does not require a license edition
     */
    public SaxonCAPI() {
        processor = new Processor(false);

        if (debug) {
            System.err.println("New processor created in SaxonCAPI(), Processor: " + System.identityHashCode(processor));
        }

    }

    /**
     * Constructor with license edition flag
     *
     * @param license - specify license edition flag
     */
    public SaxonCAPI(boolean license) {
        processor = new Processor(license);

        if (debug) {
            System.err.println("New processor created in SaxonCAPI(l), Processor: " + System.identityHashCode(processor));
        }
    }

    public SaxonCAPI(Processor proc) {
        processor = proc;

        if (debug) {
            System.err.println("Reused processor in SaxonCAPI(l), Processor: " + System.identityHashCode(processor));
        }
    }


    @CEntryPoint(name = "j_getArgumentTypesFromSignature", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_getArgumentTypesFromSignature(IsolateThread thread, ObjectHandle signatureRef) throws SaxonApiException{
        String func_signature = null;

        XdmValue signatureValue = objectHandles.get(signatureRef);

        if(signatureValue != null && signatureValue instanceof XdmMap) {
                XdmMap paramMap = (XdmMap) signatureValue;

                if (paramMap != null) {
                    if (SaxonCAPI.debug) {
                        System.out.println("Signature = " + (signatureValue).toString());
                    }
                    XdmValue paramValue = paramMap.get("param");

                    if (paramValue != null && paramValue instanceof XdmArray) {
                        if (SaxonCAPI.debug) {
                            System.out.println("param = " + ((XdmArray) paramValue).toString());
                        }
                        XdmArray paramArray = (XdmArray) paramValue;
                        ObjectHandle handle = createHandle(paramArray);
                        return handle.rawValue();
                    }
                }
            }
        return SaxonCAPI.STATUS_FAIL;
    }


    @CEntryPoint(name = "j_getReturnTypeFromSignature", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_getReturnTypeFromSignature(IsolateThread thread, ObjectHandle signatureRef) throws SaxonApiException{

        XdmValue signatureValue = objectHandles.get(signatureRef);

        if(signatureValue != null && signatureValue instanceof XdmMap) {
                XdmMap signatureMap = (XdmMap) signatureValue;

                    if (SaxonCAPI.debug) {
                        System.out.println("Signature = " + (signatureValue).toString());
                    }
                    XdmValue returnValue = signatureMap.get("as");
                    if (returnValue != null && returnValue instanceof XdmAtomicValue) {
                        if(SaxonCAPI.debug) {
                            System.out.println("return type (as)= " + ((XdmAtomicValue) returnValue).getStringValue());
                        }
                        XdmAtomicValue atomicValue = (XdmAtomicValue) returnValue;
                        ObjectHandle handle = createHandle(atomicValue);
                        return handle.rawValue();
                }
            }
        return SaxonCAPI.STATUS_FAIL;
    }

    @CEntryPoint(name = "j_registerFunctionDefinition", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_registerFunctionDefinition(IsolateThread thread, ObjectHandle procRef, CCharPointer c_namespace, ObjectHandle signatureRef, CFunctionPointer pythonProc, CFunctionPointer func, AllocatorFn alloc, int c_useNativeTypes) throws SaxonApiException {
        if (SaxonCAPI.debug) {
            System.err.println("Java DEBUG entered setLibrary!!");
        }
//#if EE==true || PE==true
        boolean isNativeType = CTypeConversion.toBoolean(c_useNativeTypes);

        if (procRef == WordFactory.nullPointer()) {
            return net.sf.saxon.option.cpp.SaxonCAPI.STATUS_FAIL;
        }

        Processor processor = objectHandles.get(procRef);

        String namespaceStr = null;
        if (c_namespace != WordFactory.nullPointer()) {
            namespaceStr = SaxonCAPI.toJavaString(c_namespace, WordFactory.nullPointer());
        }
        XdmValue signatureValue = null;

        if (signatureRef != WordFactory.nullPointer()) {
             signatureValue = objectHandles.get(signatureRef);

        }

        return registerFunctionDefinition(processor, namespaceStr, signatureValue, pythonProc, func, alloc, isNativeType);

    }

    public static long registerFunctionDefinition(Processor processor, String namespaceStr, XdmValue signatureValue, CFunctionPointer pythonProc, CFunctionPointer func, AllocatorFn alloc, boolean useNativeTypes) throws SaxonApiException{

        QName funcQname = null;

        SequenceType [] sequenceTypes = null;
        SequenceType returnSequenceType = null;
        XdmValue returnValue = null;

        if (signatureValue != null) {
            if(signatureValue instanceof XdmMap) {
                XdmMap functionMap = (XdmMap) signatureValue;
                XdmAtomicValue functionKey = functionMap.keySet().iterator().next();
                XdmMap signatureMap  = (XdmMap) ((XdmMap)signatureValue).get(functionKey);

                funcQname = new QName(namespaceStr, functionKey.getStringValue());

                if(signatureMap != null) {
                    if(SaxonCAPI.debug) {
                        System.out.println("Java DEBUG: Function name = " + funcQname);
                          System.out.println("Java DEBUG: Signature = " + (signatureValue).toString());
                      }
                    XdmValue paramValue = signatureMap.get("param");
                    if (paramValue instanceof XdmArray) {
                         if(SaxonCAPI.debug) {
                             System.out.println("Java DEBUG: param = " + ((XdmArray) paramValue).toString());
                             System.out.println("Java DEBUG: param length= " + ((XdmArray) paramValue).arrayLength());
                         }
                        XdmArray paramArray = (XdmArray) paramValue;
                        sequenceTypes = new SequenceType[paramArray.arrayLength()];

                        for (int i =0; i< paramArray.arrayLength(); i++) {
                            XdmValue val = paramArray.get(i);
                            if(SaxonCAPI.debug) {
                                System.out.println("param (i="+i+") item= " + val.toString());
                            }
                            sequenceTypes[i] = SaxonCAPI.parseSequenceType(processor, val.toString());

                        }
                        if(SaxonCAPI.debug && sequenceTypes == null) {
                            System.err.println("No sequence types");
                        }

                    }
                    returnValue = signatureMap.get("as");
                    if (returnValue != null && returnValue instanceof XdmAtomicValue) {
                        if(SaxonCAPI.debug) {
                            System.out.println("Java DEBUG: return type (as)= " + ((XdmAtomicValue) returnValue).getStringValue());
                        }
                        returnSequenceType = SaxonCAPI.parseSequenceType(processor, ((XdmAtomicValue) returnValue).getStringValue());
                    }

                }
            }
        } else {
            throw new SaxonApiException("Invalid signature value");
        }

        processor.registerExtensionFunction(new NativeCall(funcQname, sequenceTypes, returnSequenceType, pythonProc, func, alloc, useNativeTypes));

//#endif
        return  SaxonCAPI.STATUS_OK;

    }


        public static SequenceType parseSequenceType(Processor processor, String typeDesignation) throws SaxonApiException{
           Configuration config = processor.getUnderlyingConfiguration();
            IndependentContext env = new IndependentContext(config);
            XPathParser parser = new XPathParser(env);
            env.setXPathLanguageLevel("4.0".equals(config.getConfigurationProperty(Feature.XQUERY_VERSION)) ? 40 : 31);
            env.declareNamespace("xs", NamespaceUri.SCHEMA);
            net.sf.saxon.value.SequenceType st = null;
            try {
                st = parser.parseSequenceType(typeDesignation, env);
            } catch (XPathException e) {
                throw new SaxonApiException(e);
            }
            return SequenceType.fromUnderlyingSequenceType(processor, st);
        }



    @CEntryPoint(name = "j_setLibrary", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long setLibrary(IsolateThread thread, ObjectHandle procRef, CCharPointer ccwd, CCharPointer libNameRef) throws SaxonApiException {
        if(SaxonCAPI.debug) {
            System.err.println("Java DEBUG en entered setLibrary!!");
        }
        return  SaxonCAPI.STATUS_OK;
    }

    /**
     * Static method to create the SaxonProcessor object
     * @param thread - Graalvm runtime data structure for a thread
     * @param license - indicates whether the Processor requires features of Saxon that need a license
     *                        file (that is, features not available in Saxon HE (Home Edition). If true, the method will create
     *                        a Configuration appropriate to the version of the software that is running: for example, if running
     *                        Saxon-EE, it will create an EnterpriseConfiguration. The method does not at this stage check that a license
     *                        is available, and in the absence of a license, it should run successfully provided no features that
     *                        require licensing are actually used. If the argument is set to false, a plain Home Edition Configuration
     *                        is created unconditionally.
     * @return  long value referencing the created SaxonProcessor object in the ObjectHandle pool
     */
    @CEntryPoint(name = "createSaxonProcessor", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createSaxonProcessor(IsolateThread thread, int license) {
        boolean licensei = CTypeConversion.toBoolean(license);
        String debugFlag  = System.getenv("SAXONC_DEBUG_FLAG");
        if(debugFlag != null && debugFlag.equals("true")) {
            SaxonCAPI.debug = true;
            PathHelper.debug = true;
            //com.oracle.svm.core.SubstrateGCOptions.VerboseGC.update(true);
        }
        if (debug && licensei) {
            System.err.println("Flag is true to create SaxonProcessor with license");
        }

        String charsetFlag  = System.getenv("SAXONC_DEBUG_CHARSET_FLAG");
        if(charsetFlag != null && charsetFlag.equals("true")) {
           System.err.println("Default JVM charset = " + java.nio.charset.Charset.defaultCharset().displayName());
        }
        ObjectHandle handle = createHandle(new Processor(licensei));
        return handle.rawValue();
    }


    @CEntryPoint(name = "j_gc")
    public static void gc(IsolateThread thread) {
        System.gc();
    }

    /**
     *  Static method to check if the Processor object is schema aware
     * @param thread - Graalvm runtime data structure for a thread
     * @param procRef - long value pointing to a SaxonProcessor object in the ObjectHandle pool
     * @return  byte representing boolean. true if this processor is licensed for schema processing, false otherwise
     */
    @CEntryPoint(name = "j_isSchemaAware")
    public static byte j_isSchemaAware(IsolateThread thread, ObjectHandle procRef) {
        Processor proc = objectHandles.get(procRef);
        boolean result = proc.isSchemaAware();

        return CTypeConversion.toCBoolean(result);
    }

    /**
     *  Static method to check if the Processor object is licensed
     * @param thread - Graalvm runtime data structure for a thread
     * @param procRef - long value pointing to a SaxonProcessor object in the ObjectHandle pool
     * @return  byte representing boolean. true if this processor is licensed, false otherwise
     */
    @CEntryPoint(name = "j_isLicensed")
    public static byte j_isLicensed(IsolateThread thread, ObjectHandle procRef) {
        Processor proc = objectHandles.get(procRef);
        Configuration config = proc.getUnderlyingConfiguration();
//#if EE==true || PE==true
        if(config instanceof com.saxonica.config.ProfessionalConfiguration) {
            com.saxonica.config.ProfessionalConfiguration pc = (com.saxonica.config.ProfessionalConfiguration) config;
            return CTypeConversion.toCBoolean(pc.isLicenseFound());
        }
//#endif
        return CTypeConversion.toCBoolean(false);

    }

    /**
     *  Static Support method for main transform program.
     * @param thread   - Graalvm runtime data structure for a thread
     * @param processorDataRef - long value referencing ProcessorDataAccumulator object in the ObjectHandle pool. Used to pass processor data and options
     * @return  success status. 1 for success, -1 for fail and -2 for exception thrown
     */
    @CEntryPoint(name = "j_run_transform")
    public static long run_transform(IsolateThread thread, ObjectHandle processorDataRef) {

        String[] args;


        if (processorDataRef == WordFactory.nullPointer()) {
            return STATUS_FAIL;
        } else {
            ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            args = processorDataAccumulator.getParameterNames();
        }

        new Transform().doTransform(args);


        return STATUS_OK;
    }


    /**
     *  Static method to run the Gizmo tool
     * @param thread  - Graalvm runtime data structure for a thread
     * @param processorDataRef  - long value referencing Processor object in the ObjectHandle pool
     * @return   success status. 1 for success, -1 for fail and -2 for exception thrown
     */
    @CEntryPoint(name = "j_run_gizmo")
    public static long run_gizmo(IsolateThread thread, ObjectHandle processorDataRef) {

        String[] args;


        if (processorDataRef == WordFactory.nullPointer()) {
            return STATUS_FAIL;
        } else {
            ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            args = processorDataAccumulator.getParameterNames();
        }

        if(SaxonCAPI.debug) {
            System.err.println("Gizmo argument length: "+args.length);
            for(String arg: args){
                System.err.println("arg="+arg);

            }

        }

        new Gizmo(args);


        return STATUS_OK;
    }


    /**
     * Support method for main program in C.
     * @param thread  - Graalvm runtime data structure for a thread
     * @param processorDataRef  - long value referencing Processor object in the ObjectHandle pool
     * @return    success status. 1 for success, -1 for fail and -2 for exception thrown
     */
    @CEntryPoint(name = "j_run_query")
    public static long run_query(IsolateThread thread, ObjectHandle processorDataRef) {

        String[] args;


        if (processorDataRef == WordFactory.nullPointer()) {
            return STATUS_FAIL;
        } else {
            ProcessorDataAccumulator processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            args = processorDataAccumulator.getParameterNames();
        }

        new Query().doQuery(args, "java net.sf.saxon.Query");


        return STATUS_OK;
    }

    /**
     *  Create the Graalvm ObjectHandle for the Java object
     * @param obj - Java Object
     * @return  ObjectHandle
     */
    public static ObjectHandle createHandle(Object obj) {
        if (debug) {
            objHandlesCreated++;
            System.err.println("Total Object Handles created: " + objHandlesCreated);
        }
        ObjectHandle handle = objectHandles.create(obj);
        if (debug) {
            if(objectHandlesPool.containsKey(handle.rawValue())){
                System.err.println("Object handle key already exists!");
            }
            System.err.println("Created Object Handle ref=" + handle.rawValue() + " Class name = "+ obj.getClass().getName());
            objectHandlesPool.put(handle.rawValue(), obj.getClass().getName());
        }
        return handle;
    }

    /**
     *  Create new copy of the Graalvm ObjectHandle for the Java object
     * @param thread - IsolateThread
     * @param handle - Java Object
     * @return  ObjectHandle
     */
    @CEntryPoint(name = "j_copy_handles", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long copyHandle(IsolateThread thread, ObjectHandle handle) throws SaxonApiException {
        if (handle != WordFactory.nullPointer()) {
            if (debug) {
                System.err.println("copyHandle method Class= " + (objectHandles.get(handle)).getClass().getName());
            }
            Object obj = objectHandles.get(handle);
            return createHandle(obj).rawValue();
        }
        throw new SaxonApiException("Failed to copy Object");
    }


    /**
     *  Get the length of the C char array
     * @param str   - C char array
     * @return  UnsignedWord
     */
    private static UnsignedWord strlen(CCharPointer str) {
            UnsignedWord n = WordFactory.zero();
            while (((org.graalvm.word.Pointer) str).readByte(n) != 0) {
                n = n.add(1);
            }
            if (debug) {
                System.err.println("SaxonC Java DEBUG: UnsignedWord len =" + n.rawValue());
            }
            return n;
        }

    /**
     *  Convert C string to Java string object. Encoding name given as a char pointer.
     *  If the encoding is null use the platform default encoding
      * @param str  - C char array
     * @param c_encoding - C char array for the encoding name. Also, can be a C NULL pointer
     * @return  Java String represented by the given encoding
     * @throws SaxonApiException  - if the str is null
     */
    public static String toJavaString(CCharPointer str, CCharPointer c_encoding) throws SaxonApiException {
        if(str.isNull()) {
            throw new SaxonApiException("Null found in Java string conversation");
        }
        try {
            if (c_encoding.isNull()) {
                if (debug) {
                    System.err.println("Java DEBUG toJavaString: no encoding found");
                }
                return CTypeConversion.toJavaString(str);
            } else {
                String encoding = CTypeConversion.toJavaString(c_encoding);
                if (debug) {
                    System.err.println("Java DEBUG toJavaString: ENCODING="+ encoding);
                }
                return CTypeConversion.toJavaString(str, strlen(str), Charset.forName(encoding));// .utf8ToJavaString(str);
            }
        } catch(Exception ex) {
            if (debug) {
                System.err.println("SaxonApiException: " + ex.getMessage());
            }
            throw new SaxonApiException(ex);
        }
    }

    /**
     * Convert C string to Java string object. Encoding name given as a Java string.
     * If the encoding is null use the platform default encoding
     * @param str  - given as a C char array
     * @param encoding- encoding to be used to convert  C string to Java string. If encoding is null
     *                then use platform default
     * @return  Java String represented by the given encoding
     * @throws SaxonApiException  - if java string is null or some other conversion error
     */
    public static String toJavaString2(CCharPointer str, String encoding) throws SaxonApiException {
        if(str.isNull()) {
            throw new SaxonApiException("Null found in Java string conversation");
        }
        try {
            if (encoding == null) {
                if (debug) {
                    System.err.println("Java DEBUG toJavaString2: no encoding found");
                }
                return CTypeConversion.toJavaString(str);
            } else {
                if (debug) {
                    System.err.println("Java DEBUG toJavaString2: ENCODING="+ encoding);
                }
                return CTypeConversion.toJavaString(str, strlen(str), Charset.forName(encoding));
            }
        } catch(Exception ex) {
            if (debug) {
                System.err.println("SaxonApiException: " + ex.getMessage());
            }
            throw new SaxonApiException(ex);
        }
    }

    /**
     *  Static method to remove objects no longer used in the ObjectHandle pool.
     *  Once removed the Java GC will clear up associated memory used for this object in its cycle
     * @param thread  - Graalvm runtime data structure for a thread
     * @param handle reference to the object to be removed from ObjectHandle pool
     */
    @CEntryPoint(name = "j_handles_destroy")
    public static void destroy(IsolateThread thread, ObjectHandle handle) {
        j_internal_destroy(handle);
    }


    public static void j_internal_destroy(ObjectHandle handle) {
        if (handle != WordFactory.nullPointer()) {
            if (debug) {
                System.err.println("destroy method, Class to be deleted= " + (objectHandles.get(handle)).getClass().getName() + " ref=" + handle.rawValue());
                String value = objectHandlesPool.remove(handle.rawValue());
                if(value == null){
                    System.err.println("SaxonCAPI objectHandlesPool returned null value for ref="+handle.rawValue());
                } else {
                    System.err.println("SaxonCAPI objectHandlesPool value removed for ref="+handle.rawValue());
                }
            }
            objectHandles.destroy(handle);
            if (debug) {
                objHandlesCreated--;
                System.err.println("Destroyed Object Handle.  Current count: " + objHandlesCreated);
            }
        } else if (SaxonCAPI.debug) {
            System.err.println("ObjectHandle not destroyed: Null found");
        }
    }

    @CEntryPoint(name = "j_printObjectHandlesPoolInfo")
    public static void printObjectHandlesPoolInfo(IsolateThread thread){
        if (SaxonCAPI.debug) {
            System.err.println("Java Debug ObjectHandlesPoolInfo size="+objectHandlesPool.size());
            for(Map.Entry<Long, String> entry : objectHandlesPool.entrySet()) {
                System.err.println("Java Debug ObjectHandlesPoolInfo ref=" + entry.getKey() + " value=" + entry.getValue());
            }
        }

    }

    /**
     * Test method
     * @param thread  - Graalvm runtime data structure for a thread
     * @param live - flag for dumpHeap
     */
    @CEntryPoint(name = "j_createHeapDump")
    public static void createHeapDump(IsolateThread thread, int live) {
        try {
            File file = File.createTempFile("SVMHeapDump-", ".hprof");
            VMRuntime.dumpHeap(file.getAbsolutePath(), CTypeConversion.toBoolean(live));
            System.out.println("  Heap dump created " + file.getAbsolutePath() + ", size: " + file.length());
        } catch (UnsupportedOperationException unsupported) {
            System.out.println("  Heap dump creation failed." + unsupported.getMessage());
        } catch (IOException ioe) {
            System.out.println("IO went wrong: " + ioe.getMessage());
        }
    }


    /**
     * DataWrapper class
     */
    public static class DataWrapper {
        long[] valueRefs;

        /**
         *
         * @param vals - array of values to be wrapped
         */
        public DataWrapper(long[] vals) {
            this.valueRefs = vals;
        }

        /**
         *
         * @return  array of long values
         */
        public long[] getDataRefs() {
            return valueRefs;
        }


    }

    /**
     * AllocateFn interface - implemented in the C/C++ code
     */
    public interface AllocatorFn extends CFunctionPointer {

        /**
         *
         * @param size - of the space in C memory to allocate
         * @return CLongPointer - reference to the allocated space in the C memory
         */
        @InvokeCFunctionPointer
        CLongPointer call(long size);
    }

    /**
     * Convert wrapped long values to a C long array
     * @param thread  - Graalvm runtime data structure for a thread
     * @param alloc  AllocatorFn - represented by C++ allocation
     * @param refsObj  - Wrapped data values
     * @return a C long array
     */
    @CEntryPoint(name = "j_getLongArray")
    static CLongPointer getLongArray(IsolateThread thread, AllocatorFn alloc, ObjectHandle refsObj) {
        if (refsObj != WordFactory.nullPointer()) {
            DataWrapper obj = objectHandles.get(refsObj);
            return longArrayToNative(alloc, obj.getDataRefs());
        } else {
            return WordFactory.nullPointer();
        }
    }

    /**
     *  Convert an array of long values to a C long array
     * @param alloc  - representing the C++ allocation
     * @param refs - Java long array
     * @return a C long array
     */
    public static CLongPointer longArrayToNative(AllocatorFn alloc, long[] refs) {
        if (refs.length == 0) {
            return WordFactory.nullPointer();
        }
        CLongPointer a = alloc.call(refs.length * 8);
        for (int i = 0; i < refs.length; i++) {
            a.write(i, refs[i]);
        }
        return a;
    }


    /**
     *  Convert an array of long values to a C long array.
     *  The first item in the array is the length of the array
     * @param alloc - representing the C++ allocation
     * @param refs - Java long array
     * @return  CLongPointer - array of long values in the C memory space
     */
    static CLongPointer longArrayToNativeWithLength(AllocatorFn alloc, long[] refs) {
        if (refs.length == 0) {
            return WordFactory.nullPointer();
        }
        CLongPointer a = alloc.call((refs.length+1) * 8);
        a.write(0, refs.length);
        for (int i = 1; i < refs.length+1; i++) {
            a.write(i, refs[i - 1]);
        }
        return a;
    }

    /**
     * Allocate Char array in the C/C++ memory space
     */
    public interface AllocatorCCharFn extends CFunctionPointer {
        @InvokeCFunctionPointer
        CCharPointer call(long size);
    }


    /**
     *
     * @param alloc
     * @param bStream
     * @return
     */
    static CCharPointer stringToNative(AllocatorCCharFn alloc, ByteArrayOutputStream bStream) {
        byte[] b = bStream.toByteArray();
        /*if (b.length == 0) {
            WordFactory.nullPointer();
        } */
        CCharPointer a = alloc.call(b.length + 1);
        for (int i = 0; i < b.length; i++) {
            a.write(i, b[i]);
        }
        a.write(b.length, (byte) 0);
        return a;
    }

    /**
     *   Method to convert a Java string to native C char array in the C memory space
     * @param alloc  - The allocator from C/C++
     * @param string - The Java string object
     * @return  Pointer to the C char array held in the C/C++ memory space. It is the caller's responsibility to delete the associated memory
     */
    static CCharPointer stringToNative2(net.sf.saxon.option.cpp.SaxonCAPI.AllocatorCCharFn alloc, String string) {

        byte[] b = string.getBytes(StandardCharsets.UTF_8);
        CCharPointer a = alloc.call(b.length + 1);
        for (int i = 0; i < b.length; i++) {
            a.write(i, b[i]);
        }
        a.write(b.length, (byte) 0);
        return a;
    }

    /**
     * static method to create a Java String object from a C char array
     * @param thread  - Graalvm runtime data structure for a thread
     * @param cValue - C char array
     * @return long value pointing to the Java string object in the ObjectHandle pool. Return -1 if the
     * @throws SaxonApiException if fails to convert C string to Java String
     */
    @CEntryPoint(name = "j_createStringObject", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createStringObject(IsolateThread thread, CCharPointer cValue) throws SaxonApiException {
        if (cValue != WordFactory.nullPointer()) {
            String value = SaxonCAPI.toJavaString(cValue, WordFactory.nullPointer());
            ObjectHandle handle = createHandle(value);
            return handle.rawValue();
        }
        return -1;

    }


    /**
     *  Static method to create Processor object given with a configuration file
     * @param thread - Graalvm runtime data structure for a thread
     * @param c_configFile - C char array to the full path of the configuration file
     * @return long value pointing to the SaxonProcessor object in the ObjectHandle pool
     * @throws SaxonApiException if failure to convert c string to Java String. Also if the configuration file cannot be read
     */
    @CEntryPoint(name = "createSaxonProcessorWithConfigurationFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createSaxonProcessorWithConfigurationFile(IsolateThread thread, CCharPointer c_configFile) throws SaxonApiException {
        Configuration config = null;
        if (c_configFile == WordFactory.nullPointer()) {
            throw new SaxonApiException("Configuration file is null");
        }
        String configFile = SaxonCAPI.toJavaString(c_configFile, WordFactory.nullPointer());
        try {
            config = Configuration.readConfiguration(new StreamSource(configFile));

        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }


        if (config == null) {
            config = Configuration.newConfiguration();
        }
        
        ObjectHandle handle = createHandle(new Processor(config));
        return handle.rawValue();

    }


    /**
     * Error Listener to capture errors
     */
   /* protected ErrorListener errorListener = new StandardErrorListener() {

        @Override
        public void warning(TransformerException exception) {
            saxonWarnings.add(new SaxonCException(exception));

            try {
                super.error(exception);
            } catch (Exception ex) {
            }

        }

        @Override
        public void error(TransformerException exception) {
            if (Configuration.RECOVER_WITH_WARNINGS == Configuration.RECOVER_SILENTLY && !(exception instanceof ValidationException)) {
                // do nothing
                return;
            }
            saxonExceptions.add(new SaxonApiException(exception));
            try {
                super.error(exception);
            } catch (Exception ex) {
            }
        }

        @Override
        public void fatalError(TransformerException exception) {
            if (exception instanceof XPathException && ((XPathException) exception).hasBeenReported()) {
                // don't report the same error twice
                return;
            }
            saxonExceptions.add(new SaxonApiException(exception));
            try {
                super.fatalError(exception);
            } catch (Exception ex) {
            }
        }
    };   */

    /**
     * Static method to get the product version number
     * @param thread - Graalvm runtime data structure for a thread
     * @param processorRef  - long reference to the processor object in the ObjectHandle
     * @return product version given as a C string - C Char pointer array
     */
    @CEntryPoint(name = "j_c_getProductVersion")
    public static CCharPointer getProductVersion(IsolateThread thread, ObjectHandle processorRef) {
        if (processorRef == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        Processor processor = objectHandles.get(processorRef);
        final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(processor.getUnderlyingConfiguration().getProductTitle());
        return holder.get();
    }


    /**
     * Static method to get the product version number
     * @param thread - Graalvm runtime data structure for a thread
     * @param processorRef  - long reference to the processor object in the ObjectHandle
     * @param alloc - C allocator for creating data in the C memory space
     * @return  product version given as a C string - C Char pointer array
     */
    @CEntryPoint(name = "j_cpp_getProductVersion")
    public static CCharPointer getProductVersion(IsolateThread thread, ObjectHandle processorRef, SaxonCAPI.AllocatorCCharFn alloc) {
        if (processorRef == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }
        Processor processor = objectHandles.get(processorRef);
        String title = processor.getUnderlyingConfiguration().getProductTitle();
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, title);
        return a;
    }

    /* @CEntryPoint(name = "j_testProg")
    public static CCharPointer j_testProg(IsolateThread thread, int flag) throws Exception{

        if(flag == 2) {
            Properties props = System.getProperties();
            props.setProperty("xmlresolver.properties", "/home/ond1/work/repository/saxondev-11/build/releases/saxoncee/samples/data/xmlresolver.properties");

        }
        try {
            Processor processor = new Processor(false);
            String dir = "/home/ond1/work/repository/saxondev-11/build/releases/saxoncee/samples/data/";

            XsltCompiler compilerx = processor.newXsltCompiler();
            Xslt30Transformer transformerx = compilerx.compile(new File(dir + "example.xsl")).load30();
            XdmValue valuex = processor.newDocumentBuilder().build(new File(dir + "example2.xml"));
            XdmValue resultx = transformerx.applyTemplates(valuex);


            System.err.println("Java Catalog result cp0 = " + resultx.toString());



            String[] catalogs = {dir + "catalog.xml"};
            processor.setCatalogFiles(catalogs);
            XsltCompiler compiler = processor.newXsltCompiler();
            Xslt30Transformer transformer = compiler.compile(new File(dir + "example.xsl")).load30();
            XdmValue value = processor.newDocumentBuilder().build(new File(dir + "example.xml"));
            XdmValue result = transformer.applyTemplates(value);

            System.err.println("Java Catalog result cp1 = " + result.toString());
            final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(result.toString());
            return holder.get();
        } catch (SaxonApiException ex) {
            ex.printStackTrace();
            System.err.println("SaxonException = " + ex.getMessage());
            throw ex;
        }  catch (Exception ex) {
                    ex.printStackTrace() ;
                    System.err.println("Exception = " + ex.getMessage());
                    throw ex;
                }
    }         */

    /**
     *  Set the XML catalog file
     * @param thread - Graalvm runtime data structure for a thread
     * @param processorRef  - long reference pointing to the processor object in the ObjectHandle
     * @param catalogFilesRef  - C char array pointer of the  catalog file name given as a full path
     * @return success status. 1 is success, -1 is a fail and -2 is a exception thrown
     * @throws SaxonApiException  if we cannot convert the catalogFile string to Java object
     */
    @CEntryPoint(name = "j_setCatalogFiles", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_setCatalogFiles(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, ObjectHandle catalogFilesRef) throws SaxonApiException {
        
        if (processorRef == WordFactory.nullPointer() || catalogFilesRef == WordFactory.nullPointer()) {
            return net.sf.saxon.option.cpp.SaxonCAPI.STATUS_FAIL;
        }

        String cwd = null;
        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        Processor processor = objectHandles.get(processorRef);
        ProcessorDataAccumulator accumulator = objectHandles.get(catalogFilesRef);

        ArrayList<String>  catalogFiles = accumulator.getParameterNamesAsArrayList();
        ArrayList<String>  catalogFilesConverted = new ArrayList<String>();

        int sizea = catalogFiles.size();
        for(int i =0; i < sizea;i++) {
            catalogFilesConverted.add(SaxonCAPI.resolveFileToURI(processor,cwd, catalogFiles.get(i)).toASCIIString());
        }

        if(catalogFiles.size() == 0) {
            if (debug) {
                System.err.println("catalog size = " + 0);
            }
            return net.sf.saxon.option.cpp.SaxonCAPI.STATUS_FAIL;
        }


        if (debug) {
            System.err.println("catalog size  =" + catalogFiles.size());
            for(int i =0; i < sizea;i++) {
                System.err.println("catalog file= " + catalogFiles.get(i));
            }
        }
        Configuration config = processor.getUnderlyingConfiguration();
        if (config.getResourceResolver() instanceof ConfigurableResourceResolver) {
            CommandLineOptions.setCatalogFiles(((ConfigurableResourceResolver) config.getResourceResolver() ), catalogFilesConverted);
        }  else {
            return net.sf.saxon.option.cpp.SaxonCAPI.STATUS_FAIL;
        }
        return net.sf.saxon.option.cpp.SaxonCAPI.STATUS_OK;
    }


    /**
     * Get the Processor object created
     *
     * @return Processor
     */
    public Processor getProcessor() {
        return this.processor;
    }

    /**
     * Static method to set configuration property on the Processor
     * @param thread - Graalvm runtime data structure for a thread
     * @param processorRef - processor reference in the objectHandle pool
     * @param c_propertyName reference to the property name
     * @param c_propertyValue reference to the property
     * @return success status. 1 is success, -1 is a fail and -2 is a exception thrown
     * @throws SaxonApiException if processor object is null. Also if we cannot apply a configuration
     */
    @CEntryPoint(name = "j_applyPropertyToConfiguration", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static synchronized long applyPropertyToConfiguration(IsolateThread thread, ObjectHandle processorRef, CCharPointer c_propertyName, CCharPointer c_propertyValue) throws SaxonApiException {

        if(processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor is null");
        }
        Processor processor = objectHandles.get(processorRef);
        if (debug) {
            System.err.println("New processor created, Processor: " + System.identityHashCode(processor));
        }

        String name = null;
        String value = null;
        if (c_propertyName != WordFactory.nullPointer()) {
            name = SaxonCAPI.toJavaString(c_propertyName, WordFactory.nullPointer());
        }

        if (c_propertyValue != WordFactory.nullPointer()) {
            value = SaxonCAPI.toJavaString(c_propertyValue, WordFactory.nullPointer());
        }

        Configuration config = processor.getUnderlyingConfiguration();
        if (value == null || name == null) {
            if (debug) {
                System.err.println("SaxonC DEBUG - applyConfiguration value or name is null");
            }
            return SaxonCAPI.STATUS_FAIL;
        }

            if (debug) {
                System.err.println("SaxonC DEBUG - applyConfiguration, name=" + name);
                System.err.println("SaxonC DEBUG - applyConfiguration, value=" + value);
            }

            if (name.equals("l")) {
                processor.setConfigurationProperty(Feature.LINE_NUMBERING,
                        "on".equals(value));
            } else if (name.equals("dtd")) {
                int validationMode = Validation.DEFAULT;
                switch (value) {
                    case "on":
                        validationMode = Validation.STRICT;
                        break;
                    case "off":
                        validationMode = Validation.SKIP;
                        break;
                    case "recover":
                        validationMode = Validation.LAX;
                        break;
                }
                config.setParseOptions(config.getParseOptions().withDTDValidationMode(validationMode));
            } else {
                boolean b = "on".equals(value) || "true".equals(value) || "1".equals(value);
                switch (name) {
                    case "expand":
                        config.setParseOptions(config.getParseOptions().withExpandAttributeDefaults(b));
                        break;
                    case "opt":
                        config.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, value);

                        break;
                    case "outval":

                        Boolean isRecover = "recover".equals(value);
                        config.setConfigurationProperty(Feature.VALIDATION_WARNINGS, isRecover);
                        config.setConfigurationProperty(Feature.VALIDATION_COMMENTS, isRecover);


                        break;
                    case "strip":

                        config.setConfigurationProperty(Feature.STRIP_WHITESPACE, value);
                        break;
                    case "val":
                        if ("strict".equals(value)) {
                            processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION, Validation.STRICT);
                        } else if ("lax".equals(value)) {
                            processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION, Validation.LAX);
                        }

                        break;
                    case "xsdversion":
                        processor.setConfigurationProperty(Feature.XSD_VERSION, value);

                        break;
                    case "xmlversion":

                        processor.setConfigurationProperty(Feature.XML_VERSION, value);

                        break;
                    default:
                        if (name.equals("xi")) {
                            processor.setConfigurationProperty(Feature.XINCLUDE, b);

                        } else if (name.equals("xsiloc")) {
                            processor.setConfigurationProperty(Feature.USE_XSI_SCHEMA_LOCATION, b);
                        } else if (name.equals("errorOutputFile") || name.equals("http://saxon.sf.net/feature/standardErrorOutputFile")) {
                            if (SaxonCAPI.debug) {
                                System.err.println("JAVA DEBUG: standErrorOutputFile set name=" + value);
                            }
                            //processor.setConfigurationProperty(Feature.STANDARD_ERROR_OUTPUT_FILE,value);
                            SaxonCException.errorOutputFile = value;


                            try {
                                boolean append = true;
                                boolean autoFlush = true;
                                SaxonCException.errorReporter.setStandardErrorOutput(new PrintStream(new FileOutputStream((String) value, append), autoFlush));
                            } catch (FileNotFoundException fnf) {
                                throw new IllegalArgumentException(fnf);
                            }

                        } else {
                            processor.getUnderlyingConfiguration().setConfigurationProperty(name, value);
                        }
                        break;
                }
            }


        return SaxonCAPI.STATUS_OK;
    }


    /**
     * Static method to set configuration properties on the Processor
     * @param thread - Graalvm runtime data structure for a thread
     * @param processorRef - processor reference in the objectHandle pool
     * @param processorDataRef reference to the ProcessorDataAccumulator object in he ObjectHandle pool
     * @return success status. 1 is success, -1 is a fail and -2 is a exception thrown
     * @throws SaxonApiException if processor object is null. Also if we cannot apply a configuration
     */
    @CEntryPoint(name = "j_applyPropertiesToConfiguration", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static synchronized long applyPropertiesToConfiguration(IsolateThread thread, ObjectHandle processorRef, ObjectHandle processorDataRef) throws SaxonApiException {

        Processor processor = objectHandles.get(processorRef);
        if (debug) {
            System.err.println("New processor created, Processor: " + System.identityHashCode(processor));
        }

        if (processorDataRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor is null");
        }

        ProcessorDataAccumulator processorDataAccumulator = objectHandles.get(processorDataRef);
        Configuration config = processor.getUnderlyingConfiguration();
        String[] names = processorDataAccumulator.getParameterNames();
        Object[] values = processorDataAccumulator.getValues();
        int len = processorDataAccumulator.size();
        for (int i = 0; i < len; i++) {
            String name = names[i];
            String value = (String) values[i];
            if (debug) {
                System.err.println("SaxonC DEBUG - applyConfiguration, name=" + name);
                System.err.println("SaxonC DEBUG - applyConfiguration, value=" + value);
            }
            if (value == null || name == null) {
                if (debug) {
                    System.err.println("SaxonC DEBUG - applyConfiguration value or name is null");
                }
                continue;
            }
            if (name.equals("l")) {
                processor.setConfigurationProperty(Feature.LINE_NUMBERING,
                            "on".equals(value));
            } else if (name.equals("dtd")) {
                int validationMode = Validation.DEFAULT;
                if ("on".equals(value)) {
                    validationMode = Validation.STRICT;
                } else if ("off".equals(value)) {
                    validationMode = Validation.SKIP;
                } else if ("recover".equals(value)) {
                    validationMode = Validation.LAX;
                }
                config.setParseOptions(config.getParseOptions().withDTDValidationMode(validationMode));
            } else {
                boolean b = "on".equals(value) || "true".equals(value) || "1".equals(value);
                if (name.equals("expand")) {
                    config.setParseOptions(config.getParseOptions().withExpandAttributeDefaults(b));
                } else if (name.equals("opt")) {
                    config.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, value);

                } else if (name.equals("outval")) {

                    Boolean isRecover = "recover".equals(value);
                    config.setConfigurationProperty(Feature.VALIDATION_WARNINGS, isRecover);
                    config.setConfigurationProperty(Feature.VALIDATION_COMMENTS, isRecover);


                } else if (name.equals("strip")) {

                    config.setConfigurationProperty(Feature.STRIP_WHITESPACE, value);
                } else if (name.equals("val")) {
                    if ("strict".equals(value)) {
                        processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION, Validation.STRICT);
                    } else if ("lax".equals(value)) {
                        processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION, Validation.LAX);
                    }

                } else if (name.equals("xsdversion")) {
                    processor.setConfigurationProperty(Feature.XSD_VERSION, value);

                } else if (name.equals("xmlversion")) {

                    processor.setConfigurationProperty(Feature.XML_VERSION, value);

                } else {
                    if (name.equals("xi")) {
                        processor.setConfigurationProperty(Feature.XINCLUDE, b);

                    } else if (name.equals("xsiloc")) {
                        processor.setConfigurationProperty(Feature.USE_XSI_SCHEMA_LOCATION, b);
                    } else if (name.equals("errorOutputFile") || name.equals("http://saxon.sf.net/feature/standardErrorOutputFile")) {
                        if(SaxonCAPI.debug) {
                            System.err.println("JAVA DEBUG: standErrorOutputFile set name=" + value);
                        }
                        //processor.setConfigurationProperty(Feature.STANDARD_ERROR_OUTPUT_FILE,value);
                        SaxonCException.errorOutputFile = value;


                        try {
                            boolean append = true;
                            boolean autoFlush = true;
                            SaxonCException.errorReporter.setStandardErrorOutput(new PrintStream(new FileOutputStream((String) value, append), autoFlush));
                        } catch (FileNotFoundException fnf) {
                            throw new IllegalArgumentException(fnf);
                        }

                    }else if (name.startsWith("http://saxon.sf.net/feature/")) {
                        processor.setConfigurationProperty(name, value);
                    }
                }
            }
        }

        return SaxonCAPI.STATUS_OK;
    }


    /**
     * set debug mode on or off
     * @param thread - Graalvm runtime data structure for a thread
     * @param d - flag to enable or disable debug mode. int converted to java boolean
     */
    @CEntryPoint(name = "setDebugMode")
    public static void setDebugMode(IsolateThread thread, int d) {
        debug = CTypeConversion.toBoolean(d);
    }


    /**
     * Method to convert arrays received from C++ to Java map object
     * this method also extracts serialization properties and parameters
     *
     * @param processorDataAccumulatorRef     - Data held as a map structure when passing from C++ to Java
     * @param props                           - Serialisation properties to be setup
     * @param parameters                      - Passed Processor parameters as map. Updated within method
     * @param staticParameters                - For XSLT use
     * @param globals                         - Map to retrieve global variables to be used in XSLT transformer
     * @param initialTemplateParameters       - Initial template parameter for use with XSLT
     * @param initialTemplateTunnelParameters
     * @param forXslt                         - Indicate XSLT use
     * @return Map of String, Object pairs
     * @throws SaxonApiException failure in the setup of configuration parameters
     */
    public static Map<String, Object> setupConfigurationAndBuildMap(ObjectHandle processorDataAccumulatorRef, Properties props, Map<QName, XdmValue> parameters, Map<QName, XdmValue> staticParameters, Map<QName, XdmValue> globals, Map<QName, XdmValue> initialTemplateParameters, Map<QName, XdmValue> initialTemplateTunnelParameters, boolean forXslt) throws SaxonApiException {

        Map<String, Object> map = new HashMap();
        ProcessorDataAccumulator processorDataAccumulator = null;
        if (processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = objectHandles.get(processorDataAccumulatorRef);
        } else {
            return map;
        }
        String[] params = null;
        Object[] values = null;
        if (processorDataAccumulator != null) {
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
            if(debug) {
                for(int i = 0 ; i < values.length; i++) {
                    System.err.println("value ["+i+"] = "+values[i].getClass().getName());
                }

            }
        }

        if (params == null || values == null) {
            return map;
        }
        if (params.length != values.length) {
            throw new SaxonApiException("SaxonC internal Error: params array length does not match values length");
        }
        for (int i = 0; i < params.length; i++) {
            if (params[i].startsWith("!")) {
                String name = params[i].substring(1);
                Serializer.Property prop = Serializer.Property.get(name);
                if (prop == null) {
                    throw new SaxonApiException("Property name " + name + " not found");
                }
                if (props != null) {
                    props.put(prop.getQName().getClarkName(), values[i]);
                } else {
                    if (debug) {
                        System.err.println("Error Java property option found but Java Property object is null: Probably XPathProcessor in use");
                    }
                }
            } else if (params[i].equals("resources")) {
                char separatorChar = '/';
                if (SaxonCAPI.RESOURCES_DIR == null && values[i] instanceof String) {
                    String dir1 = (String) values[i];
                    if (!dir1.endsWith("/")) {
                        dir1 = dir1.concat("/");
                    }
                    if (File.separatorChar != '/') {
                        dir1.replace(separatorChar, File.separatorChar);
                        separatorChar = '\\';
                        dir1.replace('/', '\\');
                    }
                    SaxonCAPI.RESOURCES_DIR = dir1;
                }

            } else if (initialTemplateParameters != null && params[i].startsWith("itparam:")) {
                //initial template parameters
                String paramName = params[i].substring(8);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                initialTemplateParameters.put(qname, valueForCpp);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI itparam: " + paramName);
                }

            } else if (initialTemplateTunnelParameters != null && params[i].startsWith("tnparam:")) {
                //initial template parameters
                String paramName = params[i].substring(8);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                initialTemplateTunnelParameters.put(qname, valueForCpp);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI tnparam: " + paramName);
                }

            } else if (staticParameters != null && params[i].startsWith("sparam:")) {
                //static parameters
                String paramName = params[i].substring(7);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                staticParameters.put(qname, valueForCpp);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI sparam: " + paramName);
                }

            } else if (params[i].startsWith("param:")) {
                String paramName = params[i].substring(6);
                Object value = values[i];
                XdmValue valueForCpp = null;
                QName qname = QName.fromClarkName(paramName);
                valueForCpp = convertObjectToXdmValue(values[i]);
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI param: " + paramName);
                }

                if (qname != null && valueForCpp != null) {
                    if (forXslt) {
                        globals.put(qname, valueForCpp);
                    } else {
                        if (parameters != null) {
                            parameters.put(qname, valueForCpp);
                        }
                    }
                }
            } /*else if (!forXslt && params[i].startsWith("dvar:")) {
                QName varQName = QName.fromClarkName((String)values[i]);
                variables.add(varQName);
            } */ else {
                if (debug) {
                    System.err.println("DEBUG: SaxonCAPI: param-name: " + params[i] + ", type of Value= " + values[i].getClass().getName());
                }
                map.put(params[i], values[i]);
            }
        }
        return map;

    }


    /**
     * Utility method when XdmValues retrieved as objects from C++ to convert object to an XmdValue.
     *
     * @param value - object to be converted
     * @return XdmValue
     */
    public static XdmValue convertObjectToXdmValue(Object value) {

        XdmValue valueForCpp = null;
        if (value instanceof ProcessorDataAccumulator) {
            value = ((ProcessorDataAccumulator) value).getValues();
        }
        if (value instanceof XdmValue) {
            valueForCpp = (XdmValue) value;
            if (debug) {

                System.err.println("DEBUG: XSLTTransformerForCpp: " + valueForCpp.getUnderlyingValue().toString());
                System.err.println("DEBUG: XSLTTransformerForCpp: " + valueForCpp.getUnderlyingValue());
            }

        } else if (value instanceof Object[]) {

            Object[] arr = (Object[]) value;
            if (debug) {
                System.err.println("DEBUG: Array of parameters found. arr len=" + arr.length);

            }
            if (arr.length == 0) {
                return null;
            }
            List<Item> valueList = new ArrayList<Item>();
            for (int j = 0; j < arr.length; j++) {
                Object itemi = arr[j];
                if (itemi == null) {
                    if (debug) {
                        System.err.println("Error: Null item at " + j + "th position in array of XdmValues");
                    }
                    break;
                }
                if (debug) {
                    System.err.println("Java object:" + itemi);
                }
                if (itemi instanceof XdmValue) {
                    valueList.add((Item) (((XdmValue) itemi).getUnderlyingValue()));
                } else {
                    XdmValue valuex = getXdmValue(itemi);
                    if (valuex == null) {
                        if (debug) {
                            System.err.println("Error: Null item at " + j + "th position in array of XdmValues when converting");
                        }
                        break;
                    }
                    valueList.add((Item) (getXdmValue(itemi)).getUnderlyingValue());
                }
            }
            GroundedValue gv = SequenceExtent.makeSequenceExtent(valueList);
            valueForCpp = XdmValue.wrap(gv);
        } else {
            //fast track for primitive values
            valueForCpp = getXdmValue(value);
            if (debug) {
                if(value != null) {
                    System.err.println("Java DEBUG: value type = " + value.getClass().getName());
                    System.err.println("Java DEBUG: value = " + value);
                }
                if(valueForCpp == null) {
                    System.err.println("Java DEBUG: value is null");
                }
                System.err.println("DEBUG: primitive value found");
            }
        }
        return valueForCpp;
    }


    /**
     * parse JSON supplied by file
     *
     * @param thread     - The Graalvm IsolateThread
     * @param processorRef - The Reference to the processor object coming from the C++ code
     * @param cfilename - File name of the JSON document to parse
     * @return   long value as  reference to the created XdmValue in the ObjectHandle pool
     * @throws SaxonApiException failure to read JSON file or create JSON string. Also if processor object is null
     */
    @CEntryPoint(name = "parseJsonFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long parseJsonFile(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer cfilename) throws SaxonApiException {

        Processor proc = null;
        if(processorRef == WordFactory.nullPointer()){
            throw new SaxonApiException("Processor object is null");
        } else {
            proc =  objectHandles.get(processorRef);
        }

        String cwd = null;
        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }


        String filename = null;
        if(cfilename != WordFactory.nullPointer()) {
            filename = SaxonCAPI.toJavaString(cfilename, WordFactory.nullPointer());
        } else {
            throw new SaxonApiException("JSON file name is null");
        }



        JsonBuilder builder = proc.newJsonBuilder();
        XdmValue result = null;

        URI resultURI = resolveFileToURI(proc, cwd, filename);

        try {
            BufferedReader in = new BufferedReader(new InputStreamReader(resultURI.toURL().openStream()));
            result = builder.parseJson(in);
        } catch (MalformedURLException e) {
            throw new SaxonApiException(e);
        } catch (IOException e) {
            throw new SaxonApiException(e);
        }
        
        ObjectHandle handle = createHandle(result);
        return handle.rawValue();

    }

    /**
     * parse JSON supplied as a string
     *
     * @param thread     - The Graalvm IsolateThread
     * @param processorRef - The Reference to the processor object coming from the C++ code
     * @param cjson_string - String representation of the JSON
     * @param c_encoding - encoding of the JSON string. If null use the platform default
     * @return   long value as  reference to the created XdmValue in the ObjectHandle pool
     * @throws SaxonApiException  failure read JSON string or encoding name. Also if processor object is null
     */
    @CEntryPoint(name = "parseJsonString", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long parseJsonString(IsolateThread thread, ObjectHandle processorRef, CCharPointer cjson_string, CCharPointer c_encoding) throws SaxonApiException {

        Processor proc;
        if(processorRef == WordFactory.nullPointer()){
            throw new SaxonApiException("Processor object is null");
        } else {
            proc =  objectHandles.get(processorRef);
        }

        String json = null;
        if(cjson_string != WordFactory.nullPointer()) {
            json = SaxonCAPI.toJavaString(cjson_string, c_encoding);
        } else {
            throw new SaxonApiException("JSON string is null");
        }
        JsonBuilder builder = proc.newJsonBuilder();
        XdmValue result = builder.parseJson(json);

        ObjectHandle handle = createHandle(result);
        return handle.rawValue();
    }


    /**
     * parse XML document supplied by file
     *
     * @param thread   - The Graalvm IsolateThread
     * @param processorRef - long value pointing to the processor object in the ObjectHandle pool
     * @param ccwd     - Current Working directory
     * @param filename - File name of the XML document to parse
     * @return   long value as  reference to the created XdmNode in the ObjectHandle pool
     * @throws SaxonApiException if XML file cannot be read
     */
    @CEntryPoint(name = "parseXmlFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long parseXmlFile(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer filename) throws SaxonApiException {
        Processor proc = objectHandles.get(processorRef);
        String cwd = null;
        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        return j_main_parseXmlFile(thread, ccwd, processorRef, WordFactory.nullPointer(), WordFactory.nullPointer(), filename);

    }

    /**
     *  parse XML document from file. Also accept SchemaValidator.
     *  The XML resolver is used to resolve path and file name.
     * @param thread - The Graalvm IsolateThread
     * @param ccwd  - directory path to the XML file name.
     * @param processorRef - long value referencing the processor object in the ObjectHandle pool
     * @param validatorRef  - long value referencing the SchemaValidator in the ObjectHandle pool
     * @param filename   - XML document file name
     * @return    long value as  reference to the created XdmNode in the ObjectHandle pool
     * @throws SaxonApiException  if the file cannot be read
     */
    @CEntryPoint(name = "parseXmlFileWithValidator", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long parseXmlFileWithValidator(IsolateThread thread, CCharPointer ccwd, ObjectHandle processorRef, ObjectHandle validatorRef, CCharPointer filename) throws SaxonApiException {
        return j_main_parseXmlFile(thread, ccwd, processorRef, WordFactory.nullPointer(), validatorRef, filename);
    }


    /**
     * parse XML document supplied as a string
     *
     * @param thread - The Graalvm IsolateThread
     * @param processorRef - long value pointing to the processor object in the ObjectHandle pool
     * @param encoding - encoding of the JSON string. If null use the platform default
     * @param xml - string representation of XML document
     * @return   long value as  reference to the created XdmNode in the ObjectHandle pool
     */
    @CEntryPoint(name = "parseXmlString", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long parseXmlString(IsolateThread thread, ObjectHandle processorRef, CCharPointer encoding,  CCharPointer xml) throws SaxonApiException {
        return j_main_parseXmlString(thread, WordFactory.nullPointer(), processorRef, WordFactory.nullPointer(), WordFactory.nullPointer(), xml, encoding);
    }


    /**
     *  parse XML document supplied as a string. Also accept SchemaValidator to be used
     *
     * @param thread - The Graalvm IsolateThread
     * @param processorRef  - long value pointing to the processor object in the ObjectHandle pool
     * @param encoding  - encoding of the JSON string. If null use the platform default
     * @param validatorRef - long value referencing the SchemaValidator in the ObjectHandle pool
     * @param xml  - string representation of XML document
     * @return  long value as  reference to the created XdmNode in the ObjectHandle pool
     * @throws SaxonApiException if cannot read XML string. Also if any other failure occurs building the document, for example
     *                                   a parsing error
     */
    @CEntryPoint(name = "parseXmlStringWithValidator", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long parseXmlString(IsolateThread thread, ObjectHandle processorRef, CCharPointer encoding,  ObjectHandle validatorRef, CCharPointer xml) throws SaxonApiException {
        return j_main_parseXmlString(thread, WordFactory.nullPointer(), processorRef, WordFactory.nullPointer(), validatorRef, xml, encoding);

    }

    /**
     * Create an Xdm atomic value from string representation and add it to the ObjectHandle pool
     * @param thread  - The Graalvm IsolateThread
     * @param ctypeStr  - Local name of a type in the XML Schema namespace.
     * @param cvalueStr The value given in a string form.
     *                 In the case of a QName the value supplied must be in clark notation. {uri}local
     * @return   long value as  reference to the created XdmAtomicValue in the ObjectHandle pool
     * @throws SaxonApiException  if failure in creating the Java String or in creation of an AtomicValue
     */
    @CEntryPoint(name = "createXdmAtomicItem", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createXdmAtomicItem(IsolateThread thread, CCharPointer ctypeStr, CCharPointer cvalueStr) throws SaxonApiException {
        String valueStr = SaxonCAPI.toJavaString(cvalueStr, WordFactory.nullPointer());
        String typeStr = SaxonCAPI.toJavaString(ctypeStr, WordFactory.nullPointer());

        ObjectHandle handle = null;

        handle = createHandle(createXdmAtomicItem(valueStr, typeStr));
        return handle.rawValue();

    }

    /**
     * Create an Xdm atomic value from string representation
     *
     * @param typeStr  - Local name of a type in the XML Schema namespace.
     * @param valueStr - The value given in a string form.
     *                 In the case of a QName the value supplied must be in clark notation. {uri}local
     * @return XdmValue - value
     */

    public static XdmValue createXdmAtomicItem(String typeStr, String valueStr) throws SaxonApiException {

        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);


        BuiltInAtomicType type = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        if (type == null) {
            throw new SaxonApiException("Unknown built in type: " + typeStr + " not found");
        }

        if (type.isNamespaceSensitive()) {
            StructuredQName value = StructuredQName.fromClarkName(valueStr);
            return XdmValue.wrap(new QNameValue(value, type));
        }

        ConversionRules rules = new ConversionRules();
        Converter converter = rules.getConverter(BuiltInAtomicType.STRING, type);


        try {
            return XdmValue.wrap(converter.convert(new StringValue(valueStr)).asAtomic());
        } catch (ValidationException e) {
            throw new SaxonApiException(e);
        }


    }

    /**
     * Wrap a boxed primitive type as an XdmValue.
     *
     * @param value - boxed primitive type
     * @return XdmValue
     */
    public static XdmValue getXdmValue(Object value) {
        XdmValue valueForCpp = null;
        if (value instanceof Integer) {
            valueForCpp = XdmValue.wrap(new Int64Value(((Integer) value).intValue()));
        } else if (value instanceof Boolean) {
            valueForCpp = XdmValue.wrap(BooleanValue.get(((Boolean) value).booleanValue()));
        } else if (value instanceof Double) {
            valueForCpp = XdmValue.wrap(DoubleValue.makeDoubleValue(((Double) value).doubleValue()));
        } else if (value instanceof Float) {
            valueForCpp = XdmValue.wrap(FloatValue.makeFloatValue(((Float) value).floatValue()));
        } else if (value instanceof Long) {
            valueForCpp = XdmValue.wrap(Int64Value.makeIntegerValue((((Long) value).longValue())));
        } else if (value instanceof String) {
            valueForCpp = XdmValue.wrap(StringValue.makeStringValue(((String) value)));
        }
        return valueForCpp;
    }

    /**
     *  Static method to parse the XML string to a XdmNode object.
     *  Long value reference given which the object stored in the ObjectHandle pool
     * @param thread  - The Graalvm IsolateThread
     * @param ccwd- directory path to the XML file name.
     * @param procRef - long value pointing to a SaxonProcessor object in the ObjectHandle pool
     * @param builderRef - long value pointing to a DocumentBuilder object in the ObjectHandle pool
     * @param validatorRef - long value pointing to a SchemaValidator object in the ObjectHandle pool
     * @param cxml - c char pointer array to the string representation of the XML document
     * @param c_encoding - C char array for the encoding name
     * @return
     * @throws SaxonApiException  if any other failure occurs building the document, for example
     *      *                                  a parsing error
     */
    @CEntryPoint(name = "j_main_parseXmlString", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_main_parseXmlString(IsolateThread thread, CCharPointer ccwd, ObjectHandle procRef, ObjectHandle builderRef, ObjectHandle validatorRef, CCharPointer cxml, CCharPointer c_encoding) throws SaxonApiException {
        DocumentBuilder builder = null;
        Processor proci = null;
        String cwd = null;
        String xml = null;
        SchemaValidatorForCpp validatorForCpp = null;
        if (cxml == WordFactory.nullPointer()) {
            throw new SaxonApiException("XML string is null");
        } else {
            xml = SaxonCAPI.toJavaString(cxml, c_encoding);
        }
        if (xml == null) {
            throw new SaxonApiException("XML string is null");
        }

        if (procRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor pointer is null");
        } else {
            proci = objectHandles.get(procRef);
            if (proci == null) {
                throw new SaxonApiException("Processor object is null");
            }
        }


        StringReader reader = new StringReader(xml);
        if (builderRef == WordFactory.nullPointer()) {
            builder = proci.newDocumentBuilder();
            if (debug) {
                System.err.println("New documentBuilder obj: " + System.identityHashCode(builder));
            }
        } else {
            builder = objectHandles.get(builderRef);
            if (debug) {

                System.err.println("documentBuilder obj passed as argument: " + System.identityHashCode(builder));
            }
        }
        if (validatorRef != WordFactory.nullPointer()) {
            validatorForCpp = objectHandles.get(validatorRef);
            builder.setSchemaValidator(validatorForCpp.getSchemaManager().newSchemaValidator());
        }
        Source source = new SAXSource(new InputSource(reader));
        if (cwd != null && cwd.length() > 0) {
            if (!cwd.endsWith("/")) {
                cwd = cwd.concat("/");
            }
            source.setSystemId(cwd);
            if(debug) {
                System.err.println("DEBUG: source.setSystemId(cwd) cwd= " + cwd);
            }
        }

        XdmNode doc = builder.build(source);


        if (debug) {
            System.err.println("DEBUG: builder.getBaseURI= " + builder.getBaseURI());
            if(doc.getBaseURI() != null) {
                System.err.println("DEBUG: parseXML doc.getBaseURI= " + doc.getBaseURI());
            }
            System.err.println("xmlParseString, Processor: " + System.identityHashCode(proci));
            System.err.println("documentBuilder obj: " + System.identityHashCode(builder));
            if (doc != null) {
                System.err.println("XdmNode: " + System.identityHashCode(doc));
            }
        }
        
        ObjectHandle handle = createHandle(doc);

        return handle.rawValue();


    }

    /**
     *  Static method to parse the XML string to a XdmNode object.
     *  Long value reference given which the object stored in the ObjectHandle pool
     * @param cwd- directory path to the XML file name.
     * @param proci - SaxonProcessor object to create a DocumentBuilder if it does not already exist
     * @param builder - DocumentBuilder object. If null then the method will create one
     * @param validator - long value pointing to a SchemaValidator object in the ObjectHandle pool
     * @param xml - c char pointer array to the string representation of the XML document
     * @return
     * @throws SaxonApiException  if any other failure occurs building the document, for example
     *                                  a parsing error
     */
    public static XdmNode main_parseXmlString(String cwd, Processor proci, DocumentBuilder builder, SchemaValidator validator, String xml) throws SaxonApiException {

        try {

            StringReader reader = new StringReader(xml);
            if (builder == null) {
                builder = proci.newDocumentBuilder();
                if (debug) {

                    System.err.println("New documentBuilder obj: " + System.identityHashCode(builder));
                }
            } else if (debug) {

                System.err.println("documentBuilder obj passed as argument: " + System.identityHashCode(builder));
            }
            if (validator != null) {
                builder.setSchemaValidator(validator);
            }
            Source source = new SAXSource(new InputSource(reader));
            if (cwd != null && cwd.length() > 0) {
                if (!cwd.endsWith("/")) {
                    cwd = cwd.concat("/");
                }
                source.setSystemId(cwd);
            }

            XdmNode doc = builder.build(source);
            if (debug) {
                System.err.println("xmlParseString, Processor: " + System.identityHashCode(proci));
                System.err.println("documentBuilder obj: " + System.identityHashCode(builder));
                if (doc != null) {
                    System.err.println("XdmNode: " + System.identityHashCode(doc));
                }
            }
            return doc;
        } catch (SaxonApiException ex) {
            throw ex;
        }
    }

    /**
     * Utility method to get the name of this type as a StructuredQName, unless the type is anonymous, in which case
     *  return null
     *
     * @param thread  - The Graalvm IsolateThread
     * @param ctypeStr  the type name
     * @return the C char pointer array of the typed name
     */
    @CEntryPoint(name = "getTypeName", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer getTypeName(IsolateThread thread, CCharPointer ctypeStr) throws SaxonApiException {
        String typeStr = SaxonCAPI.toJavaString(ctypeStr, WordFactory.nullPointer());
        int fp = StandardNames.getFingerprint(NamespaceUri.SCHEMA, typeStr);

        BuiltInAtomicType typei = (BuiltInAtomicType) BuiltInType.getSchemaType(fp);
        StructuredQName sname = typei.getTypeName();
        if (sname == null) {
            return WordFactory.nullPointer();
        }
        String typeName = new QName(sname.getPrefix(), sname.getNamespaceUri().toString(), sname.getLocalPart()).getClarkName();
        final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(typeName);
        return holder.get();

    }


    /**
     * Utility method to get the String value from the XdmValue representation
     *
     * @param thread   - The Graalvm IsolateThread
     * @param xdmvalueRef  - long value referencing the XdmValue object held in the ObjectHandle pool
     * @return C Pointer to the string value in the Java memory space
     */
    @CEntryPoint(name = "j_getStringValue")
    public static CCharPointer getStringValue(IsolateThread thread, ObjectHandle xdmvalueRef) {
        XdmValue value = objectHandles.get(xdmvalueRef);
        final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(value.toString());
        return holder.get();
    }


    /**
     * Utility method to get the String value from the XdmValue representation
     *
     * @param thread   - The Graalvm IsolateThread
     * @param xdmvalueRef   - long value referencing the XdmValue object held in the ObjectHandle pool
     * @return C Pointer to the string value in the C++ memory space
     */
    @CEntryPoint(name = "j_cpp_getStringValue")
    public static CCharPointer getStringValue(IsolateThread thread, ObjectHandle xdmvalueRef, SaxonCAPI.AllocatorCCharFn alloc) {
        XdmValue value = objectHandles.get(xdmvalueRef);
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, value.toString());
        return a;
    }

    @CEntryPoint(name = "j_cpp_encodeString",  exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer encodeStringValue(IsolateThread thread, CCharPointer c_string, CCharPointer c_outEncoding, SaxonCAPI.AllocatorCCharFn alloc) throws net.sf.saxon.s9api.SaxonApiException, UnsupportedEncodingException {
            String outEncoding = "utf-8";
            if (!c_outEncoding.isNull()) {
                outEncoding = CTypeConversion.toJavaString(c_outEncoding);
            }

            String str = toJavaString(c_string, c_outEncoding);
            if(SaxonCAPI.debug) {
                System.err.println("Java DEBUG string before encoding (from: "+outEncoding+ ") = " + str);
            }
            ByteArrayOutputStream stream = new ByteArrayOutputStream();
            stream.writeBytes(str.toString().getBytes(outEncoding));
            CCharPointer a = SaxonCAPI.stringToNative(alloc, stream);
            return a;
    }


    /**
     * Parse XML document given as a file name and return reference to XdmNode in the ObjectHandle pool. Also accept the SchemaValidator to validate document
     *
     * @param thread  - The Graalvm IsolateThread
     * @param ccwd         - The current working directory for parsing the XML file
     * @param builderRef - The long value pointing to the DocumentBuilder object held in the ObjectHandle pool
     * @param validatorRef - The long value pointing to the Schema Validator object held in the ObjectHandle pool
     * @param cfilename - The file name to the XML document
     * @return XdmNode object for the XML file
     * @throws SaxonApiException   if filename cannot be read
     */
    @CEntryPoint(name = "j_main_parseXmlFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_main_parseXmlFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle procRef, ObjectHandle builderRef, ObjectHandle validatorRef, CCharPointer cfilename) throws SaxonApiException {
        DocumentBuilder builder = null;
        Processor proci = null;
        String cwd = null;
        String filename = null;

        try {

            if (procRef == WordFactory.nullPointer()) {
                throw new SaxonApiException("Processor pointer is null");
            } else {
                proci = objectHandles.get(procRef);
                if (proci == null) {
                    throw new SaxonApiException("Processor object is null");
                }
            }

            if (builderRef == WordFactory.nullPointer()) {

                builder = proci.newDocumentBuilder();
            } else {
                builder = objectHandles.get(builderRef);
            }
            if (ccwd != WordFactory.nullPointer()) {
                cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
            }
            if (cfilename == WordFactory.nullPointer()) {
                throw new SaxonApiException("The XML file name is null");
            } else {
                filename = SaxonCAPI.toJavaString(cfilename, WordFactory.nullPointer());
            }

            Source source = resolveFileToSource(proci, cwd, filename, ResourceRequest.XML_NATURE);
            if (validatorRef != WordFactory.nullPointer()) {
                SchemaValidatorForCpp validatorForCpp = objectHandles.get(validatorRef);

                builder.setSchemaValidator(validatorForCpp.getSchemaManager().newSchemaValidator());
            }
            XdmNode node = builder.build(source);

            ObjectHandle handle = createHandle(node);
            return handle.rawValue();
        } catch (SaxonApiException ex) {
            if (debug) {
                System.err.println("DEBUG: " + ex.getStackTrace());
            }
            throw ex;
        }
    }

    /**
     * Parse XML document given as a file name and return XdmNode object. Also accept the SchemaValidator to validate document
     * @param cwd  The current working directory for parsing the XML file
     * @param builder  - The DocumentBuilder object to parse the XML document. If null then the Processor creates the DocumentBuilder
     * @param proci   - The Processor object to build the DocumentBuilder if needed
     * @param validator  - The SchemaValidator to validate the XML document if required
     * @param filename  - the file name of the XML document to parse
     * @return XdmNode object
     * @throws SaxonApiException   if parse error occurs
     */
    public static XdmNode main_parseXmlFile(String cwd, DocumentBuilder builder, Processor proci, SchemaValidator validator, String filename) throws SaxonApiException {

        try {

            if (builder == null) {

                builder = proci.newDocumentBuilder();
            }

            if (filename == null) {
                throw new SaxonApiException("The XML file name is null");
            }

            Source source = resolveFileToSource(proci, cwd, filename, ResourceRequest.XML_NATURE);
            if (validator != null) {
                builder.setSchemaValidator(validator);
            }
            XdmNode node = builder.build(source);

            return node;
        } catch (SaxonApiException ex) {
            throw ex;
        }
    }


    /**
     * Resolve the file with the cwd
     * deprecated method
     *
     * @param processor
     * @param cwd       - Current working directory
     * @param filename  -
     * @param nature
     * @return File object
     * @throws SaxonApiException if error in resolving file
     */
    public static File resolveFile(Processor processor, String cwd, String filename, String nature) throws SaxonApiException {
        String pathStr = null;
        try {
            pathStr = (new URI(resolveFileToSource(processor, cwd, filename, nature).getSystemId())).getPath();
        } catch (URISyntaxException e) {
            throw  new SaxonApiException(e);
        }

        if(SaxonCAPI.debug) {
            System.err.println("DEBUG resolveFile path ="+pathStr);
        }
        return new File(pathStr);

    }

    public static String escapeURIPathParam(String path) {
        try {
             return new URI(null, null, path, null).toASCIIString();
         } catch (URISyntaxException e) {
             // do some error handling
         }
         return "";
     }

     private static char toHex(int ch) {
      return (char) (ch < 10 ? '0' + ch : 'A' + ch - 10);
     }

     private static boolean isUnsafe(char ch) {
      if (ch > 128 || ch < 0)
       return true;
      return " %$&+,/:;=?@<>#%".indexOf(ch) >= 0;
     }


    /**
     * Resolve file name. Returns the file as a Source object
     *
     * @param processor
     * @param cwd       - Current working directory
     * @param filename - the file name
     * @param nature  - the nature of the file
     * @return
     * @throws SaxonApiException if error occurs when parsing file
     */
    public static Source resolveFileToSource(Processor processor, String cwd, String filename, String nature) throws SaxonApiException {
        return PathHelper.resolveToSource(processor.getUnderlyingConfiguration(), cwd, filename, nature);
    }


    /**
     * Resolve file name. Returns the file as a URI object
     *
     * @param processor  - processor object
     * @param cwd       - Current working directory
     * @param filename - the file name
     * @return
     * @throws SaxonApiException if error occurs when parsing file
     */
    public static URI resolveFileToURI(Processor processor, String cwd, String filename) throws SaxonApiException {
        return PathHelper.resolveToURI(processor.getUnderlyingConfiguration(), cwd, filename);
    }

    /**
     * Resolve the output file and wrap it into a Serializer for use in XQuery and XSLT processors
     *
     * @param processor - The same processor used in XQuery or XSLT engine must be used. Otherwise errors might occur
     * @param cwd       - Current working directory
     * @param outfile   - the output filename where result will be stored
     * @param nature
     * @return Serializer
     * @throws SaxonApiException   if file name is null or cannot resolve file
     */
    public static Serializer resolveOutputFile(Processor processor, String cwd, String outfile, String nature) throws SaxonApiException {
        Serializer serializer = null;
        File file = null;
        try {
            file = resolveFile(processor, cwd, outfile, nature);
        } catch (Exception ex) {
            throw new SaxonApiException(ex.getMessage());

        }
        if (file == null) {
            throw new SaxonApiException("Output file is null");
        }
        serializer = processor.newSerializer(file);
        return serializer;
    }

    public static void testj() throws SaxonApiException {
        SaxonCAPI.debug  = true;


        //Source result = SaxonCAPI.resolveFileToSource(processor, "/Users/ond1/work/development/git/saxon-dev/saxondev/build/releases/libsaxon-EEC-mac-x86_64-v12.3/samples/cppTests", "../php/xsl/foo.xsl", ResourceRequest.XSLT_NATURE);
        String input = "home/ond1/file.txt";
        Path path = Paths.get("/", input).normalize();
        URI uri = Paths.get(input).toUri();


        //(new File(input)).toURI();

        System.err.println("Path = " + path);
        System.err.println("URI = " + uri.toString());
        //Source source = SaxonCAPI.resolveFileToSource(processor, "/Users/ond1/work/development/git/saxon-dev/saxondev/build/releases/libsaxon-EEC-mac-x86_64-v12.1/samples/cppTests/", "../data/test.xsl", ResourceRequest.XSLT_NATURE);

        //XsltCompiler compiler = processor.newXsltCompiler();
        //XsltPackage pack = compiler.compilePackage(source);
        //File file = SaxonCAPI.resolveFile(processor, "/Users/ond1/work/development/git/saxon-dev/saxondev/build/releases/libsaxon-EEC-mac-x86_64-v12.1/samples/cppTests", "test.sef", ResourceRequest.XSLT_NATURE);
        //pack.save(file);

    }

    /**
     * Test code
     * @throws SaxonApiException if error occurs
     */
   /*public static void testj() throws SaxonApiException {
        SaxonCAPI.debug  = true;
        Processor processor = new Processor(false);
        //processor.setConfigurationProperty("http://saxon.sf.net/feature/allowedProtocols", "http,https");
        //processor.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "http,https");
        //Source source = SaxonCAPI.resolveFileToSource(processor, "file:///Users/ond1/work/", "test.sef", ResourceRequest.XSLT_NATURE);

       CCharPointer c_str = CTypeConversion.toCString("<doc><e>تيست</e></doc>").get();
       CCharPointer c_encoding = CTypeConversion.toCString("UTF-8").get();
       String result = SaxonCAPI.toJavaString(c_str, c_encoding);
       System.out.println(result);
    }   */



    public static void main(String [] args) throws SaxonApiException, URISyntaxException {
          SaxonCAPI.testj();

    }

}
