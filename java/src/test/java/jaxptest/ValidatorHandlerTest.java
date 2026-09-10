package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import net.sf.saxon.lib.NamespaceConstant;
import org.junit.jupiter.api.Test;
import org.w3c.dom.ls.LSInput;
import org.w3c.dom.ls.LSResourceResolver;
import org.xml.sax.*;
import org.xml.sax.ext.DefaultHandler2;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParser;
import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.TypeInfoProvider;
import javax.xml.validation.ValidatorHandler;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for JAXP validatorHandler interface
 */
public class ValidatorHandlerTest {
    private final ConfigTest configTest = new ConfigTest();

    private void parseToValidator(ValidatorHandler handler, InputSource source) {
        try {
            SAXParserFactory parserFactory = SAXParserFactory.newInstance();
            parserFactory.setNamespaceAware(true);
            SAXParser parser = parserFactory.newSAXParser();
            XMLReader reader = parser.getXMLReader();
            reader.setContentHandler(handler);
            reader.parse(source);
        } catch (ParserConfigurationException | SAXException | IOException e) {
            fail();
        }
    }

    @Test
    public void testTypeAnnotations() {
        try {
            SchemaFactoryImpl schemaFactory = new SchemaFactoryImpl();
            Schema grammar = schemaFactory.newSchema(new File(configTest.getDataDir() + "books.xsd"));
            ValidatorHandler validator = grammar.newValidatorHandler();
            TypeInfoProvider provider = validator.getTypeInfoProvider();
            validator.setContentHandler(new TypeInfoChecker(provider));
            parseToValidator(validator, new InputSource(Files.newInputStream(Paths.get(configTest.getDataDir() + "books.xml"))));

        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    private static class TypeInfoChecker extends DefaultHandler2 {

        private TypeInfoProvider provider;

        public TypeInfoChecker(TypeInfoProvider provider) {
            this.provider = provider;
        }

        @Override
        public void startElement(String uri, String localName, String qName, Attributes attributes) throws SAXException {
            if (localName.equals("AUTHOR")) {
                assertEquals(provider.getElementTypeInfo().getTypeName(), "string");
                assertEquals(provider.getElementTypeInfo().getTypeNamespace(), NamespaceConstant.SCHEMA);
            }
            if (localName.equals("DIMENSIONS")) {
                assertEquals(provider.getElementTypeInfo().getTypeName(), "dimensionsType");
                assertEquals(provider.getElementTypeInfo().getTypeNamespace(), "");
            }
            if (localName.equals("ITEM")) {
                int index = attributes.getIndex("", "CAT");
                assertEquals("IDREF", provider.getAttributeTypeInfo(index).getTypeName());
                assertEquals(NamespaceConstant.SCHEMA, provider.getAttributeTypeInfo(index).getTypeNamespace());
            }
            if (localName.equals("CATEGORY")) {
                int index = attributes.getIndex("", "CODE");
                assertEquals("ID", provider.getAttributeTypeInfo(index).getTypeName());
                assertEquals(NamespaceConstant.SCHEMA, provider.getAttributeTypeInfo(index).getTypeNamespace());
                assertTrue(provider.isIdAttribute(index));
            }
        }
    }


    @Test
    public void testDefaultErrorHandling() {
        try {
            SchemaFactoryImpl schemaFactory = new SchemaFactoryImpl();
            Schema grammar = schemaFactory.newSchema(new File(configTest.getDataDir() + "books.xsd"));
            ValidatorHandler validator = grammar.newValidatorHandler();
            parseToValidator(validator, new InputSource(Files.newInputStream(Paths.get(configTest.getDataDir() + "books-invalid.xml"))));

        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void testResolver() {
        try {
            SchemaFactoryImpl schemaFactory = new SchemaFactoryImpl();
            Schema grammar = schemaFactory.newSchema(new File(configTest.getDataDir() + "books.xsd"));
            ValidatorHandler validator = grammar.newValidatorHandler();
            Resolver resolver = new Resolver();
            validator.setResourceResolver(resolver);
            assertEquals(resolver, validator.getResourceResolver());
            parseToValidator(validator, new InputSource(Files.newInputStream(Paths.get(configTest.getDataDir() + "books-invalid.xml"))));

        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Inner class to implement a resource resolver. This version always returns null, which
     * has the same effect as not supplying a resource resolver at all. The LSResourceResolver
     * is part of the DOM Level 3 load/save module.
     */

    protected static class Resolver implements LSResourceResolver {

        /**
         * Resolve a reference to a resource
         *
         * @param type      The type of resource, for example a schema, source XML document, or query
         * @param namespace The target namespace (in the case of a schema document)
         * @param publicId  The public ID
         * @param systemId  The system identifier (as written, possibly a relative URI)
         * @param baseURI   The base URI against which the system identifier should be resolved
         * @return an LSInput object typically containing the character stream or byte stream identified
         * by the supplied parameters; or null if the reference cannot be resolved or if the resolver chooses
         * not to resolve it.
         */

        public LSInput resolveResource(String type, String namespace, String publicId, String systemId, String baseURI) {
            return null;
        }

    }

    // Switch to allow running tests with Xerces, for comparison purposes
    private final static boolean USE_XERCES = false;

    private SchemaFactory makeSchemaFactory() {
        if (USE_XERCES) {
            return new org.apache.xerces.jaxp.validation.XMLSchemaFactory();
        } else {
            return new SchemaFactoryImpl();
        }
    }

    @Test
    public void testRecoverableErrorHandling() {
        final List<Integer> counts = new ArrayList<Integer>();
        counts.add(0);
        counts.add(0);
        counts.add(0);
        try {
            SchemaFactory schemaFactory = makeSchemaFactory();
            Schema grammar = schemaFactory.newSchema(new File(configTest.getDataDir() + "books.xsd"));
            ValidatorHandler validator = grammar.newValidatorHandler();

            validator.setErrorHandler(new ErrorHandler() {
                @Override
                public void warning(SAXParseException exception) throws SAXException {
                    counts.set(0, counts.get(0) + 1);
                }

                @Override
                public void error(SAXParseException exception) throws SAXException {
                    counts.set(1, counts.get(1) + 1);
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXException {
                    counts.set(2, counts.get(2) + 1);
                }
            });
            parseToValidator(validator, new InputSource(Files.newInputStream(Paths.get(configTest.getDataDir() + "books-invalid.xml"))));
            assertEquals(0, (int) counts.get(0), "warnings");
            assertEquals(3, (int) counts.get(1), "errors");
            assertEquals(0, (int) counts.get(2), "fatals");

        } catch (SAXException | IOException e) {
            e.printStackTrace();
            fail();
        }
    }





    private void validate(String xsd, String xml, int expectedErrors) {
        try {
            EnterpriseConfiguration conf = new EnterpriseConfiguration();
            SchemaFactory schemaFactory = new SchemaFactoryImpl(conf);
            Schema schema = schemaFactory.newSchema(new StreamSource(new StringReader(xsd)));

            ValidatorHandler validator = schema.newValidatorHandler();
            CustomHandler handler = new CustomHandler();
            validator.setErrorHandler(handler);
            parseToValidator(validator, new InputSource(new StringReader(xml)));
            assertEquals(expectedErrors, handler.errorCount());
        } catch (Exception err) {
            fail(err);
        }
    }

    // Following tests are related to bug 5208

    private final static String listOfElements = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n"
            + "\n"
            + " <xs:element name='list'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence>\n"
            + "    <xs:element ref='item' maxOccurs='unbounded'/>\n"
            + "   </xs:sequence>\n"
            + "  </xs:complexType>\n"
            + "  <xs:unique name='u'>\n"
            + "   <xs:selector xpath='item'/>\n"
            + "   <xs:field xpath='value'/>\n"
            + "  </xs:unique>\n"
            + " </xs:element>\n"
            + "\n"
            + " <xs:element name='item'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence>\n"
            + "    <xs:element name='value' type='xs:$$$$'/>\n"
            + "   </xs:sequence>\n"
            + "  </xs:complexType>\n"
            + " </xs:element>\n"
            + "    \n"
            + "</xs:schema>";

    private final static String listOfAttributes = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n"
            + "\n"
            + " <xs:element name='list'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence>\n"
            + "    <xs:element ref='item' maxOccurs='unbounded'/>\n"
            + "   </xs:sequence>\n"
            + "  </xs:complexType>\n"
            + "  <xs:unique name='u'>\n"
            + "   <xs:selector xpath='item'/>\n"
            + "   <xs:field xpath='@value'/>\n"
            + "  </xs:unique>\n"
            + " </xs:element>\n"
            + "\n"
            + " <xs:element name='item'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence/>\n"
            + "   <xs:attribute name='value' type='xs:$$$$'/>\n"
            + "  </xs:complexType>\n"
            + " </xs:element>\n"
            + "    \n"
            + "</xs:schema>";

    @Test
    public void testUniqueIntsE() {
        String xsd = listOfElements.replace("$$$$", "int");
        validate(xsd, "<list><item><value>1</value></item><item><value>10</value></item><item><value>20</value></item></list>", 0);
        validate(xsd, "<list><item><value>1</value></item><item><value>10</value></item><item><value>1</value></item></list>", 1);
        validate(xsd, "<list><item><value>1</value></item><item><value>Z</value></item><item><value>1</value></item></list>", 2);
    }

    @Test
    public void testUniqueStringsE() {
        String xsd = listOfElements.replace("$$$$", "string");
        validate(xsd, "<list><item><value>X1</value></item><item><value>X10</value></item><item><value>X20</value></item></list>", 0);
        validate(xsd, "<list><item><value>X1</value></item><item><value>X10</value></item><item><value>X1</value></item></list>", 1);
    }

    @Test
    public void testUniqueQNamesE() {
        String xsd = listOfElements.replace("$$$$", "QName");
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item><value>X:A</value></item><item><value>X:B</value></item><item><value>Y:C</value></item></list>", 0);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item><value>X:A</value></item><item><value>X:B</value></item><item><value>Y:A</value></item></list>", 1);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item><value>X:A</value></item><item><value>X:22</value></item><item><value>Y:A</value></item></list>", 2);
    }

    @Test
    public void testUniqueIntsA() {
        String xsd = listOfAttributes.replace("$$$$", "int");
        validate(xsd, "<list><item value='1'/><item value='20'/><item value='10'/></list>", 0);
        validate(xsd, "<list><item value='1'/><item value='-20'/><item value='1'/></list>", 1);
        validate(xsd, "<list><item value='1'/><item value='XIII'/><item value='1'/></list>", 2);
    }

    @Test
    public void testUniqueStringsA() {
        String xsd = listOfAttributes.replace("$$$$", "string");
        validate(xsd, "<list><item value='x1'/><item value='x20'/><item value='x10'/></list>", 0);
        validate(xsd, "<list><item value='x1'/><item value='x-20'/><item value='x1'/></list>", 1);
    }

    @Test
    public void testUniqueQNamesA() {
        String xsd = listOfAttributes.replace("$$$$", "QName");
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item value='X:A'/><item value='X:B'/><item value='Y:C'/></list>", 0);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item value='X:A'/><item value='X:B'/><item value='Y:A'/></list>", 1);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item value='X:A'/><item value='X:22'/><item value='X:A'/></list>", 2);
    }

    private static class CustomHandler implements ErrorHandler {

        int errors = 0;

        @Override
        public void warning(SAXParseException exception) throws SAXException {
            System.out.println("Warning: " + exception);
        }

        @Override
        public void error(SAXParseException exception) throws SAXException {
            System.out.println("Error: " + exception);
            errors++;
        }

        @Override
        public void fatalError(SAXParseException exception) throws SAXException {
            System.out.println("Fatal error: " + exception);
        }

        public int errorCount() {
            return errors;
        }
    }


}


