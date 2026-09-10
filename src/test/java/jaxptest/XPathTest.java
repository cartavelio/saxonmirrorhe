package jaxptest;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.xpath.XPathFactoryImpl;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.xml.sax.InputSource;

import javax.xml.namespace.NamespaceContext;
import javax.xml.namespace.QName;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.xpath.*;
import java.io.StringReader;
import java.util.Collections;
import java.util.Iterator;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class XPathTest {

    @Test
    public void testDummy() {
        assertEquals(2, 2);
    }

    @Test
    public void testXOMXPath() {
        try {
            nu.xom.Element element = new nu.xom.Element("doc");
            nu.xom.Document doc = new nu.xom.Document(element);
            XPathFactory xpathFactory = XPathFactory.newInstance(NamespaceConstant.OBJECT_MODEL_XOM, "net.sf.saxon.xpath.XPathFactoryImpl", null);
            String s = xpathFactory.newXPath().evaluate("count(//*)", doc);
            assertEquals("1", s);
        } catch (XPathFactoryConfigurationException | XPathExpressionException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testXPathParseError() {
        try {
            Configuration config = new Configuration();
            XPathFactory xpf = new XPathFactoryImpl(config);
            XPath xPath = xpf.newXPath();
            XPathExpression compiledExpression = xPath.compile("/*");
            compiledExpression.evaluate(new InputSource(new StringReader("This is invalid xml")));
            fail();
        } catch (Exception e) {
            // OK;
        }
    }

    @Test
    public void testBug2554() {
        try {

            String xml = "<root><test name=\"test\"/></root>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            DocumentBuilder db = factory.newDocumentBuilder();
            Document doc = db.parse(new InputSource(new StringReader(xml)));
            XPathFactory factory2 = XPathFactory.newInstance(
                    XPathFactory.DEFAULT_OBJECT_MODEL_URI,
                    "net.sf.saxon.xpath.XPathFactoryImpl",
                    ClassLoader.getSystemClassLoader());
            XPath xPath = factory2.newXPath();
            final Map<String, String> variableMappings = Collections.singletonMap("name", "test");
            xPath.setXPathVariableResolver(new XPathVariableResolver() {

                @Override
                public Object resolveVariable(QName varName) {
                    return variableMappings.get(varName.getLocalPart());
                }
            });
            XPathExpression expr = xPath.compile("boolean(/root/test[@name=$name])");
            expr.evaluate(doc, XPathConstants.BOOLEAN);

        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testIssue6386() {
        // https://saxonica.plan.io/boards/3/topics/6386
        try {
            XPathFactory xpathFactory = new XPathFactoryImpl();
            XPath xpath = xpathFactory.newXPath();

            xpath.setNamespaceContext(new NamespaceContext() {
                @Override
                public String getNamespaceURI(String prefix) {
                    if (prefix.equals("xhtml")) {
                        return "http://www.w3.org/1999/xhtml";
                    } else {
                        return null;
                    }
                }

                @Override
                public String getPrefix(String str) {
                    return null;
                }

                @Override
                public Iterator getPrefixes(String str) {
                    return null;
                }
            });

            String xhtmlSample = "<html xmlns='http://www.w3.org/1999/xhtml'><head><title>This is a test</title></head><body><h1>Test</h1></body></html>";

            InputSource source = new InputSource(new StringReader(xhtmlSample));

            System.out.println("Found: " + xpath.evaluate("//xhtml:title", source));
        } catch (XPathExpressionException e) {
            fail();
        }
    }

    @Test
    public void testXPathQNameConstructor() {
        try {

            String xml = "<root><test name=\"test\"/></root>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            DocumentBuilder db = factory.newDocumentBuilder();
            Document doc = db.parse(new InputSource(new StringReader(xml)));

            XPathFactory factory2 = new net.sf.saxon.xpath.XPathFactoryImpl();
            XPath xPath = factory2.newXPath();
            String expression = "xs:QName('my:name') eq QName('http://my.com/', 'name')";
            xPath.setNamespaceContext(new NamespaceContext() {
                @Override
                public String getNamespaceURI(String prefix) {
                    if (prefix.equals("my")) {
                        return "http://my.com/";
                    } else if (prefix.equals("xs")) {
                        return NamespaceConstant.SCHEMA;
                    } else {
                        return null;
                    }
                }

                @Override
                public String getPrefix(String namespaceURI) {
                    throw new UnsupportedOperationException();
                }

                @Override
                public Iterator getPrefixes(String namespaceURI) {
                    throw new UnsupportedOperationException();
                }
            });

            XPathExpression expr = xPath.compile(expression);
            boolean b = (Boolean) expr.evaluate(doc, XPathConstants.BOOLEAN);
            assertTrue(b);

        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBug5361() {
        try {
            Configuration configuration = Configuration.newConfiguration();

            IndependentContext staticCtx = new IndependentContext(configuration) {
                {
                    declareVariable(null, "falseVar");
                }

                @Override
                public net.sf.saxon.expr.Expression bindVariable(StructuredQName qName) throws XPathException {
                    LocalVariableReference local = (LocalVariableReference) super.bindVariable(qName);
                    if (qName.getLocalPart().equals("falseVar")) {
                        local.setStaticType(
                                SequenceType.SINGLE_BOOLEAN,
                                BooleanValue.FALSE,
                                0);
                    }
                    return local;
                }
            };

            final XPathEvaluator xpathEvaluator = new XPathEvaluator(configuration);
            xpathEvaluator.setStaticContext(staticCtx);

            net.sf.saxon.sxpath.XPathExpression xpathExpression = xpathEvaluator.createExpression(
                    "//A[$falseVar=true()][child::*[2]]"
            );
            assertTrue(Literal.isEmptySequence(xpathExpression.getInternalExpression()));
        } catch (net.sf.saxon.trans.XPathException e) {
            fail();
        }

    }

    @Test
    public void testBug6003() {
        try {
            String xml = "<root><test name=\"test\"/></root>";
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            DocumentBuilder db = factory.newDocumentBuilder();
            Document doc = db.parse(new InputSource(new StringReader(xml)));

            XPathFactory factory2 = new XPathFactoryImpl();
            XPath xPath = factory2.newXPath();
            xPath.setXPathVariableResolver(variableName -> {
                if (variableName.getLocalPart().equals("y")) {
                    return 18;
                } else {
                    fail();
                    return null;
                }
            });
            String expression = "function($x){$x + $y}(42)";
            XPathExpression exp = xPath.compile(expression);
            double result = (double)exp.evaluate(doc, XPathConstants.NUMBER);
            assertEquals(60.0e0, result);
        } catch (Exception e) {
            fail(e);
        }


    }


}



