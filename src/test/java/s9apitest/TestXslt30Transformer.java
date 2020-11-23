////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. 
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.option.jdom.JDOMNodeWrapper;
import net.sf.saxon.option.jdom.JDOMObjectModel;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMDocumentWrapper;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.resource.BinaryResource;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.InstructionInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.QNameValue;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.*;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;


/**
 * JUnit tests for the s9api Xslt30Transformer
 *
 * @author Michael Kay
 * @date March 2015
 */
public class TestXslt30Transformer extends TestCase {

    /**
     * Simple test
     */

    public void testSimple() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty("http://saxon.sf.net/feature/timing", "true");

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(ConfigTest.DATA_DIR + "caffo/Transform2.xslt"))).load30();

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            long start = new Date().getTime();

            // Transform the source XML to System.out.
            for (int i = 0; i < 10; i++) {
                // Load the source document
                XdmNode input = processor.newDocumentBuilder().build(
                        new File(ConfigTest.DATA_DIR + "caffo/Input.xml"));
                // Set the root node of the source document to be the initial context node
                transformer.applyTemplates(input, serializer);
            }

            long end = new Date().getTime();
            System.err.println("Av Time: " + (end - start) / 10 + "ms");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Simple test using the transform() method
     */

    public void testTransformMethod() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setParameter(new QName("USE_GLOBAL_CONTEXT"), XdmAtomicValue.makeAtomicValue(true));
            Xslt30Transformer transformer = xsltCompiler.compile(
                    new StreamSource(new File(ConfigTest.DATA_DIR + "caffo/Transform2.xslt"))).load30();

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            long start = new Date().getTime();

            // Transform the source XML to System.out.

            StreamSource source = new StreamSource(
                    new File(ConfigTest.DATA_DIR + "caffo/Input.xml"));
            transformer.transform(source, serializer);

            long end = new Date().getTime();
            System.err.println("Av Time: " + (end - start) / 10 + "ms");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test for the case where the context item is not the root
     */

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
            Xslt30Transformer trans = xqcomp.compile(style).load30();

            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);

            XdmSequenceIterator iter = node.axisIterator(Axis.DESCENDANT, new QName("e"));
            XdmNode e = (XdmNode) iter.next();
            trans.setGlobalContextItem(node);
            trans.applyTemplates(e, serializer);

            String result = os.toString();
            assertFalse("matched root", result.contains("errorA"));
            assertTrue("matched e", result.contains("[]"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    public void testResolveUri() {

        try {
            Processor proc = new Processor(false);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version=\"3.0\"\n" +
                                                       "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                                       "  xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                                       "  xmlns:err=\"http://www.w3.org/2005/xqt-errors\">\n" +
                                                       "\n" +
                                                       "  <xsl:template name=\"go\">\n" +
                                                       "    <xsl:try>\n" +
                                                       "      <xsl:variable name=\"uri\" as=\"xs:anyURI\" select=\"\n" +
                                                       "        resolve-uri('notice trailing space /out.xml')\"/>\n" +
                                                       "\n" +
                                                       "      <xsl:message select=\"$uri\"/>\n" +
                                                       "\n" +
                                                       "      <xsl:result-document href=\"{$uri}\">\n" +
                                                       "        <out/>\n" +
                                                       "      </xsl:result-document>\n" +
                                                       "\n" +
                                                       "      <xsl:catch>\n" +
                                                       "        <xsl:sequence select=\"'$err:code: ' || $err:code  || ', $err:description: ' || $err:description\"/>\n" +
                                                       "      </xsl:catch>\n" +
                                                       "    </xsl:try>\n" +
                                                       "  </xsl:template>\n" +
                                                       "\n" +
                                                       "</xsl:stylesheet>");

            SAXSource style = new SAXSource(new InputSource(sr));
            Xslt30Transformer trans = xqcomp.compile(style).load30();

            XdmValue value = trans.callTemplate(new QName("go"));
            String result = value.toString();
            assertTrue("matched e", result.contains("code"));

        } catch (SaxonApiException ex) {
            fail(ex.getMessage());

        }
    }


    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    public void testErrorLocationFromXdm() {
        try {
            // Norm Walsh reported getting line numbers > 2^20. Can't reproduce.
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();

            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                                          "<xsl:import href='file:///" + ConfigTest.DATA_DIR + "identity.xsl'/>\n" +
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
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.startsWith("http:")) {
                        return style1.asSource();
                    } else {
                        return null;
                    }
                }
            });

            Xslt30Transformer trans = xqcomp.compile(style2.asSource()).load30();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.applyTemplates(node, serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            int line = ((XPathException) e.getCause()).getLocator().getLineNumber();
            assertTrue("lineNumber", line >= 2 && line <= 6);
        }
    }

    /**
     * Test created ErrorListener picks up errors in imported stylesheet
     */

    public void testErrorInImportedStylesheet() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);


            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorListener(listener);

            Xslt30Transformer trans = xqcomp.compile(new StreamSource((new File(ConfigTest.DATA_DIR + "main.xsl")))).load30();
            trans.setErrorListener(listener);
            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.callTemplate(new QName("main"), serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue("warnings", warnings.isEmpty());
            assertTrue("errors", errors.isEmpty());
            assertTrue("fatals", fatals.size() > 0);
            //e.printStackTrace();
        }
    }

    public void testEmbeddedStylesheet() {

        try {
            // Create a Processor instance.
            Processor processor = new Processor(false);
            XsltExecutable exec;

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(new File(ConfigTest.DATA_DIR, "../../../samples/data/books.xml"));
            //Console.WriteLine("=============== source document ===============");
            //Console.WriteLine(input.OuterXml);
            //Console.WriteLine("=========== end of source document ============");

            // Navigate to the xml-stylesheet processing instruction having the pseudo-attribute type=text/xsl;
            // then extract the value of the href pseudo-attribute if present

            String path = "/processing-instruction(xml-stylesheet)[matches(.,'type\\s*=\\s*[''\"\"]text/xsl[''\" \"]')]" +
                    "/replace(., '.*?href\\s*=\\s*[''\" \"](.*?)[''\" \"].*', '$1')";

            XPathSelector eval = processor.newXPathCompiler().compile(path).load();
            eval.setContextItem(input);
            XdmAtomicValue hrefval = (XdmAtomicValue) eval.evaluateSingle();
            String href = (hrefval == null ? null : hrefval.toString());

            if (href == null || href == "") {
                System.err.println("No suitable xml-stylesheet processing instruction found");
                return;

            } else if (href.charAt(0) == '#') {

                // The stylesheet is embedded in the source document and identified by a URI of the form "#id"

                System.err.println("Locating embedded stylesheet with href = " + href);
                String idpath = "id('" + href.substring(1) + "')";
                eval = processor.newXPathCompiler().compile(idpath).load();
                eval.setContextItem(input);
                XdmNode node = (XdmNode) eval.evaluateSingle();
                if (node == null) {
                    System.err.println("No element found with ID " + href.substring(1));
                    return;
                }
                exec = processor.newXsltCompiler().compile(node.asSource());

            } else {

                // The stylesheet is in an external document
                URI uri = ResolveURI.makeAbsolute(href, input.getBaseURI().toASCIIString());
                System.err.println("Locating stylesheet at uri = " + uri.toASCIIString());


                // Fetch and compile the referenced stylesheet
                exec = processor.newXsltCompiler().compile(new StreamSource((new File(uri))));
            }

            // Create a transformer
            XsltTransformer transformer = exec.load();

            // Run it
            transformer.setInitialContextNode(input);
            XdmDestination results = new XdmDestination();
            transformer.setDestination(results);
            transformer.transform();
            System.err.println(results.getXdmNode());


        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }


    /**
     * Test for the case where the context item is not the root, when starting at a named template
     */

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
            Xslt30Transformer trans = xqcomp.compile(style).load30();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setGlobalContextItem(node);
            trans.callTemplate(new QName("main"), serializer);

            String result = os.toString();
            assertFalse("matched root", result.contains("errorA"));
            assertTrue("matched e", result.contains("[]"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testUseAssociated() {

        try {

            Processor processor = new Processor(false);

            XsltCompiler compiler = processor.newXsltCompiler();

            String d = ConfigTest.DATA_DIR + "trax/";

            String foo_xml = d + "xml/foo.xml";
            Source sources =
                    compiler.getAssociatedStylesheet(new StreamSource(new File(foo_xml).toURI().toASCIIString()), null,
                                                     null, null);


            Xslt30Transformer transformer = compiler.compile(sources).load30();

            transformer.applyTemplates(new StreamSource(foo_xml),
                                       processor.newSerializer(System.out));


        } catch (SaxonApiException ex) {
            ex.printStackTrace();
            fail();
        }
    }

    /**
     * Build a document from a non-root element of a DOMSource
     */

    public void testNonRootDomSource() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template match='/'>///<xsl:apply-templates/></xsl:template>" +
                                                       "<xsl:template match='*'>***<xsl:apply-templates/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            String in = "<?xml version='1.0'?> \n" +
                    "<doc><a><b/></a></doc>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            org.w3c.dom.Document doc = db.parse(is);
            org.w3c.dom.Element ele = doc.getDocumentElement();
            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(new DOMSource(ele), out);
            assertFalse("started at element node", writer.toString().contains("///"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    public void testResultDocHandlerFunction() {

        Processor processor = new Processor(true);

        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        StringReader sr = new StringReader("<?xml version='1.0' encoding='UTF-8'?>" +
                                                   "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                                                   "   xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all' version='3.0'> " +
                                                   "<xsl:output method='html' indent='yes' html-version='5'/>" +
                                                   "<xsl:template match='/' name='xsl:initial-template'>" +
                                                   "   <html><head>" +
                                                   " <title>Test</title></head>" +
                                                   "<body> " +
                                                   "   <h1>Test</h1>" +
                                                   "</body>" +
                                                   "</html>" +
                                                   "<xsl:result-document href='result-1.json' method='json' indent='yes'>" +
                                                   "   <xsl:sequence select=\"map { 'name' : 'foo', 'values' : [ 'a', 'b', 'c' ]}\"/>" +
                                                   "</xsl:result-document>" +
                                                   " </xsl:template></xsl:stylesheet>");

        try {
            String base = new File(ConfigTest.DATA_DIR + "/sandpit").toURI().toASCIIString();

            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.TIMING, true);
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer transformer = proc.newXsltCompiler().compile(style).load();

            transformer.setInitialTemplate(new QName("http://www.w3.org/1999/XSL/Transform", "initial-template"));
            transformer.setDestination(proc.newSerializer(System.err));
            transformer.setBaseOutputURI(base);
            transformer.transform();

            System.err.println();

            MyResultDocumentHandler resDocHandler = new MyResultDocumentHandler(proc, base);
            transformer.setBaseOutputURI(base);

            transformer.setResultDocumentHandler(resDocHandler);

            transformer.setDestination(proc.newSerializer(System.err));
            transformer.transform();

            for (Serializer ser : resDocHandler.Serializers) {
                //  Console.WriteLine("{0}:\n{1}", ser.Href, ser.Result);
                System.err.println("test");
            }
            assertTrue("output", true);

        } catch (Exception ex) {
            //e.printStackTrace();

            fail(ex.getMessage());
        }
    }

    class MyResultDocumentHandler implements java.util.function.Function<URI, Destination> {
        private Processor proc;

        public List<Serializer> Serializers;
        private String baseUri;

        public MyResultDocumentHandler(Processor proc, String baseUri) {
            this.proc = proc;
            Serializers = new ArrayList<Serializer>();
            this.baseUri = baseUri;
        }

        @Override
        public Destination apply(URI uri) {
            Serializer ser = proc.newSerializer();
            try {
                ser.setDestinationBaseURI(new URI(baseUri));
            } catch (URISyntaxException e) {
                e.printStackTrace();
            }
            ser.setOutputFile(new File(uri));
            Serializers.add(ser);
            return ser;
        }
    }


    /**
     * Send output to an XdmDestination
     */

    public void testXdmDestination() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue("result is document node", root.getNodeKind() == XdmNodeKind.DOCUMENT);
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to an XdmDestination with item separators
     */

    public void testXdmDestinationWithItemSeparator() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='go'><xsl:comment>A</xsl:comment><out/><xsl:comment>Z</xsl:comment></xsl:template>" +
                            "<xsl:output method='xml' item-separator='§'/>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertEquals("<!--A-->§<out/>§<!--Z-->", root.toString());
            assertTrue("result is document node", root.getNodeKind() == XdmNodeKind.DOCUMENT);
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    /**
     * Send output to a XOM tree
     */

    public void testXOMDestination() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            out.setTreeModel(XOMObjectModel.getInstance());
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue("result is XOM document node", root.getUnderlyingNode() instanceof XOMDocumentWrapper);
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to a JDOM tree
     */

    public void testJDOMDestination() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            out.setTreeModel(JDOMObjectModel.getInstance());
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue("result is JDOM document node", root.getUnderlyingNode() instanceof JDOMNodeWrapper);
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to a JDOM2 tree
     */

    public void testJDOM2Destination() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            out.setTreeModel(JDOM2ObjectModel.getInstance());
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue("result is JDOM2 document node", root.getUnderlyingNode() instanceof net.sf.saxon.option.jdom2.JDOM2NodeWrapper);
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    public void testExtensionWithSchemaAccess() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty("http://saxon.sf.net/feature/timing", "true");
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                                                       "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                                                       "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();

            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xsd")));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(sm.newSchemaValidator());
            XdmNode books = builder.build(new File(ConfigTest.DATA_DIR + "books.xml"));
            // Create a serializer
            //String outfile = @"C:/MyJava/users/caffo/Output.xml";
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            transformer.applyTemplates(books, serializer);
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
     * type of parent.
     */

    public static boolean isOptional(NodeInfo parent, QNameValue childName) throws Exception {
        Configuration config = parent.getConfiguration();
        SchemaType parentType = parent.getSchemaType();
        StructuredQName qn = new StructuredQName("", childName.getComponent(AccessorFn.Component.NAMESPACE).getStringValue(),
                                                 childName.getComponent(AccessorFn.Component.LOCALNAME).getStringValue());
        int cardinality = ((ComplexType) parentType).getElementParticleCardinality(qn, true);
        return Cardinality.allowsZero(cardinality);
    }

    /**
     * s9api transformation pipeline
     */

    public void testPipeline() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            XdmNode in = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));
            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage3 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage4 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage5 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            XdmDestination d1 = new XdmDestination();
            stage1.applyTemplates(in, d1);
            XdmDestination d2 = new XdmDestination();
            stage2.applyTemplates(d1.getXdmNode(), d2);
            XdmDestination d3 = new XdmDestination();
            stage3.applyTemplates(d2.getXdmNode(), d3);
            XdmDestination d4 = new XdmDestination();
            stage4.applyTemplates(d3.getXdmNode(), d4);
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            stage5.applyTemplates(d4.getXdmNode(), out);
            assertTrue("output", sw.toString().contains("<a><a><a><a><a><z/></a></a></a></a></a>"));

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    /**
     * s9api streaming transformation pipeline
     */

    public void testPipeline2() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage3 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage4 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage5 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            stage1.applyTemplates(new StreamSource(new StringReader(xml)),
                                  stage2.asDocumentDestination(
                                          stage3.asDocumentDestination(
                                                  stage4.asDocumentDestination(
                                                          stage5.asDocumentDestination(out)))));
            System.err.println(sw.toString());
            assertTrue("output", sw.toString().contains("<a><a><a><a><a><z/></a></a></a></a></a>"));

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    public void testPipelineShort() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            stage1.applyTemplates(new StreamSource(new StringReader(xml)),
                                  stage2.asDocumentDestination(out));
            System.err.println(sw);
            assertTrue("output", sw.toString().contains("<a><a><z/></a></a>"));

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    public void testErrorNotificationFromSaxonParse() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {
            // Norm Walsh reported saxon:parse errors not notified to the ErrorListener. Can't reproduce.

            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().setErrorListener(listener);

            XsltCompiler xqcomp = proc.newXsltCompiler();

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:template name='main'>\n" +
                                                       "<xsl:copy-of select=\"saxon:parse('&lt;doc>&lt;e>text&lt;/f>&lt;/doc>')\"/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");


            Xslt30Transformer trans = xqcomp.compile(new StreamSource(sr)).load30();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.callTemplate(new QName("main"), serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue("warnings", warnings.isEmpty());
            assertTrue("errors", errors.isEmpty());
            assertTrue("fatals", fatals.size() == 1);
            //e.printStackTrace();
        }
    }

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
            Xslt30Transformer transformer = exec.load30();

            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            transformer.callTemplate(new QName("main"), serializer);
            assertTrue(sw.toString().contains("<e>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test option to treat validation failures as non-fatal
     */

    public void testValidationWarnings() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, Boolean.TRUE);
            processor.getUnderlyingConfiguration().addSchemaSource(
                    new StreamSource(ConfigTest.DATA_DIR + "books.xsd"));

            Serializer serializer = processor.newSerializer();
            serializer.setOutputFile(new File(ConfigTest.DATA_DIR + "out/validationWarnings.xml"));

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
            Xslt30Transformer transformer = compiler.compile(new StreamSource(new StringReader(xsl))).load30();

            transformer.applyTemplates(new StreamSource(new StringReader(xsl)), serializer);
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

    public void testOutputBaseUri() throws SaxonApiException {
        try {
            Processor processor = new Processor(false);

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(false);
            builder.setLineNumbering(true);

            XdmNode document = builder.build(new File(ConfigTest.DATA_DIR + "books.xml"));
            XdmNode stylesheet = builder.build(new File(ConfigTest.DATA_DIR + "books.xsl"));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(processor.isSchemaAware());
            XsltExecutable exec = compiler.compile(stylesheet.asSource());
            Xslt30Transformer transformer = exec.load30();

            XdmDestination result = new XdmDestination();
            transformer.setBaseOutputURI("file://tmp/out.xml");

            transformer.setSchemaValidationMode(ValidationMode.DEFAULT);
            transformer.setGlobalContextItem(document);
            transformer.applyTemplates(document, result);

            XdmNode xformed = result.getXdmNode();
            assertEquals("output base uri", "file://tmp/out.xml", xformed.getBaseURI().toString());
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }
    }

    /**
     * test a call on XsltExecutable.getGlobalParameters
     *
     * @throws net.sf.saxon.s9api.SaxonApiException
     */

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

        XsltExecutable xsltExecutable = xsltCompiler.compile(new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8"))));
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
     * Get declared types of global parameters
     *
     * @throws SaxonApiException
     * @throws UnsupportedEncodingException
     */

    public void testGlobalParameterTypes() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='2.0'>" +

                "                <xsl:param name='surname' as='xs:string' />" +
                "                <xsl:param name='age' as='xs:integer?' />" +
                "                <xsl:param name='doc' as='document-node()' />" +
                "                <xsl:param name='ele' as='element(pre:a, xs:date)+' xmlns:pre='http://pre/'/>" +

                "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                "                </xsl:stylesheet>";

        XsltExecutable xsltExecutable = xsltCompiler.compile(new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8"))));
        Map<QName, XsltExecutable.ParameterDetails> params = xsltExecutable.getGlobalParameters();
        ItemType t1 = params.get(new QName("surname")).getDeclaredItemType();
        assertTrue("t1", t1.toString().equals("Q{" + NamespaceConstant.SCHEMA + "}string"));
        OccurrenceIndicator c1 = params.get(new QName("surname")).getDeclaredCardinality();
        assertTrue("c1", c1.toString().equals(""));
        ItemType t2 = params.get(new QName("age")).getDeclaredItemType();
        assertTrue("t2", t2.toString().equals("Q{" + NamespaceConstant.SCHEMA + "}integer"));
        OccurrenceIndicator c2 = params.get(new QName("age")).getDeclaredCardinality();
        assertTrue("c2", c2.toString().equals("?"));
        ItemType t3 = params.get(new QName("doc")).getDeclaredItemType();
        assertTrue("t3", t3.toString().equals("document-node()"));
        OccurrenceIndicator c3 = params.get(new QName("doc")).getDeclaredCardinality();
        assertTrue("c3", c3.toString().equals(""));
        ItemType t4 = params.get(new QName("ele")).getDeclaredItemType();
        assertEquals("t4", "element(Q{http://pre/}a, Q{" + NamespaceConstant.SCHEMA + "}date)", t4.toString());
        OccurrenceIndicator c4 = params.get(new QName("ele")).getDeclaredCardinality();
        assertTrue("c4", c4.toString().equals("+"));
    }

    /**
     * Test an external  call on a stylesheet function
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallFunction() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                xmlns:f='http://localhost/'" +
                    "                version='3.0'>" +
                    "                <xsl:function name='f:add' visibility='public'>" +
                    "                  <xsl:param name='a'/><xsl:param name='b'/>" +
                    "                  <xsl:sequence select='$a + $b'/></xsl:function>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)});
            assertEquals(((XdmAtomicValue) v).getLongValue(), 5);
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test an external  call on a stylesheet function with argument conversion
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallFunctionArgConversion() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                xmlns:f='http://localhost/'" +
                    "                version='3.0'>" +
                    "                <xsl:function name='f:add' visibility='public'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:double'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:function>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)});
            assertEquals(((XdmAtomicValue) v).getDoubleValue(), 5.0e0);
            assertEquals(((XdmAtomicValue) v).getPrimitiveTypeName().getLocalName(), "double");
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test an external  call on a stylesheet function serializing the results
     * Also makes access to a stylesheet parameter which in turn accesses the global context item
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallFunctionWrapResults() throws SaxonApiException {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, "false");
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                xmlns:f='http://localhost/'" +
                    "                version='3.0'>" +
                    "                <xsl:param name='x' as='xs:integer'/>" +
                    "                <xsl:param name='y' select='.+2'/>" +
                    "                <xsl:function name='f:add' visibility='public'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:double'/>" +
                    "                   <xsl:sequence select='$a + $b + $x + $y'/>" +
                    "                </xsl:function>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            t.setGlobalContextItem(new XdmAtomicValue(20));
            Map<QName, XdmValue> globals = new HashMap<QName, XdmValue>();
            globals.put(new QName("x"), new XdmAtomicValue(30));
            t.setStylesheetParameters(globals);
            StringWriter sw = new StringWriter();
            Serializer s = processor.newSerializer();
            s.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            s.setOutputWriter(sw);
            t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)}, s);
            assertEquals("57", sw.toString());
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }


    /**
     * Test an external  call on a stylesheet function with invalid argument conversion
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallFunctionArgInvalid() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                xmlns:f='http://localhost/'" +
                    "                version='2.0'>" +
                    "                <xsl:function name='f:add'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:double'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:function>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue("3")});
            fail("Failed to throw error");
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            // OK
        }
    }

    /**
     * Test calling a stylesheet with an initial template with parameters
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallNamedTemplateWithParams() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='t'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("t"), se);
            assertEquals("output", "17", sw.toString());
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test calling a stylesheet with an initial template omitting a required parameters
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallNamedTemplateWithParamsRequired() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='t'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float' required='yes'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            t.setInitialTemplateParameters(params, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("t"), se);
            fail("failed to detect error");
        } catch (SaxonApiException e) {
            assertTrue("message", e.getMessage().contains("required parameter $b"));
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test calling a stylesheet with an initial template with tunnel parameters
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallNamedTemplateWithTunnelParams() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='t'>" +
                    "                   <xsl:call-template name='u'/>" +
                    "                </xsl:template>" +
                    "                <xsl:template name='u'>" +
                    "                   <xsl:param name='a' as='xs:double' tunnel='yes'/>" +
                    "                   <xsl:param name='b' as='xs:float' tunnel='yes'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, true);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("t"), se);
            assertEquals(sw.toString(), "17");
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test calling a stylesheet with an initial template rule with parameters
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallTemplateRuleWithParams() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template match='*'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='name(.), $a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.applyTemplates(new StreamSource(new StringReader("<e/>")), se);
            assertEquals(sw.toString(), "e 17");
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test calling a stylesheet with an initial template rule with parameters
     */

    public void testCallTemplateWithResultValidation() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document validation='strict'>" +
                    "                     <x>3</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("main"), se);
            assertEquals("output", "<x>3</x>", sw.toString());
        } catch (UnsupportedEncodingException | SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testCallTemplateWithResultValidationFailure() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' expand-text='yes' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:param name='p'>zzz</xsl:param>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document validation='strict'>" +
                    "                     <x>{$p}</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("main"), se);
            fail("unexpected success");
        } catch (UnsupportedEncodingException e) {
            fail(e.getMessage());
        } catch (SaxonApiException e) {
            System.err.println("Failed as expected: " + e.getMessage());
        }
    }

    public void testCallTemplateWithResultValidationAndItemSeparator() throws SaxonApiException {
        // The presence of the item separator in the output makes the document invalid
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' expand-text='yes' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:param name='p'>3</xsl:param>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document validation='strict' item-separator='|'>" +
                    "                     <xsl:comment>start</xsl:comment><x>{$p}</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.setResultDocumentHandler(uri -> se);
            t.callTemplate(new QName("main"), se);
            fail("unexpected success");
        } catch (UnsupportedEncodingException e) {
            fail();
        } catch (SaxonApiException e) {
            assertEquals("XTTE1550", e.getErrorCode().getLocalName());
        }
    }

    public void testCallTemplateWithSecondaryResultValidationAndItemSeparator() throws SaxonApiException {
        // The presence of the item separator in the output makes the document invalid
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' expand-text='yes' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:param name='p'>3</xsl:param>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document href='test-out.xml' validation='strict' item-separator='|'>" +
                    "                     <xsl:comment>start</xsl:comment><x>{$p}</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            t.setBaseOutputURI(new File(ConfigTest.DATA_DIR + "sandpit/dummy.xml").toURI().toASCIIString());
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("main"), se);
            fail("unexpected success");
        } catch (UnsupportedEncodingException e) {
            fail();
        } catch (SaxonApiException e) {
            assertEquals("XTTE1550", e.getErrorCode().getLocalName());
        }
    }

    /**
     * Raw output from named template
     */

    public void testCallTemplateNoParamsRaw() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='xsl:initial-template'><xsl:sequence select='42'/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmValue result = xsl.callTemplate(null);
            assertTrue("result is atomic value", result instanceof XdmAtomicValue);
            assertEquals("result is 42", 42, ((XdmAtomicValue) result).getLongValue());
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    /**
     * Test calling a stylesheet named template getting raw results back
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallNamedTemplateWithParamsRaw() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='t'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='$a+1, $b+1'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            XdmValue val = t.callTemplate(new QName("t"));
            assertEquals(2, val.size());
            assertEquals(13, ((XdmAtomicValue) val.itemAt(0)).getLongValue());
            assertEquals(6, ((XdmAtomicValue) val.itemAt(1)).getLongValue());
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test calling a stylesheet named template getting raw results back as a Destination
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testCallNamedTemplateWithParamsRawDestination() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='t'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='$a+1, $b+1'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            RawDestination out = new RawDestination();
            t.callTemplate(new QName("t"), out);
            XdmValue val = out.getXdmValue();
            assertEquals(2, val.size());
            assertEquals(13, ((XdmAtomicValue) val.itemAt(0)).getLongValue());
            assertEquals(6, ((XdmAtomicValue) val.itemAt(1)).getLongValue());
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }


    /**
     * Test calling a stylesheet with an initial template rule getting the raw result
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testApplyTemplatesRaw() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template match='*'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='., $a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            XdmValue result = t.applyTemplates(new StreamSource(new StringReader("<e/>")));
            assertEquals("size", 2, result.size());
            XdmItem first = result.itemAt(0);
            assertTrue("t1", first instanceof XdmNode);
            assertEquals("v1", ((XdmNode) first).getNodeName().getLocalName(), "e");
            XdmItem second = result.itemAt(1);
            assertTrue("t2", second instanceof XdmAtomicValue);
            assertEquals("v2", ((XdmAtomicValue) second).getDoubleValue(), 17e0);
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test calling a stylesheet with an initial template rule getting the serialized
     * result with item separators
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testApplyTemplatesToSerializer() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:output method='text' item-separator='~~'/>" +
                    "                <xsl:template match='.'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='., $a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.applyTemplates(new XdmAtomicValue(16), se);
            assertEquals("16~~17", sw.toString());
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }


    /**
     * Test calling a stylesheet with an initial template rule getting the raw result
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testApplyTemplatesToXdm() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template match='*'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float'/>" +
                    "                   <xsl:sequence select='., $a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            params.put(new QName("b"), new XdmAtomicValue(5));
            t.setInitialTemplateParameters(params, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            XdmValue result = t.applyTemplates(new StreamSource(new StringReader("<e/>")));
            assertEquals("size", 2, result.size());
            XdmItem first = result.itemAt(0);
            assertTrue("t1", first instanceof XdmNode);
            assertEquals("v1", ((XdmNode) first).getNodeName().getLocalName(), "e");
            XdmItem second = result.itemAt(1);
            assertTrue("t2", second instanceof XdmAtomicValue);
            assertEquals("v2", ((XdmAtomicValue) second).getDoubleValue(), 17e0);
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }


    /**
     * Test Strip-type-annotations with ApplyTemplates
     */

    public void testStripTypeAnnotations() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xslt:transform xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                                       "                xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                                                       "                xmlns:xslt=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                                       "                exclude-result-prefixes=\"xs my\"\n" +
                                                       "                input-type-annotations=\"strip\"\n" +
                                                       "                version=\"2.0\">\n" +
                                                       "<!-- Purpose: Show that when @input-type-annotations=\"strip\", is-nilled property of element  nodes is set to false.-->\n" +
                                                       "\n" +
                                                       "   <xslt:template match=\"/my:userNode\">\n" +
                                                       "      <out>\n" +
                                                       "         <xslt:value-of select=\"nilled(.)\"/>\n" +
                                                       "      </out>\n" +
                                                       "   </xslt:template>\n" +
                                                       "</xslt:transform>");

            SchemaManager manager = proc.getSchemaManager();
            comp.setSchemaAware(true);

            manager.load(new StreamSource(new File(ConfigTest.DATA_DIR + "varietyOfTypes.xsd")));

            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            String in = "<?xml version='1.0'?> \n" +
                    "<my:userNode xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                    "\t\t  xsi:schemaLocation=\"http://ns.example.com/strip-type varietyOfTypes.xsd\"\n" +
                    "\t\t  xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                    "\t \t  specialPart=\"123-AB\" listParts=\"000-SS 000-RR\" xsi:nil=\"true\"/>";

            SchemaValidator validator = manager.newSchemaValidator();
            validator.setLax(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(validator);
            StreamSource sDoc = new StreamSource(new ByteArrayInputStream(in.getBytes("UTF-8")), "file:///dummy/");
            XdmNode doc = builder.build(sDoc);

            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(doc, out);
            assertTrue("strip-type-annotations not working", writer.toString().contains("false"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test Strip-space with ApplyTemplates
     */

    public void testStripSpace() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xslt:transform xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                                       "                xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                                                       "                xmlns:xslt=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                                       "                exclude-result-prefixes=\"xs my\"\n" +
                                                       "                version=\"3.0\">\n" +
                                                       "   <xslt:strip-space elements='foo my:foo'/>\n" +
                                                       "   <xslt:mode on-no-match='deep-copy'/>\n" +
                                                       "</xslt:transform>");

            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            String in = "<?xml version='1.0'?> \n" +
                    "<my:userNode xmlns:my='http://ns.example.com/strip-type'><foo> </foo><bar> </bar><my:foo> </my:foo><my:bar> </my:bar></my:userNode>";

            DocumentBuilder builder = proc.newDocumentBuilder();
            StreamSource sDoc = new StreamSource(new ByteArrayInputStream(in.getBytes("UTF-8")), "file:///dummy/");
            XdmNode doc = builder.build(sDoc);

            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(doc, out);
            assertTrue("strip-space not working", writer.toString().contains("<foo/>"));
            assertTrue("strip-space not working", writer.toString().contains("<my:foo/>"));
            assertFalse("strip-space not working", writer.toString().contains("<bar/>"));
            assertFalse("strip-space not working", writer.toString().contains("<my:bar/>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    public void testFernandesPerf() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable t1 = comp.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "fernandes/t1.xsl")));
            XsltExecutable t2 = comp.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "fernandes/t2.xsl")));
            XdmNode xml = proc.newDocumentBuilder().build(new File(ConfigTest.DATA_DIR + "fernandes/test.xml"));
            for (int i = 0; i < 100; i++) {
                XdmDestination dest1 = new XdmDestination();
                t1.load30().applyTemplates(xml, dest1);
                XdmDestination dest2 = new XdmDestination();
                t2.load30().applyTemplates(dest1.getXdmNode(), dest2);
                xml = dest2.getXdmNode();
            }
            System.err.println("Starting...");
            long start = System.nanoTime();
            for (int i = 0; i < 1000; i++) {
                XdmDestination dest1 = new XdmDestination();
                t1.load30().applyTemplates(xml, dest1);
                XdmDestination dest2 = new XdmDestination();
                t2.load30().applyTemplates(dest1.getXdmNode(), dest2);
                xml = dest2.getXdmNode();
            }
            long actual = (System.nanoTime() - start) / 1000000;
            assertTrue("Actual: " + actual, actual < 6000);
            System.err.println("Total time: " + actual + "ms");
            if (actual > 4000) {
                System.err.println("**** Possible performance regression - run further checks ****");
            }
        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }

    public void testResultDocument() {
        // bug 2771
        try {
            String xsl = "<xsl:stylesheet version=\"3.0\" \n" +
                    "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "\n" +
                    "  <xsl:template match='a'>\n" +
                    "    <c>d</c>\n" +
                    "  </xsl:template>\n" +
                    "\n" +
                    "  <xsl:template match='whatever'>\n" +
                    "    <xsl:result-document href='out.xml'>\n" +
                    "      <e>f</e>\n" +
                    "    </xsl:result-document>\n" +
                    "  </xsl:template>\n" +
                    "\n" +
                    "</xsl:stylesheet>";
            Processor proc = new Processor(false);
            XsltCompiler compiler = proc.newXsltCompiler();
            Xslt30Transformer transformer = compiler.compile(new StreamSource(new StringReader(xsl))).load30();
            StreamSource source = new StreamSource(new StringReader("<a>b</a>"));
            XdmValue xdmValue = transformer.applyTemplates(source);
            assertEquals("size", 1, xdmValue.size());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test saxon:next-in-chain
     */

    public void testNextInChain() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty("http://saxon.sf.net/feature/timing", "true");

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
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(ConfigTest.DATA_DIR + "next-in-chain/main.xsl"))).load30();

            // Create a destination
            Serializer out = processor.newSerializer();
            StringWriter sw = new StringWriter();
            out.setOutputWriter(sw);

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(
                    new File(ConfigTest.DATA_DIR + "next-in-chain/test.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.applyTemplates(input, out);
            assertTrue(sw.toString().equals("<second>principal</second>"));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    public void testTraceListenerTemplates() {
        // Bug 3472
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setCompileWithTracing(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                            "<xsl:template match='a'><a><xsl:apply-templates/></a></xsl:template>\n" +
                            "<xsl:template match='b'/></xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            List<String> log = new ArrayList<String>();
            transformer.setTraceListener(new TraceListener() {
                public void setOutputDestination(Logger stream) {

                }

                public void open(Controller controller) {
                    log.add("open");
                }

                public void close() {
                    log.add("close");
                }

                public void enter(InstructionInfo instruction, XPathContext context) {
                    log.add("enter " + instruction.getConstructType());
                    if (instruction.getConstructType() == StandardNames.XSL_TEMPLATE) {
                        log.add("matching " + instruction.getProperty("match"));
                    }
                }

                public void leave(InstructionInfo instruction) {
                    log.add("leave " + instruction.getConstructType());
                }

                public void startCurrentItem(Item currentItem) {
                    log.add("start " + (currentItem instanceof NodeInfo ? ((NodeInfo) currentItem).getLocalPart() : currentItem.getStringValue()));
                }

                public void endCurrentItem(Item currentItem) {
                    log.add("end " + (currentItem instanceof NodeInfo ? ((NodeInfo) currentItem).getLocalPart() : currentItem.getStringValue()));
                }
            });
            String xml = "<a><b>stuff</b></a>";
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            transformer.applyTemplates(new StreamSource(new StringReader(xml)), serializer);
            assertTrue(log.size() > 10);
            assertTrue(log.contains("matching a"));
            assertTrue(log.contains("matching b"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    /**
     * Test result document handler
     */

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
        Xslt30Transformer transformer = exec.load30();
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
        transformer.callTemplate(new QName("main"), result);

        assertEquals(6, results.size());
        assertNotNull(results.get("out1.xml"));
        System.err.println(results.get("out1.xml").getXdmNode().toString());
        assertTrue(results.get("out1.xml").getXdmNode().toString().contains("<out nr=\"5\"/>"));
        assertEquals("base-uri", "http://base.output.com/out1.xml", results.get("out1.xml").getXdmNode().getBaseURI().toString());
    }

    /**
     * Test result document handler with raw destination
     */

    public void testResultDocHandlerRaw() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xsl = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                version='3.0'>" +
                "                <xsl:template name='main'>" +
                "                   <xsl:for-each select='5 to 10'> " +
                "                      <xsl:result-document href='out{position()}.xml' build-tree='no'>" +
                "                          <xsl:sequence select='. to .+10'/>" +
                "                      </xsl:result-document>" +
                "                   </xsl:for-each>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";


        XsltCompiler compiler = processor.newXsltCompiler();
        compiler.setSchemaAware(false);
        XsltExecutable exec = compiler.compile(new StreamSource(new StringReader(xsl)));
        Xslt30Transformer transformer = exec.load30();
        transformer.setBaseOutputURI("http://base.output.com/");
        final Map<String, RawDestination> results = new HashMap<>();
        transformer.setResultDocumentHandler(uri -> {
            RawDestination dest = new RawDestination();
            assertTrue(uri.toASCIIString().startsWith("http://base.output.com/"));
            String href = uri.toASCIIString().replace("http://base.output.com/", "");
            results.put(href, dest);
            return dest;
        });
        XdmDestination result = new XdmDestination();
        transformer.callTemplate(new QName("main"), result);

        assertEquals(6, results.size());
        assertNotNull(results.get("out1.xml"));
        System.err.println(results.get("out1.xml").getXdmValue().toString());
        assertEquals("size", 11, results.get("out1.xml").getXdmValue().size());
        assertEquals("value", 5, ((XdmAtomicValue) results.get("out1.xml").getXdmValue().itemAt(0)).getLongValue());
    }

    /**
     * Send output to an XdmDestination
     */

    public void testItemSeparatorToSerializer() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "  <xsl:template name='go'>"
                            + "<xsl:comment>start</xsl:comment><a><b/><c/></a><xsl:comment>end</xsl:comment>"
                            + "</xsl:template>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.METHOD, "xml");
            out.setOutputProperty(Serializer.Property.INDENT, "no");
            out.setOutputProperty(Serializer.Property.ITEM_SEPARATOR, "+++");
            xsl.callTemplate(new QName("go"), out);
            System.err.println(sw.toString());
            assertTrue(sw.toString().contains("<!--start-->+++"));
            assertTrue(sw.toString().contains("+++<!--end-->"));
            assertTrue(sw.toString().contains("<a><b/><c/></a>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    public void testTransformReturningFunction() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+42'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    public void testJarFileInput() {
        String xsl = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
                + "   <xsl:template match=\"/\">\n"
                + "    <html>\n"
                + "      <xsl:comment><xsl:copy-of select=\"doc('jar:file:" + ConfigTest.DATA_DIR + "temp/booksCopy.zip!/books.xml')\"/></xsl:comment>\n"
                + "\n"
                + "      <head>\n"
                + "        <xsl:attribute name=\"title\" select=\"current-dateTime()\"/>\n"
                + "      </head>\n"
                + "\n"
                + "      <body leftmargin=\"100\" >\n"
                + "        <xsl:apply-templates/>\n"
                + "      </body>\n"
                + "    </html>\n"
                + "  </xsl:template>"
                + "</xsl:transform>";
        try {
            File input = new File(ConfigTest.DATA_DIR + "books.xml");
            File inputCopy = new File(ConfigTest.DATA_DIR + "temp/booksCopy.zip");
            ZipOutputStream out = new ZipOutputStream(new FileOutputStream(inputCopy));
            ZipEntry e = new ZipEntry("books.xml");
            out.putNextEntry(e);

            byte[] data = BinaryResource.readBinaryFromStream(new FileInputStream(input), "books.xml");
            out.write(data, 0, data.length);
            out.closeEntry();

            out.close();

            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exp = comp.compile(new StreamSource(new StringReader(xsl), ConfigTest.DATA_DIR));
            XdmNode source = proc.newDocumentBuilder().build(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml")));
            StringWriter sw = new StringWriter();
            Serializer ser = proc.newSerializer(sw);
            ser.setOutputProperty(Serializer.Property.METHOD, "html");
            ser.setOutputProperty(Serializer.Property.INDENT, "yes");
            XsltTransformer trans = exp.load();
            trans.setInitialContextNode(source);
            trans.setDestination(ser);
            trans.transform();

            System.out.println("Output written to books.html");
            System.out.println("File delete (booksCopy.zip): " + inputCopy.delete());

            System.out.println("complete");
            //assertTrue(result instanceof XdmAtomicValue);
            //assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException | IOException | XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    /**
     * Test an external  call on a stylesheet function with argument conversion
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    public void testSequenceResult() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='xsl:initial-template' as='xs:integer+'>" +
                    "                     <xsl:sequence select='(1 to 5)' />        " +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes("UTF-8")), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            XdmValue res = t.callTemplate(null);
            int count = res.size();
            XdmAtomicValue value = (XdmAtomicValue) res.itemAt(0);
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Load and run a SEF file that specifies relocatable="yes" and calls the doc() function
     */

    public void testRelocatableExport() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            XsltExecutable executable = xsltCompiler.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "booksWithMain.sef")));

            Xslt30Transformer transformer = executable.load30();

            Serializer dest = processor.newSerializer();
            dest.setOutputFile(new File(ConfigTest.DATA_DIR + "/sandpit/OutputStep2.xml"));

            transformer.callTemplate(new QName("main"), dest);


        } catch (Exception e) {
            e.printStackTrace();
            fail();

        }

    }

    public void test4301() {
        try {
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <xsl:param name='code' as='xs:string'/> "
                    + "           <xsl:sequence select=\"if ($code = ('test')) then $code else error(QName('http://www.ctk.cz/error-code/', 'NotExists'))\"/>"
                    + "        </xsl:template>"
                    + "        </xsl:transform>";
            Processor processor = new Processor(true);
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            Map<QName, XdmValue> p = new HashMap<>();
            p.put(new QName("code"), new XdmAtomicValue("abc"));
            transformer.setInitialTemplateParameters(p, false);
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {

            if (err.getErrorCode().getNamespaceURI().equals("http://www.ctk.cz/error-code/")) {
                return;
            } else {
                err.printStackTrace();
                fail();
            }
        }
    }

    public void test3929inlineSchema() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, true);
            SchemaManager sm = processor.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            sm.load(new StreamSource(new StringReader(xsd0)));
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:import-schema>"
                    + "        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n"
                    + "         <xs:element name='b' type='xs:integer'/>\n"
                    + "        </xs:schema>"
                    + "       </xsl:import-schema>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <b xmlns='ns:a'>12</b>"
                    + "        </xsl:template>"
                    + "        </xsl:transform>";
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }


    public void test3929externalSchema() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, false);
            SchemaManager sm = processor.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            String xsd1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='b'/>\n" +
                    "</xs:schema>";
            sm.load(new StreamSource(new StringReader(xsd0)));
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:import-schema namespace='ns:a' schema-location='ns:a'/>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <b xmlns='ns:a'>12</b>"
                    + "        </xsl:template>"
                    + "        </xsl:transform>";
            XsltCompiler comp = processor.newXsltCompiler();
            comp.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.equals("ns:a")) {
                        return new StreamSource(new StringReader(xsd1));
                    } else {
                        return null;
                    }
                }
            });
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }


}



