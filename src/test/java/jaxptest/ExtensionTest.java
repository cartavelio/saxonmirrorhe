package jaxptest;

import com.saxonica.config.JavaExtensionFunctionFactory;
import com.saxonica.config.JavaExtensionLibrary;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.expr.JavaExtensionFunctionCall;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.option.jdom2.JDOM2DocumentWrapper;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.xpath.XPathFactoryImpl;
import org.jdom2.input.SAXBuilder;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Templates;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import javax.xml.xpath.XPathVariableResolver;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * tests for mapping of Java types to XPath types
 */
public class ExtensionTest {



    /**
     * Test that a Java map can be used as an XPath map
     */

    @Test
    public void testMapParam() {

        HashMap<String, String> map= new HashMap<String, String>();
        map.put("abc", "x");
        map.put("def", "y");

        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("map", NamespaceConstant.MAP_FUNCTIONS);
            XQueryExecutable exec = comp.compile("declare variable $in as map(*) external; $in('abc')");
            XQueryEvaluator eval = exec.load();
            eval.setExternalVariable(new QName("in"), new ItemTypeFactory(proc).newMap(map));
            XdmItem result = eval.evaluateSingle();
            assertTrue(result.getStringValue().equals("x"));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    /**
     * Test conversion of Java map on untyped JAXP interface
     */

    @Test
    public void testUntypedMapConversion() {
        final HashMap<String, String> map = new HashMap<String, String>();
        map.put("abc", "x");
        map.put("def", "y");

        XPathFactory factory = new XPathFactoryImpl();
        XPath x = factory.newXPath();
        x.setXPathVariableResolver(new XPathVariableResolver() {
            @Override
            public Object resolveVariable(javax.xml.namespace.QName variableName) {
                if (variableName.getLocalPart().equals("x")) {
                    return XdmMap.makeMap(map);
                } else {
                    return null;
                }
            }
        });
        try {
            String result = x.evaluate("$x('def')", new InputSource(new StringReader("<dummy/>")));
            assertEquals("y", result);
        } catch (XPathExpressionException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test mapping of classnames for Java array types
     */

    @Test
    public void testArrayParam() {

        // Changed 2021-10-21 to use spelling type-annotation rather than typeAnnotation

        byte[] bytes = new byte[]{'x', 'y', 'z'};

        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("map", NamespaceConstant.MAP_FUNCTIONS);
            XQueryExecutable exec = comp.compile(
                    "declare variable $in as Q{http://saxon.sf.net/java-type}_-B external; " +
                    "Q{java:java.lang.String}new($in) || '=>' || Q{http://saxon.sf.net/}type-annotation($in)");
            XQueryEvaluator eval = exec.load();
            eval.setExternalVariable(new QName("in"), new XdmExternalObject(bytes));
            XdmItem result = eval.evaluateSingle();
            assertEquals("xyz=>jt:_-B", result.getStringValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    /**
     * exslt:node-set is supported as a no-op even in Saxon-HE
     */

    @Test
    public void testExsltNodeset() {

        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            String xsl = "<e xsl:version='2.0' xmlns:c='http://exslt.org/common' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
            "<xsl:variable name='ns'><a><b/></a></xsl:variable>" +
            "<xsl:value-of select='name(c:node-set($ns)/a/b)'/></e>";
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = exec.load();
            t.setSource(new StreamSource(new StringReader("<x/>")));
            StringWriter sw = new StringWriter();
            t.setDestination(proc.newSerializer(sw));
            t.transform();
            assertTrue(sw.toString().endsWith(">b</e>"));
        } catch (SaxonApiException e) {
            fail();
        }
    }


    @Test
    public void testVarArgs() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("map", NamespaceConstant.MAP_FUNCTIONS);
            XQueryExecutable exec = comp.compile(
                    "Q{java:java.lang.String}format('%d|%d', (1,2))");
            XQueryEvaluator eval = exec.load();
            XdmItem result = eval.evaluateSingle();
            assertTrue(result.getStringValue().equals("1|2"));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testAtomize() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("map", NamespaceConstant.MAP_FUNCTIONS);
            XQueryExecutable exec = comp.compile(
                    "Q{java:java.lang.Integer}parseInt(<a>23</a>)");
            XQueryEvaluator eval = exec.load();
            XdmItem result = eval.evaluateSingle();
            assertTrue(result.getStringValue().equals("23"));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testVarArgsAtomize() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("map", NamespaceConstant.MAP_FUNCTIONS);
            XQueryExecutable exec = comp.compile(
                    "Q{java:jaxptest.ExtensionTest}format(';', (<a>A</a>,<b>B</b>))");
            XQueryEvaluator eval = exec.load();
            XdmItem result = eval.evaluateSingle();
            assertEquals("A;B;", result.getStringValue(), "result");
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testClassNameInLocalName() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryExecutable exec = comp.compile(
                    "Q{" + NamespaceConstant.JAVA_TYPE + "}java.lang.Integer.parseInt(<a>23</a>)");
            XQueryEvaluator eval = exec.load();
            XdmItem result = eval.evaluateSingle();
            assertTrue(result.getStringValue().equals("23"));
        } catch (SaxonApiException e) {
            fail();
        }
    }


    public static String format(String t, String... args) {
        StringBuilder f = new StringBuilder(16);
        for (String s : args) {
             f.append(s);
             f.append(t);
        }
        return f.toString();
    }

    /**
     * Test the Java extension function factory mechanism with XQuery
     */

    @Test
    public void testExtensionFactoryXQuery() {
        try {
            messages.clear();
            Processor proc = new Processor(true);
            ProfessionalConfiguration config = (ProfessionalConfiguration)proc.getUnderlyingConfiguration();
            config.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            JavaExtensionFunctionFactory eff = new JavaExtensionFunctionFactory(config);
            eff.setExtensionFunctionClass(CallInterceptor.class);
            JavaExtensionLibrary jel = (JavaExtensionLibrary)config.getExtensionBinder("java");
            jel.setExtensionFunctionFactory(eff);

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            XdmNode doc = builder.build(new StreamSource(new StringReader("<a>\n<b/>\n<b/>\n<b/>\n<b/>\n<c/>\n</a>")));
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryExecutable exec = comp.compile("declare namespace jint='java:java.lang.Integer'; jint:bitCount(5)");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(doc);
            XdmAtomicValue result = (XdmAtomicValue)eval.evaluateSingle();
            assertTrue(result.getLongValue() == 2);
            assertTrue(messages.size() == 2);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    /**
     * Test the Java extension function factory mechanism with XSLT
     */

    @Test
    public void testExtensionFactoryXSLT() {
        try {
            messages.clear();
            Processor proc = new Processor(true);
            ProfessionalConfiguration config = (ProfessionalConfiguration)proc.getUnderlyingConfiguration();
            config.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            JavaExtensionFunctionFactory eff = new JavaExtensionFunctionFactory(config);
            eff.setExtensionFunctionClass(CallInterceptor.class);
            JavaExtensionLibrary jel = (JavaExtensionLibrary)config.getExtensionBinder("java");
            jel.setExtensionFunctionFactory(eff);

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            XdmNode doc = builder.build(new StreamSource(new StringReader("<a>\n<b/>\n<b/>\n<b/>\n<b/>\n<c/>\n</a>")));
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(
                    "<a xmlns:t='http://www.w3.org/1999/XSL/Transform' xmlns:jint='java:java.lang.Integer' t:version='2.0'>" +
                            "<t:value-of select='jint:bitCount(15)'/></a>")));
            XsltTransformer eval = exec.load();
            eval.setInitialContextNode(doc);
            StringWriter sw = new StringWriter();
            eval.setDestination(proc.newSerializer(sw));
            eval.transform();
            assertTrue(sw.toString().endsWith(">4</a>"), "result");
            assertEquals(2, messages.size(), "messages");
        } catch (SaxonApiException e) {
            fail();
        }
    }


    public static List<String> messages = new ArrayList<>();

    public static class CallInterceptor extends JavaExtensionFunctionCall {

        @Override
        public Sequence call(XPathContext context, Sequence[] argValues) throws XPathException {
            messages.add("before " + getDisplayName());
            Sequence result = super.call(context, argValues);
            messages.add("after " + getDisplayName());
            return result;
        }
    }

    /**
     * Test that an externally-supplied parameter can be accessed from an extension function
     */

    @Test
    public void testParameterAccess() {
        try {
            Processor proc = new Processor(true);
            XQueryEvaluator eval = proc.newXQueryCompiler().compile(
                    "declare variable $p external; Q{java:jaxptest.ExtensionTest}paramValue()").load();
            eval.setExternalVariable(new QName("p"), new XdmAtomicValue(17));
            XdmValue result = eval.evaluate();
            assertEquals("17", ((XdmAtomicValue)result).getStringValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public static int paramValue(XPathContext context) {
        try {
            Sequence value = context.getController().getParameter(new StructuredQName("", "", "p"));
            return (int)((NumericValue)value.head()).longValue();
        } catch (XPathException e) {
            return -999;
        }
    }


    public class DateExtensionFunction extends ExtensionFunctionDefinition {
        @Override
        public StructuredQName getFunctionQName() {
            return new StructuredQName("", "http://date.com", "getFormattedNow");
        }

        @Override
        public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.value.SequenceType[]{net.sf.saxon.value.SequenceType.OPTIONAL_STRING};
        }

        @Override
        public net.sf.saxon.value.SequenceType getResultType(net.sf.saxon.value.SequenceType[] sequenceTypes) {
            return net.sf.saxon.value.SequenceType.SINGLE_STRING;
        }

        @Override
        public boolean trustResultType() {
            return true;
        }

        @Override
        public int getMinimumNumberOfArguments() {
            return 0;
        }

        @Override
        public int getMaximumNumberOfArguments() {
            return 1;
        }

        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new ExtensionFunctionCall() {

                @Override
                public Sequence call(XPathContext context, Sequence[] arguments) {
                    return StringValue.bmp("TEST");
                }
            };
        }
    }

    @Test
    public void testIntrinsicExtension() {
        try {
            TransformerFactoryImpl factory = new TransformerFactoryImpl();
            factory.getConfiguration().registerExtensionFunction(new DateExtensionFunction());
            String xsl = "<e xsl:version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                    "result='{Q{http://date.com}getFormattedNow()}'/>";
            Templates t = factory.newTemplates(new StreamSource(new StringReader(xsl)));
            StringWriter sw = new StringWriter();
            t.newTransformer().transform(new StreamSource(new StringReader("<a/>")), new StreamResult(sw));
            System.err.println(sw.toString());
        } catch (TransformerConfigurationException tce) {
            tce.printStackTrace();
            fail();
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    public static org.jdom2.Document parseToJdom(String content) throws Exception {
        SAXBuilder builder = new SAXBuilder();
        return builder.build(new StringReader(content));
    }

    public static JDOM2DocumentWrapper parseToJdom2Wrapper(XPathContext context, String content) throws Exception {
        SAXBuilder builder = new SAXBuilder();
        org.jdom2.Document doc = builder.build(new StringReader(content));
        return new JDOM2DocumentWrapper(doc, context.getConfiguration());
    }

    @Test
    public void testJDOM2extension() {
        try {
            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().registerExternalObjectModel(new JDOM2ObjectModel());
            XQueryEvaluator eval = proc.newXQueryCompiler().compile(
                    "name(Q{java:jaxptest.ExtensionTest}parseToJdom('<magic/>')/*)").load();
            XdmValue result = eval.evaluate();
            assertEquals("magic", ((XdmAtomicValue) result).getStringValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testJDOM2WrapperExtension() {
        try {
            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().registerExternalObjectModel(new JDOM2ObjectModel());
            XQueryEvaluator eval = proc.newXQueryCompiler().compile(
                    "name(Q{java:jaxptest.ExtensionTest}parseToJdom2Wrapper('<magic/>')/*)").load();
            XdmValue result = eval.evaluate();
            assertEquals("magic", ((XdmAtomicValue) result).getStringValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testInstanceMethodStaticallyNull() {  // Bug 5044
        try {
            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().registerExternalObjectModel(new JDOM2ObjectModel());
            XQueryEvaluator eval = proc.newXQueryCompiler().compile(
                    "Q{java:java.lang.Object}hash-code(())").load();
            XdmValue result = eval.evaluate();
            assertEquals(1, result.size());
            assertTrue(result.itemAt(0).isAtomicValue());
            assertTrue(result.itemAt(0).getUnderlyingValue() instanceof Int64Value);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testInstanceMethodDynamicallyNull() {  // Bug 5044
        try {
            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().registerExternalObjectModel(new JDOM2ObjectModel());
            XQueryEvaluator eval = proc.newXQueryCompiler().compile(
                    "Q{java:java.lang.Object}hash-code(current-date()[year-from-date(.) lt 2000])").load();
            XdmValue result = eval.evaluate();
            assertEquals(1, result.size());
            assertTrue(result.itemAt(0).isAtomicValue());
            assertTrue(result.itemAt(0).getUnderlyingValue() instanceof Int64Value);
        } catch (SaxonApiException e) {
            fail();
        }
    }

}
