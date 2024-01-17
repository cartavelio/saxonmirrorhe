////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import com.saxonica.functions.extfn.cpp.CPPFunctionSet;
import com.saxonica.functions.extfn.cpp.PHPFunctionSet;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.AugmentedSource;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.s9api.*;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.*;

import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.word.WordFactory;

class Encoding {
    private String encoding;

    public Encoding(String e) {
        this.encoding = e;
    }

    public Encoding() {
        this.encoding = null;
    }

    public String getEncoding() {
        return encoding;
    }

    public void setEncoding(String encoding) {
        this.encoding = encoding;
    }
};


/**
 * * This class is to use with SaxonC on C++
 */
public class Xslt30Processor extends SaxonCAPI {

     



    /**
     * Constructor to initialise XsltProcessor with processor and license flag
     *
     * @param proc - s9api processor
     */
    public Xslt30Processor(Processor proc) {
        super(proc);
        Configuration config = processor.getUnderlyingConfiguration();
        schemaAware = config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT);
        if (debug) {
            System.err.println("Xslt30Processor constructor1(proc, l), Processor: " + System.identityHashCode(proc));
        }
//#if EE==true || PE==true
        if (config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            if (debug) {
                System.err.println("Xslt30Processor1-1(l), Processor: function libraries added");
            }
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif
    }


    /**
     * default Constructor to initialise XsltProcessor. Assume no license file available
     */
    public Xslt30Processor() {
        super();
        if (debug) {
            System.err.println("Xslt30Processor constructor2(), Processor: " + System.identityHashCode(processor));
        }
    }


    /**
     * Constructor to initialise XsltProcessor with license flag
     *
     * @param license - flag indicating presence of license file
     */
    public Xslt30Processor(boolean license) {
        processor = new Processor(license);
        Configuration config = processor.getUnderlyingConfiguration();
        schemaAware = config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT);
        if (debug) {
            System.err.println("XsltProcessor3(l), Processor: " + System.identityHashCode(processor));
        }
//#if EE==true || PE==true
        if (config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            if (debug) {
                System.err.println("XsltProcessor3-1(l), Processor: function libraries added");
            }
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif

    }


