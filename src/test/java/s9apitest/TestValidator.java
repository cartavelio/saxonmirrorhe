package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import com.saxonica.ee.jaxp.ValidatorImpl;
import jaxptest.ConfigTest;
import millicom.MillicomTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Steps;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.tiny.TinyTree;
import net.sf.saxon.type.ValidationException;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;
import org.xml.sax.ErrorHandler;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.SAXParseException;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.Validator;
import java.io.*;
import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for the s9api schema validation interface
 */
public class TestValidator {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testReportMultipleErrors() {

        // This validation run should report exactly two validation errors.

        String source_xml = "<?xml version='1.0'?><request><user_name>ed</user_name><password>sdsd</password><date1>a2009-01-01</date1><date2>b2009-01-01</date2></request>";
        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"user_name\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"password\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"date1\" type=\"xs:date\"/>\n" +
                "\t\t\t\t<xs:element name=\"date2\" type=\"xs:date\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

        final List<TransformerException> errorList = new ArrayList<>();
        FileOutputStream out = null;
        try {
            out = new FileOutputStream(configTest.getDataDir() + "sandpit/validation.out");
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));

            ErrorListener listener = new ErrorListener() {
                @Override
                public void error(TransformerException exception) {
                    errorList.add(exception);
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
                    throw exception;
                }

                @Override
                public void warning(TransformerException exception) {
                    errorList.add(exception);
                }
            };

            SchemaValidator sv = sm.newSchemaValidator();
            sv.setErrorListener(listener);
            sm.load(source_xsd);
            StreamSource source = new StreamSource(new StringReader(source_xml));


            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputStream(out);
            sv.setDestination(serializer);


            sv.validate(source);

