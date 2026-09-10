package jaxptest;

import com.saxonica.config.JavaExtensionFunctionFactory;
import com.saxonica.config.JavaExtensionLibrary;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.expr.JavaExtensionFunctionCall;
import junit.framework.TestCase;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.xpath.XPathFactoryImpl;
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

/**
 * tests for mapping of Java types to XPath types
 */
public class ExtensionTest extends TestCase {



    /**
     * Test that a Java map can be used as an XPath map
     */

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

    public void testUntypedMapConversion() {
        final HashMap<String, String> map = new HashMap<String, String>();
        map.put("abc", "x");
        map.put("def", "y");

        XPathFactory factory = new XPathFactoryImpl();
        XPath x = factory.newXPath();
        x.setXPathVariableResolver(new XPathVariableResolver() {
            public Object resolveVariable(javax.xml.namespace.QName variableName) {
                if (variableName.getLocalPart().equals("x")) {
                    return XdmMap.makeMap(map);
                } else {
                    return null;
                }
            };
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

    public void testArrayParam() {

        byte[] bytes = new byte[]{'x', 'y', 'z'};

        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("map", NamespaceConstant.MAP_FUNCTIONS);
            XQueryExecutable exec = comp.compile(
                    "declare variable $in as Q{http://saxon.sf.net/java-type}_-B external; " +
                    "Q{java:java.lang.String}new($in) || '=>' || Q{http://saxon.sf.net/}typeAnnotation($in)");
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
            assertEquals("result", "A;B;", result.getStringValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

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
        FastStringBuffer f = new FastStringBuffer(16);
        for (String s : args) {
             f.append(s);
             f.append(t);
        }
        return f.toString();
    }

    /**
     * Test the Java extension function factory mechanism with XQuery
     */

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
            XQueryExecutable exec = comp.compile("declare namespace saxon='http://saxon.sf.net/'; saxon:line-number(//c)");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(doc);
            XdmAtomicValue result = (XdmAtomicValue)eval.evaluateSingle();
            assertTrue(result.getLongValue() == 6);
            assertTrue(messages.size() == 2);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    /**
     * Test the Java extension function factory mechanism with XSLT
     */

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
                    "<a xmlns:t='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/' t:version='2.0'>" +
                            "<t:value-of select='saxon:line-number(//c)'/></a>")));
            XsltTransformer eval = exec.load();
            eval.setInitialContextNode(doc);
            StringWriter sw = new StringWriter();
            eval.setDestination(proc.newSerializer(sw));
            eval.transform();
            assertTrue("result", sw.toString().endsWith(">6</a>"));
            assertEquals("messages", 2, messages.size());
        } catch (SaxonApiException e) {
            fail();
        }
    }


    public static List<String> messages = new ArrayList<>();

    public static class CallInterceptor extends JavaExtensionFunctionCall {

        @Override
        public Sequence<?> call(XPathContext context, Sequence[] argValues) throws XPathException {
            messages.add("before " + getDisplayName());
            Sequence<?> result = super.call(context, argValues);
            messages.add("after " + getDisplayName());
            return result;
        }
    }

    /**
     * Test that an externally-supplied parameter can be accessed from an extension function
     */

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
        public StructuredQName getFunctionQName() {
            return new StructuredQName("", "http://date.com", "getFormattedNow");
        }

        public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.value.SequenceType[]{net.sf.saxon.value.SequenceType.OPTIONAL_STRING};
        }

        public net.sf.saxon.value.SequenceType getResultType(net.sf.saxon.value.SequenceType[] sequenceTypes) {
            return net.sf.saxon.value.SequenceType.SINGLE_STRING;
        }

        public boolean trustResultType() {
            return true;
        }

        public int getMinimumNumberOfArguments() {
            return 0;
        }

        public int getMaximumNumberOfArguments() {
            return 1;
        }

        public ExtensionFunctionCall makeCallExpression() {
            return new ExtensionFunctionCall() {

                public Sequence<?> call(XPathContext context, Sequence[] arguments) {
                    return new StringValue("TEST");
                }
            };
        }
    }

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

}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



