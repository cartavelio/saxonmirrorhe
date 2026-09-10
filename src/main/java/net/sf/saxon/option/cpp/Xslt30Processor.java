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
            config.getBuiltInExtensionLibraryList().addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList().addFunctionLibrary(CPPFunctionSet.getInstance());
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
            config.getBuiltInExtensionLibraryList().addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList().addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif

    }


    /**
     * Create new object of this class
     *
     * @param proc - s9api processor
     * @return XsltProcessor
     */
    @CEntryPoint(name = "createXslt30Processor")
    public static long createXslt30Processor(IsolateThread thread) {
            ObjectHandle handle = objectHandles.create(new net.sf.saxon.option.cpp.Xslt30Processor(false));
            return handle.rawValue();
    }

    @CEntryPoint(name = "createXslt30WithProcessor")
    public static long createXslt30Processor1(IsolateThread thread, ObjectHandle processorRef) {
            Processor processor = objectHandles.get(processorRef);
            if(processor == null) {
                return 0;
            }
            ObjectHandle handle = objectHandles.create(Xslt30Processor.newInstance(processor));
            return handle.rawValue();
    }


    public static Xslt30Processor newInstance(Processor proc) {
        return new Xslt30Processor(proc);
    }


    /**
     * Compile package from source file and save resulting sef to file store.
     *
     * @param cwd         - current working directory
     * @param xsl         - File name of the stylesheet
     * @param outFilename - the file to which the compiled package should be saved
     */
    @CEntryPoint(name = "compileFromFileAndSave")
    public static void compileFromFileAndSave(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer xsl, CCharPointer outFilename) throws SaxonApiException {
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        XsltCompiler compiler = processor.newXsltCompiler();
        String cwd = CTypeConversion.toJavaString(ccwd);
        Source source = resolveFileToSource(processor, cwd, CTypeConversion.toJavaString(xsl), ResourceRequest.XSLT_NATURE);
        XsltPackage pack = compiler.compilePackage(source);
        File file = resolveFile(processor, cwd, CTypeConversion.toJavaString(outFilename), ResourceRequest.XSLT_NATURE);
        pack.save(file);

    }


    /**
     * Produce a representation of the compiled stylesheet and save to file, in XML form, suitable for
     * distribution and reloading.
     * @param executable
     * @param filename
     */
    @CEntryPoint(name = "save")
    public static void save(IsolateThread thread, CCharPointer cwd, ObjectHandle executableRef, CCharPointer filename) throws SaxonApiException {
        File file = SaxonCAPI.resolveFile(null, CTypeConversion.toJavaString(cwd), CTypeConversion.toJavaString(filename), null);
        FileOutputStream stream = null;
        try {
            stream = new FileOutputStream(file);
        } catch (FileNotFoundException e) {
            throw new SaxonApiException(e.getMessage());
        }
        XsltExecutable executable = objectHandles.get(executableRef);
        executable.export(stream);
    }

    /**
     * Compile package from string and save resulting sef to file store.
     *
     * @param cwd      - current working directory
     * @param str      - File name of the stylsheet
     * @param filename - the file to which the compiled package should be saved
     */
    @CEntryPoint(name = "compileFromStringAndSave")
    public static void compileFromStringAndSave(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer cstr, CCharPointer cfilename) throws SaxonApiException {
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();

        String str = CTypeConversion.toJavaString(cstr);
        String filename = CTypeConversion.toJavaString(cfilename);
        String cwd = CTypeConversion.toJavaString(ccwd);
        if (processor == null || str == null || filename == null) {
            if (SaxonCAPI.debug) {
                System.err.println("DEBUG: compileFromStringAndSave str is null");
            }
            return;
        }
        XsltCompiler compiler = processor.newXsltCompiler();
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
    }

    /**
     * Compile package from Xdm node and save resulting sef to file store.
     *
     * @param cwd      - current working directory
     * @param obj      -
     * @param filename - the file to which the compiled package should be saved
     */
    @CEntryPoint(name = "compileFromXdmNodeAndSave")
    public static void compileFromXdmNodeAndSave(IsolateThread thread, ObjectHandle xslt30ProcessorRef, ObjectHandle xdmNodeRef, CCharPointer ccwd, CCharPointer cfilename) throws SaxonApiException {
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        if(processor == null) {
            return;
        }
        XsltCompiler compiler = processor.newXsltCompiler();
        XdmNode node = objectHandles.get(xdmNodeRef);
        if (node == null) {
            SaxonApiException ex = new SaxonApiException("Failed to create Stylesheet from XdoNode");
            throw ex;
        }

        XsltPackage pack = compiler.compilePackage(node.asSource());
        String cwd = CTypeConversion.toJavaString(ccwd);
        String filename = CTypeConversion.toJavaString(cfilename);

        if(pack != null) {
            File file = resolveFile(processor, cwd, filename, ResourceRequest.XSLT_NATURE);
            pack.save(file);
        }

    }

    @CEntryPoint(name = "compileFromAssociatedFile")
    public static XsltExecutable compileFromAssociatedFile(ObjectHandle xslt30ProcessorRef, CCharPointer cwd, CCharPointer filename, boolean jit, String[] sparams, Object[] values) throws SaxonApiException {

        try {
            Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
            Processor processor = xslt30Processor.getProcessor();
            XsltCompiler compiler = processor.newXsltCompiler();
            setStaticParametersFromArray(compiler, sparams, values);
            if (jit) {
                compiler.setJustInTimeCompilation(jit);
            }
            XsltExecutable executable = null;
            /*if (packagesToLoad.size() > 0) {
                compilePackages(compiler);
            } */
            Source source = resolveFileToSource(processor, CTypeConversion.toJavaString(cwd), CTypeConversion.toJavaString(filename), ResourceRequest.XML_NATURE);

            //compiler.setErrorListener(errorListener);
            compiler.setSchemaAware(xslt30Processor.schemaAware);
            executable = compiler.compile(compiler.getAssociatedStylesheet(source, null, null, null));
            return executable;
        } catch (SaxonApiException ex) {
            throw ex;
        }
    }


    /**
     * Compile the stylesheet from file  for use later
     *
     * @param cwd      - current working directory
     * @param filename - File name of the stylsheet
     * @return XsltExecutable
     */


    @CEntryPoint(name = "compileFromFile")
   public static XsltExecutable compileFromFile(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer cfilename, boolean jit/*, String[] sparams, Object[] values*/) throws SaxonApiException {

        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        Processor processor = xslt30Processor.getProcessor();
        XsltCompiler compiler = processor.newXsltCompiler();
        //setStaticParametersFromArray(compiler, sparams, values); //TODO fixup parameters

        if (jit) {
            compiler.setJustInTimeCompilation(jit);
        }
        XsltExecutable executable = null;

        Source source = resolveFileToSource(processor, CTypeConversion.toJavaString(ccwd), CTypeConversion.toJavaString(cfilename), ResourceRequest.XSLT_NATURE);

        //compiler.setErrorListener(errorListener);
        compiler.setSchemaAware(xslt30Processor.schemaAware);
        executable = compiler.compile(source);
        return executable;

    }

    /**
     * Compile the stylesheet from string  for use later
     *
     * @param cwd - current working directory
     * @param str - stylesheet as a string
     * @param jit - Falg for Just in Time compilation
     * @return XsltExecutable
     */






    @CEntryPoint(name = "compileFromString")
    public static XsltExecutable compileFromString(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd, CCharPointer stylesheet, boolean jit /*, String[] sparams, Object[] values*/) throws SaxonApiException {
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        XsltCompiler compiler = xslt30Processor.getProcessor().newXsltCompiler();
        //setStaticParametersFromArray(compiler, sparams, values); //TODO remove comment - fixup parameters
        if (jit) {
            compiler.setJustInTimeCompilation(jit);
        }
        Source source;
        String cwd = CTypeConversion.toJavaString(ccwd);
        StringReader reader = new StringReader(CTypeConversion.toJavaString(stylesheet));
        compiler.setSchemaAware(xslt30Processor.schemaAware);
        source = new StreamSource(reader);
        if (cwd != null && cwd.length() > 0) {
            if (!cwd.endsWith("/")) {
                cwd = cwd.concat("/");
            }
            source.setSystemId(cwd);

        }

        XsltExecutable executable = compiler.compile(source);
        return executable;


    }

    /**
     * Compile the stylesheet from string  for use later
     *
     * @param cwd     - current working directory
     * @param obj     - stylesheet as a XdmNode object
     * @param sparams - Supplied static parameters
     * @param values
     * @return XsltExecutable
     */
    @CEntryPoint(name = "compileFromXdmNode")
    public static XsltExecutable compileFromXdmNode(IsolateThread thread, ObjectHandle xslt30ProcessorRef, CCharPointer ccwd,  ObjectHandle xdmNodeRef, boolean jit /*, String[] sparams, Object[] values*/) throws SaxonApiException {

        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        XsltCompiler compiler = xslt30Processor.getProcessor().newXsltCompiler();
        //setStaticParametersFromArray(compiler, sparams, values); //TODO fix up the handling of parameters in this method
        if (jit) {
            compiler.setJustInTimeCompilation(jit);
        }

        XdmNode node = objectHandles.get(xdmNodeRef);
        if(node == null){
            SaxonApiException ex = new SaxonApiException("Failed to create Stylesheet from XdmNode");
            throw ex;
        }

        //compiler.setErrorListener(errorListener);
        /* if (packagesToLoad.size() > 0) {
            compilePackages(compiler);
        }  */
        XsltExecutable executable = compiler.compile(node.asSource());
        return executable;


    }

    private static void setStaticParametersFromArray(XsltCompiler compiler, String[] sparams, Object[] values) throws SaxonApiException {
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
                }
            }

        }
    }


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



    @CEntryPoint(name = "applyTemplatesReturningValue")
    public static long applyTemplatesReturningValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle xsltExecutableRef, ObjectHandle sourceObjRef /*, String[] params, Object[] values*/) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(xsltExecutableRef);
        
        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = new HashMap(); //TODO remove comment setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        if(executable == null) {
            if(SaxonCAPI.debug) {
                System.err.println("Error: executable is null");
            }
            throw new SaxonApiException("executable is null");
        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        String cwd= CTypeConversion.toJavaString(ccwd);
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
        Object sourceObj = objectHandles.get(sourceObjRef);
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
        } else if (sourceObj instanceof XdmItem[]) {
            XdmItem[] itemArr = (XdmItem[]) sourceObj;
            XdmValue valueSeq;
            if (itemArr.length == 0) {
                throw new SaxonApiException("Source object is an empty array of XdmItems");

            } else if (itemArr.length == 1) {
                valueSeq = itemArr[0];
            } else {
                List<XdmItem> itemList = Arrays.asList((XdmItem[]) sourceObj);
                valueSeq = new XdmValue(itemList);
            }

            transformer.applyTemplates(valueSeq, xdmResult);
        } else {
            throw new SaxonApiException("Source object is neither a String value of an XdmValue");
        }


        XdmValue value = getXdmValue(xdmResult, paramsMap);

        ObjectHandle handle = objectHandles.create(value);
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
     *  Call applyTemplates returnign result to the given file name
     * @param cwd
     * @param executable
     * @param sourceObj
     * @param outFilename
     * @param params
     * @param values
     * @throws SaxonApiException
     */
    @CEntryPoint(name = "applyTemplatesReturningFile")
    public static void applyTemplatesReturningFile(CCharPointer ccwd, ObjectHandle executableRef, ObjectHandle sourceObjRef, CCharPointer coutFilename /*, String[] params, Object[] values*/) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(executableRef);
        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");

        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();
        Serializer serializer = null;
        String outFilename = CTypeConversion.toJavaString(coutFilename);
        String cwd = CTypeConversion.toJavaString(ccwd);
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
        Object sourceObj = objectHandles.get(sourceObjRef);
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
        } else if (sourceObj instanceof XdmItem[]) {
            XdmItem[] itemArr = (XdmItem[]) sourceObj;
            XdmValue valueSeq;
            if (itemArr.length == 0) {
                throw new SaxonApiException("Source object is an empty array of XdmItem objects");

            } else if (itemArr.length == 1) {
                valueSeq = itemArr[0];
            } else {
                List<XdmItem> itemList = Arrays.asList((XdmItem[]) sourceObj);
                valueSeq = new XdmValue(itemList);
            }

            transformer.applyTemplates(valueSeq, serializer);
        } else {
            throw new SaxonApiException("Source object is not of type (String, XdmValue or XdmValue[])");
        }


    }

    @CEntryPoint(name = "applyTemplatesReturningString")
    public static CCharPointer applyTemplatesReturningString(String cwd, XsltExecutable executable, Object sourceObj /*, String[] params, Object[] values*/) throws SaxonApiException {
        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");

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
        } else if (sourceObj instanceof XdmItem[]) {
            XdmItem[] itemArr = (XdmItem[]) sourceObj;
            XdmValue valueSeq;
            if (itemArr.length == 0) {
                throw new SaxonApiException("Source object is an empty array of XdmItem objects");

            } else if (itemArr.length == 1) {
                valueSeq = itemArr[0];
            } else {
                List<XdmItem> itemList = Arrays.asList((XdmItem[]) sourceObj);
                valueSeq = new XdmValue(itemList);
            }

            transformer.applyTemplates(valueSeq, serializer);
        } else {
            throw new SaxonApiException("Source object is not of type (String, XdmValue or XdmValue[])");
        }
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(bStream.toString());
        return holder.get();
    }


    private static XdmValue[] getArguments(Object[] arguments) throws SaxonApiException {
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

    @CEntryPoint(name = "callFunctionReturningFile")
    public static void callFunctionReturningFile(String cwd, XsltExecutable executable, String cFuncName, String outFilename /*, Objects[] arguments, String[] params, Object[] values*/) throws SaxonApiException {
        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = QName.fromClarkName(cFuncName);
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
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

        //transformer.callFunction(qname, getArguments(arguments), serializer); //TODO uncomment

    }

    @CEntryPoint(name = "callFunctionReturningValue")
    public static long callFunctionReturningValue(String cwd, XsltExecutable executable, String cFuncName /*, Object[] arguments, String[] params, Object[] values*/) throws SaxonApiException {

        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = QName.fromClarkName(cFuncName);
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, null, initialTemplateParameters, true);

        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        applyXsltTransformerProperties(cwd, processor, transformer, null, props, paramsMap, globalParameters, initialTemplateParameters);

        XdmValue result = null;// TODO transformer.callFunction(qname, getArguments(arguments));
        ObjectHandle handle = objectHandles.create(result);
        return handle.rawValue();
    }

    @CEntryPoint(name = "callFunctionReturningString")
    public static CCharPointer callFunctionReturningString(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer ccFuncName /*, Object[] arguments, String[] params, Object[] values*/) throws SaxonApiException {
        String cwd = CTypeConversion.toJavaString(ccwd);
        XsltExecutable executable = objectHandles.get(executableRef);
        String cFuncName = CTypeConversion.toJavaString(ccFuncName);

        Map<QName, XdmValue> staticParameters = (executable!= null?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = QName.fromClarkName(cFuncName);
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);

        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        ByteArrayOutputStream bStream = new ByteArrayOutputStream();
        Serializer serializer = processor.newSerializer(bStream);
        applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);

        // TODO transformer.callFunction(qname, getArguments(arguments), serializer);
        serializer = null;
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(bStream.toString());
        return holder.get();
    }


    @CEntryPoint(name = "callTemplateReturningFile")
    public static void callTemplateReturningFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef,  CCharPointer ctemplateName, CCharPointer coutFilename /*, String[] params, Object[] values*/) throws SaxonApiException {
        String cwd = CTypeConversion.toJavaString(ccwd);
        XsltExecutable executable = objectHandles.get(executableRef);
        String templateName = CTypeConversion.toJavaString(ctemplateName);
        String outFilename = CTypeConversion.toJavaString(coutFilename);

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();

        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }
        QName qname = null;
        if (templateName != null) {
            qname = QName.fromClarkName(templateName);
        }
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
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
    }

    @CEntryPoint(name = "callTemplateReturningValue")
    public static long callTemplateReturningValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer cclarkName /*, String[] params, Object[] values*/) throws SaxonApiException {
        String clarkName = CTypeConversion.toJavaString(cclarkName);
        String cwd = CTypeConversion.toJavaString(ccwd);
        XsltExecutable executable = objectHandles.get(executableRef);

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        QName qname = null;

        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }

        if (clarkName != null) {
            qname = QName.fromClarkName(clarkName);
        }
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
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
        ObjectHandle handle = objectHandles.create(value);
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

    @CEntryPoint(name = "callTemplateReturningString")
    public static CCharPointer callTemplateReturningString(IsolateThread thread, CCharPointer ccwd, ObjectHandle executableRef, CCharPointer cclarkName /*, String[] params, Object[] values*/) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(executableRef);
        String cwd = CTypeConversion.toJavaString(ccwd);
        String clarkName = CTypeConversion.toJavaString(cclarkName);

        QName qname = null;
        if (clarkName != null) {
            qname = QName.fromClarkName(clarkName);
        }
        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();

        if(executable == null) {
            throw new SaxonApiException("Error XsltExecutable is null");
        }

        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
        Xslt30Transformer transformer = executable.load30();
        Processor processor = executable.getProcessor();

        ByteArrayOutputStream bStream = new ByteArrayOutputStream();

        Serializer serializer = processor.newSerializer(bStream);

        //setsource(cwd, transformer, sourceObj, paramsMap);

        applyXsltTransformerProperties(cwd, processor, transformer, serializer, props, paramsMap, globalParameters, initialTemplateParameters);


        transformer.callTemplate(qname, serializer);
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(bStream.toString());
        return holder.get();

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
     * @param cwd            - current working directory
     * @param sourceFilename - source supplied as a file name
     * @param stylesheet     - File name of the stylesheet
     * @param outFilename    - Save result of transformation to the given file name
     * @param params         - parameters and property names given as an array of stings
     * @param values         - the values of the paramaters and properties. given as a array of Java objects
     */
    @CEntryPoint(name = "transformToFile")
    public static void transformToFile(IsolateThread thread, CCharPointer ccwd, ObjectHandle xslt30ProcessorRef, ObjectHandle executableRef, CCharPointer csourceFilename, CCharPointer cstylesheet, CCharPointer coutFilename /*, String[] params, Object[] values*/) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(executableRef);
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        String cwd = CTypeConversion.toJavaString(ccwd);
        String stylesheet = CTypeConversion.toJavaString(cstylesheet);
        String sourceFilename = CTypeConversion.toJavaString(csourceFilename);
        String outFilename = CTypeConversion.toJavaString(coutFilename);


        try {
            Serializer serializer = null;
            Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
            Map<QName, XdmValue> globalParameters = new HashMap<>();
            Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
            Properties props = new Properties();
            Source source;
            Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
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

        } catch (SaxonApiException e) {
            SaxonCException ex = new SaxonCException(e);
            throw e;
        } catch (NullPointerException ex) {
            SaxonApiException ex2 = new SaxonApiException(ex);
            throw ex2;
        } catch (Exception ex) {
            SaxonApiException ex2 = new SaxonApiException(ex);
            throw ex2;
        }
    }


    /**
     * Applies the properties and parameters required in the transformation.
     * In addition we can supply the source, stylesheet and output file names.
     * We can also supply values to xsl:param and xsl:variables required in the stylesheet.
     * The parameter names and values are supplied as a two arrays in the form of a key and value.
     *  @param cwd                       - current working directory
     * @param processor                 - required to use the same processor as for the compiled stylesheet
     * @param transformer               - pass the current object to set local variables supplied in the parameters
     * @param serializer
     * @param props
     * @param map                       - parameters and property names given as string keys. Their Values given as a array of Java objects
     * @param globals
     * @param initialTemplateParameters
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
                    transformer.setInitialMode(new QName(initialMode));
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
     * @param cwd        - current working directory
     * @param sourceFile - source supplied as a file name
     * @param stylesheet - File name of the stylesheet
     * @param params     - parameters and property names given as an array of stings
     * @param values     - the values of the paramaters and properties. given as a array of Java objects
     * @return result as an XdmNode
     */
    @CEntryPoint(name = "transformToValue")
    public static long transformToValue(IsolateThread thread, CCharPointer ccwd, ObjectHandle xslt30ProcessorRef, ObjectHandle executableRef, CCharPointer csourceFile, CCharPointer cstylesheet /*, String[] params, Object[] values*/) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(executableRef);
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        String cwd = CTypeConversion.toJavaString(ccwd);
        String stylesheet = CTypeConversion.toJavaString(cstylesheet);
        String sourceFile = CTypeConversion.toJavaString(csourceFile);


        Source source;

        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        try {
            Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
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
            ObjectHandle handle = objectHandles.create(value);
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
     * @param cwd        - current working directory
     * @param sourceFile - source supplied as a file name
     * @param stylesheet - File name of the stylesheet
     * @param params     - parameters and property names given as an array of stings
     * @param values     - the values of the paramaters and properties. given as a array of Java objects
     * @return result as a string representation in a byte array
     */
    @CEntryPoint(name = "transformToString")
    public static CCharPointer transformToString(IsolateThread thread, CCharPointer ccwd, ObjectHandle xslt30ProcessorRef, ObjectHandle executableRef, CCharPointer csourceFile, CCharPointer cstylesheet /*, String[] params, Object[] values*/) throws SaxonApiException {
        XsltExecutable executable = objectHandles.get(executableRef);
        Xslt30Processor xslt30Processor = objectHandles.get(xslt30ProcessorRef);
        String cwd = CTypeConversion.toJavaString(ccwd);
        String stylesheet = CTypeConversion.toJavaString(cstylesheet);
        String sourceFile = CTypeConversion.toJavaString(csourceFile);
        Map<QName, XdmValue> staticParameters = (executable != null ?  null : new HashMap<>());
        Map<QName, XdmValue> globalParameters = new HashMap<>();
        Map<QName, XdmValue> initialTemplateParameters = new HashMap<>();
        Properties props = new Properties();
        Map<String, Object> paramsMap = new HashMap(); //TODO setupConfigurationAndBuildMap(params, values, props, null, staticParameters, globalParameters, initialTemplateParameters, true);
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
            final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(bStream.toString());
            return holder.get();
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






    public static void main(String[] args) throws Exception {
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