    /**
     * Create new object of this class
     *
     * @param thread -  Graalvm runtime data structure for a thread
     * @return long value referencing the created Xslt30Processor in the ObjectHandles pool
     */
    @CEntryPoint(name = "createXslt30Processor", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createXslt30Processor(IsolateThread thread) {
            ObjectHandle handle = SaxonCAPI.createHandle(new net.sf.saxon.option.cpp.Xslt30Processor(false));
            return handle.rawValue();
    }

    /**
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param processorRef  - long value referencing the Processor object in the ObjectHandles pool
     * @return  long value referencing the created Xslt30Processor i nthe ObjectHandles pool
     */
    @CEntryPoint(name = "createXslt30WithProcessor", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createXslt30Processor1(IsolateThread thread, ObjectHandle processorRef) {
            Processor processor = objectHandles.get(processorRef);
            if(processor == null) {
                return 0;
            }
            ObjectHandle handle = SaxonCAPI.createHandle(Xslt30Processor.newInstance(processor));
            return handle.rawValue();
    }

    /**
     * Create new SaxonC Xslt30Processor object from a Java Processor
     * @param proc  - underlying Processor object
     * @return  Xslt30Processor
     */
    public static Xslt30Processor newInstance(Processor proc) {
        return new Xslt30Processor(proc);
    }


    /**
     * Compile package from source file and save resulting sef to file store.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd   - - Pointer to a C char array for the current working directory
     * @param xsl  - File name of the stylesheet
     * @param outFilename  - the file to which the compiled package should be saved
     * @param processorDataAccumulatorRef  - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of static parameter names to values
     * @return  long value for the status of compiling and the saving of the stylesheet
     * @throws SaxonApiException if failure to compile or create the SEF stylesheet. Also if Xslt30Processor or stylesheet is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_compileFromFileAndSave", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromFileAndSave(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer xsl, CCharPointer outFilename, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        String cwd = null;

        if(xsl == WordFactory.nullPointer()) {
            throw new SaxonApiException("Stylesheet file is NULL");
        }

        if(outFilename == WordFactory.nullPointer()) {
            throw new SaxonApiException("Output file name is NULL");
        }

        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        Source source = resolveFileToSource(processor, cwd, SaxonCAPI.toJavaString(xsl, WordFactory.nullPointer()), ResourceRequest.XSLT_NATURE);

        XsltPackage pack = compilePackage(xslt30ProcessorRef, ccwd, source, processorDataAccumulatorRef);
        File file = resolveFile(processor, cwd, SaxonCAPI.toJavaString(outFilename, WordFactory.nullPointer()), ResourceRequest.XSLT_NATURE);
        pack.save(file);
        return SaxonCAPI.STATUS_OK;

    }


    /**
     * Compile package from source file and save resulting sef to file store.
     * @param thread  - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd  - Pointer to a C char array for the current working directory
     * @param xsl
     * @param outFilename - pointer to a C char array. The file to which the compiled package should be saved
     * @param processorDataAccumulatorRef   - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of static parameter names to values
     * @return
     * @throws SaxonApiException if error in the compilation. Also if Xslt30Processor or stylesheet is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_compileFromFileAndImport", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromFileAndImport(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer xsl, CCharPointer outFilename, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }

        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        String cwd = null;

        if(xsl == WordFactory.nullPointer()) {
            throw new SaxonApiException("Stylesheet file is NULL");
        }


        if (ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        Source source = resolveFileToSource(processor, cwd, SaxonCAPI.toJavaString(xsl, WordFactory.nullPointer()), ResourceRequest.XSLT_NATURE);

        XsltPackage pack = compilePackage(xslt30ProcessorRef, ccwd, source, processorDataAccumulatorRef);
        ObjectHandle handle = SaxonCAPI.createHandle(pack);
        return handle.rawValue();

    }

    private static XsltPackage compilePackage(ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, Source xslSource, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        XsltCompiler compiler = processor.newXsltCompiler();
        String cwd = null;
        if (ccwd.isNonNull()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, null);


        compiler.setErrorList(SaxonCException.errorList);


        return compiler.compilePackage(xslSource);
    }



    /**
     * Produce a representation of the compiled stylesheet and save to file, in XML form, suitable for
     * distribution and reloading.
     * @param executableRef
     * @param filename
     */
    @CEntryPoint(name = "j_save", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long save(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer filename) throws SaxonApiException {
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        if(filename == WordFactory.nullPointer()) {
            throw new SaxonApiException("XSL filename is NULL");
        }
        XsltExecutable executable = objectHandles.get(executableRef);
        File file = SaxonCAPI.resolveFile(executable.getProcessor(), cwd, SaxonCAPI.toJavaString(filename, WordFactory.nullPointer()), null);
        FileOutputStream stream = null;
        try {
            stream = new FileOutputStream(file);
        } catch (FileNotFoundException e) {
            throw new SaxonApiException(e.getMessage());
        }

        executable.export(stream);
        return SaxonCAPI.STATUS_OK;
    }


    /**
     * Compile package from string and save resulting sef to file store.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd - Pointer to a C char array for the current working directory
     * @param cstr - Pointer to a C char array for the file name of the stylesheet
     * @param cfilename  - Pointer to C char array for the file to which the compiled package should be saved
     * @param processorDataAccumulatorRef  - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of static parameter names to values
     * @return  Status of running this method. Success=1, Failure=0 or exception thrown = -2
     * @throws SaxonApiException  If there is a failure in the transformation, saving of file. Also if Xslt30Processor or stylesheet is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_compileFromStringAndSave", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromStringAndSave(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer cstr, CCharPointer cfilename, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        if(cstr == WordFactory.nullPointer()) {
            throw new SaxonApiException("Stylesheet string is null");
        }

        if(cfilename == WordFactory.nullPointer()) {
            throw new SaxonApiException("Output filename is null");
        }


        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        XsltCompiler compiler = processor.newXsltCompiler();
        Encoding encoding = new Encoding();
        setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, encoding);

        String str = SaxonCAPI.toJavaString2(cstr, encoding.getEncoding());
        String filename = SaxonCAPI.toJavaString(cfilename, WordFactory.nullPointer());
        if (processor == null || str == null || filename == null) {
            throw new SaxonApiException("Stylesheet string or output filename is null");
        }

        compiler.setErrorList(SaxonCException.errorList);
        XsltPackage pack = null;
        try {
            //StreamSource stream = new StreamSource(sr);
            pack = compiler.compilePackage(new StreamSource(new StringReader(str)));
        } catch(Exception ex) {
            throw new SaxonApiException(ex.getMessage());
        }
        if (pack != null) {

            File file = resolveFile(processor, cwd, filename, ResourceRequest.XSLT_NATURE);

            pack.save(file);
        }
        return SaxonCAPI.STATUS_OK;
    }


    /**
     * Compile package from Xdm node and save resulting sef to file store.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef  - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param xdmNodeRef  - long value referencing the XdmNode object in the ObjectHandler pool. Stylesheet given as an XdmNodeobject
     * @param ccwd      - Pointer to C char array representing the current working directory
     * @param cfilename - Pointer to the C char array representing the file to which the compiled package should be saved
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of static parameter names to values
     * @return Status of running this method. Success=1, Failure=0 or exception thrown = -2
     * @throws SaxonApiException if the Xslt30Processor is null or if there is a compile error
     */
    @CEntryPoint(name = "j_compileFromXdmNodeAndSave", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromXdmNodeAndSave(IsolateThread thread, ObjectHandle xslt30ProcessorRef, ObjectHandle xdmNodeRef, CCharPointer ccwd, CCharPointer cfilename, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        if(processor == null) {
            //TODO throw exception
            return SaxonCAPI.STATUS_FAIL;
        }
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        XsltCompiler compiler = processor.newXsltCompiler();
        setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, null);
        XdmNode node = objectHandles.get(xdmNodeRef);
        if (node == null) {
            SaxonApiException ex = new SaxonApiException("Failed to create Stylesheet from XdmNode");
            throw ex;
        }
        compiler.setErrorList(SaxonCException.errorList);
        XsltPackage pack = compiler.compilePackage(node.asSource());

        String filename = SaxonCAPI.toJavaString(cfilename, WordFactory.nullPointer());

        if(pack != null) {
            File file = resolveFile(processor, cwd, filename, ResourceRequest.XSLT_NATURE);
            pack.save(file);
        }

        return SaxonCAPI.STATUS_OK;

    }

    /**
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef  - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd  - Pointer to a C char array for the current working directory
     * @param filename   - pointer to a C char array. The stylesheet file name
     * @param cjit   - int value which is converted to boolean. Flag for Just in Time compilation
     * @param processorDataAccumulatorRef  - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of static parameter names to values
     * @return
     * @throws SaxonApiException if error in the compilation. Also if Xslt30Processor or stylesheet is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_compileFromAssociatedFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromAssociatedFile(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer filename, int cjit, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {


        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }

        try {
            Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);

            Processor processor = xslt30Processor.getProcessor();
            XsltCompiler compiler = processor.newXsltCompiler();
            String cwd = null;
            if(ccwd != WordFactory.nullPointer()) {
                cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
            }
            setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, null);
            boolean jit = CTypeConversion.toBoolean(cjit);
            if (jit) {
                compiler.setJustInTimeCompilation(jit);
            }

            if(filename == WordFactory.nullPointer()) {
                throw new SaxonApiException("XSL filename is NULL");
            }
            XsltExecutable executable = null;
            /*if (packagesToLoad.size() > 0) {
                compilePackages(compiler);
            } TODO */
            Source source = resolveFileToSource(processor, cwd, SaxonCAPI.toJavaString(filename, WordFactory.nullPointer()), ResourceRequest.XML_NATURE);

            //compiler.setErrorListener(errorListener);
            compiler.setSchemaAware(xslt30Processor.schemaAware);
            compiler.setErrorList(SaxonCException.errorList);
            executable = compiler.compile(compiler.getAssociatedStylesheet(source, null, null, null));
            ObjectHandle handle = SaxonCAPI.createHandle(executable);

            return handle.rawValue();
        } catch (SaxonApiException ex) {
            throw ex;
        }
    }


    /**
     *
     *
     * @param cwd      - current working directory
     * @param filename - File name of the stylsheet
     * @return XsltExecutable
     */

    /**
     * Compile the stylesheet from file  for use later
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd  - Pointer to a C char array for the current working directory
     * @param cfilename   - Pointer to C char array for the stylesheet file name
     * @param cjit    - int value which is converted to boolean. Flag for Just in Time compilation
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of static parameter names to values
     * @return long value referencing the XsltExecutable created in this method.
     * @throws SaxonApiException  if error in the compilation. Also if Xslt30Processor or stylesheet is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_compileFromFile", exceptionHandler = SaxonCExceptionLongHandler.class)
   public static long compileFromFile(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer cfilename, int cjit, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        XsltCompiler compiler = processor.newXsltCompiler();
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, null);
        boolean jit = CTypeConversion.toBoolean(cjit);
        if (jit) {
            compiler.setJustInTimeCompilation(jit);
        }
        XsltExecutable executable = null;

        if(cfilename == WordFactory.nullPointer()) {
            throw new SaxonApiException("XSL filename is NULL");
        }
        Source source = resolveFileToSource(processor, cwd, SaxonCAPI.toJavaString(cfilename, WordFactory.nullPointer()), ResourceRequest.XSLT_NATURE);

        //compiler.setErrorListener(errorListener);
        compiler.setSchemaAware(xslt30Processor.schemaAware);
        compiler.setErrorList(SaxonCException.errorList);
        executable = compiler.compile(source);
        ObjectHandle handle = SaxonCAPI.createHandle(executable);

        return handle.rawValue();

    }

    /**
     * Compile the stylesheet from string  for use later
     *
     * @param thread - Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd  - Pointer to the C char array. Current working directory
     * @param stylesheet - Pointer to the C char array. Stylesheet as a string
     * @param cjit  - int value which is converted to boolean. Flag for Just in Time compilation
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied static parameter names to values
     * @return  long value
     * @throws SaxonApiException if error in the compilation. Also if Xslt30Processor or stylesheet is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_compileFromString", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromString(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer stylesheet, int cjit , ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        XsltCompiler compiler = xslt30Processor.getProcessor().newXsltCompiler();
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        Encoding encoding = new Encoding();

        setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, encoding);

        boolean jit = CTypeConversion.toBoolean(cjit);
        if (jit) {
            compiler.setJustInTimeCompilation(jit);
        }
        Source source;

        if(stylesheet == WordFactory.nullPointer()) {
            throw new SaxonApiException("XSL filename is NULL");
        }
        StringReader reader = new StringReader(SaxonCAPI.toJavaString2(stylesheet, encoding.getEncoding()));
        compiler.setSchemaAware(xslt30Processor.schemaAware);
        source = new StreamSource(reader);
        if (cwd != null && cwd.length() > 0) {
            if (!cwd.endsWith("/")) {
                cwd = cwd.concat("/");
            }
            source.setSystemId(cwd);

        }
        compiler.setErrorList(SaxonCException.errorList);
        XsltExecutable executable = compiler.compile(source);
        ObjectHandle handle = SaxonCAPI.createHandle(executable);

        return handle.rawValue();

    }

    /**
     * Compile the stylesheet from string  for use later
     *
     * @param thread- Graalvm runtime data structure for a thread
     * @param xslt30ProcessorRef - long value referencing the Xslt30Processor in the ObjectHandler pool. Cannot be null
     * @param ccwd    - pointer to the C char array for the current working directory. User supplies this as base URI. Accept null
     * @param xdmNodeRef - long value referencing the XdmNode object in the ObjectHandler pool. Stylesheet given as an XdmNodeobject
     * @param cjit  - int value representing boolean for the passing of the JIT flag
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied static parameters
     * @return   long value referencing the created XsltExecutable object held in the ObjectHandles pool
     * @throws SaxonApiException if the XdmNodeRef or the xslt30ProcessorRef is null or if there is a failure in the compile phase. The SaxonCExceptionLongHandler handles the exception and returns -1.
     */
    @CEntryPoint(name = "j_compileFromXdmNode", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long compileFromXdmNode(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd,  ObjectHandle xdmNodeRef, int cjit , ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        if(xslt30ProcessorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("The Xslt30Processor is null");
        }
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        XsltCompiler compiler = xslt30Processor.getProcessor().newXsltCompiler();

        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        setStaticParametersFromArray(compiler, cwd, processorDataAccumulatorRef, null);
        boolean jit = CTypeConversion.toBoolean(cjit);
        if (jit) {
            compiler.setJustInTimeCompilation(jit);
        }
        if(xdmNodeRef == WordFactory.nullPointer()) {
             throw new SaxonApiException("Failed to create Stylesheet from XdmNode");
        }
        XdmNode node = objectHandles.get(xdmNodeRef);

        //compiler.setErrorListener(errorListener);
        /* if (packagesToLoad.size() > 0) {
            compilePackages(compiler);
        }  */
        compiler.setErrorList(SaxonCException.errorList);
        XsltExecutable executable = compiler.compile(node.asSource());
        ObjectHandle handle = SaxonCAPI.createHandle(executable);

        return handle.rawValue();


    }

    /**
     * Utility method to apply the static parameters to the XsltCompiler
     *
     * @param compiler - the XsltCompiler where we set the static parameter data
     * @param cwd  - current working directory. User supplies this as base URI. Accept null
     * @param processorDataAccumulatorRef- long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @param encoding  -  Set encoding data coming from the C++ API if available
     * @throws SaxonApiException  if the ProcessorDataAccumulator is null or error in applying static parameter
     */
    private static void setStaticParametersFromArray(XsltCompiler compiler, String cwd, ObjectHandle processorDataAccumulatorRef, Encoding encoding) throws SaxonApiException {
        ProcessorDataAccumulator processorDataAccumulator = null;
        Processor processor = compiler.getProcessor();
        if(processorDataAccumulatorRef != WordFactory.nullPointer()) {
            processorDataAccumulator = objectHandles.get(processorDataAccumulatorRef);
        }  else {
            if(debug) {
                System.err.println("DEBUG: setStaticParametersFromArray -  processorDataAccumulator is null");
            }
            return;
        }

        String[] sparams = null;
        Object[] values = null;
        if(processorDataAccumulator != null) {
            sparams = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        if (sparams != null && values != null) {
            for (int i = 0; i < sparams.length; i++) {
                if (sparams[i].startsWith("sparam:")) {
                    //static parameters
                    String paramName = sparams[i].substring(7);
                    Object value = values[i];
                    XdmValue valueForCpp = null;
                    QName qname = null;
                    try {
                        qname = QName.fromClarkName(paramName);
                    } catch(IllegalArgumentException ex) {
                        throw new SaxonApiException(ex);
                    }
                    valueForCpp = convertObjectToXdmValue(values[i]);
                    compiler.setParameter(qname, valueForCpp);
                } else if (sparams[i].equals("-target")) {
                    String value = (String)values[i];
                    compiler.setTargetEdition(value);
                } else if(sparams[i].equals("-relocate")) {
                    String value = (String)values[i];
                    if(value.equals("1")) {
                        compiler.setRelocatable(true);
                    } else {
                        compiler.setRelocatable(false);
                    }
                } else if(sparams[i].equals("-lang")) {
                    String value = (String)values[i];
                    compiler.setXsltLanguageVersion(value);
                } else if(sparams[i].equals("-fastCompile")) {
                    String value = (String)values[i];
                    if(value.equals("1")) {
                        compiler.setFastCompilation(true);
                    }
                } else if(sparams[i].equals("importPack:")) {
                    ProcessorDataAccumulator packageValue = (ProcessorDataAccumulator) values[i];

                    String [] packageFileNames = packageValue.getParameterNames();

                    for(int z = 0 ; z < packageFileNames.length; z++) {
                        Source source = resolveFileToSource(processor, cwd, packageFileNames[z], ResourceRequest.XSLT_NATURE);
                        XsltPackage pack = compiler.loadLibraryPackage(source);
                        compiler.importPackage(pack);
                    }



                } else if(sparams[i].equals("!input-encoding")) {
                    String value = (String)values[i];
                    encoding.setEncoding(value);
                }
            }

        }
    }


    /**
     *  Utility method to create an Xslt30Transformer from a XsltExecutable after applyign static parameters and creating a stylesheet if the XsltExecutable is null
     *
     * @param cwd - current working directory. User supplies this as base URI. Accept null
     * @param executable  - The XsltExecutable. Accept null
     * @param stylesheet  - The lexical string representation of the stylesheet, which is used by the compiler if the XsltExecutable is null.
     * @param staticParameters  map of the static parameters to apply against the XsltCompiler
     * @return  The Xslt30Transformer object
     * @throws SaxonApiException  if the XsltExecutable and stylesheet string are both null. Also if failure to compile stylesheet
     */
    public Xslt30Transformer getXslt30Transformer(String cwd, XsltExecutable executable, String stylesheet, Map<QName, XdmValue> staticParameters) throws SaxonApiException {
        Xslt30Transformer transformer = null;
        if (stylesheet == null && executable != null) {
            if(debug){
                System.err.println("Reusing executable");
            }
            transformer = executable.load30();
            return transformer;
        } else {
            if (stylesheet == null) {
                throw new SaxonApiException("Stylesheet file is null");
            }
            XsltCompiler compiler = processor.newXsltCompiler();

            Source source = resolveFileToSource(processor, cwd, stylesheet, ResourceRequest.XSLT_NATURE);
            //compiler.setErrorListener(errorListener);

            compiler.setSchemaAware(schemaAware);

            /*if ( packagesToLoad.size() > 0) {
                compilePackages(compiler);
            } */
            if (!staticParameters.isEmpty()) {
                for (Map.Entry<QName, XdmValue> entry : staticParameters.entrySet()) {
                    compiler.setParameter(entry.getKey(), entry.getValue());
                }
            }
            transformer = compiler.compile(source).load30();
            return transformer;
        }

    }


    /**
     * Invoke the stylesheet by applying templates to a supplied Source document, sending the results to
     * a document node). The invocation uses any initial mode set using setInitialMode(QName), via a C++ API method,
     * and any template parameters passed as the argument ProcessorDataAccumulator.
     *
     * <p>Note that this method does <b>not</b> cause the supplied Source document to become the global
     * context item (that is, to be available as the value of "." when evaluating global variables. If
     * a global context item is required, this must be set separately using setGlobalContextItem(XdmItem) via the C++ API.
     * Alternatively, use one of the transform methods.</p>
     *
     * @param thread - Graalvm runtime data structure for a thread
     * @param ccwd - current working directory. User supplies this as base URI. Accept null
     * @param xsltExecutableRef - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param sourceObjRef - long value referencing a Source object in th ObjectHandles pool. Multiple types of source are accepted and checked with this argument.
     *                     Source object can be an Xdm object, string or an array of Xdm values given as a ProcessorDataAccumulator.
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return long value referencing the results as an XdmValue object which is held in the ObjectHandles pool to previous GC.
     * Caller must remove object from ObjectHandles pool when no longer required
     * @throws SaxonApiException if error in the transformation. Also if XsltExecutable is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_applyTemplatesReturningValue", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long applyTemplatesReturningValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle xsltExecutableRef, ObjectHandle sourceObjRef , ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(xsltExecutableRef);
        
        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        if(executable == null) {
            if(SaxonCAPI.debug) {
                System.err.println("Error: executable is null");
            }
            throw new SaxonApiException("executable is null");
        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        String cwd= SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        try {
            applyXsltTransformerProperties(cwd, processor, transformer, null, props, paramsMap, globalParameters, initialTemplateParameters);
        } catch(Exception ex) {
            throw new SaxonApiException(ex);

        }

        Destination xdmResult;
        boolean returnXdmValue = false;
        if (paramsMap.containsKey("outvalue")) {
            //Set if the return type of callTemplate, applyTemplate and transform methods is to return XdmValue,
            //otherwise return XdmNode object with root Document node
            Object valuei = paramsMap.get("outvalue");
            if (valuei instanceof Boolean) {
                returnXdmValue = (boolean) valuei;
            } else if (valuei instanceof String && valuei.equals("yes") || valuei.equals("true")) {
                returnXdmValue = true;
            }
        }

        if (returnXdmValue) {
            xdmResult = new RawDestination();
        } else {
            xdmResult = new XdmDestination();
        }
        if(sourceObjRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Source Object is NULL");
        }
        Object sourceObji = objectHandles.get(sourceObjRef);
        Object sourceObj = null;
        if(sourceObji instanceof ProcessorDataAccumulator) {
             sourceObj = ((ProcessorDataAccumulator)sourceObji).getValues();
        }  else {
            sourceObj = sourceObji;
        }
        if (sourceObj instanceof String) {
            Source source = resolveFileToSource(processor, cwd, (String) sourceObj, ResourceRequest.XML_NATURE);
            if (paramsMap.containsKey("dtd")) {
                AugmentedSource asource = AugmentedSource.makeAugmentedSource(source);
                String option = (String) paramsMap.get("dtd");
                if (!option.isEmpty()) {
                    asource.setDTDValidationMode(Validation.getCode(option));
                }
                source = asource;
            }

            transformer.applyTemplates(source, xdmResult);
        } else if (sourceObj instanceof XdmValue) {
            XdmValue selection = (XdmValue) sourceObj;

            transformer.applyTemplates(selection, xdmResult);
        } else if (sourceObj instanceof Object[]) {
            Object [] valueObj = (Object[]) sourceObj;
            if (valueObj.length == 0) {
                throw new SaxonApiException("Source object found is an empty array");
            }
            List<XdmItem> itemList = new ArrayList<>(valueObj.length);
            for(int i=0; i < valueObj.length; i++) {
                 itemList.add((XdmItem) valueObj[i]);
            }
            XdmValue valueSeq;
            valueSeq = new XdmValue(itemList);
            transformer.applyTemplates(valueSeq, xdmResult);
        } else {
            if(SaxonCAPI.debug) {
                
                if(sourceObj == null) {
                    System.err.println("sourceObj is null ");
                } else {
                    System.err.println("sourceObj Type = " + sourceObj.getClass().getName());
                }
            }
            throw new SaxonApiException("Source object is neither a String value or an XdmValue object");
        }


        XdmValue value = getXdmValue(xdmResult, paramsMap);

        ObjectHandle handle = SaxonCAPI.createHandle(value);
        return handle.rawValue();
    }

    private static XdmValue getXdmValue(Destination xdmResult, Map<String, Object> paramsMap) {
        boolean returnXdmValue = false;
        if (paramsMap.containsKey("outvalue")) {
            //Set if the return type of callTemplate, applyTemplate and transform methods is to return XdmValue,
            //otherwise return XdmNode object with root Document node
            Object valuei = paramsMap.get("outvalue");
            if (valuei instanceof Boolean) {
                returnXdmValue = (boolean) valuei;
            } else if (valuei instanceof String && valuei.equals("yes") || valuei.equals("true")) {
                returnXdmValue = true;
            }
        }
        if (returnXdmValue) {
            return ((RawDestination) xdmResult).getXdmValue();
        } else {
            return ((XdmDestination) xdmResult).getXdmNode();
        }
    }

    /**
     * Invoke the stylesheet by applying templates to a supplied Source document, sending the results to
     * a file. The invocation uses any initial mode set using setInitialMode(QName), via a C++ API method,
     * and any template parameters passed as the argument ProcessorDataAccumulator.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param ccwd    - Pointer to a C char array for the supplied current working directory or given base URI by the caller.
     * @param executableRef - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param sourceObjRef - long value referencing a Source object in th ObjectHandles pool. Multiple types of source are accepted and checked with this argument.
     *                     Source object can be an Xdm object, string or an array of Xdm values given as a ProcessorDataAccumulator.
     *                     Used as source document to the Xslt30Executable
     * @param coutFilename - Pointer to a C char array. The file name to save the result of the apply template transformation
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return long value representing the status of the transformation. Success=1, failure = -1 and exception=-2
     * @throws SaxonApiException if error in the transformation. If XsltExecutable is null. If error with the source object. Also if there is a failure to write to file
     */
    @CEntryPoint(name = "j_applyTemplatesReturningFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long applyTemplatesReturningFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, ObjectHandle sourceObjRef, CCharPointer coutFilename , ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        XsltExecutable executable = objectHandles.get(executableRef);


        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);

        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        Serializer serializer = null;
        String outFilename = SaxonCAPI.toJavaString(coutFilename, WordFactory.nullPointer());
        String cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        if (outFilename != null) {
            if (debug) {
                System.err.println("DEBUG: XSLT30TransformerForCpp param: " + outFilename);
            }
            serializer = resolveOutputFile(processor, cwd, outFilename, ResourceRequest.XML_NATURE);
        } else if (paramsMap.containsKey("o")) {
            String outfile = (String) paramsMap.get("o");
            serializer = resolveOutputFile(processor, cwd, outfile, ResourceRequest.XML_NATURE);
            if (debug) {
                System.err.println("DEBUG: XSLT30TransformerForCpp param: " + outfile);
            }
        } else if (serializer == null) {
            throw new SaxonApiException("Output file not set for this transformation");
        }

        try {
            applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);
        } catch(Exception ex) {
            throw new SaxonApiException(ex);

        }
        if(sourceObjRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Source object is null");
        }
        Object sourceObji = objectHandles.get(sourceObjRef);
        Object sourceObj = null;
        if(sourceObji instanceof ProcessorDataAccumulator) {
             sourceObj = ((ProcessorDataAccumulator)sourceObji).getValues();
        }  else {
            sourceObj = sourceObji;
        }
        if (sourceObj instanceof String) {

            Source source = resolveFileToSource(processor, cwd, (String) sourceObj, ResourceRequest.XML_NATURE);
            if (paramsMap.containsKey("dtd")) {
                AugmentedSource asource = AugmentedSource.makeAugmentedSource(source);
                String option = (String) paramsMap.get("dtd");
                if (!option.isEmpty()) {
                    asource.setDTDValidationMode(Validation.getCode(option));
                }
                source = asource;
            }
            transformer.applyTemplates(source, serializer);
        } else if (sourceObj instanceof XdmValue) {
            XdmValue selection = (XdmValue) sourceObj;
            transformer.applyTemplates(selection, serializer);
        } else if (sourceObj instanceof Object[]) {
            Object [] valueObj = (Object[]) sourceObj;
            if (valueObj.length == 0) {
                throw new SaxonApiException("Source object found is an empty array");
            }
            List<XdmItem> itemList = new ArrayList<>(valueObj.length);
            for(int i=0; i < valueObj.length; i++) {
                 itemList.add((XdmItem) valueObj[i]);
            }
            XdmValue valueSeq;
            valueSeq = new XdmValue(itemList);
            transformer.applyTemplates(valueSeq, serializer);
        } else {
            throw new SaxonApiException("Source object is not of type (String, XdmValue or XdmValue[])");
        }

         return SaxonCAPI.STATUS_OK;
    }

    /**
     * Invoke the stylesheet by applying templates to a supplied Source document, returning the results
     * as a string value. The invocation uses any initial mode set using setInitialMode(QName), via a C++ API method,
     * and any template parameters passed as the argument ProcessorDataAccumulator.
     *
     * <p>Note that this method does <b>not</b> cause the supplied Source document to become the global
     * context item (that is, to be available as the value of "." when evaluating global variables. If
     * a global context item is required, this must be set separately using setGlobalContextItem(XdmItem) via the C++ API.
     * Alternatively, use one of the transform methods.</p>
     *
     * @param thread - Graalvm runtime data structure for a thread
     * @param alloc - reference to the C++ allocator object
     * @param ccwd - Pointer to a C char array. current working directory. User supplies this as base URI. Accept null
     * @param executableRef - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param sourceObjRef - long value referencing a Source object in th ObjectHandles pool. Multiple types of source are accepted and checked with this argument.
     *                     Source object can be an Xdm object, string or an array of Xdm values given as a ProcessorDataAccumulator.
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return the result as a pointer to a C char array held in the C/C++ heap memory space. Caller must destroy char array when no longer required.
     * Caller must remove object from ObjectHandles pool when no longer required
     * @throws SaxonApiException if error in the transformation. If XsltExecutable is null. Also if error with the source object
     */
    @CEntryPoint(name = "j_applyTemplatesReturningString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer applyTemplatesReturningString(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, CCharPointer ccwd, ObjectHandle executableRef, ObjectHandle sourceObjRef , ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        XsltExecutable executable = objectHandles.get(executableRef);
        Object sourceObji = objectHandles.get(sourceObjRef);
        Object sourceObj = null;
        if(sourceObji instanceof ProcessorDataAccumulator) {
             sourceObj = ((ProcessorDataAccumulator)sourceObji).getValues();
        }  else {
            sourceObj = sourceObji;
        }

        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        if(executable == null) {
            throw new SaxonApiException("XsltExecutable is null");

        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        ByteArrayOutputStream bStream = new ByteArrayOutputStream();
        Serializer serializer = processor.newSerializer(bStream);

        try {
            applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);
        } catch(Exception ex) {
            throw new SaxonApiException(ex);

        }

        if (sourceObj instanceof String) {
            Source source = resolveFileToSource(processor, cwd, (String) sourceObj, ResourceRequest.XML_NATURE);
            if (paramsMap.containsKey("dtd")) {
                AugmentedSource asource = AugmentedSource.makeAugmentedSource(source);
                String option = (String) paramsMap.get("dtd");
                if (!option.isEmpty()) {
                    asource.setDTDValidationMode(Validation.getCode(option));
                }
                source = asource;
            }
            transformer.applyTemplates(source, serializer);
        } else if (sourceObj instanceof XdmValue) {
            XdmValue selection = (XdmValue) sourceObj;
            transformer.applyTemplates(selection, serializer);
        } else if (sourceObj instanceof Object[]) {
            Object [] valueObj = (Object[]) sourceObj;
            if (valueObj.length == 0) {
                throw new SaxonApiException("Source object found is an empty array");
            }
            List<XdmItem> itemList = new ArrayList<>(valueObj.length);
            for(int i=0; i < valueObj.length; i++) {
                 itemList.add((XdmItem) valueObj[i]);
            }
            XdmValue valueSeq;
            valueSeq = new XdmValue(itemList);
            transformer.applyTemplates(valueSeq, serializer);
        } else {
            throw new SaxonApiException("Source object is not of type (String, XdmValue or XdmValue[])");
        }
        String charsetFlag  = System.getenv("SAXONC_DEBUG_CHARSET_FLAG");
        /*if(charsetFlag != null && charsetFlag.equals("true")) {
            String charsetStr = serializer.getOutputProperty(Serializer.Property.ENCODING);
            if(charsetStr != null) {
                System.err.println("Charset used by the serializer = " + charsetStr);
            } else {
                System.err.println("JAVA DEBUG: Charset in the serializer is null");
            }
        } */
        CCharPointer a = SaxonCAPI.stringToNative(alloc, bStream);
        return a;
    }

    /**
     * Utility method to convert array of objects to array of XdmValues
     * @param arguments - Java Objects to convert
     * @return array of XdmValues. If length is empty then return empty array of XdmValus
     * @throws SaxonApiException  if the arguments ar not of type XdmValue
     */
    public static XdmValue[] getArguments(Object[] arguments) throws SaxonApiException {
        if (arguments != null && arguments.length > 0) {
            XdmValue[] values = new XdmValue[arguments.length];
            if (debug && arguments != null) {
                System.err.println("Xslt30Processor getArguments.length=" + arguments.length);
            }
            for (int i = 0; i < arguments.length; i++) {
                values[i] = convertObjectToXdmValue(arguments[i]);
                if (values[i] == null) {
                    throw new SaxonApiException("Argument is not of type xdmValue");
                }

            }
            return values;
        }
        return new XdmValue[]{};

    }

    /**
     * Call a public user-defined function in the stylesheet, wrapping the result in an XML document, and sending
     * this document to a specified file name
     *
     * @param thread - Graalvm runtime data structure for a thread
     * @param ccwd  - Pointer to the C char array representing the file to which the compiled package should be saved
     * @param executableRef  - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param ccFuncName  - Pointer to  C char array. The name of the function to be called
     * @param coutFilename - Pointer to a C char array. The file name to save the result of the apply template transformation
     * @param argumentsRef  - wrapped function arguments in a ProcessorDataAccumulator in the ObjectHandles pool. The values of the arguments to be supplied to the function. These
     *                            will be converted if necessary to the type as defined in the function signature, using
     *                             the function conversion rules.
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return   long value representing the status of the transformation. Success=1, failure = -1 and exception=-2
     * @throws SaxonApiException if error in the transformation. Also if XsltExecutable is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_callFunctionReturningFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long callFunctionReturningFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer ccFuncName, CCharPointer coutFilename , ObjectHandle argumentsRef, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String cFuncName = SaxonCAPI.toJavaString(ccFuncName, WordFactory.nullPointer());
        String outFilename = SaxonCAPI.toJavaString(coutFilename, WordFactory.nullPointer());
        XsltExecutable executable = objectHandles.get(executableRef);
        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = null;
        if(cFuncName.startsWith("Q{")) {
            qname = QName.fromEQName(cFuncName);
        } else {
            qname = QName.fromClarkName(cFuncName);
        }

        Map<String, Object> paramsMap =  setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        Serializer serializer = null;
        if (outFilename != null) {
            serializer = resolveOutputFile(processor, cwd, outFilename, ResourceRequest.XML_NATURE);
        } else if (paramsMap.containsKey("o")) {
            String outfile = (String) paramsMap.get("o");
            serializer = resolveOutputFile(processor, cwd, outfile, ResourceRequest.XML_NATURE);
        } else {
            throw new SaxonApiException("File name for CallFunction has not been set");
        }

        applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);

        ProcessorDataAccumulator argumentsAccumulator = null;
        if(argumentsRef != WordFactory.nullPointer()) {
            argumentsAccumulator = objectHandles.get(argumentsRef);
        }
        Object [] arguments = argumentsAccumulator.getValues();
        transformer.callFunction(qname, getArguments(arguments), serializer);
        return SaxonCAPI.STATUS_OK;
    }


    /**
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param ccwd  - Pointer to a C char array. current working directory. User supplies this as base URI. Accept null
     * @param executableRef - wrapped function arguments in a ProcessorDataAccumulator in the ObjectHandles pool. The values of the arguments to be supplied to the function. These
     *                        will be converted if necessary to the type as defined in the function signature, using
     *                         the function conversion rules.
     * @param ccFuncName    - Pointer to  C char array. The name of the function to be called
     * @param argumentsRef  - wrapped function arguments in a ProcessorDataAccumulator in the ObjectHandles pool. The values of the arguments to be supplied to the function. These
     *                       will be converted if necessary to the type as defined in the function signature, using
     *                       the function conversion rules.
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return long value referencing the results as an XdmValue object which is held in the ObjectHandles pool to previous GC.
     *      Caller must remove object from ObjectHandles pool when no longer required
     * @throws SaxonApiException if error in the transformation. Also if XsltExecutable is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_callFunctionReturningValue", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long callFunctionReturningValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer ccFuncName, ObjectHandle argumentsRef, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String cFuncName = SaxonCAPI.toJavaString(ccFuncName, WordFactory.nullPointer());
        XsltExecutable executable = objectHandles.get(executableRef);
        
        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = QName.fromClarkName(cFuncName);
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, null, initialTemplateParameters, true);

        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        applyXsltTransformerProperties(cwd, processor, transformer, null, props, paramsMap, globalParameters, initialTemplateParameters);
        Object[] arguments = null;
        if(argumentsRef != WordFactory.nullPointer()) {
            ProcessorDataAccumulator argumentsAccumulator = objectHandles.get(argumentsRef);
            arguments = argumentsAccumulator.getValues();
        }
        XdmValue result =  transformer.callFunction(qname, getArguments(arguments));
        ObjectHandle handle = SaxonCAPI.createHandle(result);
        return handle.rawValue();
    }

    /**
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param alloc - reference to the C++ allocator object
     * @param ccwd   - Pointer to a C char array. current working directory. User supplies this as base URI. Accept null
     * @param executableRef  - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param ccFuncName - Pointer to  C char array. The name of the function to be called
     * @param argumentsRef - wrapped function arguments in a ProcessorDataAccumulator in the ObjectHandles pool. The values of the arguments to be supplied to the function. These
     *                       will be converted if necessary to the type as defined in the function signature, using
     *                        the function conversion rules.
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return
     * @throws SaxonApiException if error in the transformation. Also if XsltExecutable is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_callFunctionReturningString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer callFunctionReturningString(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer ccFuncName, ObjectHandle argumentsRef, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        XsltExecutable executable;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        } else {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        String cFuncName = SaxonCAPI.toJavaString(ccFuncName, WordFactory.nullPointer());

        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = QName.fromClarkName(cFuncName);
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);

        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        ByteArrayOutputStream bStream = new ByteArrayOutputStream();
        Serializer serializer = processor.newSerializer(bStream);
        applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);
        Object[] arguments = null;
        if(argumentsRef != WordFactory.nullPointer()) {
            ProcessorDataAccumulator argumentsAccumulator = objectHandles.get(argumentsRef);
            arguments = argumentsAccumulator.getValues();
        }
        transformer.callFunction(qname, getArguments(arguments), serializer);

        CCharPointer a = SaxonCAPI.stringToNative(alloc, bStream);
        return a;
    }

    /**
     * Invoke a transformation by calling a named template. The results of calling
     * the template are wrapped in a document node, which is then sent to a file
     * If setInitialTemplateParameters via the C++ API has been
     * called, then the parameters supplied are made available to the called template (no error
     * occurs if parameters are supplied that are not used).
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param ccwd   - Pointer to a C char array. current working directory. User supplies this as base URI. Accept null
     * @param executableRef  - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param ctemplateName - The name of the initial template given as a pointer to the C char array. This must match the name of a
     *                        public named template in the stylesheet. If the value is null,
     *                        the QName <code>xsl:initial-template</code> is used.
     * @param coutFilename   - Pointer to a C char array. The file name to save the result of the apply template transformation
     * @param processorDataAccumulatorRef  - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return long value representing the status of the transformation. Success=1, failure = -1 and exception=-2
     * @throws SaxonApiException if error in the transformation. Also if XsltExecutable is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_callTemplateReturningFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long callTemplateReturningFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef,  CCharPointer ctemplateName, CCharPointer coutFilename, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        XsltExecutable executable;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        } else {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        String templateName = null;
        QName qname = null;
        if(ctemplateName != WordFactory.nullPointer()) {
            templateName = SaxonCAPI.toJavaString(ctemplateName, WordFactory.nullPointer());
            qname = QName.fromClarkName(templateName);
        }
        String outFilename = SaxonCAPI.toJavaString(coutFilename, WordFactory.nullPointer());

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();

        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        Serializer serializer = null;
        if (outFilename != null) {
            serializer = resolveOutputFile(processor, cwd, outFilename, ResourceRequest.XML_NATURE);
        } else if (paramsMap.containsKey("o")) {
            String outfile = (String) paramsMap.get("o");
            serializer = resolveOutputFile(processor, cwd, outfile, ResourceRequest.XML_NATURE);
        } else {
            throw new SaxonApiException("File name for CallFunction has not been set");
        }

        applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);

        transformer.callTemplate(qname, serializer);
        return SaxonCAPI.STATUS_OK;
    }

    /**
     * Invoke a transformation by calling a named template. The results of calling
     * the template are wrapped in a document node, which is then returned as a XdmValue referenced in the ObjectHandles pool.
     * If setInitialTemplateParameters via the C++ API has been
     * called, then the parameters supplied are made available to the called template (no error
     * occurs if parameters are supplied that are not used).
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param ccwd  - Pointer to a C char array. current working directory. User supplies this as base URI. Accept null
     * @param executableRef  - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param cclarkName - Pointer to a C char array for the the name of the initial template. This must match the name of a
     *                     public named template in the stylesheet. If the value is null,
     *                     the QName <code>xsl:initial-template</code> is used.
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return long value referencing the results as an XdmValue object which is held in the ObjectHandles pool to previous GC.
     *   Caller must remove object from ObjectHandles pool when no longer required
     * @throws SaxonApiException if error in the transformation. Also if XsltExecutable is null. Exception handled and returned as -2
     */
    @CEntryPoint(name = "j_callTemplateReturningValue", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long callTemplateReturningValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer cclarkName, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {

        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        XsltExecutable executable;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        } else {
            throw new SaxonApiException("Error XsltExecutable is null");
        }

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = null;

        if (cclarkName != WordFactory.nullPointer()) {
            String clarkName = SaxonCAPI.toJavaString(cclarkName, WordFactory.nullPointer());
            qname = QName.fromClarkName(clarkName);
        }
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        //setsource(cwd, transformer, sourceObj, paramsMap);
        applyXsltTransformerProperties(cwd, processor, transformer, null, props, paramsMap, globalParameters, initialTemplateParameters);
        Destination xdmResult;
        boolean returnXdmValue = false;
        if (paramsMap.containsKey("outvalue")) {
            //Set if the return type of callTemplate, applyTemplate and transform methods is to return XdmValue,
            //otherwise return XdmNode object with root Document node
            Object valuei = paramsMap.get("outvalue");
            if (valuei instanceof Boolean) {
                returnXdmValue = (boolean) valuei;
            } else if (valuei instanceof String && valuei.equals("yes") || valuei.equals("true")) {
                returnXdmValue = true;
            }
        }
        if (returnXdmValue) {
            xdmResult = new RawDestination();
        } else {
            xdmResult = new XdmDestination();
        }

        transformer.callTemplate(qname, xdmResult);

        XdmValue value =  getXdmValue(xdmResult, paramsMap);
        ObjectHandle handle = SaxonCAPI.createHandle(value);
        return handle.rawValue();
    }

    private void setsource(String cwd, Xslt30Transformer transformer, Object sourceObj, Map<String, Object> paramsMap) throws SaxonApiException {
        if (sourceObj != null) {
            if (sourceObj instanceof String) {
                XdmNode node;
                DocumentBuilder builder = processor.newDocumentBuilder();
                Source source = resolveFileToSource(processor, cwd, (String) sourceObj, ResourceRequest.XML_NATURE);
                if (paramsMap.containsKey("dtd")) {
                    AugmentedSource asource = AugmentedSource.makeAugmentedSource(source);
                    String option = (String) paramsMap.get("dtd");
                    if (!option.isEmpty()) {
                        asource.setDTDValidationMode(Validation.getCode(option));
                    }
                    source = asource;
                    node = builder.build(source);
                    transformer.setGlobalContextItem(node);
                }

            } else if (sourceObj instanceof XdmItem) {
                XdmItem node = (XdmItem) sourceObj;
                transformer.setGlobalContextItem(node);
            }
        }

    }

    /**
     * Invoke a transformation by calling a named template. The results of calling
     * the template are wrapped in a document node, which is then returned as a string (C pointer to a char array)
     * in the C++ heap memory space. Caller must clear associated memory when finished.
     * If setInitialTemplateParameters via the C++ API has been
     * called, then the parameters supplied are made available to the called template (no error
     * occurs if parameters are supplied that are not used).
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param alloc
     * @param ccwd  - Pointer to a C char array. current working directory. User supplies this as base URI. Accept null
     * @param executableRef  - long value referencing the Xslt30Executable object held in the ObjectHandles pool.
     * @param cclarkName
     * @param processorDataAccumulatorRef  - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of supplied parameter and property names to values
     * @return
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "j_callTemplateReturningString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer callTemplateReturningString(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc,  CCharPointer ccwd, ObjectHandle executableRef, CCharPointer cclarkName, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        XsltExecutable executable;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        } else {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }

        QName qname = null;
        if(cclarkName != WordFactory.nullPointer()) {
            qname = QName.fromClarkName(SaxonCAPI.toJavaString(cclarkName, WordFactory.nullPointer()));
        }

        if(SaxonCAPI.debug) {
            if(qname!= null) {
                System.err.println("DEBUG: j_callTemplateReturningString qname="+qname);
            } else {
                System.err.println("DEBUG: j_callTemplateReturningString qname is null");
            }
        }

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        ByteArrayOutputStream bStream = new ByteArrayOutputStream();

        Serializer serializer = processor.newSerializer(bStream);

        //setsource(cwd, transformer, sourceObj, paramsMap);

        applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);


        transformer.callTemplate(qname, serializer);
        CCharPointer result = SaxonCAPI.stringToNative(alloc, bStream);

        if(SaxonCAPI.debug) {
            if(result == WordFactory.nullPointer()) {
                System.err.println("DEBUG: j_callTemplateReturningString result is null");
            }
        }
        return result;

    }

    /**
     * Do transformation. Save result to file
     * This method is designed to be used with the compile[File/String] methods above,
     * therefore executable can be loaded in a previous step.
     * However this method can be used as a one-shot method to go
     * through the stages of compilation, loading any source documents and execution.
     * Here we supply parameters and properties required to do the transformation.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param ccwd - current working directory supplied as a pointer to the C char array
     * @param xslt30ProcessorRef - long value reference to the Executable object held in the ObjectHandle pool. Null accepted, but the executable ,cannot be null
     * @param executableRef - long value reference to the Executable object held in the ObjectHandle pool. Null accepted, but the Xslt30Processor cannot be null
     * @param csourceFilename  - source file name  supplied as a pointer to a C char array. Converted to a Java string
     * @param cstylesheet  - stylesheet string  supplied as a pointer to a C char array. Converted to a Java string
     * @param coutFilename   - Save result of transformation to the given file name. Passed as a pointer to a C char array
     * @param processorDataAccumulatorRef - long value referencing ProcessorDataAccumulator object in the ObjectHandles pool. This is the mapping of parameters and properties to values.
     * @return   long value representing the success status of the transformation. success=0, fail=-1 and exception thrown=-2
     * @throws SaxonApiException  if the transformation fails. SaxonCExceptionLongHandler class handlers any exception thrown
     */
    @CEntryPoint(name = "j_transformToFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long transformToFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle xslt30ProcessorRef, ObjectHandle executableRef, CCharPointer csourceFilename, CCharPointer cstylesheet, CCharPointer coutFilename, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        XsltExecutable executable = null;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        }


        Xslt30Processor xslt30Processor = null;
        if(xslt30ProcessorRef != WordFactory.nullPointer()) {
            xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        }
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String stylesheet = null;
        if(cstylesheet != WordFactory.nullPointer()) {
            stylesheet = SaxonCAPI.toJavaString(cstylesheet, WordFactory.nullPointer());
        }
        String sourceFilename = null;
        if(csourceFilename != WordFactory.nullPointer()) {
            sourceFilename = SaxonCAPI.toJavaString(csourceFilename, WordFactory.nullPointer());
        }
        String outFilename = null;
        if(coutFilename != WordFactory.nullPointer()) {
            outFilename = SaxonCAPI.toJavaString(coutFilename, WordFactory.nullPointer());
        }


        try {
            Serializer serializer = null;
            Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
            Map<QName, XdmValue> globalParameters = new HashMap<>();
            Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
            Properties props = new Properties();
            Source source;
            Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
            Xslt30Transformer transformer;
            Processor processor;

            if (executable != null){
                transformer = executable.load30();
                processor = executable.getProcessor();
            }else if(xslt30Processor != null) {
                processor = xslt30Processor.getProcessor();
                transformer = xslt30Processor.getXslt30Transformer(cwd, executable, stylesheet, staticParameters);
            } else {
                  throw new SaxonApiException("Error XsltExecutable is null");
            }

            if (outFilename != null) {
                serializer = resolveOutputFile(processor, cwd, outFilename, ResourceRequest.XML_NATURE);
            } else if (paramsMap.containsKey("o")) {
                String outfile = (String) paramsMap.get("o");
                serializer = resolveOutputFile(processor, cwd, outfile, ResourceRequest.XML_NATURE);
            } else if (serializer == null) {
                throw new SaxonApiException("Output file not set for this transformation");
            }

            applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);
            XdmNode doc = null;

            Object valuei = paramsMap.get("node");
            if (valuei == null) {
                valuei = paramsMap.get("param:node");
            }
            if (valuei != null && valuei instanceof XdmNode) {
                doc = (XdmNode) valuei;
            }

            if (sourceFilename == null && doc != null) {
                transformer.setGlobalContextItem(doc);
                transformer.transform(doc.asSource(), serializer);
            } else if (sourceFilename != null) {
                source = resolveFileToSource(processor, cwd, sourceFilename, ResourceRequest.XML_NATURE);

                transformer.transform(source, serializer);
            }


            serializer = null;

        }  catch (NullPointerException ex) {
            SaxonApiException ex2 = new SaxonApiException(ex);
            throw ex2;
        } catch (Exception ex) {
            SaxonApiException ex2 = new SaxonApiException(ex);
            throw ex2;
        }
        return SaxonCAPI.STATUS_OK;
    }


