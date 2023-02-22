package jaxptest;

import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMNodeWrapper;
import net.sf.saxon.dom.DOMObjectModel;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.xpath.XPathFactoryImpl;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import javax.xml.namespace.NamespaceContext;
import javax.xml.namespace.QName;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.xpath.*;
import java.util.Iterator;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * Unit tests for free-standing XPath evaluation using a DOM.
 */
public class XPathDomVariableTest {

    @Test
    public void testFunctionCall() throws Throwable {
        // Initialize DOM objects
        DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
        dfactory.setNamespaceAware(true);
        DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
        final Document doc = docBuilder.newDocument();
        final Node node = doc.createElement("Y");
        doc.appendChild(node);

        // Initialize XPath
        System.setProperty(
                "javax.xml.xpath.XPathFactory:" + NamespaceConstant.OBJECT_MODEL_SAXON,
                "net.sf.saxon.xpath.XPathFactoryImpl");
        XPathFactory xpf = XPathFactory.newInstance(NamespaceConstant.OBJECT_MODEL_SAXON);
        XPath xpe = xpf.newXPath();

        // Initialize various resolvers
        xpe.setXPathVariableResolver(new XPathVariableResolver() {
            @Override
            public Object resolveVariable(QName variableName) {
                if ("Y".equals(variableName.getLocalPart())) {
                    return node;
                }
                return null;
            }
        });
        xpe.setXPathFunctionResolver(new XPathFunctionResolver() {
            @Override
            public XPathFunction resolveFunction(QName functionName, int arity) {
                if ("X".equals(functionName.getLocalPart())) {
                    return new XPathFunction() {
                        @Override
                        public Object evaluate(List args)
                                throws XPathFunctionException {
                            if (args.size() == 1) {
                                if (args.get(0) instanceof Node) {
                                    return args.get(0);
                                } else {
                                    fail("Argument is not a node");
                                }
                            }
                            fail("Argument is not a singleton");
                            return null;
                        }
                    };
                }
                return null;
            }
        });
        xpe.setNamespaceContext(new NamespaceContext() {
            @Override
            public String getNamespaceURI(String prefix) {
                if (prefix.equals("saxon")) {
                    return "http://saxon.sf.net/";
                } else {
                    if (prefix.equals("tns")) {
                        return "http://my.org.com/";
                    } else {
                        return null;
                    }
                }
            }

            @Override
            public String getPrefix(String uri) {
                return null;
            }

            @Override
            public Iterator getPrefixes(String uri) {
                return null;
            }
        });

        // Compile
        XPathExpression expr =
                xpe.compile("tns:X($Y)");

        // Execute
        Object result = expr.evaluate(doc, XPathConstants.NODESET);
        assertTrue(result instanceof NodeList, "Result is NodeList");
        assertEquals(1, ((NodeList)result).getLength(), "Result length 1?");
    }

    @Test
    public void testSetSource() throws Throwable {
        // Initialize DOM objects
        DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
        dfactory.setNamespaceAware(true);
        DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
        final Document doc = docBuilder.newDocument();
        final Node node = doc.createElement("Y");
        doc.appendChild(node);

        // Initialize XPath engine
        System.setProperty(
                "javax.xml.xpath.XPathFactory:" + NamespaceConstant.OBJECT_MODEL_SAXON,
                "net.sf.saxon.xpath.XPathFactoryImpl");
        XPathFactory xpf = XPathFactory.newInstance(NamespaceConstant.OBJECT_MODEL_SAXON);
        XPath xpe = xpf.newXPath();

        // Call buildDocument
        Configuration config = ((XPathFactoryImpl)xpf).getConfiguration();
        DOMObjectModel model = DOMObjectModel.getInstance();
        NodeInfo wrapper = model.wrap(node, config);
        assertTrue(wrapper instanceof DOMNodeWrapper, "Wrapped DOM node");

        // Check we can get a nodeList back
        XPathExpression exp = xpe.compile("/descendant-or-self::node()");
        NodeList nodeList = (NodeList)exp.evaluate(node, XPathConstants.NODESET);
        assertEquals(2, nodeList.getLength(), "2 nodes");
    }

    @Test
    public void testStringVariable() throws Throwable {
        // Initialize DOM objects
        DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
        dfactory.setNamespaceAware(true);
        DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
        final Document doc = docBuilder.newDocument();
        final Node node = doc.createElement("Y");
        doc.appendChild(node);

        // Initialize XPath
        System.setProperty(
                "javax.xml.xpath.XPathFactory:" + NamespaceConstant.OBJECT_MODEL_SAXON,
                "net.sf.saxon.xpath.XPathFactoryImpl");
        XPathFactory xpf = XPathFactory.newInstance(NamespaceConstant.OBJECT_MODEL_SAXON);
        XPath xpe = xpf.newXPath();

        // Initialize various resolvers
        xpe.setXPathVariableResolver(new XPathVariableResolver() {
            @Override
            public Object resolveVariable(QName variableName) {
                if ("S".equals(variableName.getLocalPart())) {
                    return "banana custard";
                }
                return null;
            }
        });
        xpe.setXPathFunctionResolver(new XPathFunctionResolver() {
            @Override
            public XPathFunction resolveFunction(QName functionName, int arity) {
                if ("F".equals(functionName.getLocalPart())) {
                    return new XPathFunction() {
                        @Override
                        public Object evaluate(List args)
                                throws XPathFunctionException {
                            if (args.size() == 1) {
                                if (args.get(0) instanceof String) {
                                    return args.get(0) + "!!";
                                } else {
                                    fail("Argument is a " + args.get(0).getClass().getName());
                                }
                            }
                            fail("Argument is not a singleton");
                            return null;
                        }
                    };
                }
                return null;
            }
        });
        xpe.setNamespaceContext(new NamespaceContext() {
            @Override
            public String getNamespaceURI(String prefix) {
                if (prefix.equals("saxon")) {
                    return "http://saxon.sf.net/";
                } else {
                    if (prefix.equals("tns")) {
                        return "http://my.org.com/";
                    } else {
                        return null;
                    }
                }
            }

            @Override
            public String getPrefix(String uri) {
                return null;
            }

            @Override
            public Iterator getPrefixes(String uri) {
                return null;
            }
        });

        // Compile
        XPathExpression expr =
                xpe.compile("tns:F($S)");

        // Execute
        String result = (String)expr.evaluate(doc, XPathConstants.STRING);
        assertTrue(result.equals("banana custard!!"), "Result OK?");
    }

}

