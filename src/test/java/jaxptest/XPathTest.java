package jaxptest;

import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.xpath.XPathFactoryImpl;
import org.w3c.dom.Document;
import org.xml.sax.InputSource;

import javax.xml.namespace.NamespaceContext;
import javax.xml.namespace.QName;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import javax.xml.xpath.*;
import java.io.StringReader;
import java.util.*;

/**
 *
 */
public class XPathTest extends TestCase {

    public void testDummy() {
        assertEquals(2,2);
    }

    public void testXOMXPath() {
        try {
            nu.xom.Element element = new nu.xom.Element("doc");
            nu.xom.Document doc = new nu.xom.Document(element);
            XPathFactory xpathFactory = XPathFactory.newInstance(NamespaceConstant.OBJECT_MODEL_XOM, "net.sf.saxon.xpath.XPathFactoryImpl", null);
            String s = xpathFactory.newXPath().evaluate("count(//*)", doc);
            assertEquals(s, "1");
        } catch (XPathFactoryConfigurationException e) {
            e.printStackTrace();
            fail();
        } catch (XPathExpressionException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testXPathParseError() {
        final List<TransformerException> errors = new ArrayList<TransformerException>();
        try {
            Configuration config = new Configuration();
            config.setErrorListener(new ErrorListener() {
                public void error(TransformerException exception) throws TransformerException {
                    errors.add(exception);
                }

                public void warning(TransformerException exception) throws TransformerException {
                    errors.add(exception);
                }

                public void fatalError(TransformerException exception) throws TransformerException {
                    errors.add(exception);
                }
            });
            XPathFactory xpf = new XPathFactoryImpl(config);
            XPath xPath = xpf.newXPath();
            XPathExpression compiledExpression = xPath.compile("/*");
            compiledExpression.evaluate(new InputSource(new StringReader("This is invalid xml")));
            fail();
        } catch (Exception e) {
            assertFalse(errors.isEmpty());
        }
    }

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
                public String getPrefix(String string) {
                    return null;
                }

                @Override
                public Iterator getPrefixes(String string) {
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


}



