package jaxptest;

import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.dom.NodeOverNodeInfo;
import net.sf.saxon.event.Builder;
import net.sf.saxon.jaxp.TransformerImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.OutputURIResolver;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.StringView;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.pattern.NodeKindTest;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.xml.sax.*;
import org.xml.sax.helpers.AttributesImpl;
import org.xml.sax.helpers.XMLFilterImpl;

import javax.xml.parsers.*;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXResult;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TransformerHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the JAXP Transformation API (trax).
 * Adapted by converting the TraxExamples file
 * originally created by Scott Boag, but much evolved since.
 */
public class TraxTest {
    private final ConfigTest configTest = new ConfigTest();

    private final String d = configTest.getDataDir() + "trax/";

    private final String foo_xml = d+"xml/foo.xml";
    private final String foo_xsl = d+"xsl/foo.xsl";
    private final String baz_xml = d+"xml/baz.xml";
    private final String baz_xsl = d+"xsl/baz.xsl";
    private final String foo2_xsl = d+"xsl/foo2.xsl";
    private final String foo3_xsl = d+"xsl/foo3.xsl";
    private final String text_xsl = d+"xsl/text.xsl";
    private final String cities_xml = d+"xml/cities.xml";
    private final String embedded_xml = d+"xml/embedded.xml";
    private final String multidoc_xsl = d+"xsl/multidoc.xsl";
    private final String identity_xsl = d+"xsl/identity.xsl";

    @Test
    public void testExampleParseOnly() {
        try {
            //System.setProperty("javax.xml.parsers.SAXParserFactory",
            //        "net.sf.saxon.aelfred.SAXParserFactoryImpl");
            SAXParserFactory factory = SAXParserFactory.newInstance();
            SAXParser parser = factory.newSAXParser();
            parser.parse(foo_xml, new org.xml.sax.helpers.DefaultHandler());
            System.out.println("\nParsing complete\n");
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleSimple1() {

        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(foo_xsl));

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(foo_xml),
                new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleSimple2() {


        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(foo_xsl));

            String outpath = configTest.getDataDir() + "temp/Simple2.out";

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(foo_xml),
                    new StreamResult(new File(outpath)));

            System.out.println("\nOutput written to " + outpath + "\n");
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleErrorListener() {

        try {
            TransformerFactory tfactory = TransformerFactory.newInstance();
            tfactory.setErrorListener(new ErrorListener() {
                @Override
                public void error(TransformerException exception) throws TransformerException {
                    throw exception;
                }

                @Override
                public void warning(TransformerException exception) throws TransformerException {
                    fail();
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
                    throw exception;
                }
            });
            // Create a transformer for the stylesheet.
            Transformer transformer =
                tfactory.newTransformer(new StreamSource(new StringReader("<badXML></goodXML>")));

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(foo_xml),
                new StreamResult(System.out));
            fail("Unexpected success");
        } catch (TransformerException ex) {
            System.err.println(ex);
            // OK, expected
        }
    }

