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
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import javax.xml.transform.Source;
import java.io.*;
import java.net.MalformedURLException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;

/**
 * * XQuery engine class to use with SaxonC on C++
 */
public class XQueryEngine extends SaxonCAPI {

    //private XQueryExecutable executable = null;

    /**
     * Default Constructor to initialise XQueryEngine. s9api Processor is created with license flag as false
     */
    public XQueryEngine() {
        super(false);

    }

    /**
     * Constructor to initialise XQueryEngine with license flag
     *
     * @param license - flag indicating presence of license file
     */
    public XQueryEngine(boolean license) {
        super(license);
        Configuration config = processor.getUnderlyingConfiguration();
        schemaAware = config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT);
//#if EE==true || PE==true
        if (config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif
    }

    /**
     * Constructor to initialise XQueryEngine with processor and license flag
     *
     * @param proc - s9api processor
     */
    public XQueryEngine(Processor proc) {
        super(proc);
        Configuration config = processor.getUnderlyingConfiguration();
        schemaAware = config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT);
//#if EE==true || PE==true
        if (config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(PHPFunctionSet.getInstance());
            config.getBuiltInExtensionLibraryList(31).addFunctionLibrary(CPPFunctionSet.getInstance());
        }
//#endif
    }



    @CEntryPoint(name = "j_createXQueryEngine", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createXQueryEngine(IsolateThread thread) throws SaxonApiException{
            ObjectHandle handle = SaxonCAPI.createHandle(new net.sf.saxon.option.cpp.XQueryEngine(false));
            return handle.rawValue();
    }

    @CEntryPoint(name = "j_createXQueryEngineWithProcessor", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_createXQueryEngine(IsolateThread thread, ObjectHandle processorRef) throws SaxonApiException{
            if(processorRef == WordFactory.nullPointer()) {
                throw new SaxonApiException("Processor object is null");
            }
            Processor processor = objectHandles.get(processorRef);
            if(processor == null) {
                throw new SaxonApiException("Processor object is null");
            }
            ObjectHandle handle = SaxonCAPI.createHandle(XQueryEngine.newInstance(processor));
            return handle.rawValue();
    }

    public static XQueryEngine newInstance(Processor proc) {
        return new XQueryEngine(proc);
    }

    static Map<String, Object> convertArraysToMap(Processor processor, XQueryCompiler compiler, String[] params, Object[] values, Map<QName, XdmValue> parameters, Properties props) throws SaxonApiException {
            Map<String, Object> map = new HashMap();

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
                    props.put(prop.getQName().getClarkName(),  values[i]);
                } else if (params[i].startsWith("--") && values[i] != null) {
                    try {
                        processor.setConfigurationProperty("http://saxon.sf.net/feature/" + params[i].substring(2), (String) values[i]);
                    } catch (IllegalArgumentException err) {
                        throw new SaxonApiException(err.getMessage());
                    }
                }  else if (params[i].equals("resources")) {
                    char separatorChar = '/';
                    if (SaxonCAPI.RESOURCES_DIR == null && values[i] instanceof String) {
                        String dir1 = (String) values[i];
                        /*if (!dir1.endsWith("/")) {
                            dir1 = dir1.concat("/");
                        } */
                        if (File.separatorChar != '/') {
                            dir1.replace(separatorChar, File.separatorChar);
                            separatorChar = '\\';
                            dir1.replace('/', '\\');
                        }
                        SaxonCAPI.RESOURCES_DIR = dir1;
                    }
                }else if (params[i].equals("sa")) {
                    String value = ((String)values[i]).toLowerCase();
                    if(value.equals("true") || value.equals("on")) {
                        compiler.setSchemaAware(true);
                    } else {
                        compiler.setSchemaAware(false);
                    }

                } else if (params[i].equals("base")) {
                    String baseURI = (String) values[i];
                    try {
                        compiler.setBaseURI(new URI(baseURI));
                    } catch (URISyntaxException e) {
                        SaxonApiException ex = new SaxonApiException(e);
                        throw ex;
                                    }
                } else if (params[i].startsWith("ns-prefix:")) {
                    String prefix = params[i].substring(10);
                    String namespace = null;
                    if(values[i] instanceof String) {
                        namespace = (String) values[i];
                    } else {
                        throw new SaxonApiException("XQuery declare-namespace error: namespace is not a String value");
                    }
                    compiler.declareNamespace(prefix, namespace);

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
                            if(parameters != null) {
                                parameters.put(qname, valueForCpp);

                            }
                        }

                }else {
                    if (debug) {
                        System.err.println("DEBUG: SaxonCAPI: param-name: " +params[i] + ", type of Value= "+values[i].getClass().getName());
                    }
                    map.put(params[i], values[i]);
                }
            }

            return map;

        }


    public static XQueryEvaluator xqueryEvaluator(Processor processor, String cwd, String query, String[] params, Object[] values, Serializer serializer) throws SaxonApiException {
        File queryFile = null;
        XQueryCompiler compiler = processor.newXQueryCompiler();
        Map<QName, XdmValue> parameters = new HashMap<>();
        Properties props = new Properties();

        Map<String, Object> optionsMap = convertArraysToMap(processor, compiler, params, values, parameters, props);
        //compiler.setErrorListener(errorListener);

        if (params != null && params.length != values.length) {
            SaxonApiException ex = new SaxonApiException("Length of params array not equal to the length of values array");
            throw ex;
        }

        XQueryExecutable executable = null;

        compiler.setErrorReporter(SaxonCException.errorReporter);
        if(query != null) {
            executable = compiler.compile(query);
        }

        if (params != null && params.length != 0) {
                if(optionsMap.containsKey("updating")) {
                    if(optionsMap.get("updating").equals("on")) {
                         compiler.setUpdatingEnabled(true);
                    } else {
                        compiler.setUpdatingEnabled(false);
                    }
                }

                if(optionsMap.containsKey("-streaming")) {
                    if(optionsMap.get("-streaming").equals("1")) {
                         compiler.setStreaming(true);
                    } else {
                        compiler.setStreaming(false);
                    }
                }

                if(optionsMap.containsKey("-lang-version")) {
                    String versionStr = (String)optionsMap.get("-lang-version");
                    compiler.setLanguageVersion(versionStr);
                }

                if (optionsMap.containsKey("qs")) {
                    query = (String) optionsMap.get("qs");
                    executable = compiler.compile(query);
                } else if (optionsMap.containsKey("q")) {
                    if (cwd != null && cwd.length() > 0 && cwd.startsWith("http")) {
                        URI cwdURI = null;
                        try {
                            cwdURI = SaxonCAPI.resolveFileToURI(processor, cwd, (String)optionsMap.get("q"));
                            URL url = cwdURI.toURL();
                            InputStream in = url.openStream();
                            executable = compiler.compile(in);
                            queryFile = new File("");
                        } catch (MalformedURLException e) {
                            throw new SaxonApiException(e);
                        } catch (IOException e) {
                            throw new SaxonApiException(e);
                        }

                    } else {
                        queryFile = resolveFile(processor, cwd, (String) optionsMap.get("q"), ResourceRequest.XQUERY_NATURE);

                        try {
                            executable = compiler.compile(queryFile);
                        } catch (IOException e) {
                            SaxonApiException ex = new SaxonApiException(e);
                            throw ex;
                        }
                    }
                }
            
        }


        if (query == null && queryFile == null) {
            SaxonApiException ex = new SaxonApiException("No Query supplied");
            throw ex;
        }

        XQueryEvaluator eval = executable.load();

        try {
            applyXQueryProperties(cwd, serializer, processor, eval, optionsMap, parameters, props);
        } catch (SaxonApiException ex) {
            throw ex;
        }
        eval.setErrorReporter(SaxonCException.errorReporter);
        return eval;
    }


    @CEntryPoint(name = "j_executeQueryToString", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer executeQueryToString(IsolateThread thread, AllocatorCCharFn alloc, ObjectHandle processorRef, CCharPointer ccwd,  CCharPointer c_queryStr, CCharPointer c_encoding, ObjectHandle processorDataRef) throws SaxonApiException {
        ProcessorDataAccumulator processorDataAccumulator = null;
        String[] params = null;
        Object[] values = null;
        if(processorDataRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        if(processorRef == WordFactory.nullPointer()) {
            throw new SaxonApiException("Processor pointer is null");
        }
        Processor processor = SaxonCAPI.objectHandles.get(processorRef);

        String queryStr = null;
        if(c_queryStr != WordFactory.nullPointer()) {
            queryStr = SaxonCAPI.toJavaString(c_queryStr, c_encoding);
        }

        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        ByteArrayOutputStream bStream = new ByteArrayOutputStream();
        Serializer serializer = processor.newSerializer(bStream);
        XQueryEvaluator eval = xqueryEvaluator(processor, cwd, queryStr, params, values, serializer);
        eval.run(serializer);
        return SaxonCAPI.stringToNative(alloc, bStream);

    }


    @CEntryPoint(name = "j_executeQueryToString_c", exceptionHandler = SaxonCExceptionCCharHandler.class)
        public static CCharPointer executeQueryToString_c(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, ObjectHandle processorDataRef) throws SaxonApiException {
            ProcessorDataAccumulator processorDataAccumulator = null;
            String[] params = null;
            Object[] values = null;
            if(processorDataRef != WordFactory.nullPointer()) {
                processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
                params = processorDataAccumulator.getParameterNames();
                values = processorDataAccumulator.getValues();
            }
            if(processorRef == WordFactory.nullPointer()) {
                throw new SaxonApiException("Processor pointer is null");
            }
            Processor processor = SaxonCAPI.objectHandles.get(processorRef);
            String cwd = null;
            if(ccwd != WordFactory.nullPointer()) {
                cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
            }
            ByteArrayOutputStream bStream = new ByteArrayOutputStream();
            Serializer serializer = processor.newSerializer(bStream);
            if(SaxonCAPI.debug) {
                System.err.println("Params length="+ params.length + ", Values length="+values.length);
                if(params.length > 0) {
                    for(int i=0; i< params.length;i++){
                        System.err.println("Params["+i+"]= "+ params[i]);
                    }
                }
            }
            XQueryEvaluator eval = xqueryEvaluator(processor, cwd, null, params, values, serializer);
            eval.run(serializer);
            final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(bStream.toString());
            return holder.get();

        }

    @CEntryPoint(name = "j_executeQueryToValue", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long executeQueryToValue(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer c_queryStr, CCharPointer c_encoding, ObjectHandle processorDataRef) throws SaxonApiException {
        ProcessorDataAccumulator processorDataAccumulator = null;
        String[] params = null;
        Object[] values = null;
        if(processorDataRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }
        Processor processor = SaxonCAPI.objectHandles.get(processorRef);
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String queryStr = null;
        if(c_queryStr != WordFactory.nullPointer()) {
            queryStr = SaxonCAPI.toJavaString(c_queryStr, c_encoding);
        }
        XQueryEvaluator eval = xqueryEvaluator(processor, cwd, queryStr, params, values, null);
        XdmValue value = eval.evaluate();
        ObjectHandle handle = SaxonCAPI.createHandle(value);
        return handle.rawValue();
    }

    @CEntryPoint(name = "j_executeQueryToFile", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long executeQueryToFile(IsolateThread thread, ObjectHandle processorRef, CCharPointer ccwd, CCharPointer c_queryStr, CCharPointer c_outFilename, CCharPointer c_encoding, ObjectHandle processorDataRef) throws SaxonApiException {
        ProcessorDataAccumulator processorDataAccumulator = null;
        String[] params = null;
        Object[] values = null;
        if(processorDataRef != WordFactory.nullPointer()) {
            processorDataAccumulator = SaxonCAPI.objectHandles.get(processorDataRef);
            params = processorDataAccumulator.getParameterNames();
            values = processorDataAccumulator.getValues();
        }

        Processor processor = SaxonCAPI.objectHandles.get(processorRef);
        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd = SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String outFilename = null;

        if(c_outFilename != WordFactory.nullPointer()) {
            outFilename = SaxonCAPI.toJavaString(c_outFilename, WordFactory.nullPointer());
        }

        String queryStr = null;
        if(c_queryStr != WordFactory.nullPointer()) {
            queryStr = SaxonCAPI.toJavaString(c_queryStr, c_encoding);
        }

        Serializer serializer = null;
            if(outFilename != null) {
                serializer = resolveOutputFile(processor, cwd, outFilename, ResourceRequest.XML_NATURE);
            }
            XQueryEvaluator eval = xqueryEvaluator(processor, cwd, queryStr, params, values,  serializer);
            if (serializer != null) {

                eval.run(serializer);
            } else {
                eval.run();
            }
            return SaxonCAPI.STATUS_OK;

    }

    public static void applyXQueryProperties(String cwd, Serializer serializer, Processor processor, XQueryEvaluator eval, Map<String, Object> optionsMap, Map<QName, XdmValue> parameters, Properties props) throws SaxonApiException {

        if (debug) {
            for (Object key : optionsMap.keySet()) {
                System.err.println((String)key);
            }
        }

        XdmItem item = null;
        File sourceFile = null;
        Source source = null;
        DocumentBuilder builder = processor.newDocumentBuilder();
        if (optionsMap.size() > 0) {


                if (optionsMap.containsKey("o")) {
                    serializer = SaxonCAPI.resolveOutputFile(processor, cwd, (String) optionsMap.get("o"), ResourceRequest.XML_NATURE);
                    eval.setDestination(serializer);
                }

                if (optionsMap.containsKey("dtd")) {
                    String option = (String) optionsMap.get("dtd");
                    if (option.equals("on")) {
                        builder.setDTDValidation(true);
                    } else {
                        builder.setDTDValidation(false);
                    }

                }

                if (optionsMap.containsKey("s")) {
                    source = SaxonCAPI.resolveFileToSource(processor, cwd, (String) optionsMap.get("s"), null);
                    eval.setSource(source);

                }

                if (optionsMap.containsKey("extc")) {
                    //extension function library path
                    //String libName = (String) optionsMap.get("extc");
                    //SaxonCAPI.setLibrary(cwd, libName);


                }

                if (optionsMap.containsKey("item") ) {
                    Object value = optionsMap.get("item");
                    if (value instanceof XdmItem) {
                        item = (XdmItem) value;
                    }
                    eval.setContextItem(item);
                }

                if (optionsMap.containsKey("node")) {
                    Object value = optionsMap.get("node");
                    if (value instanceof XdmItem) {
                        item = (XdmItem) value;
                    }
                    eval.setContextItem(item);
                }

            }

        if (!props.isEmpty() && serializer != null) {
            serializer.setOutputProperties(props);
        }

        for(Map.Entry<QName, XdmValue> entry : parameters.entrySet()) {
            eval.setExternalVariable(entry.getKey(), entry.getValue());
        }



    }

    public static void main(String[] args){


        String sourcefile = "xmark10.xml";
        String q1 = "q1";
        String outfile = "outfile.xml";
        Processor processor = new Processor(true);
        String[] paramcon = {"l"};

        String[] valuesCon = {"on"};
        //SaxonCAPI.applyToConfiguration(processor, paramcon, valuesCon);
        XQueryEngine xquery = new XQueryEngine(processor);
        /*XdmNode doc = xquery.parseXmlString(null, "<bookstore>\n" +
                "\n" +
                "<book category=\"COOKING\">\n" +
                "  <title lang=\"en\">Everyday Italian</title>\n" +
                "  <author>Giada De Laurentiis</author>\n" +
                "  <year>2005</year>\n" +
                "  <price>30.00</price>\n" +
                "</book></bookstore>");

        String[] params1 = {"s", "qs"};
        Object[] values1 = {"xmark10.xml", "count(//*)"};


        String[] params2 = {"node", "qs"};
        String[] params3 = {"qs"};
        String[] params4 = {"qs"};
        Object[] values2 = {doc, "saxon:line-number(/bookstore/book/title)"};
        Object[] values3 = {"declare variable $n external := 10; (1 to $n)!(. * .)"};
        Object[] value4 = {"declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
                "\n" +
                "declare option output:method 'json';\n" +
                "\n" +
                "map { 'prop1' : 'value 1', 'prop2' : 'value 2' }"};
        String cwd = "/Users/ond1/work/development/files/xmark";
        //String cwd = "C:///www///html///query";
        //String cwd = "http://localhost/query";
        //xquery.executeQueryToFile(cwd, "output1a.xml", params1, values1);
        String result0 = new String(xquery.executeQueryToString(cwd, params2, values2));
        //String result2 = xquery.executeQueryToString(cwd, params1, values1);
        XdmValue result = xquery.executeQueryToValue(cwd, params4, value4);

        String result2 = new String(xquery.executeQueryToString(cwd, params4, value4));


        // xquery.executeQueryToFile("/home/ond1/test/saxon9-5-1-1source", outfile, params1, values1);
        System.out.println("Result1=" + result.toString());

        System.out.println("Result2=" + result2);
        //System.out.println("Result2" + result2);


        QName qname = QName.XS_INTEGER;

        System.out.println(qname.getNamespaceURI());
        System.out.println(qname.getPrefix());
        System.out.println(qname.getLocalName());
        System.out.println(qname.toString());  */

    }


}