    /**
     * Applies the properties and parameters required in the transformation.
     * In addition we can supply the source, stylesheet and output file names.
     * We can also supply values to xsl:param and xsl:variables required in the stylesheet.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *  @param cwd                       - current working directory
     * @param processor                 - required to use the same processor as for the compiled stylesheet
     * @param transformer               - pass the current object to set local variables supplied in the parameters
     * @param serializer                - pass the serializer to set properties
     * @param props                     - properties to apply to the Serializer
     * @param map                       - parameters and property names given as string keys. Their Values given as a array of Java objects
     * @param globals                   - Supply the values of global stylesheet parameters.
     * @param initialTemplateParameters  - Supply the initial template parameters
     */
    public static void applyXsltTransformerProperties(String cwd, Processor processor, Xslt30Transformer transformer, Serializer serializer, Properties props, Map<String, Object> map, Map<QName, XdmValue> globals, Map<QName, XdmValue> initialTemplateParameters) throws SaxonApiException {
        boolean tunnel = false;
        if (!map.isEmpty()) {
            String initialMode;
            XdmItem item;
            String outfile = null;
            Source source;
            Object valuei = null;
            DocumentBuilder builder = processor.newDocumentBuilder();

            if (cwd != null && cwd.length() > 0) {
                if (!cwd.endsWith("/")) {
                    cwd = cwd.concat("/");
                }
            }

            if (map.containsKey("extc")) {
                //extension function library path
                String libName = (String) map.get("extc");
                SaxonCAPI.setLibrary("", libName);


            }

            if (map.containsKey("baseoutput")) {
                valuei = map.get("baseoutput");
                if (valuei instanceof String) {
                    transformer.setBaseOutputURI((String) valuei);
                }

            }

            if (map.containsKey("im")) {
                valuei = map.get("im");
                if (valuei instanceof String) {
                    initialMode = (String) valuei;
                    QName qname = null;
                    if(initialMode.equals("xsl:unnamed")) {
                         qname = new QName("xsl", NamespaceConstant.XSLT, "unnamed");
                    } else if(initialMode.equals("xsl:default")) {
                        qname = new QName("xsl", NamespaceConstant.XSLT, "default");
                    } else if(initialMode.startsWith("Q{")){
                        qname = QName.fromEQName(initialMode);
                    }  else if(initialMode.startsWith("{")) {
                        qname = QName.fromClarkName(initialMode);
                    } else {
                        qname = new QName(initialMode);
                    }
                    transformer.setInitialMode(qname);
                } else if (debug) {
                    System.err.println("DEBUG: value error for property 'im'");
                }
            }
            if (map.containsKey("s")) {
                valuei = map.get("s");
                if (valuei instanceof String) {
                    source = SaxonCAPI.resolveFileToSource(processor, cwd, (String) valuei, ResourceRequest.XML_NATURE);
                    XdmNode node = builder.build(source);
                    transformer.setGlobalContextItem(node);
                } else if (debug) {
                    System.err.println("DEBUG: value error for property 's'");
                }
            }
            if (map.containsKey("item")) {


                valuei = map.get("item");
                if (valuei instanceof XdmItem) {
                    if (debug && valuei != null) {
                        System.err.println("DEBUG: Type of value=" + (valuei).getClass().getName());

                    }
                }
                item = (XdmItem) valuei;
                transformer.setGlobalContextItem(item);

            } else if (map.containsKey("node")) {


                valuei = map.get("node");
                if (valuei instanceof XdmItem) {
                    if (debug && valuei != null) {
                        System.err.println("DEBUG: Type of value=" + (valuei).getClass().getName());

                    }
                }
                item = (XdmItem) valuei;
                transformer.setGlobalContextItem(item);
            } else if (map.containsKey("param:node")) {


                valuei = map.get("param:node");
                if (valuei instanceof XdmItem) {
                    if (debug && valuei != null) {
                        System.err.println("DEBUG: Type of value=" + (valuei).getClass().getName());

                    }
                }
                item = (XdmItem) valuei;
                transformer.setGlobalContextItem(item);
            }

            if (map.containsKey("m")) {
                Object value = map.get("m");
                if(value instanceof SaxonCMessageListener) {
                   transformer.setMessageListener((SaxonCMessageListener)value);
                } else {
                    transformer.setMessageListener(new SaxonCMessageListener(processor, cwd, (String)value));
                }

            }

            if (map.containsKey("rd")) {
                Object value = map.get("rd");
                if(value instanceof SaxonCResultDocumentHandler) {
                    if(SaxonCAPI.debug) {
                        System.err.println("DEBUG: result-document handler set");
                    }
                   transformer.setResultDocumentHandler((SaxonCResultDocumentHandler)value);
                } else if(SaxonCAPI.debug) {
                    System.err.println("DEBUG: Not able to set the result-document handler. Please check type of object");
                }

            }



            if (map.containsKey("tunnel")) {
                valuei = map.get("tunnel");
                if (valuei instanceof Boolean) {
                    tunnel = (Boolean) valuei;
                } else if (valuei instanceof String) {
                    tunnel = ((String) valuei).equals("yes") || valuei.equals("true");
                }
            }


        }
        if (!props.isEmpty() && serializer != null) {
            serializer.setOutputProperties(props);
        }

        if (!initialTemplateParameters.isEmpty()) {
            if (SaxonCAPI.debug) {
               for(Map.Entry<QName, XdmValue> entry : initialTemplateParameters.entrySet()) {

                 if(entry.getKey() != null) {
                     System.err.println("DEBUG: key = "+ entry.getKey());
                 } else {
                     System.err.println("DEBUG: initialTemplateParameter Found null key");

                 }
                   if(entry.getValue() != null) {
                       System.err.println("DEBUG: key = "+ entry.getValue().toString());
                   } else {
                       System.err.println("DEBUG: initialTemplateParameter Found null value");

                   }
               }
            }
            transformer.setInitialTemplateParameters(initialTemplateParameters, tunnel);
        }

        transformer.setStylesheetParameters(globals);

    }


