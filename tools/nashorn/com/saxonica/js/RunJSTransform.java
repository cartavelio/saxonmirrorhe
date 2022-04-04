package com.saxonica.js;

import jdk.nashorn.api.scripting.JSObject;
import jdk.nashorn.api.scripting.NashornException;
import jdk.nashorn.api.scripting.ScriptObjectMirror;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StandardErrorListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import javax.script.Invocable;
import javax.script.ScriptEngine;
import javax.script.ScriptEngineManager;
import javax.script.ScriptException;
import java.io.File;
import java.io.IOException;
import java.io.Reader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class RunJSTransform {

    private final static boolean TRACING = false;

    protected static JSObject driver;
    private String stylesheetFileName;
    private String sourceFileName;
    private String sourceContent;
    private String sourceBaseUri;
    private String initialTemplateName; /* as an EQName */
    private String initialModeName; /* as an EQName */
    private String initialFunctionName; /* as an EQName */
    private final Map<String, File> resources = new HashMap<String, File>();
    private Map<String, String> resultDocumentLocation = new HashMap<String, String>();
    private final Map<String, XdmValue> globalParams = new HashMap<String, XdmValue>();

    private List<XdmValue> initialFunctionArgs;
    private Map<String, List<String>> collections;

    public static void main(String[] args) throws Exception {
        String jsbase = args.length > 2 ? args[2] : System.getProperty("user.dir");
        RunJSTransform t = new RunJSTransform(jsbase);
        t.setStylesheetFileName(args[0]);
        t.setSourceFileName(args[1]);
        t.run();
    }

    protected static Invocable invocable;

    public RunJSTransform(String jsbase) throws ScriptException {
        init(jsbase);
    }

    public void init(String jsbase) throws ScriptException {
        if (invocable == null) {
            if (jsbase == null) {
                jsbase = System.getProperty("user.dir");
            }
            ScriptEngine engine = new ScriptEngineManager().getEngineByName("nashorn");
            if (engine == null) {
                throw new RuntimeException("No nashorn");
            }
            jsbase = jsbase.replaceAll("\\\\","/");

            JSHelper.jsBase = "file://" + jsbase + "/src/";

            engine.eval("load('" + jsbase + "/nashorn/JSTestDriver.js');");

            engine.eval("load('" + jsbase + "/src/showval.js');");
            engine.eval("load('" + jsbase + "/src/Atomic.js');");
            engine.eval("load('" + jsbase + "/src/Compare.js');");
            engine.eval("load('" + jsbase + "/src/nsnode.js');");
            engine.eval("load('" + jsbase + "/src/domutils.js');");
            engine.eval("load('" + jsbase + "/src/HashTrie.js');"); //
            engine.eval("load('" + jsbase + "/src/axis.js');");
            engine.eval("load('" + jsbase + "/src/xerror.js');");
            engine.eval("load('" + jsbase + "/src/Expr.js');");
            engine.eval("load('" + jsbase + "/src/mode.js');");
            engine.eval("load('" + jsbase + "/src/Iter.js');");
            engine.eval("load('" + jsbase + "/src/pattern.js');");
            engine.eval("load('" + jsbase + "/src/regex.js');");
            engine.eval("load('" + jsbase + "/src/CoreFn.js');");
            engine.eval("load('" + jsbase + "/src/ExtraFn.js');");
            engine.eval("load('" + jsbase + "/src/Calculate.js');");
            engine.eval("load('" + jsbase + "/src/Numberer.js');");
            engine.eval("load('" + jsbase + "/src/lib/Big.js');");
            engine.eval("load('" + jsbase + "/src/opt/ConvertJson.js');");
            engine.eval("load('" + jsbase + "/src/opt/Accum.js');");
            engine.eval("load('" + jsbase + "/src/opt/Merge.js');");

            engine.eval("load('" + jsbase + "/nashorn/xmldom/sax.js');");
            engine.eval("load('" + jsbase + "/nashorn/xmldom/dom.js');");
            engine.eval("load('" + jsbase + "/nashorn/xmldom/dom-parser.js');");
            engine.eval("load('" + jsbase + "/nashorn/xmldom/sax2.js');");

            engine.eval("load('" + jsbase + "/src/lib/xpath/XPathJS.js');");

            engine.eval("load('" + jsbase + "/src/transform.js');");



            invocable = (Invocable) engine;
            try {
                driver = (JSObject)engine.eval("JSTestDriver");
                invocable.invokeMethod(driver, "reset");
                invocable.invokeMethod(driver, "setJSBase", "file://" + jsbase + "/src/");
            } catch (NoSuchMethodException e) {
                throw new AssertionError(e);
            }
        }

    }

    public void addResource(String uri, File file) {
        System.err.println("Add resource " + uri + " -> " + file);
        resources.put(uri, file);
    }

    public void setStylesheetFileName(String fileName) {
        this.stylesheetFileName = fileName;
    }

    public void setSourceFileName(String fileName) {
        this.sourceFileName = fileName;
    }

    public void setSourceContent(String content, String baseURI) {
        this.sourceContent = content;
        this.sourceBaseUri = baseURI;
    }

    /**
     * Supply the initial template name
     * @param name the initial template name as an EQName
     */
    public void setInitialTemplateName(String name) {
        this.initialTemplateName = name;
    }

    /**
     * Supply the initial mode name
     * @param name the initial template name as an EQName
     */

    public void setInitialModeName(String name) {
        if ("Q{http://www.w3.org/1999/XSL/Transform}unnamed".equals(name)) {
            this.initialModeName = "#unnamed";
        } else {
            this.initialModeName = name;
        }
    }

    /**
     * Supply a global stylesheet parameter
     * @param name the parameter name as an EQName "Q{uri}local"
     * @param value the parameter value - must currently be a string, interpreted as UntypedAtomic
     */

    public void setParameter(String name, XdmValue value) {
        System.err.println("Set param " + name + " := " + value);
        globalParams.put(name, value);
    }

    public void setInitialFunctionDetails(QName initialFunctionName, List<XdmValue> initialFunctionArgs) {
        this.initialFunctionName = initialFunctionName.getEQName();
        this.initialFunctionArgs = initialFunctionArgs;
    }
    
    /**
     * Supply a map to receive result documents
     */

    public void setResultDocumentLocation(Map<String, String> map) {
        resultDocumentLocation = map;
    }

    public Map<String, List<String>> getCollections() {
        return collections;
    }

    public void setCollections(Map<String, List<String>> collections) {
        this.collections = collections;
    }

    /**
     * Run a transformation
     * @return the transformation result as an XDM value (i.e. the raw result sequence)
     * @throws SaxonApiException
     */

    public XdmValue run() throws SaxonApiException {

        try {
            boolean nativeResult = false;
            invocable.invokeMethod(driver, "reset");

            ScriptObjectMirror options = (ScriptObjectMirror) ((ScriptEngine) invocable).eval("new Object()");

            for (Map.Entry<String, XdmValue> entry : globalParams.entrySet()) {
                if (!(entry.getValue() instanceof XdmAtomicValue)) {
                    throw new IllegalArgumentException("parameter value must be an atomic value");
                }
                XdmItem value = (XdmItem) entry.getValue();
                invocable.invokeMethod(driver, "setParameter", options, entry.getKey(), value.getStringValue(),
                        ((AtomicValue)value.getUnderlyingValue()).getPrimitiveType().getName(), "global");
            }

            if (initialFunctionName != null) {
                nativeResult = true;
                invocable.invokeMethod(driver, "setInitialFunction", options, initialFunctionName);
                int argNr = 0;
                for (XdmValue arg : initialFunctionArgs) {
                    int index = 0;
                    for (XdmItem item : arg) {
                        if (!(item instanceof XdmAtomicValue)) {
                            throw new IllegalArgumentException("function parameter value must be an atomic value");
                        }
                        XdmAtomicValue value = (XdmAtomicValue) item;
                        String stringValue;
                        if (value.getPrimitiveTypeName().getLocalName().equals("QName")) {
                            QName qVal = value.getQNameValue();
                            stringValue = qVal.getEQName();
                        } else {
                            stringValue = value.getStringValue();
                        }
                        invocable.invokeMethod(driver, "setFunctionArg", options, argNr, index++, stringValue,
                                               value.getUnderlyingValue().getItemType().getName());

                    }
                    argNr++;
                }
            }

            for (Map.Entry<String, File> entry : resources.entrySet()) {
                invocable.invokeMethod(driver, "setRedirection", entry.getKey(), entry.getValue().getAbsolutePath());
            }

            if (collections != null) {
                for (Map.Entry<String, List<String>> entry : collections.entrySet()) {
                    String uri = entry.getKey();
                    invocable.invokeMethod(driver, "clearCollection", uri);
                    for (String content : entry.getValue()) {
                        invocable.invokeMethod(driver, "addCollectionEntry", uri, content);
                    }
                    if (entry.getValue().isEmpty()) {
                        invocable.invokeMethod(driver, "addCollectionEntry", uri, null);
                    }
                }
            }

            options.put("stylesheetFileName", stylesheetFileName);
            if (sourceFileName != null) {
                options.put("sourceFileName", sourceFileName);
                options.put("sourceLocation", new File(sourceFileName).toURI().toString());
            }
            if (sourceContent != null) {
                options.put("sourceText", sourceContent);
                options.put("sourceLocation", sourceBaseUri);
            }
            if (initialTemplateName != null) {
                options.put("initialTemplate", initialTemplateName);
            }
            if (initialModeName != null) {
                options.put("initialMode", initialModeName);
            }
            if (TRACING) {
                options.put("trace", true);
            }

            Object principalResult =
                    invocable.invokeMethod(driver, "transform", options);

            ScriptObjectMirror results = (ScriptObjectMirror)options.getMember("resultDocuments");
            for (Map.Entry<String, Object> e : results.entrySet()) {
                resultDocumentLocation.put(e.getKey(), e.getValue().toString());
            }

            return deliverPrincipalResult(principalResult, nativeResult, options);



        } catch (ScriptException scrEx) {
            if (scrEx.getCause() instanceof NashornException) {
                try {
                    Object errorObject = ((NashornException)scrEx.getCause()).getEcmaError();
                    String message = scrEx.getMessage();
                    String code = null;
                    Location loc = Loc.NONE;
                    if (errorObject instanceof ScriptObjectMirror) {
                        ScriptObjectMirror eo = (ScriptObjectMirror)errorObject;
                        code = errInfo(eo, "code");
                        message = errInfo(eo, "message");
                        String lineNr = errInfo(eo, "xsltLineNr");
                        String module = errInfo(eo, "xsltModule");
                        String compiledLineNr = errInfo(eo, "compiledLineNr");
                        if (module != null && compiledLineNr != null) {
                            module += " [sef#" + compiledLineNr + "]";
                        }
                        if (module != null && lineNr != null) {
                            loc = new Loc(module, Integer.parseInt(lineNr), -1);
                        }
                    }
                    //System.err.println(code + ": " + message);
                    if (code == null) {
                        System.err.println(message);
                        String jsStackTrace = NashornException.getScriptStackString(scrEx.getCause());
                        System.err.println(jsStackTrace);
                    } else {
                        XPathException err = new XPathException(message, code, loc);
                        if (code.charAt(0) == 'Q' && code.charAt(1) == '{') {
                            StructuredQName sqcode = StructuredQName.fromEQName((code));
                            err.setErrorCodeQName(sqcode);
                        }
                        new StandardErrorListener().fatalError(err);
                        throw new SaxonApiException(err);
                    }
                } catch (Exception e) {
                    if (!(e instanceof SaxonApiException)) {
                        scrEx.printStackTrace();
                    } else {
                        throw (SaxonApiException)e;
                    }
                }
            } else {
                scrEx.printStackTrace();
            }
            throw new SaxonApiException(scrEx);
//        } catch (IOException io) {
//            io.printStackTrace();
        } catch (NoSuchMethodException nsm) {
            nsm.printStackTrace();
        }
        return null;

    }

    public XdmValue deliverPrincipalResult(Object principalResult, boolean nativeResult, ScriptObjectMirror options) {
        //if (nativeResult) {
            try {
                return getPrincipalResult(options);
            } catch (XPathException e) {
                e.printStackTrace();
                return null;
            }

        /*} else {
            System.err.println("Result: " + principalResult);
            return new XdmAtomicValue(principalResult.toString());
        }*/
    }

    public XdmValue getPrincipalResult(ScriptObjectMirror options) throws XPathException {
        List<String> types = new ArrayList<String>();
        List<String> values = new ArrayList<String>();
        try {
            invocable.invokeMethod(driver, "getPrincipalResult", options, types, values);
        } catch (ScriptException e) {
            e.printStackTrace();
        } catch (NoSuchMethodException e) {
            e.printStackTrace();
        }
        return makeXdmValue(types, values);
    }

    private static XdmValue makeXdmValue(List<String> types, List<String> values) throws XPathException {
        List<Item> result = new ArrayList<>(types.size());
        for (int i=0; i<types.size(); i++) {
            String type = types.get(i);
            String value = values.get(i);
            if (type.equals("QName")) {
                result.add(new QNameValue(StructuredQName.fromEQName((value)), BuiltInAtomicType.QNAME));
            } else {
                BuiltInAtomicType bat = (BuiltInAtomicType) BuiltInType.getSchemaTypeByLocalName(type);
                AtomicValue av = Converter.convert(new StringValue(value), bat, ConversionRules.DEFAULT);
                result.add(av);
            }
        }
        return XdmValue.wrap(SequenceExtent.makeSequenceExtent(result));
    }

    /**
     * Remove unwanted JS object wrappers. Really not sure how these arise...
     * @param som a JS object
     * @return the JS object stripped of unwanted wrappers
     */

    private static ScriptObjectMirror unwrap(ScriptObjectMirror som) {
        while (som.size() == 1 && som.getSlot(0) instanceof ScriptObjectMirror) {
            som = (ScriptObjectMirror) som.getSlot(0);
        }
        return som;
    }

    private static XdmValue toXdmValue(ScriptObjectMirror jsValue) {
        //jsValue = unwrap(jsValue);
        int size = jsValue.size();
        List<Item> items = new ArrayList<Item>(size);
        for (int i=0; i<size; i++) {
            // TODO: handle nodes...
            ScriptObjectMirror som = (ScriptObjectMirror)jsValue.getSlot(i);
            som = unwrap(som);
            String type = (String)som.getMember("type");
            String stringValue = (String)som.callMember("toString");
            BuiltInAtomicType bat = (BuiltInAtomicType) BuiltInType.getSchemaTypeByLocalName(type);
            try {
                items.add(Converter.convert(new StringValue(stringValue), bat, ConversionRules.DEFAULT));
            } catch (ValidationException e) {
                e.printStackTrace();
            }
        }
        SequenceExtent extent = new SequenceExtent.Of<>(items);
        return XdmValue.wrap(extent);
    }

    private String errInfo(ScriptObjectMirror obj, String name) {
        Object value = obj.get(name);
        if (value == null) {
            return null;
        }
        if (value instanceof Double) {
            return "" + (int)((Double)value).doubleValue();
        }
        return value.toString();
    }

    /**
     * Read the contents of an unparsed text file
     *
     * @param reader  Reader to be used for reading the file
     * @return a CharSequence representing the contents of the file
     * @throws IOException    if a failure occurs reading the file
     */

    public static String readFile(Reader reader) throws IOException {
        StringBuilder sb = new StringBuilder();
        char[] buffer = new char[2048];
        boolean first = true;
        int actual;
        int line = 1;
        int column = 1;
        boolean latin = true;
        while (true) {
            actual = reader.read(buffer, 0, 2048);
            if (actual < 0) {
                break;
            }
            for (int c = 0; c < actual; ) {
                int ch32 = buffer[c++];
                if (ch32 == '\n') {
                    line++;
                    column = 0;
                }
                column++;
                if (ch32 > 255) {
                    latin = false;
                }
            }
            if (first) {
                first = false;
                if (buffer[0] == '\ufeff') {
                    // don't include the BOM in the result
                    sb.append(buffer, 1, actual - 1);
                } else {
                    sb.append(buffer, 0, actual);
                }
            } else {
                sb.append(buffer, 0, actual);
            }
        }
        reader.close();
        return sb.toString();
    }
//
//    public String fetchResource(String uri) {
//        try {
//            System.out.println("JSHelper resource " + uri);
//            uri = uri.replace(" ", "%20");
//            InputStream stream;
//            if (resources.get(uri) != null) {
//                stream = new FileInputStream(resources.get(uri));
//            } else {
//                stream = new URL(uri).openStream();
//            }
//            Reader reader = new BufferedReader(new InputStreamReader(stream, "utf-8"));
//            StringBuilder sb = new StringBuilder(2048);
//            char[] buffer = new char[2048];
//            boolean first = true;
//            int actual;
//            while (true) {
//                actual = reader.read(buffer, 0, 2048);
//                if (actual < 0) {
//                    break;
//                }
//                if (first) {
//                    first = false;
//                    if (buffer[0] == '\ufeff') {
//                        // don't include the BOM in the result
//                        sb.append(buffer, 1, actual - 1);
//                    } else {
//                        sb.append(buffer, 0, actual);
//                    }
//                } else {
//                    sb.append(buffer, 0, actual);
//                }
//            }
//            reader.close();
//            return sb.toString();
//        } catch (Exception err) {
//            String message = err.getMessage();
//            if (!message.contains(uri)) {
//                message = "Failed to read " + uri + ": " + message;
//            }
//            return "\ufeff" + err.getClass().getName() + ": " + message;
//        }
//    }

}

