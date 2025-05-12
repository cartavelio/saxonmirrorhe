package jaxptest;

import net.sf.saxon.lib.FeatureKeys;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLReader;

import javax.xml.parsers.*;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.lang.management.ManagementFactory;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


public class TestIdentityTransformer {
    private final ConfigTest configTest = new ConfigTest();

    private static TransformerFactory makeFactory() {
        // We don't simply do newInstance(), because that can give us Xalan in cases where the test is
        // run from a development environment.
        return TransformerFactory.newInstance("net.sf.saxon.TransformerFactoryImpl", ClassLoader.getSystemClassLoader());
    }

    @Test
    public void testIdentityTransformToDom() {
        try {
            Node node = DocumentBuilderFactory.newInstance().newDocumentBuilder().newDocument();
            Transformer identityTransformer = makeFactory().newTransformer();
            SAXSource source = new SAXSource(new InputSource(new ByteArrayInputStream("<hello/>".getBytes())));
            identityTransformer.transform(source, new DOMResult(node));
            assertEquals("hello", node.getChildNodes().item(0).getNodeName());
            //System.out.println(node.getChildNodes().item(0).getNodeName());
        } catch (ParserConfigurationException | TransformerException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testIdentityTransformFromDom() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            Document doc = db.newDocument();
            Element root = doc.createElement("Mpeg7");
            root.setAttribute("xmlns", "urn:mpeg:mpeg7:schema:2001");
            root.setAttribute("xmlns:xsi",
                "http://www.w3.org/2001/XMLSchema-instance");
            root.setAttribute("xmlns:mpeg7", "urn:mpeg:mpeg7:schema:2001");
            root.setAttribute("xsi:schemaLocation", "urn:mpeg:mpeg7:schema:2001 Mpeg7-2001.xsd");
            doc.appendChild(root);
            Transformer tFormer = makeFactory().newTransformer();
            tFormer.setOutputProperty(OutputKeys.METHOD, "xml");
            doc.normalize();
            Source sourceMPEG7 = new DOMSource(doc);
            StringWriter writer = new StringWriter();
            Result result = new StreamResult(writer);
            tFormer.transform(sourceMPEG7, result);
            //System.err.println(writer.toString());
            String s = writer.toString();
            assertTrue(s.contains("xmlns=\"urn:mpeg:mpeg7:schema:2001\""));
            assertTrue(s.contains("xmlns:mpeg7=\"urn:mpeg:mpeg7:schema:2001\""));
            assertTrue(s.contains("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""));
        } catch (ParserConfigurationException e) {
            fail(e.getMessage());
            e.printStackTrace();
        } catch (TransformerException e) {
            fail(e.getMessage());
            e.printStackTrace();
        }

    }

    @Test
    public void testWhitespaceStripping() {
        try {
            String in = "<rows>" + "   " +
                    "<r>\t<c>1</c>\n<c>2000</c><c>inq1</c> \t\r\t\n <c>inquiry1</c></r> " +
                    "</rows>";
            String expected = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<rows>" +
                    "<r><c>1</c><c>2000</c><c>inq1</c><c>inquiry1</c></r>" +
                    "</rows>";
            //System.setProperty(TRANSFORMERFACTORY_PROPERTY,TRANSFORMERFACTORY_IMPL);
            TransformerFactory transFactory = makeFactory();
            transFactory.setAttribute(
                    FeatureKeys.STRIP_WHITESPACE, "all");
            SAXTransformerFactory saxTransFactory = (SAXTransformerFactory)transFactory;
            Transformer identityTransformer = transFactory.newTransformer();

            SAXParserFactory factory = SAXParserFactory.newInstance();
            factory.setNamespaceAware(true);
            SAXParser saxParser = factory.newSAXParser();
            XMLReader reader = saxParser.getXMLReader();
            DOMResult result = new DOMResult();
            identityTransformer.transform(new SAXSource(reader, new InputSource(new StringReader(in))), result);

            //When we further transform the result in a String using a transformer, the
            //whitespace nodes are not there
            Transformer transf = transFactory.newTransformer();
            DOMSource inDOM = new DOMSource(result.getNode());
            StringWriter resWriter = new StringWriter();
            StreamResult resStream = new StreamResult(resWriter);
            transf.transform(inDOM, resStream);
            String res = resWriter.toString();
//            System.err.println(expected);
//            System.err.println(res);
            assertEquals(expected, res);
            // But the whitespace nodes are available in the DOM represenatation
            Node docNode = result.getNode(); // The document node
            Node rowsNode = docNode.getFirstChild();
            String name = rowsNode.getNodeName();
//            System.err.println(name);
            assertEquals("rows", name);
            Node rNode = rowsNode.getFirstChild();
            assertEquals(Node.ELEMENT_NODE, rNode.getNodeType());
            //System.err.println(Node.ELEMENT_NODE == rNode.getNodeType());
        } catch (ParserConfigurationException e) {
            fail(e.getMessage());
        } catch (SAXException e) {
            fail(e.getMessage());
        } catch (TransformerException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testResetOutputProperties() {
        try {
            System.setProperty("javax.xml.transform.TransformerFactory",
            "net.sf.saxon.TransformerFactoryImpl");
            Transformer transformer = makeFactory().newTransformer();
            //System.err.println(transformer.getOutputProperty(OutputKeys.METHOD));
            transformer.setOutputProperty(OutputKeys.METHOD, "xml");
            assertEquals("xml", transformer.getOutputProperty(OutputKeys.METHOD));

            // first reset
            transformer.reset();

            transformer.setOutputProperty(OutputKeys.METHOD, "text");
            //System.err.println(transformer.getOutputProperty(OutputKeys.METHOD));
            assertEquals("text", transformer.getOutputProperty(OutputKeys.METHOD));

            // second reset
            transformer.reset();

            //Assertion tests result is [xml]
            assertNull(transformer.getOutputProperty(OutputKeys.METHOD));
        } catch (TransformerConfigurationException e) {
            fail();
        }

    }

    @Test
    public void testErrorListener() {

        MyErrorListener listener = new MyErrorListener();
        try {
            TransformerFactory factory = makeFactory();
            Transformer trans = factory.newTransformer();
            trans.setErrorListener(listener);
            String in = "<illformed>";
            StreamSource ss = new StreamSource(new StringReader(in));
            StringWriter sw = new StringWriter();
            trans.transform(ss, new StreamResult(sw));
            fail("Unexpected success");
        } catch (TransformerConfigurationException e) {
            fail(e.getMessage());
        } catch (TransformerException e) {
            assertEquals(0, listener.warnings, "W");
            assertEquals(0, listener.errors, "E");
            assertEquals(1, listener.fatalErrors, "F");
        }

    }

    private static class MyErrorListener implements ErrorListener {
        public int warnings = 0;
        public int errors = 0;
        public int fatalErrors = 0;
        @Override
        public void warning(TransformerException exception) throws TransformerException {
            warnings++;
        }

        @Override
        public void error(TransformerException exception) throws TransformerException {
            errors++;
        }

        @Override
        public void fatalError(TransformerException exception) throws TransformerException {
            fatalErrors++;
        }
    }

    @Test
    public void testIdentityNamespaces() {
        try {
            String xml = "<?xml version=\"1.0\" ?>\n" +
                    "<test xmlns=\"mynamespace\">\n" +
                    "  <subelement>\n" +
                    "     <command>say hello</command>\n" +
                    "  </subelement> \n" +
                    "</test>";
            TransformerFactory factory = makeFactory();
            Transformer copy = factory.newTransformer();
            StringWriter sw = new StringWriter();
            copy.transform(new StreamSource(new StringReader(xml)), new StreamResult(sw));
            assert(sw.toString().contains("<test xmlns=\"mynamespace\">"));
            //System.err.println(sw.toString());
        } catch (TransformerException err) {
            fail();
        }
    }

    @Test
    public void testIdentityTransformFromDomPerformance() {
        try {
            ((com.sun.management.ThreadMXBean) ManagementFactory.getThreadMXBean()).setThreadAllocatedMemoryEnabled(true);

            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            Document doc = db.parse(new FileInputStream(configTest.getXMarkDir() + "xmark1.xml"));

            Transformer saxon = makeFactory().newTransformer();
            //Transformer xalan = TransformerFactory.newDefaultInstance();

            for (int i=0; i<10; i++) {
                saxon.transform(new DOMSource(doc), new StreamResult(new StringWriter()));
                //xalan.transform(new DOMSource(doc), new StreamResult(new StringWriter()));
            }

            for (int i = 0; i < 10; i++) {
                long used = getUsedMemory();
                long start = System.nanoTime();
                saxon.transform(new DOMSource(doc), new StreamResult(new StringWriter()));
                System.out.println("Saxon: " + (System.nanoTime() - start) / 1000000 + "ms in " + (getUsedMemory()-used) + "bytes");

//                used = getUsedMemory();
//                start = System.nanoTime();
//                xalan.transform(new DOMSource(doc), new StreamResult(new StringWriter()));
//                System.out.println("           Xalan: " + (System.nanoTime() - start) / 1000000 + "ms in " + (getUsedMemory() - used) + "bytes");
            }

        } catch (ParserConfigurationException | TransformerException | SAXException | IOException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    private static long getUsedMemory() {
        return ((com.sun.management.ThreadMXBean) ManagementFactory.getThreadMXBean()).getThreadAllocatedBytes(Thread.currentThread().getId());
//        gc();
//        long totalMemory = Runtime.getRuntime().totalMemory();
//        gc();
//        long freeMemory = Runtime.getRuntime().freeMemory();
//        long usedMemory = totalMemory - freeMemory;
//        return usedMemory;
    }

    private static void gc() {
        try {
            System.gc();
            Thread.sleep(100);
            System.runFinalization();
            Thread.sleep(100);
            System.gc();
            Thread.sleep(100);
            System.runFinalization();
            Thread.sleep(100);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

}
