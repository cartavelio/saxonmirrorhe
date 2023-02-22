package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Bindery;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.StandardOutputResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMDocumentWrapper;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.XSLTTraceListener;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api XsltTransformer
 */
public class TestXsltTransformer {
    private final ConfigTest configTest = new ConfigTest();
    
    /**
     * Simple test
     */

    @Test
    public void testSimple() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty("http://saxon.sf.net/feature/timing", "true");

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "caffo/Transform2.xslt"))).load();

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            transformer.setDestination(serializer);
            long start = new Date().getTime();

            // Transform the source XML to System.out.
            for (int i = 0; i < 10; i++) {
                // Load the source document
                XdmNode input = processor.newDocumentBuilder().build(
                        new File(configTest.getDataDir() + "caffo/Input.xml"));
                // Set the root node of the source document to be the initial context node
                transformer.setInitialContextNode(input);
                transformer.transform();
            }

            long end = new Date().getTime();
            System.err.println("Av Time: " + (end - start) / 10 + "ms");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testXmlDeclaration(){
         try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<input/>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "  <xsl:output omit-xml-declaration=\"yes\"/>\n" +
                    "  <xsl:template match=\"/\"><output1/><output2/></xsl:template>\n" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);


            trans.setInitialContextNode(node);
            trans.setDestination(serializer);
            trans.transform();

            String result = os.toString();
            assertTrue(result.equals("<output1/><output2/>"), "output");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }


    /**
     * Test for the case where the context item is not the root
     */

    @Test
    public void testContextNotRoot() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template match='e'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);

            XdmSequenceIterator iter = node.axisIterator(Axis.DESCENDANT, new QName("e"));
            XdmNode e = (XdmNode) iter.next();
            trans.setInitialContextNode(e);
            trans.setDestination(serializer);
            trans.transform();

            String result = os.toString();
            assertFalse(result.contains("errorA"), "matched root");
            assertTrue(result.contains("[]"), "matched e");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }



    @Test
     public void testKeyMapFunction() {
        Processor processor = new Processor(true);
        XsltCompiler compiler = processor.newXsltCompiler();

        try {
            String source = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n" +
                    "\n" +
                    "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "\txmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:fn=\"http://www.w3.org/2005/xpath-functions\"\n" +
                    "\txmlns:saxon=\"http://saxon.sf.net/\" xmlns:map=\"http://www.w3.org/2005/xpath-functions/map\">\n" +
                    "\t<xsl:key name=\"key\" match=\"cars/car\" use=\"@id\" saxon:range-key=\"yes\"/>\n" +
                    "\t<xsl:variable name=\"car.doc\">\n" +
                    "\t\t<cars>\n" +
                    "\t\t\t<car id=\"1\"/>\n" +
                    "\t\t\t<car id=\"2\"/>\n" +
                    "\t\t\t<car id=\"3\"/>\n" +
                    "\t\t\t<car id=\"9\"/>\n" +
                    "\t\t</cars>\n" +
                    "\t</xsl:variable>\n" +
                    "\t<xsl:variable name=\"seq\" select=\"0,1,2,3,4,5\"/>\n" +
                    "\n" +
                    "\t<xsl:template name='main'>\n" +
                    "\t\t<xsl:text>\n" +
                    "</xsl:text>\n" +
                    "\t\t<xsl:variable name=\"map1\" select=\"saxon:key-map('key',$car.doc,'0','2')\"/>\n" +
                    "\t\t<xsl:value-of select=\"map:keys($map1)\"/>\n" +
                    "\t\t<xsl:text>\n" +
                    "</xsl:text>\n" +
                    "\t\t<xsl:variable name=\"map2\" select=\"saxon:key-map('key',$car.doc,'5','8')\"/>\n" +
                    "\t\t<xsl:value-of select=\"map:keys($map2)\"/>\n" +
                    "\n" +
                    "\t</xsl:template>\n" +
                    "</xsl:stylesheet>";

            XsltTransformer transformer = compiler.compile(new StreamSource(new StringReader(source))).load();
            transformer.setInitialTemplate(new QName("main"));
            XdmDestination dest = new XdmDestination();
            transformer.setDestination(dest);
            transformer.transform();
            XdmNode result = dest.getXdmNode();
            assertTrue(result.toString().contains("1 2"), "Result should only contain 1 and 2. Found:" + result.toString());
            assertFalse(result.toString().contains("9"), "Result should not contain 9. Found:" + result.toString());
        } catch (SaxonApiException ex) {
            fail(ex.getMessage());
        }
    }

    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    @Test
    public void testErrorLocationFromXdm() {
        try {
            // Norm Walsh reported getting line numbers > 2^20. Can't reproduce.
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();

            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:import href='file:///" + configTest.getDataDir() + "identity.xsl'/>\n" +
                    "<xsl:import href='http://imported'/>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:apply-templates select='*'>\n" +
                    "<xsl:with-param name='p' select='()'/>\n" +
                    "</xsl:apply-templates>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
            XdmNode style2 = builder.build(new StreamSource(sr, "http://importer/"));

            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='e' as='node()'>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:param name='p' as='node()*' select='()'/>\n" +
                    //"<e1/>" +
                    //"<e2/>" +
                    "<xsl:sequence select='$p'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
            final XdmNode style1 = builder.build(new StreamSource(sr, "http://imported/"));

            xqcomp.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.startsWith("http:")) {
                        return style1.asSource();
                    } else {
                        return null;
                    }
                }
            });

            XsltTransformer trans = xqcomp.compile(style2.asSource()).load();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setInitialContextNode(node);
            trans.setDestination(serializer);
            trans.transform();

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            int line = ((XPathException) e.getCause()).getLocator().getLineNumber();
            assertTrue(line >= 2 && line <= 6, "line=5");
        }
    }

        /**
     * Test created ErrorListener picks up errors in imported stylesheet
     */

    @Test
    public void testErrorInImportedStylesheet() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            @Override
            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            @Override
            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            @Override
            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);


            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorListener(listener);

            XsltTransformer trans = xqcomp.compile(new StreamSource(new File(configTest.getDataDir() + "main.xsl"))).load();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setInitialTemplate(new QName("main"));
            trans.setDestination(serializer);
            trans.transform();

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertTrue(fatals.size() > 0, "fatals");
            //e.printStackTrace();
        }
    }


    /**
     * Test for the case where the context item is not the root, when starting at a named template
     */

    @Test
    public void testContextNotRootNamedTemplate() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            XdmSequenceIterator iter = node.axisIterator(Axis.DESCENDANT, new QName("e"));
            XdmNode e = (XdmNode) iter.next();
            trans.setInitialContextNode(e);
            trans.setInitialTemplate(new QName("main"));
            trans.setDestination(serializer);
            trans.transform();

            String result = os.toString();
            assertFalse(result.contains("errorA"), "matched root");
            assertTrue(result.contains("[]"), "matched e");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Build a document from a non-root element of a DOMSource
     */

    @Test
    public void testNonRootDomSource() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'>///<xsl:apply-templates/></xsl:template>" +
                    "<xsl:template match='*'>***<xsl:apply-templates/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            String in = "<?xml version='1.0'?> \n" +
                    "<doc><a><b/></a></doc>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            org.w3c.dom.Document doc = db.parse(is);
            org.w3c.dom.Element ele = doc.getDocumentElement();
            xsl.setSource(new DOMSource(ele));
            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.setDestination(out);
            xsl.transform();
            assertFalse(writer.toString().contains("///"), "started at element node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to an XdmDestination
     */

    @Test
    public void testXdmDestination() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            xsl.setInitialTemplate(new QName("go"));
            XdmDestination out = new XdmDestination();
            xsl.setDestination(out);
            xsl.transform();
            XdmNode root = out.getXdmNode();
            assertTrue(root.getNodeKind() == XdmNodeKind.DOCUMENT, "result is document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to a XOM tree
     */

    @Test
    public void testXOMDestination() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            xsl.setInitialTemplate(new QName("go"));
            XdmDestination out = new XdmDestination();
            out.setTreeModel(XOMObjectModel.getInstance());
            xsl.setDestination(out);
            xsl.transform();
            XdmNode root = out.getXdmNode();
            assertTrue(root.getUnderlyingNode() instanceof XOMDocumentWrapper, "result is XOM document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }



    /**
     * Send output to a JDOM2 tree
     */

    @Test
    public void testJDOM2Destination() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            xsl.setInitialTemplate(new QName("go"));
            XdmDestination out = new XdmDestination();
            out.setTreeModel(JDOM2ObjectModel.getInstance());
            xsl.setDestination(out);
            xsl.transform();
            XdmNode root = out.getXdmNode();
            assertTrue(root.getUnderlyingNode() instanceof net.sf.saxon.option.jdom2.JDOM2NodeWrapper, "result is JDOM2 document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testExtensionWithSchemaAccecss() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty("http://saxon.sf.net/feature/timing", "true");
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXsltTransformer' exclude-result-prefixes='my'>" +
                    "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                    "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer transformer = exec.load();

            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(sm.newSchemaValidator());
            XdmNode books = builder.build(new File(configTest.getDataDir() + "books.xml"));
            transformer.setInitialContextNode(books);
            // Create a serializer
            //String outfile = @"C:/MyJava/users/caffo/Output.xml";
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            transformer.setDestination(serializer);
            transformer.transform();
            assertTrue(sw.toString().contains("TITLE"));
            assertFalse(sw.toString().contains("AUTHOR"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Sample extension function - explores schema information
     *
     * @return true if the element defined by childName is optional in the context of the governing
     *         type of parent.
     */

    public static boolean isOptional(NodeInfo parent, QNameValue childName) throws Exception {
        Configuration config = parent.getConfiguration();
        SchemaType parentType = parent.getSchemaType();
        int fp = config.getNamePool().allocateFingerprint(
                NamespaceUri.of(childName.getComponent(AccessorFn.Component.NAMESPACE).getStringValue()),
                childName.getComponent(AccessorFn.Component.LOCALNAME).getStringValue());
        int cardinality = ((ComplexType) parentType).getElementParticleCardinality(fp, true);
        return Cardinality.allowsZero(cardinality);
    }

    /**
     * s9api transformation pipeline
     */

    @Test
    public void testShortPipeline() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();

            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            XdmNode in = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));
            XsltTransformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            XsltTransformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            stage1.setDestination(stage2);
            stage2.setDestination(out);
            stage1.setInitialContextNode(in);
            stage1.transform();
            assertTrue(sw.toString().contains("<a><a><z/></a></a>"), "output");

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testPipeline() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();

            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            XdmNode in = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));
            XsltTransformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            XsltTransformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            XsltTransformer stage3 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            XsltTransformer stage4 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            XsltTransformer stage5 = comp.compile(new StreamSource(new StringReader(xsl))).load();
            stage1.setDestination(stage2);
            stage2.setDestination(stage3);
            stage3.setDestination(stage4);
            stage4.setDestination(stage5);
            stage5.setDestination(out);
            stage1.setInitialContextNode(in);
            stage1.transform();
            assertTrue(sw.toString().contains("<a><a><a><a><a><z/></a></a></a></a></a>"), "output");

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    @Test
    public void testErrorNotificationFromSaxonParse() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            @Override
            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            @Override
            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            @Override
            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {
            // Norm Walsh reported saxon:parse errors not notified to the ErrorListener. Can't reproduce.

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                    "<xsl:template name='main'>\n" +
                    "<xsl:copy-of select=\"saxon:parse('&lt;doc>&lt;e>text&lt;/f>&lt;/doc>')\"/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");


            XsltTransformer trans = xqcomp.compile(new StreamSource(sr)).load();
            trans.setErrorListener(listener);

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setInitialTemplate(new QName("main"));
            trans.setDestination(serializer);
            trans.transform();

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertTrue(fatals.size() == 1, "fatals");
            //e.printStackTrace();
        }
    }

    @Test
    public void testSchemaAware11() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(FeatureKeys.TIMING, "true");
            proc.setConfigurationProperty(FeatureKeys.XSD_VERSION, "1.1");
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xs:element name='e'><xs:complexType><xs:sequence><xs:element name='p'/></xs:sequence><xs:assert test='exists(p)'/></xs:complexType></xs:element>" +
                            "</xs:schema></xsl:import-schema>" +
                            "<xsl:variable name='v'><e><p/></e></xsl:variable>" +
                            "<xsl:template name='main'><xsl:copy-of select='$v' validation='strict'/></xsl:template>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer transformer = exec.load();
            transformer.setInitialTemplate(new QName("main"));

            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            transformer.setDestination(serializer);
            transformer.transform();
            assertTrue(sw.toString().contains("<e>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testValidationWarnings() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, Boolean.TRUE);
            processor.getUnderlyingConfiguration().addSchemaSource(
                    new StreamSource(configTest.getDataDir() + "books.xsd"));

            Serializer serializer = processor.newSerializer();
            serializer.setOutputFile(new File(configTest.getTempDir() + "validationWarnings.xml"));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(true);

            String xsl = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "\n" +
                    "    <xsl:output indent=\"yes\" encoding=\"UTF-8\" />\n" +
                    "\n" +
                    "    <xsl:template match=\"/\">\n" +
                    "        <xsl:document validation='strict'>\n" +
                    "            <xsl:next-match />\n" +
                    "        </xsl:document>\n" +
                    "    </xsl:template>\n" +
                    "\n" +
                    "    <xsl:template match=\"*\">\n" +
                    "        <xsl:element name=\"{name()}\" namespace=\"{namespace-uri()}\">\n" +
                    "            <invalid-element/>\n" +
                    "        </xsl:element>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet> ";
            XsltTransformer transformer = compiler.compile(new StreamSource(new StringReader(xsl))).load();

            transformer.setSource(new StreamSource(new StringReader(xsl)));
            transformer.setDestination(serializer);
            transformer.transform();
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        } catch (SchemaException se) {
            se.printStackTrace();
            fail();
        }
    }

    /**
     * Test that the base URI of the result document is the base output URI
     */

    @Test
    public void testOutputBaseUri() throws SaxonApiException {
        Processor processor = new Processor(false);

        DocumentBuilder builder = processor.newDocumentBuilder();
        builder.setDTDValidation(false);
        builder.setLineNumbering(true);

        XdmNode document = builder.build(new File(configTest.getDataDir() + "books.xml"));
        XdmNode stylesheet = builder.build(new File(configTest.getDataDir() + "books.xsl"));

        XsltCompiler compiler = processor.newXsltCompiler();
        compiler.setSchemaAware(processor.isSchemaAware());
        XsltExecutable exec = compiler.compile(stylesheet.asSource());
        XsltTransformer transformer = exec.load();

        if (document != null) {
            transformer.setInitialContextNode(document);
        }
        XdmDestination result = new XdmDestination();
        transformer.setDestination(result);

        transformer.setBaseOutputURI("file://tmp/out.xml");

        transformer.setSchemaValidationMode(ValidationMode.DEFAULT);
        transformer.transform();

        XdmNode xformed = result.getXdmNode();
        assertEquals("file://tmp/out.xml", xformed.getBaseURI().toString(), "output base uri");
    }

    /**
     * test a call on XsltExecutable.getGlobalParameters
     * @throws SaxonApiException
     */

    @Test
    public void testGlobalParameters() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='2.0'>" +

                "                <xsl:param name='surname' as='xs:string' />" +
                "                <xsl:param name='age' as='xs:integer' />" +

                "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                "                </xsl:stylesheet>";

        XsltExecutable xsltExecutable = xsltCompiler.compile(new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8))));
        Map<QName, XsltExecutable.ParameterDetails> params = xsltExecutable.getGlobalParameters();

        System.out.println();
        int i = 0;
        for (QName key : params.keySet()) {
            System.out.println("Parameter n." + (++i) + ": " + key);
        }
        System.out.println();
        assertEquals(2, 2);
    }

    /**
     * test that a debugger can obtain access to all global variables including those that are unused
     * @throws SaxonApiException
     */

    @Test
    public void testDebuggerAccessToGlobalVariables() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();
        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +

                "                <xsl:variable name='surname' as='xs:string' select=\"'Kay'\"/>" +
                "                <xsl:variable name='age' as='xs:integer' select='63'/>" +

                "                <xsl:template name='main'>" +
                "                   <out a='{$age}' b='{Q{java:s9apitest.TestXsltTransformer}debug()}'/> " +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        xsltCompiler.setCompileWithTracing(true);
        XsltExecutable xsltExecutable = xsltCompiler.compile(new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8))));
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new XSLTTraceListener());
        StringWriter sw = new StringWriter();
        t.callTemplate(new QName("main"), processor.newSerializer(sw));

        assertTrue(sw.toString().contains("b=\"done\""), "result");
    }

    /**
     * Extension function used by the above test
     */
    public static StringValue debug(XPathContext context) {
        Controller controller = context.getController();
        Executable exec = controller.getExecutable();
        for (PackageData pd : exec.getPackages()) {
            Bindery bindery = controller.getBindery(pd);
            for (GlobalVariable var : pd.getGlobalVariableList()) {
                boolean used = !var.isUnused();
                System.out.println("variable " + var.getVariableQName().getDisplayName() +
                    " used=" + used +
                    " value=" + (used ? bindery.getGlobalVariableValue(var) : "*unused*"));
            }
        }
        return StringValue.makeStringValue("done");
    }

    @Test
    public void testNextInChain() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            processor.getUnderlyingConfiguration().setOutputURIResolver(new StandardOutputResolver() {
                StringWriter writer;

                @Override
                public Result resolve(String href, String base) throws XPathException {
                    writer = new StringWriter();
                    return new StreamResult(writer);
                }

                @Override
                public void close(Result result) throws XPathException {
                    System.err.println("Checking secondary result");
                    assertTrue(writer.toString().equals("<second>secondary</second>"));
                }
            });

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "next-in-chain/main.xsl"))).load();

            // Create a destination
            Serializer out = processor.newSerializer();
            StringWriter sw = new StringWriter();
            out.setOutputWriter(sw);

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(
                    new File(configTest.getDataDir() + "next-in-chain/test.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.setSource(input.asSource());
            transformer.setDestination(out);
            transformer.transform();
            assertTrue(sw.toString().equals("<second>principal</second>"));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test result document handler
     */

    @Test
    public void testResultDocHandler() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xsl = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                version='3.0'>" +
                "                <xsl:template name='main'>" +
                "                   <xsl:for-each select='5 to 10'> " +
                "                      <xsl:result-document href='out{position()}.xml'>" +
                "                          <out nr='{.}'/>" +
                "                      </xsl:result-document>" +
                "                   </xsl:for-each>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";


        XsltCompiler compiler = processor.newXsltCompiler();
        compiler.setSchemaAware(false);
        XsltExecutable exec = compiler.compile(new StreamSource(new StringReader(xsl)));
        XsltTransformer transformer = exec.load();
        transformer.setBaseOutputURI("http://base.output.com/");
        final Map<String, XdmDestination> results = new HashMap<>();
        transformer.setResultDocumentHandler(uri -> {
            XdmDestination dest = new XdmDestination();
            dest.setDestinationBaseURI(uri);
            assertTrue(uri.toASCIIString().startsWith("http://base.output.com/"));
            String href = uri.toASCIIString().replace("http://base.output.com/", "");
            results.put(href, dest);
            return dest;
        });
        XdmDestination result = new XdmDestination();
        transformer.setDestination(result);
        transformer.setInitialTemplate(new QName("main"));
        transformer.transform();

        assertEquals(6, results.size());
        assertNotNull(results.get("out1.xml"));
        System.err.println(results.get("out1.xml").getXdmNode().toString());
        assertTrue(results.get("out1.xml").getXdmNode().toString().contains("<out nr=\"5\"/>"));
        assertEquals("http://base.output.com/out1.xml", results.get("out1.xml").getXdmNode().getBaseURI().toString(), "base URI incorrect");
    }


    /**
     * Test calling a stylesheet named template getting raw results back as a Destination
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateRawDestination() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "                <xsl:template name='t'>" +
                "                   <xsl:param name='a' as='xs:double' select='3'/>" +
                "                   <xsl:param name='b' as='xs:float' select='4'/>" +
                "                   <xsl:sequence select='$a+10, $b+2'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        XsltTransformer t = xsltExecutable.load();
        t.setInitialTemplate(new QName("t"));
        RawDestination out = new RawDestination();
        t.setDestination(out);
        t.transform();
        XdmValue val = out.getXdmValue();
        assertEquals(2, val.size());
        assertEquals(13, ((XdmAtomicValue) val.itemAt(0)).getLongValue());
        assertEquals(6, ((XdmAtomicValue) val.itemAt(1)).getLongValue());
    }


    @Test
    public void testInitialTemplate() {
        // Test running xsl:initial-template by providing no source
        try {
            Processor processor = new Processor(false);

            String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                    + "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                    + "    exclude-result-prefixes=\"xs\"\n"
                    + "    version=\"3.0\" expand-text='yes'>\n"
                    + "    <xsl:param name=\"x\" as=\"xs:integer\"/>\n"
                    + "    <xsl:template name=\"xsl:initial-template\" as=\"xs:integer\">{$x + 2}</xsl:template>\n"
                    + "</xsl:stylesheet>";

            // Create a transformer for the stylesheet.
            XsltTransformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load();

            // Create a serializer
            StringWriter sw = new StringWriter();
            Serializer out = processor.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.METHOD, "text");
            transformer.setParameter(new QName("x"), new XdmAtomicValue(2));
            transformer.setDestination(out);
            transformer.transform();
            assertEquals("4", sw.toString());

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


}