    @Test
    public void testExampleFromStream() {


        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();
            InputStream xslIS =
                    new BufferedInputStream(new FileInputStream(baz_xsl));
            StreamSource xslSource = new StreamSource(xslIS);

            // The following line would be necessary if the stylesheet contained
            // an xsl:include or xsl:import with a relative URL
            // xslSource.setSystemId(xslID);

            // Create a transformer for the stylesheet.
            Transformer transformer = tfactory.newTransformer(xslSource);
            InputStream xmlIS =
                    new BufferedInputStream(new FileInputStream(foo_xml));
            StreamSource xmlSource = new StreamSource(xmlIS);

            // The following line would be necessary if the source document contained
            // a call on the document() function using a relative URL
            // xmlSource.setSystemId(sourceID);

            // Transform the source XML to System.out.
            transformer.transform(xmlSource, new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleFromReeader() {

        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Note that in this case the XML encoding can not be processed!
            Reader xslReader = new BufferedReader(new FileReader(foo_xsl));
            StreamSource xslSource = new StreamSource(xslReader);

            // Note that if we don't do this, relative URLs can not be resolved correctly!
            xslSource.setSystemId(foo_xsl);

            // Create a transformer for the stylesheet.
            Transformer transformer = tfactory.newTransformer(xslSource);

            // Note that in this case the XML encoding can not be processed!
            Reader xmlReader = new BufferedReader(new FileReader(foo_xml));
            StreamSource xmlSource = new StreamSource(xmlReader);

            // The following line would be needed if the source document contained
            // a relative URL
            // xmlSource.setSystemId(sourceID);

            // Transform the source XML to System.out.
            transformer.transform(xmlSource, new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleUseTemplatesObj() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a templates object, which is the processed,
            // thread-safe representation of the stylesheet.
            Templates templates = tfactory.newTemplates(new StreamSource(foo_xsl));

            // Illustrate the fact that you can make multiple transformers
            // from the same template.
            Transformer transformer1 = templates.newTransformer();
            Transformer transformer2 = templates.newTransformer();

            System.out.println("\n\n----- transform of " + foo_xml + " -----");
            transformer1.transform(new StreamSource(foo_xml),
                    new StreamResult(System.out));
            System.out.println("\n\n----- transform of " + baz_xml + " -----");
            transformer2.transform(new StreamSource(baz_xml),
                    new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleUseTemplatesHandler() {


        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Does this factory support SAX features?
            if (tfactory.getFeature(SAXSource.FEATURE)) {

                // If so, we can safely cast.
                SAXTransformerFactory stfactory =
                        ((SAXTransformerFactory) tfactory);

                // Create a Templates ContentHandler to handle parsing of the
                // stylesheet.
                javax.xml.transform.sax.TemplatesHandler templatesHandler =
                        stfactory.newTemplatesHandler();

                // Create an XMLReader and set its features.
                XMLReader reader = makeXMLReader();
                reader.setFeature("http://xml.org/sax/features/namespaces", true);
                reader.setFeature("http://xml.org/sax/features/namespace-prefixes", false);

                // Create a XMLFilter that modifies the stylesheet
                XMLFilter filter = new ModifyStylesheetFilter();
                filter.setParent(reader);

                filter.setContentHandler(templatesHandler);

                // Parse the stylesheet.
                filter.parse(new InputSource(foo_xsl));

                // Get the Templates object (generated during the parsing of the stylesheet)
                // from the TemplatesHandler.
                Templates templates = templatesHandler.getTemplates();

                // do the transformation
                templates.newTransformer().transform(
                        new StreamSource(foo_xml), new StreamResult(System.out));

            } else {
                System.out.println("Factory doesn't implement SAXTransformerFactory");
            }

        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleSAXResult() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Does this factory support SAX features?
            if (tfactory.getFeature(SAXResult.FEATURE)) {

                // Get a transformer in the normal way:
                Transformer transformer =
                        tfactory.newTransformer(new StreamSource(foo_xsl));

                // Get the source as a StreamSource
                Reader xmlReader = new BufferedReader(new FileReader(foo_xml));
                StreamSource xmlSource = new StreamSource(xmlReader);

                // Set the result handling to be a serialization to System.out.
                Result result = new SAXResult(new ExampleContentHandler());

                // Do the transformation
                transformer.transform(xmlSource, result);

            } else {
                System.out.println(
                        "Can't do exampleSAXResult because tfactory is not a SAXTransformerFactory");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

//    public void testStreamToSAXResult() {
//
//        try {
//
//            TransformerFactory tfactory = TransformerFactory.newInstance();
//
//            // Does this factory support SAX features?
//            if (tfactory.getFeature(SAXResult.FEATURE)) {
//
//                String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
//                        "               xmlns:atom=\"http://www.w3.org/2005/Atom\"\n" +
//                        "               version=\"3.0\">\n" +
//                        "\n" +
//                        "   <xsl:output method=\"html\" />\n" +
//                        "\n" +
//                        "   <xsl:template match=\"/\">\n" +
//                        "       <xsl:stream href=\"http://xmpp.wordpress.com:8008/firehose.xml?type=text/plain\">\n" +
//                        "           <html>\n" +
//                        "               <body>\n" +
//                        "                   <xsl:for-each select=\"stream/atom:entry/atom:source/atom:title\">\n" +
//                        "                       <p><xsl:value-of select=\".\" /></p>\n" +
//                        "                   </xsl:for-each>\n" +
//                        "               </body>\n" +
//                        "           </html>\n" +
//                        "       </xsl:stream>\n" +
//                        "   </xsl:template>\n" +
//                        "\n" +
//                        "</xsl:stylesheet>";
//
//                // Get a transformer in the normal way:
//                Transformer transformer =
//                        tfactory.newTransformer(new StreamSource(new StringReader(xsl)));
//
//                // Get the source as a StreamSource
//                StreamSource xmlSource = new StreamSource(new StringReader(xsl));
//
//                // Set the result handling to be a serialization to System.out.
//                ExampleContentHandler ech = new ExampleContentHandler();
//                ech.setPrintStream(new PrintStream(new FileOutputStream(new File("testStreamToSAXResult-ech-out.txt"))));
//                Result result = new SAXResult(ech);
//
//                // Do the transformation
//                transformer.transform(xmlSource, result);
//
//            } else {
//                System.out.println(
//                        "Can't do exampleSAXResult because tfactory is not a SAXTransformerFactory");
//            }
//        } catch (Exception ex) {
//            handleException(ex);
//        }
//    }


    @Test
    public void testExampleContentHandlerToContentHandler() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Does this factory support SAX features?
            if (tfactory.getFeature(SAXSource.FEATURE)) {

                // If so, we can safely cast.
                SAXTransformerFactory stfactory =
                        ((SAXTransformerFactory) tfactory);

                // A TransformerHandler is a ContentHandler that will listen for
                // SAX events, and transform them to the result.
                TransformerHandler handler =
                        stfactory.newTransformerHandler(new StreamSource(foo_xsl));

                // Set the result handling to be a serialization to System.out.
                Result result = new SAXResult(new ExampleContentHandler());

                handler.setResult(result);

                // Create a reader, and set it's content handler to be the TransformerHandler.
                XMLReader reader = makeXMLReader();

                reader.setContentHandler(handler);

                // It's a good idea for the parser to send lexical events.
                // The TransformerHandler is also a LexicalHandler.
                reader.setProperty(
                        "http://xml.org/sax/properties/lexical-handler", handler);

                // Parse the source XML, and send the parse events to the TransformerHandler.
                reader.parse(foo_xml);
            } else {
                System.out.println(
                        "Can't do exampleContentHandlerToContentHandler because tfactory is not a SAXTransformerFactory");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleXMLReader() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            if (tfactory.getFeature(SAXSource.FEATURE)) {
                XMLReader reader =
                        ((SAXTransformerFactory) tfactory)
                                .newXMLFilter(new StreamSource(new File(foo_xsl)));

                reader.setContentHandler(new ExampleContentHandler());
                reader.parse(new InputSource(new File(foo_xml).toURI().toString()));
            } else {
                System.out.println("tfactory does not support SAX features!");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleXMLFilter() {
        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();
            XMLReader reader = makeXMLReader();


            // The transformer will use a SAX parser as it's reader.

            try {
                reader.setFeature(
                        "http://xml.org/sax/features/namespace-prefixes", true);
            } catch (SAXException se) {
                fail("SAX Parser doesn't report namespace prefixes!");
            }

            XMLFilter filter =
                    ((SAXTransformerFactory) tfactory)
                            .newXMLFilter(new StreamSource(new File(foo_xsl)));

            filter.setParent(reader);
            filter.setContentHandler(new ExampleContentHandler());

            // Now, when you call transformer.parse, it will set itself as
            // the content handler for the parser object (it's "parent"), and
            // will then call the parse method on the parser.
            filter.parse(new InputSource(new File(foo_xml).toURI().toString()));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExampleXMLFilterChain() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // If one success, assume all will succeed.
            if (tfactory.getFeature(SAXSource.FEATURE)) {
                SAXTransformerFactory stf = (SAXTransformerFactory) tfactory;
                XMLReader reader = makeXMLReader();

                XMLFilter filter1 = stf.newXMLFilter(new StreamSource(new File(foo_xsl)));
                XMLFilter filter2 = stf.newXMLFilter(new StreamSource(new File(foo2_xsl)));
                XMLFilter filter3 = stf.newXMLFilter(new StreamSource(new File(foo3_xsl)));

                if (null != filter1)    // If one success, assume all were success.
                {

                    // transformer1 will use a SAX parser as it's reader.
                    filter1.setParent(reader);

                    // transformer2 will use transformer1 as it's reader.
                    filter2.setParent(filter1);

                    // transformer3 will use transformer2 as it's reader.
                    filter3.setParent(filter2);
                    filter3.setContentHandler(new ExampleContentHandler());

                    // filter3.setContentHandler(new org.xml.sax.helpers.DefaultHandler());
                    // Now, when you call transformer3 to parse, it will set
                    // itself as the ContentHandler for transform2, and
                    // call transform2.parse, which will set itself as the
                    // content handler for transform1, and call transform1.parse,
                    // which will set itself as the content listener for the
                    // SAX parser, and call parser.parse(new InputSource(foo_xml)).
                    filter3.parse(new InputSource(new File(foo_xml).toURI().toString()));
                } else {
                    System.out
                            .println("Can't do exampleXMLFilter because "
                                    + "tfactory doesn't support newXMLFilter()");
                }
            } else {
                System.out.println("Can't do exampleXMLFilter because "
                        + "tfactory is not a SAXTransformerFactory");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testSaxonToSaxon() {
        try {

            //TransformerFactory tfactory = TransformerFactory.newInstance();
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();
            Configuration config = ((TransformerFactoryImpl) tfactory).getConfiguration();
            NodeInfo doc = config.buildDocumentTree(new StreamSource(new File(foo_xsl))).getRootNode();
            Templates templates = tfactory.newTemplates(doc);
            System.err.println("Stylesheet built OK");

            Transformer transformer = templates.newTransformer();
            doc = config.buildDocumentTree(new StreamSource(new File(foo_xml))).getRootNode();
            System.err.println("Source document built OK");

            Builder builder = TreeModel.TINY_TREE.makeBuilder(config.makePipelineConfiguration());
            transformer.transform(doc, builder);
            System.err.println("Transformation done OK");

            // Serialize the output so we can see the transformation actually worked
            Transformer serializer = tfactory.newTransformer();
            serializer.transform(builder.getCurrentRoot(), new StreamResult(System.out));

            builder.getCurrentRoot();

        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testSaxonToSaxonNonRoot() {

        try {

            //TransformerFactory tfactory = TransformerFactory.newInstance();
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();
            Configuration config = ((TransformerFactoryImpl) tfactory).getConfiguration();
            NodeInfo sdoc = config.buildDocumentTree(new StreamSource(new File(foo_xsl).toURI().toString())).getRootNode();
            Templates templates = tfactory.newTemplates(sdoc);
            System.err.println("Stylesheet built OK");

            NodeInfo doc = config.buildDocumentTree(new StreamSource(new File(foo_xml).toURI().toString())).getRootNode();
            System.err.println("Source document built OK");
            NodeInfo start = null;
            out:
            for (NodeInfo child : doc.children(NodeKindTest.ELEMENT)) {
                for (NodeInfo gc : child.children(NodeKindTest.ELEMENT)) {
                    start = gc;
                    break out;
                }
            }
//            net.sf.saxon.sxpath.XPathEvaluator xpath = new net.sf.saxon.sxpath.XPathEvaluator(config);
//            XPathExpression xpe = xpath.createExpression("/*/*[1]");
//            XPathDynamicContext c = xpe.createDynamicContext(doc);
//            NodeInfo start = (NodeInfo) xpe.evaluateSingle(c);

            Transformer transformer = templates.newTransformer();

            Builder builder = TreeModel.TINY_TREE.makeBuilder(config.makePipelineConfiguration());
            transformer.transform(start, builder);
            System.err.println("Transformation done OK");

            // Serialize the output so we can see the transformation actually worked
            Transformer serializer = tfactory.newTransformer();
            serializer.transform(builder.getCurrentRoot(), new StreamResult(System.out));

            builder.getCurrentRoot();
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testDOMSubtree() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();
            Transformer transformer = tfactory.newTransformer();

            if (tfactory.getFeature(DOMSource.FEATURE)) {

                DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                System.err.println("Using DocumentBuilderFactory " + dfactory.getClass());

                dfactory.setNamespaceAware(true);

                DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
                System.err.println("Using DocumentBuilder " + docBuilder.getClass());


                Document doc = docBuilder.parse(
                        new InputSource(new File(foo_xml).toURI().toString()));
                Node bar = doc.getDocumentElement().getFirstChild();
                while (bar.getNodeType() != Node.ELEMENT_NODE) {
                    bar = bar.getNextSibling();
                }

                System.err.println("Source document built OK");

                DOMSource ds = new DOMSource(bar);
                ds.setSystemId(new File(foo_xml).toURI().toString());
                transformer.transform(ds, new StreamResult(System.out));
                System.err.println("Transformation done OK");
            } else {
                throw new SAXNotSupportedException(
                        "DOM node processing not supported!");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testDOMtoDOM() {

        try {

            String factory = getDocumentBuilderFactory();

            if (factory == null) {
                fail("No third-party DOM Builder found");
            }

            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", factory);

            TransformerFactory tfactory = TransformerFactory.newInstance();

            if (tfactory.getFeature(DOMSource.FEATURE)) {
                Templates templates;

                {
                    DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                    System.err.println("Using DocumentBuilderFactory " + dfactory.getClass());

                    dfactory.setNamespaceAware(true);

                    DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
                    System.err.println("Using DocumentBuilder " + docBuilder.getClass());

                    Node doc =
                            docBuilder.parse(new InputSource(new File(foo_xsl).toURI().toString()));
                    System.err.println("Stylesheet document built OK");
                    DOMSource dsource = new DOMSource(doc);

                    // If we don't do this, the transformer won't know how to
                    // resolve relative URLs in the stylesheet.
                    dsource.setSystemId(new File(foo_xsl).toURI().toString());

                    templates = tfactory.newTemplates(dsource);
                }

                Transformer transformer = templates.newTransformer();
                DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                DocumentBuilder docBuilder = dfactory.newDocumentBuilder();

                Document doc = docBuilder.parse(
                        new InputSource(new File(foo_xml).toURI().toString()));
                Node bar = doc.getDocumentElement().getFirstChild();
                while (bar.getNodeType() != Node.ELEMENT_NODE) {
                    bar = bar.getNextSibling();
                }

                System.err.println("Source document built OK");

                DOMSource ds = new DOMSource(bar);
                ds.setSystemId(new File(foo_xml).toURI().toString());

                // create a skeleton output document, to which
                // the transformation results will be added

                Document out = docBuilder.newDocument();
                Element extra = out.createElement("extra");
                out.appendChild(extra);


                transformer.transform(ds, new DOMResult(extra));
                System.err.println("Transformation done OK");

                // Serialize the output so we can see the transformation actually worked
                Transformer serializer = tfactory.newTransformer();

                serializer.transform(new DOMSource(out),
                        new StreamResult(System.out));

            } else {
                throw new SAXNotSupportedException(
                        "DOM node processing not supported!");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testDOMtoDOMAlienDoc() {

        try {

            String factory = getDocumentBuilderFactory();

            if (factory == null) {
                fail("No third-party DOM Builder found");
            }

            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", factory);

            TransformerFactory tfactory = TransformerFactory.newInstance();

            if (tfactory.getFeature(DOMSource.FEATURE)) {
                Templates templates;

                {
                    DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                    System.err.println("Using DocumentBuilderFactory " + dfactory.getClass());

                    dfactory.setNamespaceAware(true);

                    DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
                    System.err.println("Using DocumentBuilder " + docBuilder.getClass());

                    Node doc =
                            docBuilder.parse(new InputSource(new File(foo_xsl).toURI().toString()));
                    System.err.println("Stylesheet document built OK");
                    DOMSource dsource = new DOMSource(doc);

                    // If we don't do this, the transformer won't know how to
                    // resolve relative URLs in the stylesheet.
                    dsource.setSystemId(new File(foo_xsl).toURI().toString());

                    templates = tfactory.newTemplates(dsource);
                }

                Transformer transformer = templates.newTransformer();
                DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                DocumentBuilder docBuilder = dfactory.newDocumentBuilder();

                Document doc = docBuilder.parse(
                        new InputSource(new File(foo_xml).toURI().toString()));
                Node bar = doc.getDocumentElement().getFirstChild();
                while (bar.getNodeType() != Node.ELEMENT_NODE) {
                    bar = bar.getNextSibling();
                }

                System.err.println("Source document built OK");

                DOMSource ds = new DOMSource(bar);
                ds.setSystemId(new File(foo_xml).toURI().toString());

                // create a skeleton output document, to which
                // the transformation results will be added

                Document out = docBuilder.newDocument();

                transformer.transform(ds, new DOMResult(out));
                System.err.println("Transformation done OK");

                // Serialize the output so we can see the transformation actually worked
                Transformer serializer = tfactory.newTransformer();

                serializer.transform(new DOMSource(out),
                        new StreamResult(System.out));

            } else {
                throw new SAXNotSupportedException(
                        "DOM node processing not supported!");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testDOMtoDOMExisting() {

        try {

            String factory = getDocumentBuilderFactory();

            if (factory == null) {
                fail("No third-party DOM Builder found");
            }

            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", factory);

            TransformerFactory tfactory = TransformerFactory.newInstance();

            if (tfactory.getFeature(DOMResult.FEATURE)) {
                Templates templates = tfactory.newTemplates(new StreamSource(new File(foo_xsl)));
                Transformer transformer = templates.newTransformer();
                StreamSource input = new StreamSource(new File(foo_xml));

                DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                DocumentBuilder docBuilder = dfactory.newDocumentBuilder();

                // create a skeleton output document, to which
                // the transformation results will be added

                Document doc = docBuilder.newDocument();
                Element root = doc.createElementNS("", "doc");
                doc.appendChild(root);
                Element childOne = doc.createElementNS("", "childOne");
                root.appendChild(childOne);
                Element childTwo = doc.createElementNS("", "childTwo");
                root.appendChild(childTwo);

                transformer.transform(input, new DOMResult(root, childTwo));
                System.err.println("Transformation done OK");

                // Serialize the output so we can see the transformation actually worked
                Transformer serializer = tfactory.newTransformer();

                serializer.transform(new DOMSource(doc),
                        new StreamResult(System.out));

            } else {
                throw new SAXNotSupportedException(
                        "DOM node processing not supported!");
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testDOMImport() {

        try {

            String factory = getDocumentBuilderFactory();

            if (factory == null) {
                fail("No third-party DOM Builder found");
            }

            System.setProperty("javax.xml.parsers.DocumentBuilderFactory", factory);

            //TransformerFactory tfactory = TransformerFactory.newInstance();
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();

            if (tfactory.getFeature(DOMSource.FEATURE)) {
                DocumentBuilderFactory dfactory = DocumentBuilderFactory.newInstance();
                System.err.println("Using DocumentBuilderFactory " + dfactory.getClass());

                dfactory.setNamespaceAware(true);

                DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
                System.err.println("Using DocumentBuilder " + docBuilder.getClass());

                Document doc = docBuilder.newDocument();

                Configuration config = ((TransformerFactoryImpl) tfactory).getConfiguration();
                NodeInfo saxonDoc = config.buildDocumentTree(new SAXSource(new InputSource(foo_xml))).getRootNode();
                NodeInfo top = saxonDoc.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next();

                Node imported = doc.importNode(NodeOverNodeInfo.wrap(top), true);

                // Serialize the output so we can see the import actually worked
                Transformer serializer = tfactory.newTransformer();

                serializer.transform(new DOMSource(imported),
                        new StreamResult(System.out));


            } else {
                throw new SAXNotSupportedException(
                        "DOM node processing not supported!");
            }
        } catch (Exception ex) {
            ex.printStackTrace();
            handleException(ex);
        }
    }


    @Test
    public void testNodeInfo() {
        try {

            //TransformerFactory tfactory = TransformerFactory.newInstance();
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tfactory.newTemplates(new StreamSource(foo_xsl));
            Transformer transformer = templates.newTransformer();
            Configuration config = ((TransformerFactoryImpl) tfactory).getConfiguration();
            NodeInfo doc = config.buildDocumentTree(new SAXSource(new InputSource(foo_xml))).getRootNode();
            transformer.transform(doc, new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testSAXtoDOMNewIdentity() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();
            Transformer transformer = tfactory.newTransformer();
            SAXSource source = new SAXSource(new InputSource(foo_xml));
            DOMResult result = new DOMResult();
            transformer.transform(source, result);

            System.err.println("Transformation done OK");

            // Serialize the output so we can see the transformation actually worked
            Transformer serializer = tfactory.newTransformer();

            serializer.transform(new DOMSource(result.getNode()),
                    new StreamResult(System.out));

            int k = result.getNode().getChildNodes().getLength();
            System.err.println("Result root has " + k + " children");

        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testParam() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();
            Templates templates =
                    tfactory.newTemplates(new StreamSource(new File(foo_xsl)));
            Transformer transformer1 = templates.newTransformer();
            Transformer transformer2 = templates.newTransformer();

            transformer1.setParameter("a-param", "hello to you!");
            transformer1.transform(new StreamSource(new File(foo_xml)),
                    new StreamResult(System.out));
            System.out.println("\n========= (and again with a different parameter value) ===");
            transformer1.setParameter("a-param", "goodbye to you!");
            transformer1.transform(new StreamSource(new File(foo_xml)),
                    new StreamResult(System.out));
            System.out.println("\n========= (and again with a no parameter value) ===");
            transformer2.setOutputProperty(OutputKeys.INDENT, "yes");
            transformer2.transform(new StreamSource(new File(foo_xml)),
                    new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testTransformerReuse() {

        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(new File(foo_xsl)));

            transformer.setParameter("a-param", "hello to you!");

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(new File(foo_xml)),
                    new StreamResult(System.out));
            System.out.println("\n=========\n");
            transformer.setParameter("a-param", "hello to me!");
            transformer.setOutputProperty(OutputKeys.INDENT, "yes");

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(new File(foo_xml)),
                    new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testOutputProperties() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();
            Templates templates =
                    tfactory.newTemplates(new StreamSource(new File(foo_xsl)));
            Properties oprops = templates.getOutputProperties();
            oprops.setProperty(OutputKeys.INDENT, "yes");

            Transformer transformer = templates.newTransformer();

            transformer.setOutputProperties(oprops);
            transformer.transform(new StreamSource(new File(foo_xml)),
                    new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testUseAssociated() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // The DOM tfactory will have it's own way, based on DOM2,
            // of getting associated stylesheets.
            if (tfactory instanceof SAXTransformerFactory) {
                SAXTransformerFactory stf =
                        ((SAXTransformerFactory) tfactory);
                Source sources =
                        stf.getAssociatedStylesheet(new StreamSource(foo_xml), null,
                                null, null);

                if (null != sources) {
                    Transformer transformer = tfactory.newTransformer(sources);

                    transformer.transform(new StreamSource(foo_xml),
                            new StreamResult(System.out));
                } else {
                    System.out.println("Can't find the associated stylesheet!");
                }
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testUseEmbedded() {
        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // The DOM tfactory will have it's own way, based on DOM2,
            // of getting associated stylesheets.
            if (tfactory instanceof SAXTransformerFactory) {
                SAXTransformerFactory stf =
                        ((SAXTransformerFactory) tfactory);
                Source sources =
                        stf.getAssociatedStylesheet(new StreamSource(embedded_xml), null,
                                null, null);

                if (null != sources) {
                    Transformer transformer = tfactory.newTransformer(sources);

                    transformer.transform(new StreamSource(embedded_xml),
                            new StreamResult(System.out));
                } else {
                    System.out.println("Can't find the associated stylesheet!");
                }
            }
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testContentHandlerToDOM() {

        try {

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Make sure the transformer factory we obtained supports both
            // DOM and SAX.
            if (tfactory.getFeature(SAXSource.FEATURE)
                    && tfactory.getFeature(DOMSource.FEATURE)) {

                // We can now safely cast to a SAXTransformerFactory.
                SAXTransformerFactory sfactory = (SAXTransformerFactory) tfactory;

                // Create an Document node as the root for the output.
                DocumentBuilderFactory dfactory =
                        DocumentBuilderFactory.newInstance();
                DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
                Document outNode = docBuilder.newDocument();

                // Create a ContentHandler that can liston to SAX events
                // and transform the output to DOM nodes.
                TransformerHandler handler =
                        sfactory.newTransformerHandler(new StreamSource(foo_xsl));

                handler.setResult(new DOMResult(outNode));

                // Create a reader and set it's ContentHandler to be the
                // transformer.
                XMLReader reader = makeXMLReader();

                reader.setContentHandler(handler);
                reader.setProperty(
                        "http://xml.org/sax/properties/lexical-handler", handler);

                // Send the SAX events from the parser to the transformer,
                // and thus to the DOM tree.
                reader.parse(foo_xml);

                // Serialize the node for diagnosis.
                exampleSerializeNode(outNode);
            } else {
                System.out.println(
                        "Can't do exampleContentHandlerToDOM because tfactory is not a SAXTransformerFactory");
            }
        } catch (Exception ex) {
            ex.printStackTrace();
            handleException(ex);
        }
    }


    @Test
    public void testAsSerializer() {
        try {

            DocumentBuilderFactory dfactory =
                    DocumentBuilderFactory.newInstance();
            DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
            Node doc =
                    docBuilder.parse(new InputSource(foo_xml));
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // This creates a transformer that does a simple identity transform,
            // and thus can be used for all intents and purposes as a serializer.
            Transformer serializer = tfactory.newTransformer();
            Properties oprops = new Properties();

            oprops.setProperty("method", "html");
            if (tfactory instanceof ProfessionalTransformerFactory) {
                oprops.setProperty("{http://saxon.sf.net/}indent-spaces", "2");
            }
            serializer.setOutputProperties(oprops);
            serializer.transform(new DOMSource(doc),
                    new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testSerializerWithDOE() {

        try {
            TransformerFactoryImpl tf = new TransformerFactoryImpl();

            // Following is needed to ensure Saxon recognizes the JAXP-defined processing instructions
            tf.setAttribute(FeatureKeys.USE_PI_DISABLE_OUTPUT_ESCAPING, Boolean.TRUE);
            TransformerHandler t = tf.newTransformerHandler();

            // Set some serialization options
            t.getTransformer().setOutputProperty("method", "xml");
            t.getTransformer().setOutputProperty("version", "1.1");
            t.getTransformer().setOutputProperty("encoding", "iso-8859-1");
            t.getTransformer().setOutputProperty("indent", "yes");
            t.setResult(new StreamResult(System.out));

            // Write events to the serializer
            t.startDocument();
            t.startElement("", "out", "out", new AttributesImpl());
            t.characters(new char[]{'a', '&', 'b'}, 0, 3);
            t.processingInstruction(Result.PI_DISABLE_OUTPUT_ESCAPING, "");
            t.characters(new char[]{'<', 'M', '/', '>'}, 0, 4);
            t.processingInstruction(Result.PI_ENABLE_OUTPUT_ESCAPING, "");
            t.characters(new char[]{'y', '&', 'z'}, 0, 3);
            t.endElement("", "out", "out");
            t.endDocument();
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testNewDOMSerializer() {
        try {
            DocumentBuilderFactory dfactory =
                    DocumentBuilderFactory.newInstance();
            DocumentBuilder docBuilder = dfactory.newDocumentBuilder();
            Document doc = docBuilder.newDocument();
            Element docElement = doc.createElementNS("nsx.uri", "x");
            doc.appendChild(docElement);

            TransformerFactory tfactory = TransformerFactory.newInstance();

            // This creates a transformer that does a simple identity transform,
            // and thus can be used for all intents and purposes as a serializer.
            Transformer serializer = tfactory.newTransformer();
            Properties oprops = new Properties();

            oprops.setProperty("method", "xml");
            oprops.setProperty("indent", "yes");
            serializer.setOutputProperties(oprops);
            serializer.transform(new DOMSource(doc),
                    new StreamResult(System.out));
        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testUsingURIResolver() {

        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(text_xsl));

            // Set the URIResolver
            transformer.setURIResolver(new UserURIResolver(transformer));

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(foo_xml),
                    new StreamResult(System.out));

        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testOutputURIResolver() {

        try {

            // Create a transform factory instance.
            //TransformerFactory tfactory = TransformerFactory.newInstance();
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();

            // Set the OutputURIResolver
            tfactory.setAttribute("http://saxon.sf.net/feature/outputURIResolver",
                    new UserOutputResolver());
            tfactory.setAttribute("http://saxon.sf.net/feature/allow-external-functions",
                    Boolean.TRUE);     // this is the default
            tfactory.setAttribute("http://saxon.sf.net/feature/timing", Boolean.TRUE);

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(multidoc_xsl));


            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(cities_xml),
                    new StreamResult(System.out));

        } catch (Exception ex) {
            handleException(ex);
        }
    }

    @Test
    public void testExtra() {

        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(identity_xsl));

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(foo_xml),
                    new StreamResult(System.out));

        } catch (Exception ex) {
            handleException(ex);
        }
    }


    private class ModifyStylesheetFilter extends XMLFilterImpl {
        @Override
        public void startDocument() throws SAXException {
            System.err.println("ModifyStylesheetFilter#startDocument");
            super.startDocument();
        }

        @Override
        public void startElement(String namespaceURI, String localName,
                                 String qName, Attributes atts) throws SAXException {
            String lname = (qName.startsWith("xsl:") ? localName : ("XX" + localName));
            super.startElement(namespaceURI, lname, qName, atts);
        }

        @Override
        public void endElement(String namespaceURI, String localName,
                               String qName) throws SAXException {
            String lname = (qName.startsWith("xsl:") ? localName : ("XX" + localName));
            super.endElement(namespaceURI, lname, qName);
        }
    }


    /**
     * Get a DocumentBuilderFactory for a third-party DOM
     *
     * @return the name of the chosen DocumentBuilderFactory class
     */

    private static String getDocumentBuilderFactory() {
        // Try the crimson parser
        try {
            Class.forName("org.apache.crimson.jaxp.DocumentBuilderFactoryImpl");
            return "org.apache.crimson.jaxp.DocumentBuilderFactoryImpl";
        } catch (Exception e) {
            //
        }

        // Try the Xerces parser

        try {
            Class.forName("org.apache.xerces.jaxp.DocumentBuilderFactoryImpl");
            return "org.apache.xerces.jaxp.DocumentBuilderFactoryImpl";
        } catch (Exception e) {
            //
        }

        try {
            Class.forName("com.sun.org.apache.xerces.internal.jaxp.DocumentBuilderFactoryImpl");
            return "com.sun.org.apache.xerces.internal.jaxp.DocumentBuilderFactoryImpl";
        } catch (Exception e) {
            //
        }
        return null;

    }


    /**
     * A sample URIResolver. This handles a URI ending with ".txt". It loads the
     * text file identified by the URI, assuming it is in ISO-8859-1 encoding,
     * into a tree containing a single text node. It returns this
     * result tree to the transformer, exploiting the fact that a Saxon NodeInfo
     * can be used as a Source. If the URI doesn't end with ".txt", it hands over
     * to the standard URI resolver by returning null.
     */

    public static class UserURIResolver implements URIResolver {

        Transformer transformer;

        /**
         * Create a URIResolver
         *
         * @param t the JAXP Transformer
         */
        public UserURIResolver(Transformer t) {
            transformer = t;
        }

        /**
         * Resolve a URI
         *
         * @param base The base URI that should be used. May be null if uri is absolute.
         * @param href The relative or absolute URI. May be an empty string. May contain
         *             a fragment identifier starting with "#", which must be the value of an ID attribute
         *             in the referenced XML document.
         * @return a Source object representing an XML document
         */

        @Override
        public Source resolve(String href, String base)
                throws TransformerException {
            if (href.endsWith(".txt")) {
                try {
                    URL url = new URL(new URL(base), href);
                    java.io.InputStream in = url.openConnection().getInputStream();
                    java.io.InputStreamReader reader =
                            new java.io.InputStreamReader(in, StandardCharsets.ISO_8859_1);

                    // this could be vastly more efficient, but it doesn't matter here

                    StringBuilder sb = new StringBuilder();
                    while (true) {
                        int c = reader.read();
                        if (c < 0) break;
                        sb.append((char) c);
                    }
                    return new net.sf.saxon.value.TextFragmentValue(
                            ((TransformerImpl) transformer).getConfiguration(),
                            StringView.tidy(sb.toString()), url.toString());
                } catch (Exception err) {
                    throw new TransformerException(err);
                }
            } else {
                return null;
            }
        }

    } // end of inner class UserURIResolver

    /**
     * A sample OutputURIResolver. This handles a URI ending with ".out", and causes the
     * specified output to be written to standard output, preceded by "** Start ** and terminated
     * by ** End **. It also handles a URI ending with ".sax", in this case it creates a SAXResult
     * and sends the output there.
     */

    public static class UserOutputResolver implements OutputURIResolver {

        /**
         * Resolve a URI
         *
         * @param base The base URI that should be used. May be null if uri is absolute.
         * @param href The relative or absolute URI. May be an empty string. May contain
         *             a fragment identifier starting with "#", which must be the value of an ID attribute
         *             in the referenced XML document.
         * @return a Source object representing an XML document
         */

        @Override
        public Result resolve(String href, String base)
                throws TransformerException {
            if (href.endsWith(".out")) {
                System.out.println("** Start " + href + " **");
                StreamResult res = new StreamResult(System.out);
                res.setSystemId(href);
                return res;
            } else if (href.endsWith(".sax")) {
                // Output to a SAX ContentHandler. Normally we would expect this to be supplied
                // or initialized by the calling application, but this is only a demonstration
                System.out.println("** Start " + href + " **");
                SAXResult res = new SAXResult(new ExampleContentHandler());
                res.setSystemId(href);
                return res;
            } else {
                return null;
            }
        }

        @Override
        public void close(Result result) throws TransformerException {
            System.out.println("** End " + result.getSystemId() + " **");
        }

        /**
         * Get an instance of this OutputURIResolver class.
         * <p>This method is called every time an xsl:result-document instruction
         * is evaluated (with an href attribute). The resolve() and close() methods
         * will be called on the returned instance.</p>
         * <p>If the OutputURIResolver is stateless (that is, it retains no information
         * between resolve() and close()), then the same instance can safely be returned
         * each time. For a stateful OutputURIResolver, it must either take care to be
         * thread-safe (handling multiple invocations of xsl:result-document concurrently),
         * or it must return a fresh instance of itself for each call.</p>
         */
        @Override
        public OutputURIResolver newInstance() {
            return this;
        }
    } // end of inner class UserOutputResolver

    /**
     * Serialize a node to System.out.
     *
     * @param node the node to be serialized
     */
    public static void exampleSerializeNode(Node node)
            throws TransformerException {

        TransformerFactory tfactory = TransformerFactory.newInstance();

        // This creates a transformer that does a simple identity transform,
        // and thus can be used for all intents and purposes as a serializer.
        Transformer serializer = tfactory.newTransformer();

        serializer.setOutputProperty(OutputKeys.INDENT, "yes");
        serializer.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
        serializer.transform(new DOMSource(node),
                new StreamResult(System.out));
    }

    // test case for bug 2178
    @Test
    public void testExampleSetParam() {

        try {

            // Create a transform factory instance.
            TransformerFactory tfactory = TransformerFactory.newInstance();

            // Create a transformer for the stylesheet.
            Transformer transformer =
                    tfactory.newTransformer(new StreamSource(identity_xsl));

            // Set a parameter not defined in the stylesheet
            transformer.setParameter("abc", "xyz");

            // Transform the source XML to System.out.
            transformer.transform(new StreamSource(foo_xml),
                    new StreamResult(System.out));
        } catch (TransformerException ex) {
            handleException(ex);
        }
    }


    private static void handleException(Exception ex) {

        System.out.println("EXCEPTION: " + ex);
        ex.printStackTrace();

        if (ex instanceof TransformerConfigurationException) {
            System.out.println();
            System.out.println("Test failed");

            Throwable ex1 =
                    ((TransformerConfigurationException) ex).getException();

            if (ex1 != null) {    // added by MHK
                ex1.printStackTrace();

                if (ex1 instanceof SAXException) {
                    Exception ex2 = ((SAXException) ex1).getException();

                    System.out.println("Internal sub-exception: ");
                    ex2.printStackTrace();
                }
            }
        }
        fail();
    }

    /**
     * Make an XMLReader (a SAX Parser)
     *
     * @return the constructed XMLReader
     */

    private static XMLReader makeXMLReader() throws ParserConfigurationException, SAXException {
        SAXParserFactory factory = SAXParserFactory.newInstance();
        factory.setNamespaceAware(true);
        return factory.newSAXParser().getXMLReader();
    }

    private static class ExampleContentHandler implements ContentHandler {

        private PrintStream printStream = System.out;

        public void setPrintStream(PrintStream stream) {
            printStream = stream;
        }

        public PrintStream getPrintStream() {
            return printStream;
        }

        @Override
        public void setDocumentLocator(Locator locator) {
            printStream.println("setDocumentLocator");
        }

        @Override
        public void startDocument() throws SAXException {
            printStream.println("startDocument");
        }

        @Override
        public void endDocument() throws SAXException {
            printStream.println("endDocument");
        }

        @Override
        public void startPrefixMapping(String prefix, String uri)
                throws SAXException {
            printStream.println("startPrefixMapping: " + prefix + ", " + uri);
        }

        @Override
        public void endPrefixMapping(String prefix) throws SAXException {
            printStream.println("endPrefixMapping: " + prefix);
        }

        @Override
        public void startElement(
                String namespaceURI, String localName, String qName, Attributes atts)
                throws SAXException {

            printStream.print("startElement: " + namespaceURI + ", "
                    + localName + ", " + qName);

            int n = atts.getLength();

            for (int i = 0; i < n; i++) {
                printStream.print(", " + atts.getQName(i) + "='" + atts.getValue(i) + "'");
            }

            printStream.println();
        }

        @Override
        public void endElement(
                String namespaceURI, String localName, String qName)
                throws SAXException {
            printStream.println("endElement: " + namespaceURI + ", "
                    + localName + ", " + qName);
        }

        @Override
        public void characters(char[] ch, int start, int length)
                throws SAXException {

            String s = new String(ch, start, (length > 30)
                    ? 30
                    : length);

            if (length > 30) {
                printStream.println("characters: \"" + s + "\"...");
            } else {
                printStream.println("characters: \"" + s + "\"");
            }
        }

        @Override
        public void ignorableWhitespace(char[] ch, int start, int length)
                throws SAXException {
            printStream.println("ignorableWhitespace");
        }

        @Override
        public void processingInstruction(String target, String data)
                throws SAXException {
            printStream.println("processingInstruction: " + target + ", "
                    + data);
        }

        @Override
        public void skippedEntity(String name) throws SAXException {
            printStream.println("skippedEntity: " + name);
        }


    }

    @Test
    public void testBase64BinaryParam() {
        try {
            String xsl = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                " <xsl:param name='in' as='xs:base64Binary' xmlns:xs='http://www.w3.org/2001/XMLSchema'/>\n" +
                " <xsl:template match='/'>\n" +
                "   <out><xsl:value-of select='string($in)'/></out>\n" +
                " </xsl:template> \n" +
                "</xsl:stylesheet>";
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();
            Transformer transformer =
                tfactory.newTransformer(new StreamSource(new StringReader(xsl)));
            byte[] bytes = "SAMPLE".getBytes();
            transformer.setParameter("in", new net.sf.saxon.value.Base64BinaryValue(bytes));
            transformer.setOutputProperty("omit-xml-declaration", "yes");
            StringWriter sw = new StringWriter();
            transformer.transform(new StreamSource(new StringReader(xsl)),
                new StreamResult(sw));
            assertEquals("<out>U0FNUExF</out>", sw.toString());
        } catch (Exception err) {
            fail();
        }
    }



}