            out.close();
            //fail("Validation errors not detected");

        } catch (SaxonApiException sapie) {
            try {
                out.close();
            } catch (IOException ee) {
                fail();
            }
            assertEquals(2, errorList.size());
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that multiple base URIs derived from a multiple-entity input document are
     * preserved through validation
     */

    @Test
    public void testValidationWithEntities() {
        // Fails, bug 5107
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new File(configTest.getDataDir() + "book-wrapper.xml"));
            assertEquals(3, getAllBaseUris(doc).size(), "3 distinct base URIs in input");
            assertTrue(doc.getBaseURI().toString().endsWith("book-wrapper.xml"), "input doc base URI");

            SchemaManager mgr = proc.getSchemaManager();
            mgr.load(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));
            SchemaValidator v = mgr.newSchemaValidator();
            XdmDestination dest = new XdmDestination();
            v.setDestination(dest);
            v.setDestinationBaseURI(doc.getBaseURI());
            v.validate(doc.asSource());

            XdmNode validDoc = dest.getXdmNode();

            assertEquals(3, getAllBaseUris(validDoc).size(), "3 distinct base URIs in output");
            assertTrue(validDoc.getBaseURI().toString().endsWith("book-wrapper.xml"), "validated doc base URI");

        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testTypeAnnotations() {
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new File(configTest.getDataDir() + "books.xml"));
            assertEquals(new QName("xs", NamespaceConstant.SCHEMA, "untyped"), doc.getTypeAnnotationName());

            SchemaManager mgr = proc.getSchemaManager();
            mgr.load(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));
            SchemaValidator v = mgr.newSchemaValidator();
            XdmDestination dest = new XdmDestination();
            v.setDestination(dest);
            v.setDestinationBaseURI(doc.getBaseURI());
            v.validate(doc.asSource());

            XdmNode validDoc = dest.getXdmNode();
            assertEquals(new QName("xs", NamespaceConstant.SCHEMA, "anyType"), validDoc.getTypeAnnotationName());
            XdmNode books = validDoc.select(Steps.descendant("BOOKS")).firstItem();
            assertEquals(NamespaceUri.ANONYMOUS, books.getTypeAnnotationName().getNamespaceUri());
            XdmNode price = books.select(Steps.path("ITEM", "PRICE")).firstItem();
            assertEquals(new QName("moneyType"), price.getTypeAnnotationName());
            assertEquals(new QName("xs", NamespaceConstant.SCHEMA, "decimal"), ((XdmAtomicValue) price.getTypedValue()).getPrimitiveTypeName());
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    private Set<String> getAllBaseUris(XdmNode root) {
        Set<String> baseUris = new HashSet<>();
        XdmSequenceIterator<XdmNode> iter = root.axisIterator(Axis.DESCENDANT);
        while (iter.hasNext()) {
            XdmNode node = iter.next();
            if (node.getNodeKind() == XdmNodeKind.ELEMENT) {
                baseUris.add(node.getBaseURI().toASCIIString());
            }
        }
        return baseUris;
    }


    @Test
    public void testMultipleSchemaImportsOn() {
        multipleSchemaImports(true);
    }

    @Test
    public void testMultipleSchemaImportsOff() {
        multipleSchemaImports(false);
    }

    private void multipleSchemaImports(boolean on) {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, on);
            SchemaManager sm = p.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "<xs:import namespace='http://ns.example.com/' schemaLocation='file:///1'/>" +
                    "<xs:import namespace='http://ns.example.com/' schemaLocation='file:///2'/>" +
                    "</xs:schema>";
            final String xsd1 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://ns.example.com/'>" +
                            "<xs:element name=\"a\"/>\n" +
                            "</xs:schema>";
            final String xsd2 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://ns.example.com/'>" +
                            "<xs:element name=\"b\"/>\n" +
                            "</xs:schema>";
            p.getUnderlyingConfiguration().setResourceResolver(
                    request -> {
                        if ("file:///1".equals(request.relativeUri)) {
                            return new StreamSource(new StringReader(xsd1), "file:///1");
                        }
                        if ("file:///2".equals(request.relativeUri)) {
                            return new StreamSource(new StringReader(xsd2), "file:///2");
                        }
                        return null;
                    });
            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            SchemaValidator sv = sm.newSchemaValidator();
            final String xml0 = "<a xmlns='http://ns.example.com/'/>";
            final String xml1 = "<b xmlns='http://ns.example.com/'/>";
            sv.validate(new StreamSource(new StringReader(xml0)));
            sv.validate(new StreamSource(new StringReader(xml1)));
            if (!on) {
                fail("Validation should have failed");
            }
        } catch (SaxonApiException err) {
            if (on) {
                fail(err.getMessage());
            }
        }
    }

    // Test effect of multiple-schema-imports when A imports B, B imports C, and A and C have the same
    // target namespace

    @Test
    public void testCycleMultipleSchemaImportsOn() {
        cycleMultipleSchemaImports(true);
    }

    @Test
    public void testCycleMultipleSchemaImportsOff() {
        cycleMultipleSchemaImports(false);
    }

    private void cycleMultipleSchemaImports(boolean on) {
        SchemaManager sm = null;
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, on);
            sm = p.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:import namespace='ns:b' schemaLocation='file:///1'/>" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            final String xsd1 =
                    "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:b'>" +
                            "<xs:import namespace='ns:a' schemaLocation='file:///2'/>" +
                            "<xs:element name='b'/>\n" +
                            "</xs:schema>";
            final String xsd2 =
                    "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>" +
                            "<xs:element name='c'/>\n" +
                            "</xs:schema>";
            p.getUnderlyingConfiguration().setResourceResolver(
                    request -> {
                        if ("file:///1".equals(request.relativeUri)) {
                            return new StreamSource(new StringReader(xsd1), "file:///1");
                        }
                        if ("file:///2".equals(request.relativeUri)) {
                            return new StreamSource(new StringReader(xsd2), "file:///2");
                        }
                        return null;
                    });
            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }

        SchemaValidator sv = sm.newSchemaValidator();
        final String xml0 = "<a xmlns='ns:a'/>";
        final String xml1 = "<b xmlns='ns:b'/>";
        final String xml2 = "<c xmlns='ns:a'/>";
        try {
            sv.validate(new StreamSource(new StringReader(xml0)));
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
        try {
            sv.validate(new StreamSource(new StringReader(xml1)));
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
        try {
            sv.validate(new StreamSource(new StringReader(xml2)));
            if (!on) {
                fail("Validation should have failed");
            }
        } catch (SaxonApiException err) {
            if (on) {
                fail("Validation should not have failed");
            }
        }
    }


    @Test
    public void testSchemaAwareQueryUsingSCM() {
        try {
            Processor proc = new Processor(true);
            proc.getSchemaManager().importComponents(new StreamSource(new File(configTest.getDataDir() + "dita/concept.scm")));
            // define a ns prefix to be associated with a default attribute added to the instance during validation
            proc.getUnderlyingConfiguration().getNamePool().suggestPrefix("dita", NamespaceUri.of("http://dita.oasis-open.org/architecture/2005/"));
            XdmNode doc = proc.newDocumentBuilder().build(new StreamSource(new File(configTest.getDataDir() + "dita/concept.xml")));
            XQueryCompiler c = proc.newXQueryCompiler();
            c.setSchemaAware(true);
            XQueryEvaluator e = c.compile("validate{.}").load();
            e.setContextItem(doc);
            Serializer s = proc.newSerializer();
            StringWriter sw = new StringWriter();
            s.setOutputWriter(sw);
            e.setDestination(s);
            e.run();
            System.out.println(sw);
            assertTrue(sw.toString().contains("xmlns:dita"));
            assertTrue(sw.toString().contains("dita:DITAArchVersion=\"1.2\""));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testImportUsingSchemaUriResolver() {
        try {
            Processor p = new Processor(true);
            SchemaManager sm = p.getSchemaManager();
            sm.setSchemaURIResolver(new SchemaURIResolver() {

                @Override
                public void setConfiguration(Configuration config) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public Source[] resolve(String targetNamespace, String baseURI, String[] locations) {
                    if (targetNamespace.equals("http://two/")) {
                        return new Source[]{new StreamSource(new StringReader(
                                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" targetNamespace=\"http://two/\">\n" +
                                        "    <xs:simpleType name='date'><xs:restriction base='xs:date'/></xs:simpleType>\n" +
                                        "</xs:schema> "
                        ))};
                    } else {
                        return null;
                    }
                }
            });

            String xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:b='http://two/'>\n" +
                    "<xs:import namespace='http://two/'/>" +
                    "<xs:element name='a' type='b:date'/>\n" +
                    "</xs:schema>";


            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            SchemaValidator sv = sm.newSchemaValidator();
            final String xml0 = "<a>2010-11-02</a>";        // valid
            final String xml1 = "<a>orange wednesday</a>";  // invalid
            sv.validate(new StreamSource(new StringReader(xml0)));
            try {
                sv.validate(new StreamSource(new StringReader(xml1)));
                fail("Invalidity not detected");
            } catch (Exception ve) {
                // OK
            }

        } catch (SaxonApiException err) {
            fail();
            err.printStackTrace();
        }
    }

    @Test
    public void testImportUsingResourceResolver() {
        try {
            Processor p = new Processor(true);
            p.getUnderlyingConfiguration().setResourceResolver(
                    request -> {
                        if (request.uri.equals("http://two/") && request.uriIsNamespace) {
                            return new StreamSource(new StringReader(
                                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" targetNamespace=\"http://two/\">\n" +
                                            "    <xs:simpleType name='date'><xs:restriction base='xs:date'/></xs:simpleType>\n" +
                                            "</xs:schema> "));
                        } else {
                            return null;
                        }
                    });

            SchemaManager sm = p.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:b='http://two/'>\n" +
                    "<xs:import namespace='http://two/'/>" +
                    "<xs:element name='a' type='b:date'/>\n" +
                    "</xs:schema>";


            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            SchemaValidator sv = sm.newSchemaValidator();
            final String xml0 = "<a>2010-11-02</a>";        // valid
            final String xml1 = "<a>orange wednesday</a>";  // invalid
            sv.validate(new StreamSource(new StringReader(xml0)));
            try {
                sv.validate(new StreamSource(new StringReader(xml1)));
                fail("Invalidity not detected");
            } catch (Exception ve) {
                // OK
            }

        } catch (SaxonApiException err) {
            fail();
            err.printStackTrace();
        }
    }

    @Test
    public void testLocalXLinkSchemaRelyingOnResolver() {
        // Bug 5652. The user wants to pick up a local copy of the XLink schema, avoiding any
        // built-in version of the schema. This test uses a resolver to pick it up.
        try {
            Processor p = new Processor(true);
            p.getUnderlyingConfiguration().setResourceResolver(
                    request -> {
                        if (request.uri.equals(NamespaceConstant.XLINK) && request.uriIsNamespace) {
                            return new StreamSource(new StringReader(
                                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" targetNamespace='" + NamespaceConstant.XLINK + "'>\n" +
                                            "    <xs:simpleType name='date'><xs:restriction base='xs:date'/></xs:simpleType>\n" +
                                            "</xs:schema> "));
                        } else {
                            return null;
                        }
                    });
            SchemaManager sm = p.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:b='" + NamespaceConstant.XLINK + "'>\n" +
                    "<xs:import namespace='" + NamespaceConstant.XLINK + "'/>" +
                    "<xs:element name='a' type='b:date'/>\n" +
                    "</xs:schema>";

            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            SchemaValidator sv = sm.newSchemaValidator();
            final String xml0 = "<a>2010-11-02</a>";          // valid
            final String xml1 = "<a>orange wednesday</a>";    // invalid
            sv.validate(new StreamSource(new StringReader(xml0)));
            try {
                sv.validate(new StreamSource(new StringReader(xml1)));
                fail();
            } catch (Exception ve) {
                // OK
            }

        } catch (SaxonApiException err) {
            fail();
            err.printStackTrace();
        }
    }

    @Test
    public void testLocalXLinkSchemaRelyingOnLocationHint() {
        // Bug 5652. The user wants to pick up a local copy of the XLink schema, avoiding any
        // built-in version of the schema. This test tries to pick it up using a location hint,
        // which doesn't work.
        try {
            Processor p = new Processor(true);
            SchemaManager sm = p.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:b='" + NamespaceConstant.XLINK + "'>\n" +
                    "<xs:import namespace='" + NamespaceConstant.XLINK + "' schemaLocation='file:///does.not.exist/dummy.xml'/>" +
                    "<xs:element name='a' type='b:date'/>\n" +
                    "</xs:schema>";

            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            fail("Unexpected success");
        } catch (SaxonApiException err) {
            System.err.println(err.getMessage());
            assertTrue(err.getMessage().contains("Errors were found in the schema"));
        }
    }

    @Test
    public void testLocalXLinkSchemaRelyingOnLocalCatalog() {
        // Bug 5652. The user wants to pick up a local copy of the XLink schema, avoiding any
        // built-in version of the schema. This test uses a catalog to pick it up.
        try {
            Processor p = new Processor(true);
            p.setCatalogFiles(configTest.getDataDir() + "resolvers/xlink-catalog.xml");
            SchemaManager sm = p.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:b='" + NamespaceConstant.XLINK + "'>\n" +
                    "<xs:import namespace='" + NamespaceConstant.XLINK + "' schemaLocation='file:///does.not.exist/dummy.xml'/>" +
                    "<xs:element name='a' type='b:date'/>\n" +
                    "</xs:schema>";


            sm.load(new StreamSource(new StringReader(xsd0), "file:///0"));
            SchemaValidator sv = sm.newSchemaValidator();
            final String xml0 = "<a>2010-11-02</a>";           // valid
            final String xml1 = "<a>orange wednesday</a>";     // invalid
            sv.validate(new StreamSource(new StringReader(xml0)));
            try {
                sv.validate(new StreamSource(new StringReader(xml1)));
                fail();
            } catch (Exception ve) {
                // OK
            }

        } catch (SaxonApiException err) {
            fail();
            err.printStackTrace();
        }
    }


    @Test
    public void testAssertionsHidingComments() {

        // This validation run should report exactly two validation errors.

        String valid_xml = "<?xml version='1.0'?><request><a/>x<!--comment-->y<b/></request>";
        String invalid_xml = "<?xml version='1.0'?><request><a/>x<!--comment-->y</request>";
        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType mixed='true'>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test=\"count(child::node()) = 3 and text()='xy'\"/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.ASSERTIONS_CAN_SEE_COMMENTS, false);
            proc.setConfigurationProperty(Feature.XSD_VERSION, "1.1");
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));

            SchemaValidator sv = sm.newSchemaValidator();
            sm.load(source_xsd);
            StreamSource source = new StreamSource(new StringReader(valid_xml));

            XdmDestination destination = new XdmDestination();
            sv.setDestination(destination);

            sv.validate(source);

            XPathCompiler xp = proc.newXPathCompiler();
            xp.setSchemaAware(true);
            XdmAtomicValue result = (XdmAtomicValue) xp.evaluate("count(//comment())", destination.getXdmNode());
            assertEquals(1, result.getLongValue());

            try {
                sv.validate(new StreamSource(new StringReader(invalid_xml)));
                fail("Failed to spot errors");
            } catch (SaxonApiException er) {
                assertTrue(er.getMessage().contains("validation errors"));
                // OK
            }
        } catch (SaxonApiException sapie) {
            sapie.printStackTrace();
            fail();
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testAssertionsSeeingComments() {

        String valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
        String invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.ASSERTIONS_CAN_SEE_COMMENTS, true);
            proc.setConfigurationProperty(Feature.XSD_VERSION, "1.1");
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));

            SchemaValidator sv = sm.newSchemaValidator();
            sm.load(source_xsd);
            StreamSource source = new StreamSource(new StringReader(valid_xml));

            XdmDestination destination = new XdmDestination();
            sv.setDestination(destination);

            sv.validate(source);

            XPathCompiler xp = proc.newXPathCompiler();
            xp.setSchemaAware(true);
            XdmAtomicValue result = (XdmAtomicValue) xp.evaluate("count(//comment())", destination.getXdmNode());
            assertEquals(1, result.getLongValue());

            try {
                sv.validate(new StreamSource(new StringReader(invalid_xml)));
                fail("Failed to spot errors");
            } catch (SaxonApiException er) {
                // OK
            }
        } catch (SaxonApiException sapie) {
            fail();
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    private void assertionUsingXPath3(boolean allow) {

        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test='child::node() => count() = 3'/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.ASSERTIONS_CAN_SEE_COMMENTS, true);
            proc.setConfigurationProperty(Feature.XSD_VERSION, "1.1");
            proc.setConfigurationProperty(Feature.XPATH_VERSION_FOR_XSD, allow ? 31 : 20);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));

            try {
                sm.load(source_xsd);
                if (!allow) {
                    fail("unexpected success");
                }
            } catch (SaxonApiException e) {
                if (allow) {
                    fail(e.getMessage());
                    e.printStackTrace();
                }
            }
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testAssertionUsingXPath31allowed() {
        assertionUsingXPath3(true);
    }

    @Test
    public void testAssertionUsingXPath31notAllowed() {
        assertionUsingXPath3(false);
    }

    @Test
    public void testAssertionsOnBadData() {

        // This validation run should report exactly two validation errors. Bug 4172

        String invalid_xml = "<?xml version='1.0'?><request><a>x</a><b/></request>";
        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType mixed='true'>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:decimal\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test=\"if (string-length(a) = 3) then exists(b) else empty(b)\"/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

        try {
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));

            SchemaValidator sv = sm.newSchemaValidator();
            sm.load(source_xsd);

            XdmDestination destination = new XdmDestination();
            sv.setDestination(destination);

            try {
                sv.validate(new StreamSource(new StringReader(invalid_xml)));
                fail("Failed to spot errors");
            } catch (SaxonApiException er) {
                assertTrue(er.getMessage().contains("Two validation errors"));
                // OK
            }
        } catch (SaxonApiException sapie) {
            sapie.printStackTrace();
            fail();
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    /**
     * Test the effect of changing a schema to add members to a substitution group.
     * See bug 2174.
     */

    @Test
    public void testSchemaChange() {
        try {
            String sch1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <element name=\"FamilyMember\" type=\"string\" />\n" +
                    "  <element name=\"Parent\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "  <element name=\"Child\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "  <element name=\"Family\">\n" +
                    "    <complexType>\n" +
                    "      <sequence>\n" +
                    "        <element ref=\"fam:FamilyMember\" maxOccurs=\"unbounded\"/>\n" +
                    "      </sequence>\n" +
                    "    </complexType>\n" +
                    "  </element>  \n" +
                    "</schema>";

            String doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "</Family>";

            String sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                    "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

//            String doc2 = "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
//                "  <Parent>John</Parent>\n" +
//                "  <Child>Alice</Child>\n" +
//                "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
//                "</Family>";

            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(sch1)));
            System.err.println("Schema 1 OK");
            sm.newSchemaValidator().validate(new StreamSource(new StringReader(doc1)));
            System.err.println("Doc 1 OK");
            try {
                sm.load(new StreamSource(new StringReader(sch2)));
                fail("Unexpected success");
            } catch (SaxonApiException e) {
                assertTrue(e.getMessage().contains("It is not possible to add to the substitution group"), "expectedError");
            }
//            System.err.println("Schema 2 OK");
//            try {
//                sm.newSchemaValidator().validate(new StreamSource(new StringReader(doc2)));
//                fail("Unexpected success");
//            } catch (SaxonApiException e) {
//                assertTrue("expectedError", e.getMessage().contains("It is not possible to add to the substitution group"));
//            }

        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());
            fail();
        }
    }

    @Test
    public void testImplicitSchemaImportsOption() {

        String source_xml = "<?xml version='1.0'?><e xmlns='http://y.com/ns'><f/></e>";
        String xsd1 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://y.com/ns' elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:import namespace='http://x.com/ns'/>\n" +
                "\t<xs:element name='e' type='x:t' xmlns:x='http://x.com/ns'/>\n" +
                "\t<xs:element name='f' type='xs:anyType'/>\n" +
                "</xs:schema>";

        final String xsd2 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://x.com/ns' elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t\t<xs:complexType name='t'>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element ref='y:f' xmlns:y='http://y.com/ns'/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "</xs:schema>";


        FileOutputStream out = null;
        try {
            out = new FileOutputStream(configTest.getDataDir() + "sandpit/validation.out");
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.IMPLICIT_SCHEMA_IMPORTS, true);

            SchemaManager sm = proc.getSchemaManager();
            sm.setSchemaURIResolver(new SchemaURIResolver() {
                @Override
                public Source[] resolve(String targetNamespace, String baseURI, String[] locations) {
                    if (targetNamespace.equals("http://x.com/ns")) {
                        return new StreamSource[]{new StreamSource(new StringReader(xsd2))};
                    } else {
                        return null;
                    }
                }

                @Override
                public void setConfiguration(Configuration config) {

                }
            });
            sm.load(new StreamSource(new StringReader(xsd1)));
            sm.load(new StreamSource(new StringReader(xsd2)));

            SchemaValidator sv = sm.newSchemaValidator();

            StreamSource source = new StreamSource(new StringReader(source_xml));


            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputStream(out);
            sv.setDestination(serializer);


            sv.validate(source);

            out.close();

        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testValidationReporting() {
        SchemaValidator validator = null;
        Serializer dest = null;
        StringWriter sw = new StringWriter();
        try {
            String valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
            String invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
            String sch1 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

            String doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "</Family>";

            String sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                    "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

            String doc2 = "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.setXsdVersion("1.1");
            sm.load(new StreamSource(new StringReader(sch1)));
            System.err.println("Schema 1 OK");
            validator = sm.newSchemaValidator();

            dest = proc.newSerializer(sw);
            dest.setOutputProperty(Serializer.Property.INDENT, "yes");
            validator.setValidityReporting(dest);//configTest.getDataDir()+"/validationReort.xml");
            StreamSource source1 = new StreamSource(new StringReader(invalid_xml));
            source1.setSystemId("http://doc1");
            validator.validate(source1);
            System.err.println("Doc 1 OK");


        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());

        } finally {
            if (dest != null) {
                System.out.println(sw);
                assertTrue(true, sw.toString());

            }

        }


    }

    @Test
    public void testValidationReporting2() {
        SchemaValidator validator = null;
        Serializer dest = null;
        //   StringWriter writer = new StringWriter();
        try {
            String valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
            String invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
            String sch1 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

            String doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "</Family>";

            String sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                    "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

            String doc2 = "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.setXsdVersion("1.1");
            StreamSource source1 = new StreamSource(new StringReader(sch1));
            source1.setSystemId("http://doc1");
            sm.load(source1);
            //sm.load(new StreamSource(new File("/Users/ond1/work/development/files/millicom/FafaSCript.xsd")));//new StringReader(sch1)));
            System.err.println("Schema 1 OK");
            validator = sm.newSchemaValidator();

            dest = proc.newSerializer();
            dest.setOutputProperty(Serializer.Property.INDENT, "yes");
            dest.setOutputFile(new File(configTest.getTempDir() + "/validationReport.xml"));

            // dest.setOutputWriter(writer);
            validator.setValidityReporting(dest);//configTest.getDataDir()+"/validationReort.xml");
            //validator.validate(new StreamSource(new File("/Users/ond1/work/development/files/millicom/_XMLDestination.xml")));//new StringReader(invalid_xml)));
            StreamSource source2 = new StreamSource(new StringReader(invalid_xml));
            source2.setSystemId("http://doc2");
            validator.validate(source2);
            System.err.println("Doc 1 OK");
            File file = new File(configTest.getTempDir() + "/validationReport.xml");
            XdmNode resultdoc = proc.newDocumentBuilder().build(file);
            System.out.println(resultdoc.getStringValue());
            assertTrue(file.exists(), "File exists: ");

        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());
            sapie.printStackTrace();
            //  System.out.println(writer.toString());

        } catch (Exception ex) {
            ex.printStackTrace();
        }


    }

    /**
     * Test validating multiple documents with reporting feature on
     */
    @Test
    public void testValidationReporting3() {
        SchemaValidator validator = null;
        Serializer dest = null;
        StringWriter sw = new StringWriter();
        try {
            String valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
            String invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
            String sch1 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

            String doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "</Family>";

            String sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                    "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

            String doc2 = "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.setXsdVersion("1.1");
            sm.load(new StreamSource(new StringReader(sch1)));
            System.err.println("Schema 1 OK");
            validator = sm.newSchemaValidator();

            dest = proc.newSerializer(sw);
            dest.setOutputProperty(Serializer.Property.INDENT, "yes");
            dest.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            validator.setValidityReporting(dest);
            List<Source> sources = new ArrayList<>();
            sources.add(new StreamSource(new StringReader(invalid_xml)));
            sources.add(new StreamSource(new StringReader(doc1)));
            validator.validateMultiple(sources);
            System.err.println("Doc 1 OK");


        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());

        } finally {
            if (dest != null) {
                System.out.println(sw);
                assertTrue(true, sw.toString());

            }

        }


    }


    /**
     * Test validating multiple documents with reporting feature off
     */
    @Test
    public void testValidationReporting4() {
        SchemaValidator validator = null;
        Serializer dest = null;
        StringWriter sw = new StringWriter();
        try {
            String valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
            String invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
            String sch1 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

            String doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "</Family>";

            String sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                    "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                    "</schema>";

            String doc2 = "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.setXsdVersion("1.1");
            sm.load(new StreamSource(new StringReader(sch1)));
            System.err.println("Schema 1 OK");
            validator = sm.newSchemaValidator();

            dest = proc.newSerializer(sw);
            dest.setOutputProperty(Serializer.Property.INDENT, "yes");
            dest.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            // validator.setValidityReporting(dest);
            List<Source> sources = new ArrayList<>();
            sources.add(new StreamSource(new StringReader(invalid_xml)));
            sources.add(new StreamSource(new StringReader(doc1)));
            validator.validateMultiple(sources);
            System.err.println("Doc 1 OK");


        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());

        } finally {
            if (dest != null) {
                System.out.println(sw);
                assertTrue(true, sw.toString());

            }

        }


    }


    /**
     * Error handler for collecting Saxon validation errors.
     */
    private static class MyErrorHandler implements ErrorHandler {
        public int warnings = 0;
        public int errors = 0;
        public int fatals = 0;

        @Override
        public void warning(SAXParseException we) {
            warnings++;
            System.out.println("Warning: " + we);
        }

        @Override
        public void fatalError(SAXParseException fe) {
            fatals++;
            System.out.println("Fatal error: " + fe);
        }

        @Override
        public void error(SAXParseException ee) {
            errors++;
            System.out.println("Error: " + ee);
        }
    }

    @Test
    public void testSchemaListenerSaxon() throws SAXException, IOException {
        runSchemaListener(new com.saxonica.ee.jaxp.SchemaFactoryImpl());
    }

    @Test
    public void testSchemaListenerXerces() throws SAXException, IOException {
        runSchemaListener(SchemaFactory.newInstance("http://www.w3.org/2001/XMLSchema",
                                                    "org.apache.xerces.jaxp.validation.XMLSchemaFactory",
                                                    this.getClass().getClassLoader()));
    }

    public void runSchemaListener(SchemaFactory schemaFactory) throws SAXException, IOException {
        try {
            File schemaFile = new File(configTest.getDataDir() + "personal.xsd");
            File xmlFile = new File(configTest.getDataDir() + "personal-schema.xml");
            MyErrorHandler errorHandler = new MyErrorHandler();

            // Set error handler
            schemaFactory.setErrorHandler(errorHandler);

            // Strict schema validation
            //schemaFactory.setProperty(FeatureKeys.SCHEMA_VALIDATION, Validation.STRICT);
            Schema schema = schemaFactory.newSchema(schemaFile);
            Validator newValidator = schema.newValidator();
            newValidator.setErrorHandler(errorHandler);
            newValidator.validate(new SAXSource(new InputSource(new FileInputStream(xmlFile))));
            // Note: no exception expected, although the input is invalid

            assertEquals(0, errorHandler.warnings, "warnings");
            assertTrue(errorHandler.errors >= 5, "errors");
            assertEquals(0, errorHandler.fatals, "fatals");
        } catch (SAXException | IOException e) {
            e.printStackTrace();
            fail(e);
        }
    }

    @Test
    @Tag("missingfiles")
    // Documents are not in the repository
    public void testMillicomErrorListener() {
        String[] args = {
                "-xsd:" + configTest.getDataDir() + "SubsidyTrackingFactXSD11.xsd",
                "-s:" + configTest.getDataDir() + "id2278_HN201506_SubsidyTrackingFact.xml",
                "-verbose",
                "-xsdversion:1.1"};


        try {
            MillicomTest.main(args);
        } catch (IOException | SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }


    }

    @Test
    public void testLeak() {
        try {
            final Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.SCHEMA_VALIDATION_MODE, "lax");
            processor.getSchemaManager().load(new StreamSource(new File(configTest.getDataDir() + "leak/schema.xsd")));

            final DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setTreeModel(TreeModel.TINY_TREE);

            XdmNode node = builder.build(new StreamSource(new File(configTest.getDataDir() + "leak/instance.xml")));
            WeakReference<TinyTree> tinyTree = new WeakReference<>(((TinyDocumentImpl) node.getUnderlyingNode()).getTree());

            final XPathCompiler compiler = processor.newXPathCompiler();
            compiler.importSchemaNamespace("http://example.org");
            compiler.evaluate("namespace-uri-from-QName(//*/foo)", node);
            node = builder.build(new StreamSource(new File(configTest.getDataDir() + "leak/instance.xml")));
            System.gc();
            assertNull(tinyTree.get());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testMultipleSchemaImportsTT() {
        loadMultiple(true, true);
    }

    @Test
    public void testMultipleSchemaImportsTF() {
        loadMultiple(true, false);
    }

    @Test
    public void testMultipleSchemaImportsFT() {
        loadMultiple(false, true);
    }

    @Test
    public void testMultipleSchemaImportsFF() {
        loadMultiple(false, false);
    }

    private void loadMultiple(boolean multiple, boolean fromNode) {

        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();

            config.setBooleanProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, multiple);

            String dir = configTest.getDataDir() + "/petrk/AbstractType-2/";

            System.err.println("Loading from file:");

            if (fromNode) {
                loadSchemaFromNode(config, new File(dir + "src/schema.xsd"));
                loadSchemaFromNode(config, new File(dir + "src/schema1.xsd"));
                loadSchemaFromNode(config, new File(dir + "src/schema2.xsd"));
            } else {
                loadSchemaOK(config, new File(dir + "src/schema.xsd"));
                loadSchemaOK(config, new File(dir + "src/schema1.xsd"));
                loadSchemaOK(config, new File(dir + "src/schema2.xsd"));
            }

            System.err.println("Loading successful");

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    private static void loadSchemaOK(EnterpriseConfiguration config, File urlStr) throws XPathException {
        Source s = new StreamSource(urlStr);
        config.addSchemaSource(s);

    }

    private static void loadSchemaFromNode(EnterpriseConfiguration config, File urlStr) throws XPathException {
        Source s = new StreamSource(urlStr);
        ParseOptions options = new ParseOptions().withLineNumbering(true);
        // Note: At 9.8, the test fails if line numbering is switched off, because line numbers
        // are used to determined identity of schema components.
        NodeInfo ni = config.buildDocumentTree(s, options).getRootNode();
        //System.out.println( QueryResult.serialize(ni) );
        //System.out.println( ni.getSystemId() );
        config.addSchemaSource(ni);

    }


    @Test
    public void testValidateTransformationOutput() {

        // This validation run should report exactly two validation errors.

        String xml = "<?xml version='1.0'?>"
                + "<doc><request>"
                + "<user_name>ed</user_name>"
                + "<password>sdsd</password>"
                + "<d1>2009-01-01</d1>"
                + "<d1>2010-01-01</d1>"
                + "<d2>2011-01-01</d2>"
                + "</request></doc>";
        String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>"
                + "<xsl:mode on-no-match='shallow-copy'/>"
                + "<xsl:template match='/'><xsl:result-document validation='strict'><xsl:apply-templates select='//request'/></xsl:result-document></xsl:template>"
                + "<xsl:template match='d1'><date1><xsl:value-of select=\"'x' || .\"/></date1></xsl:template>"
                + "<xsl:template match='d2'><date2><xsl:value-of select=\"'x' || .\"/></date2></xsl:template>"
                + "</xsl:transform>";
        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"user_name\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"password\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"date1\" type=\"xs:date\" maxOccurs='unbounded'/>\n" +
                "\t\t\t\t<xs:element name=\"date2\" type=\"xs:date\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

        List<TransformerException> errors = new ArrayList<>();
        try {
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(xsd)));

            XsltCompiler xsltCompiler = proc.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);
            Xslt30Transformer exec = xsltCompiler.compile(new StreamSource(new StringReader(xsl))).load30();

            exec.setErrorListener(new ErrorListener() {
                @Override
                public void warning(TransformerException exception) {

                }

                @Override
                public void error(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    errors.add(exception);
                }

                @Override
                public void fatalError(TransformerException exception) {
                    System.err.println(exception.getMessage());
                    errors.add(exception);
                }
            });

            StreamSource source = new StreamSource(new StringReader(xml));
            StringWriter sw = new StringWriter();
            exec.transform(source, proc.newSerializer(sw));


            fail("Validation errors not detected");

        } catch (SaxonApiException sapie) {
            assertEquals(4, errors.size(), "No of errors"); // 3 validation errors plus one summary fatal error
            assertEquals("/doc[1]/request[1]/d1[2]", ((ValidationException) errors.get(1)).getValidationFailure().getContextPath().getPathUsingPrefixes(), "ContextPath");

        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test the effect of setting bytecode on or off
     */

    private void testByteCode(boolean byteCodeOn) {
        try {
            String sch1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "  <simpleType name='percentage'>" +
                    "     <restriction base='decimal'>" +
                    "        <minInclusive value='10'/>" +
                    "        <maxInclusive value='20'/>" +
                    "        <totalDigits value='5'/>" +
                    "        <fractionDigits value='2'/>" +
                    "     </restriction></simpleType>" +
                    "</schema>";

            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, byteCodeOn);
            //proc.setConfigurationProperty(Feature.DEBUG_BYTE_CODE, byteCodeOn);
            //proc.setConfigurationProperty(Feature.DEBUG_BYTE_CODE_DIR, configTest.getTempDir() + "Saxon-bytecode");
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(sch1)));
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setSchemaAware(true);
            XQueryExecutable qry = comp.compile("import schema namespace s='http://myexample/family'; sum(for $i in 1 to 100000 return s:percentage($i div 100 mod 10 + 10))");
            XdmItem result = qry.load().evaluateSingle();
            assertEquals("1499500", result.getStringValue(), "result");
            //System.err.println(result.getStringValue());
        } catch (SaxonApiException sapie) {
            System.err.println(sapie.getMessage());
            fail();
        }
    }

    @Test
    @Tag("bytecode")
    public void testByteCodeOn() {
        testByteCode(true);
    }

    @Test
    public void testByteCodeOff() {
        testByteCode(false);
    }

    /**
     * Test the effect of setting bytecode on or off using JAXP SchemaFactory
     */

    private void testByteCodeJaxp(boolean byteCodeOn) {
        try {
            String sch1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\"\n"
                    + "    xmlns=\"http://www.w3.org/2001/XMLSchema\">\n"
                    + "    <element name=\"doc\">\n"
                    + "        <complexType>\n"
                    //+ "            <complexContent>\n"
                    + "                <sequence>\n"
                    + "                    <element name=\"d\" type=\"f:percentage\" xmlns:f=\"http://myexample/family\"\n"
                    + "                        maxOccurs=\"unbounded\" form='qualified'/>\n"
                    + "                </sequence>\n"
                    //+ "            </complexContent>\n"
                    + "        </complexType>\n"
                    + "    </element>\n"
                    + "    <simpleType name=\"percentage\">\n"
                    + "        <restriction base=\"decimal\">\n"
                    + "            <minInclusive value=\"10\"/>\n"
                    + "            <maxInclusive value=\"200\"/>\n"
                    + "            <totalDigits value=\"5\"/>\n"
                    + "            <fractionDigits value=\"2\"/>\n"
                    + "        </restriction>\n"
                    + "    </simpleType>\n"
                    + "</schema>";

            SchemaFactory proc = new SchemaFactoryImpl();
            Schema sch = proc.newSchema(new StreamSource(new StringReader(sch1)));
            Validator val = sch.newValidator();
            StringBuilder sb = new StringBuilder();
            sb.append("<doc xmlns='http://myexample/family'>");
            for (int i = 10; i < 200; i++) {
                for (int j = 10; j <= 99; j++) {
                    sb.append("<d>").append(i).append('.').append(j).append("</d>");
                }
            }
            sb.append("</doc>");
            val.validate(new StreamSource(new StringReader(sb.toString())));

        } catch (SAXException | IOException sapie) {
            System.err.println(sapie.getMessage());
            fail();
        }
    }

    @Test
    @Tag("bytecode")
    public void testByteCodeJaxpOn() {
        testByteCodeJaxp(true);
    }

    @Test
    @Tag("bytecode")
    public void testByteCodeJaxpOff() {
        testByteCodeJaxp(false);
    }


    @Test
    public void testErrorListenerBug4744() {

        // This validation run should report exactly two validation errors.


        final List<TransformerException> errorList = new ArrayList<>();
        FileOutputStream out = null;
        try {
            out = new FileOutputStream(configTest.getDataDir() + "sandpit/validation.out");
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new File(configTest.getDataDir() + "../bugs/4744/UniqueKeyBug.xsd"));
            StreamSource source_xml = new StreamSource(new File(configTest.getDataDir() + "../bugs/4744/UniqueKeyBug.xml"));
            ErrorListener listener = new ErrorListener() {
                @Override
                public void error(TransformerException exception) {
                    errorList.add(exception);
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
                    throw exception;
                }

                @Override
                public void warning(TransformerException exception) {
                    errorList.add(exception);
                }
            };

            SchemaValidator sv = sm.newSchemaValidator();
            sv.setErrorListener(listener);
            sm.load(source_xsd);


            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputStream(out);
            sv.setDestination(serializer);


            sv.validate(source_xml);

            out.close();
            //fail("Validation errors not detected");

        } catch (SaxonApiException sapie) {
            try {
                out.close();
            } catch (IOException ee) {
                fail();
            }
            assertEquals(1, errorList.size());
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    private void testSchemaLocationHints(boolean useHints, boolean withNS) {
        try {
            Processor processor = new Processor(true);
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode source = builder.build(new File(configTest.getDataDir() + (withNS ? "xsdhints/documentNS.xml" : "xsdhints/document.xml")));

            SchemaManager manager = processor.getSchemaManager();
            SchemaValidator validator = manager.newSchemaValidator();

            validator.setUseXsiSchemaLocation(useHints);

            validator.validate(source.asSource());

            if (!useHints) {
                fail("Document should not have validated");
            }
        } catch (Exception ex) {
            if (useHints) {
                fail("Validation should not have failed");
            }
        }
    }

    @Test
    public void testIgnoreSchemaLocationHints() {
        testSchemaLocationHints(false, false);
        testSchemaLocationHints(false, true);
        testSchemaLocationHints(true, false);
        testSchemaLocationHints(true, true);
    }

    private void testSchemaLocationHintsWithDestination(boolean useHints, boolean withNS) {
        try {
            Processor processor = new Processor(true);
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode source = builder.build(new File(configTest.getDataDir() + (withNS ? "xsdhints/documentNS.xml" : "xsdhints/document.xml")));

            SchemaManager manager = processor.getSchemaManager();
            SchemaValidator validator = manager.newSchemaValidator();
            validator.setUseXsiSchemaLocation(useHints);

            XQueryCompiler comp = processor.newXQueryCompiler();
            XQueryEvaluator eval = comp.compile(".").load();
            eval.setContextItem(source);
            eval.run(validator);

            if (!useHints) {
                fail("Document should not have validated");
            }
        } catch (Exception ex) {
            if (useHints) {
                fail("Validation should not have failed");
            }
        }
    }

    @Test
    public void testIgnoreSchemaLocationHintsWithValidatorDestination() {
        testSchemaLocationHintsWithDestination(false, false);
        testSchemaLocationHintsWithDestination(false, true);
        testSchemaLocationHintsWithDestination(true, false);
        testSchemaLocationHintsWithDestination(true, true);
    }


    @Test
    public void testErrorListenerBug4744b() {

        // This validation run should report exactly two validation errors.


        final List<SAXParseException> errorList = new ArrayList<>();
        FileOutputStream out = null;
        try {
            out = new FileOutputStream(configTest.getDataDir() + "sandpit/validation.out");
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new File(configTest.getDataDir() + "../bugs/4744/UniqueKeyBug.xsd"));
            StreamSource source_xml = new StreamSource(new File(configTest.getDataDir() + "../bugs/4744/UniqueKeyBug.xml"));
            ErrorHandler handler = new ErrorHandler() {
                @Override
                public void error(SAXParseException exception) {
                    errorList.add(exception);
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXParseException {
                    throw exception;
                }

                @Override
                public void warning(SAXParseException exception) {
                    errorList.add(exception);
                }
            };

            SchemaValidator sv = sm.newSchemaValidator();
            sv.setInvalidityHandler(new ValidatorImpl.InvalidityHandlerWrappingErrorHandler(handler));
            sm.load(source_xsd);


            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputStream(out);
            sv.setDestination(serializer);


            sv.validate(source_xml);

            out.close();
            //fail("Validation errors not detected");

        } catch (SaxonApiException sapie) {
            try {
                out.close();
            } catch (IOException ee) {
                fail();
            }
            assertEquals(1, errorList.size());
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Disabled
    public void testBug4507() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(
                    new File("/Users/mike/bugs/2020/4507-Carter/2000-04-03/data/schema.xsd")));
            SchemaValidator val = sm.newSchemaValidator();
            val.validate(new StreamSource(
                    new File("/Users/mike/bugs/2020/4507-Carter/2000-04-03/data/data.xml")));
            Instrumentation.report();
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


    @Disabled
    public void testBug4507bytecode() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, true);
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(
                    new File("/Users/mike/bugs/2020/4507-Carter/2000-04-03/data/schema.xsd")));
            SchemaValidator val = sm.newSchemaValidator();
            val.validate(new StreamSource(
                    new File("/Users/mike/bugs/2020/4507-Carter/2000-04-03/data/data.xml")));
            Instrumentation.report();
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSchemaForXslt30Direct() {
        // This should use the resolver mechanism to load the schema locally
        try {
            Processor proc = new Processor(true);
            SchemaManager schemaManager = proc.getSchemaManager();
            schemaManager.load(new StreamSource("https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd"));

            SchemaValidator validator = schemaManager.newSchemaValidator();
            validator.validate(new StreamSource(new File(configTest.getDataDir(), "caffo/Transform2.xslt")));
        } catch (Exception e) {
            System.err.println(e.getMessage());
            fail();
        }
    }

    @Test
    public void testSchemaForXslt30viaXQuery() {
        // This should use the resolver mechanism to load the schema locally
        try {
            Processor proc = new Processor(true);
            SchemaManager schemaManager = proc.getSchemaManager();

            XQueryCompiler compiler = proc.newXQueryCompiler();
            XQueryEvaluator eval = compiler.compile("doc('https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd')").load();
            XdmNode schema = (XdmNode) eval.evaluate();

            schemaManager.load(schema.asSource());
            SchemaValidator validator = schemaManager.newSchemaValidator();
            validator.validate(new StreamSource(new File(configTest.getDataDir(), "caffo/Transform2.xslt")));
        } catch (Exception e) {
            System.err.println(e.getMessage());
            fail();
        }
    }

    @Test
    public void testSchemaForXslt30viaDocumentBuilder() {
        // This should use the resolver mechanism to load the schema locally
        try {
            Processor proc = new Processor(true);
            SchemaManager schemaManager = proc.getSchemaManager();

            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode schema = builder.build(new StreamSource("https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd"));
            schemaManager.load(schema.asSource());

            SchemaValidator validator = schemaManager.newSchemaValidator();
            validator.validate(new StreamSource(new File(configTest.getDataDir(), "caffo/Transform2.xslt")));
        } catch (Exception e) {
            System.err.println(e.getMessage());
            fail();
        }
    }

    @Test
    public void testMakeWarningFatal() {

        // Attempt to promote the warning on an unresolved xsl:include to a fatal error. Bug 5478.

        String xsd = "<xsd:schema\n"
                + "elementFormDefault='qualified'\n"
                + "xmlns:xsd='http://www.w3.org/2001/XMLSchema'>\n"
                + "<xsd:include schemaLocation='nonexistentA.xsd'/>\n"
                + "<xsd:include schemaLocation='nonexistentB.xsd'/>\n"
                + "</xsd:schema>";
        final List<XmlProcessingError> errorList = new ArrayList<>();
        try {
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));
            sm.setErrorReporter(err -> {
                err.setTerminationMessage("Far too many errors");
                errorList.add(err);
            });

            sm.load(source_xsd);
            fail("Validation errors not detected");

        } catch (SaxonApiException sapie) {
            assertEquals(1, errorList.size());
            assertTrue(sapie.getMessage().contains("Far too many errors"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testMakeDuplicateErrorFatal() {

        // Attempt to promote the error on a duplicate declaration to a fatal error. Bug 5478.

        String xsd = "<xsd:schema\n"
                + "elementFormDefault='qualified'\n"
                + "xmlns:xsd='http://www.w3.org/2001/XMLSchema'>\n"
                + "<xsd:element name='AAA'/>\n"
                + "<xsd:element name='AAA'/>\n"
                + "<xsd:element name='BBB'/>\n"
                + "<xsd:element name='BBB'/>\n"
                + "</xsd:schema>";
        final List<XmlProcessingError> errorList = new ArrayList<>();
        try {
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            StreamSource source_xsd = new StreamSource(new StringReader(xsd));
            sm.setErrorReporter(err -> {
                err.setTerminationMessage("Far too many errors");
                errorList.add(err);
            });

            sm.load(source_xsd);
            fail("Validation errors not detected");

        } catch (SaxonApiException sapie) {
            assertEquals(1, errorList.size());
            assertTrue(sapie.getMessage().contains("Far too many errors"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testParallelSchemaCompile() {   // Bug 5978
        try {
            Processor proc = new Processor(true);
            SchemaManager manager = proc.getSchemaManager();
            ExecutorService executor = Executors.newFixedThreadPool(10);
            for (int i = 0; i < 30; i++) {
                executor.execute(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            manager.load(new File(configTest.getDataDir() + "../bugs/5978-Rose/schema/ACSJournal-layout.xsd"));
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                            ex.printStackTrace();
                            fail();
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);
        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testKeyCardinalityInference() {   // Bug 6033
        // Should not produce any warnings
        String xsd = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                + "<xs:schema\n"
                + "  xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                + "  xmlns=\"http://erehwon.org/xsd/foo\"\n"
                + "  xmlns:t=\"http://erehwon.org/xsd/foo\"\n"
                + "  targetNamespace=\"http://erehwon.org/xsd/foo\"\n"
                + "  elementFormDefault=\"qualified\"\n"
                + "  attributeFormDefault=\"unqualified\">\n"
                + "\n"
                + "  <xs:element name=\"Docu\"\n"
                + "              type=\"DocuType\">\n"
                + "    <xs:key name=\"MassUnitKey\">\n"
                + "      <xs:selector\n"
                + "        xpath=\"t:FileUnits/t:MassUnit\"/>\n"
                + "      <xs:field xpath=\"t:UnitName\"/>\n"
                + "    </xs:key>\n"
                + "    <xs:key name=\"LinearUnitKey\">\n"
                + "      <xs:selector\n"
                + "        xpath=\"t:FileUnits/t:LinearUnit\n"
                + "\t     | t:FileUnits/t:PMILinearUnit\"/>\n"
                + "      <xs:field xpath=\"t:UnitName\"/>\n"
                + "    </xs:key>\n"
                + "  </xs:element>\n"
                + "\n"
                + "  <xs:complexType name=\"DocuType\">\n"
                + "    <xs:sequence>\n"
                + "      <xs:element ref=\"FileUnits\"\n"
                + "                  minOccurs=\"0\">\n"
                + "      </xs:element>\n"
                + "    </xs:sequence>\n"
                + "  </xs:complexType>\n"
                + "\n"
                + "  <xs:complexType name=\"UnitsType\">\n"
                + "    <xs:sequence>\n"
                + "      <xs:element name=\"LinearUnit\"\n"
                + "                  type=\"LinearUnitType\"\n"
                + "                  minOccurs=\"0\"\n"
                + "                  maxOccurs=\"unbounded\"/>\n"
                + "      <xs:element name=\"MassUnit\"\n"
                + "                  type=\"MassUnitType\"\n"
                + "                  minOccurs=\"0\"\n"
                + "                  maxOccurs=\"unbounded\"/>\n"
                + "      <xs:element name=\"PMILinearUnit\"\n"
                + "                  type=\"LinearUnitType\"\n"
                + "                  minOccurs=\"0\"/>\n"
                + "    </xs:sequence>\n"
                + "  </xs:complexType>\n"
                + "\n"
                + "  <xs:complexType name=\"LinearUnitType\">\n"
                + "    <xs:sequence>\n"
                + "      <xs:element name=\"UnitName\"\n"
                + "                  type=\"xs:token\"/>\n"
                + "    </xs:sequence>\n"
                + "  </xs:complexType>\n"
                + "\n"
                + "  <xs:complexType name=\"MassUnitType\">\n"
                + "    <xs:sequence>\n"
                + "      <xs:element name=\"UnitName\"\n"
                + "                  type=\"xs:token\"/>\n"
                + "    </xs:sequence>\n"
                + "  </xs:complexType>\n"
                + "\n"
                + "  <xs:element name=\"FileUnits\"\n"
                + "              type=\"UnitsType\">\n"
                + "  </xs:element>\n"
                + "\n"
                + "\n"
                + "</xs:schema>\n";

        Processor proc = new Processor(true);
        SchemaManager sm = proc.getSchemaManager();
        sm.setErrorReporter(error -> fail(error.getMessage()));
        try {
            sm.load(new StreamSource(new StringReader(xsd)));
        } catch (SaxonApiException e) {
            fail();
        }

    }

    @Test
    public void testAttributePathInReport() {   // Bug 6085
        String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                + "<PIM>\n"
                + "    <category id=\"c-1\" title=\"Products\"/>\n"
                + "    <category id=\"c-2\" title=\"Sub-Category\"/>\n"
                + "    <category id=\"c-3\" title=\"\"/>\n"
                + "    <category id=\"c-4\" title=\"\"/>\n"
                + "    <extra>forty-two</extra>\n"
                + "    <nonsense/>\n"
                + "</PIM>";
        String xsd = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n"
                + "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n"
                + "    <xs:element name=\"PIM\">\n"
                + "        <xs:complexType>\n"
                + "            <xs:sequence>\n"
                + "                <xs:choice maxOccurs=\"unbounded\">\n"
                + "                    <xs:element name=\"category\" type=\"categoryType\"/>\n"
                + "                    <xs:element name=\"extra\" type=\"xs:integer\"/>\n"
                + "                </xs:choice>\n"
                + "            </xs:sequence>\n"
                + "        </xs:complexType>\n"
                + "    </xs:element>\n"
                + "\n"
                + "    <xs:complexType name=\"categoryType\">\n"
                + "        <xs:attribute name=\"id\" type=\"xs:ID\" use=\"required\"/>\n"
                + "        <xs:attribute name=\"title\" use=\"required\">\n"
                + "            <xs:simpleType>\n"
                + "                <xs:restriction base=\"xs:string\">\n"
                + "                    <xs:minLength value=\"1\"/>\n"
                + "                </xs:restriction>\n"
                + "            </xs:simpleType>\n"
                + "        </xs:attribute>\n"
                + "    </xs:complexType>\n"
                + "</xs:schema>";
        StringWriter report = new StringWriter();
        try {
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(xsd)));
            SchemaValidator val = sm.newSchemaValidator();
            val.setValidityReporting(proc.newSerializer(report));
            val.validate(new StreamSource(new StringReader(xml)));
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            System.err.println(report);
            assert (report.toString().contains("path=\"/Q{}PIM[1]/Q{}category[3]/@title\""));
            assert (report.toString().contains("path=\"/Q{}PIM[1]/Q{}extra[1]\""));
            assert (report.toString().contains("path=\"/Q{}PIM[1]/Q{}nonsense[1]\""));
        }
    }
}