    /**
     * Do transformation and return result as an Xdm node in memory
     * This method is designed to be used with the createStylesheet[File/String] methods above,
     * therefore executable can be loaded in a previous step.
     * However this method can be used as a one-shot method to go
     * through the stages of compilation, loading any source documents and execution.
     * Here we supply parameters and properties required to do the transformation.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param ccwd - current working directory supplied as a pointer to the C char array
     * @param xslt30ProcessorRef - long value reference to the Executable object held in the ObjectHandle pool. Null accepted, but the executable ,cannot be null
     * @param executableRef - long value reference to the Executable object held in the ObjectHandle pool. Null accepted, but the Xslt30Processor cannot be null
     * @param csourceFile  - source file name  supplied as a pointer to a C char array. Converted to a Java string
     * @param cstylesheet  - stylesheet string  supplied as a pointer to a C char array. Converted to a Java string
     * @param processorDataAccumulatorRef - long value referencing processorData object in the ObjectHandle pool.  Used to pass processor data and options
     * @return  long value referencing the XdmNode held in the ObjectHandles pool. Caller must remove object from ObjectHandles pool when no longer required
     * @throws SaxonApiException   if failure in transformation or if both the Xslt30Processor and XsltExecutable are null
     */
    @CEntryPoint(name = "j_transformToValue", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long transformToValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle xslt30ProcessorRef, ObjectHandle executableRef, CCharPointer csourceFile, CCharPointer cstylesheet, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        XsltExecutable executable = null;
        Xslt30Processor xslt30Processor = null;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        } else if(xslt30ProcessorRef != WordFactory.nullPointer()) {
            xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        } else {
            throw new SaxonApiException("XSLT30 Processor not created");
        }
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }


        String stylesheet = null;
        if(cstylesheet != WordFactory.nullPointer()) {
            stylesheet = SaxonCAPI.toJavaString(cstylesheet, WordFactory.nullPointer());
        }

        String sourceFile =  null;
        if(csourceFile != WordFactory.nullPointer()) {
            sourceFile = SaxonCAPI.toJavaString(csourceFile, WordFactory.nullPointer());
        }


        Source source;

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        try {
            Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
            Xslt30Transformer transformer;
            Processor processor;

            if (executable != null){
                transformer = executable.load30();
                processor = executable.getProcessor();
            }else if(xslt30Processor != null) {
                processor = xslt30Processor.getProcessor();
                transformer = xslt30Processor.getXslt30Transformer(cwd, executable, stylesheet, staticParameters);
            } else {
                  throw new SaxonApiException("Error XsltExecutable is null");
            }

            applyXsltTransformerProperties(cwd, processor, transformer, null, props, paramsMap, globalParameters, initialTemplateParameters);
            Destination destination;
            boolean returnXdmValue = false;
            if (paramsMap.containsKey("outvalue")) {
                //Set if the return type of callTemplate, applyTemplate and transform methods is to return XdmValue,
                //otherwise return XdmNode object with root Document node
                Object valuei = paramsMap.get("outvalue");
                if (valuei instanceof Boolean) {
                    returnXdmValue = (boolean) valuei;
                } else if (valuei instanceof String && valuei.equals("yes") || valuei.equals("true")) {
                    returnXdmValue = true;
                }
            }
            if (returnXdmValue) {
                destination = new RawDestination();
            } else {
                destination = new XdmDestination();
            }
            XdmNode doc = null;

            if(sourceFile == null && paramsMap.containsKey("node")) {
                Object valuei = paramsMap.get("node");
                if (valuei == null) {
                    valuei = paramsMap.get("param:node");
                }
                if (valuei != null && valuei instanceof XdmNode) {
                    doc = (XdmNode) valuei;
                }
            } else if (sourceFile == null && paramsMap.containsKey("s")) {
                 Object valuei = paramsMap.get("s");
                 if (valuei instanceof String) {
                     DocumentBuilder builder = processor.newDocumentBuilder();
                     source = SaxonCAPI.resolveFileToSource(processor, cwd, (String) valuei, ResourceRequest.XML_NATURE);
                     doc = builder.build(source);
                 } else if (debug) {
                     System.err.println("DEBUG: value error for property 's'");
                 }
             }

            if (sourceFile == null && doc != null) {
                transformer.transform(doc.asSource(), destination);
            } else if (sourceFile != null) {
                source = resolveFileToSource(processor, cwd, sourceFile, ResourceRequest.XML_NATURE);

                transformer.transform(source, destination);

            } else {
                throw new SaxonApiException("Source not set in transform method");
            }

            XdmValue value = getXdmValue(destination, paramsMap);
            ObjectHandle handle = SaxonCAPI.createHandle(value);
            return handle.rawValue();
        } catch (Exception ex) {
            throw new SaxonApiException(ex);
        }

    }


    /**
     * Do transformation. The result is serialized as string representation in memory
     * This method is designed to be used with the createStylesheet[File/String] methods above,
     * therefore executable can be loaded in a previous step.
     * However this method can be used as a one-shot method to go
     * through the stages of compilation, loading any source documents and execution.
     * Here we supply parameters and properties required to do the transformation.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *
     * @param thread  - Graalvm runtime data structure for a thread
     * @param alloc - reference to the C++ allocator object
     * @param ccwd - current working directory supplied as a pointer to the C char array
     * @param xslt30ProcessorRef - long value reference to the Executable object held in the ObjectHandle pool. Null accepted, but the executable ,cannot be null
     * @param executableRef - long value reference to the Executable object held in the ObjectHandle pool. Null accepted, but the Xslt30Processor cannot be null
     * @param csourceFile  - source file name  supplied as a pointer to a C char array. Converted to a Java string
     * @param cstylesheet  - stylesheet string  supplied as a pointer to a C char array. Converted to a Java string
     * @param processorDataAccumulatorRef - long value referencing processorData object in the ObjectHandles pool.  Used to pass processor data and options
     * @return  result as a string representation in a byte array and converted as a pointer to a C char array. Data is held in the C/C++ heap memory space. Caller must clear associated memory
     * @throws SaxonApiException   if failure in transformation
     */
    @CEntryPoint(name = "j_transformToString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer transformToString(IsolateThread thread, AllocatorCCharFn alloc, CCharPointer ccwd, ObjectHandle xslt30ProcessorRef, ObjectHandle executableRef, CCharPointer csourceFile, CCharPointer cstylesheet, ObjectHandle processorDataAccumulatorRef) throws SaxonApiException {
        XsltExecutable executable = null;
        if(executableRef != WordFactory.nullPointer()) {
            executable = objectHandles.get(executableRef);
        }


        Xslt30Processor xslt30Processor = null;
        if(xslt30ProcessorRef != WordFactory.nullPointer()) {
            xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        }
        String cwd = null;
        if(ccwd !=WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }


        String stylesheet = null;
        if(cstylesheet != WordFactory.nullPointer()) {
            stylesheet = SaxonCAPI.toJavaString(cstylesheet, WordFactory.nullPointer());
        }

        String sourceFile =  null;
        if(csourceFile != WordFactory.nullPointer()) {
            sourceFile = SaxonCAPI.toJavaString(csourceFile, WordFactory.nullPointer());
        }
        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = setupConfigurationAndBuildMap(processorDataAccumulatorRef, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        try {
            Source source = null;
            Xslt30Transformer transformer;
            Processor processor;

            if (executable != null){
                transformer = executable.load30();
                processor = executable.getProcessor();
            }else if(xslt30Processor != null) {
                processor = xslt30Processor.getProcessor();
                transformer = xslt30Processor.getXslt30Transformer(cwd, null, stylesheet, staticParameters);
            } else {
                  throw new SaxonApiException("Error XsltExecutable is null");
            }

            if (debug) {
                System.err.println("transformToString, Processor: " + System.identityHashCode(processor));
            }

            ByteArrayOutputStream bStream = new ByteArrayOutputStream();
            Serializer serializer = processor.newSerializer(bStream);


            applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);
            XdmNode doc = null;

            if(paramsMap.containsKey("node")) {
                Object valuei = paramsMap.get("node");
                if (valuei == null) {
                    valuei = paramsMap.get("param:node");
                }
                if (valuei != null && valuei instanceof XdmNode) {
                    doc = (XdmNode) valuei;
                }
            }

            if (doc == null && paramsMap.containsKey("s")) {
                 Object valuei = paramsMap.get("s");
                 if (valuei instanceof String) {
                     DocumentBuilder builder = processor.newDocumentBuilder();
                     source = SaxonCAPI.resolveFileToSource(processor, cwd, (String) valuei, ResourceRequest.XML_NATURE);
                     doc = builder.build(source);
                 } else if (debug) {
                     System.err.println("DEBUG: value error for property 's'");
                 }
             }

            if (sourceFile == null && doc != null) {
                source = doc.asSource();
            } else if (sourceFile != null) {
                source = resolveFileToSource(processor, cwd, sourceFile, ResourceRequest.XML_NATURE);
            }
            //transformer.setErrorListener(errorListener);
            transformer.transform(source, serializer);
            serializer = null;
            CCharPointer a = SaxonCAPI.stringToNative(alloc, bStream);
            return a;
        } catch (Exception e) {
            SaxonApiException ex = new SaxonApiException(e);
            throw ex;
        }
    }


    /***
     * Compile a library package and link it for use.
     * <p>The source argument identifies an XML file containing an &lt;xsl:package&gt; element. Any packages
     * on which this package depends must have been made available to the <code>XsltCompiler</code>
     * by importing them.</p>
     */
    /*public void compilePackages(XsltCompiler compiler) throws SaxonApiException {
        try {
            PackageLibrary library = new PackageLibrary(compiler.getUnderlyingCompilerInfo(), packagesToLoad);
            compiler.getUnderlyingCompilerInfo().setPackageLibrary(library);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }    */






    public static void main(String[] args)  {
        String cwd2 = "/Users/ond1/work/development/svn/archive/opensource/latest9.9/hec/samples/php/trax";
        String cwd = "/Users/ond1/work/development/tests/jeroen";
        // String cwd = "C:///www///html///trax";
        //String cwd = "http://localhost/trax";
        /*if (args.length > 0) {
            cwd = args[0];
        }

             String */
        


        /*
        XdmNode node = cpp.parseXmlString("<doc><e>text</e></doc>");
        String [] pname =  {"node"};
        Object [] pObj = {node};
        String style = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:output omit-xml-declaration=\"true\" /><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>";
        XsltExecutable executable = cpp.compileFromString(null, style, false, null, null);
        String result = new String(net.sf.saxon.option.cpp.Xslt30Processor.callTemplateReturningString(null,executable, "main", pname, pObj));
        System.out.println("Result = "+ result);   */

    }

    
}