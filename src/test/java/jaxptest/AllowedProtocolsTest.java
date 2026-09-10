package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.FeatureKeys;
import org.junit.jupiter.api.Test;

import javax.xml.XMLConstants;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test use of options XMLConstants.ACCESS_EXTERNAL_STYLESHEET, ACCESS_EXTERNAL_DTD
 */
public class AllowedProtocolsTest {


    private void externalProtocolsCompileTime(String baseURI, String declaration, String rule, boolean expectSuccess) {
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    version=\"3.0\">\n" +
                            declaration +
                            "    <xsl:variable name='g' select='/'/>" +
                            "    <xsl:template match='/'><out val='{$g}'/></xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactoryImpl factory = new EnterpriseTransformerFactory();
            factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_STYLESHEET, rule);
            StreamSource xslS = new StreamSource(new StringReader(xsl), baseURI);
            Templates templates = factory.newTemplates(xslS);
            if (!expectSuccess) {
                fail("Expected failure to access external resource");
            }
        } catch (TransformerException e) {
            if (expectSuccess) {
                fail("Expected compilation to succeed");
            }
        }
    }

    @Test
    public void testExternalProtocolsCompileTime001() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:include href='identity.xsl'/>",
                                     "all",
                                     true);
    }

    @Test
    public void testExternalProtocolsCompileTime002() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:include href='identity.xsl'/>",
                                     "file",
                                     true);
    }

    @Test
    public void testExternalProtocolsCompileTime003() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:include href='identity.xsl'/>",
                                     "http, file",
                                     true);
    }

    @Test
    public void testExternalProtocolsCompileTime004() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:include href='identity.xsl'/>",
                                     "http, https",
                                     false);
    }

    @Test
    public void testExternalProtocolsCompileTime005() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:include href='identity.xsl'/>",
                                     "",
                                     false);
    }

    @Test
    public void testExternalProtocolsCompileTime006() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:import href='identity.xsl'/>",
                                     "all",
                                     true);
    }

    @Test
    public void testExternalProtocolsCompileTime007() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:import href='identity.xsl'/>",
                                     "",
                                     false);
    }

    @Test
    public void testExternalProtocolsCompileTime008() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:import-schema schema-location='books.xsd'/>",
                                     "all",
                                     true);
    }

    @Test
    public void testExternalProtocolsCompileTime009() {
        externalProtocolsCompileTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                     "<xsl:import-schema schema-location='books.xsd'/>",
                                     "",
                                     false);
    }

    private void externalProtocolsRunTime(String baseURI, String instruction, String rule, boolean expectSuccess) {
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    version='3.0' expand-text='yes'>\n" +
                            "    <xsl:variable name='g' select='/'/>" +
                            "    <xsl:template match='/'>" + instruction + "</xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xmlA = "<doc>A</doc>";
            String xmlB = "<doc>B</doc>";

            TransformerFactoryImpl factory = new EnterpriseTransformerFactory();
            factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_STYLESHEET, rule);
            StreamSource xslS = new StreamSource(new StringReader(xsl), baseURI);
            Templates templates = factory.newTemplates(xslS);
            Transformer transformer = templates.newTransformer();
            transformer.transform(new StreamSource(new StringReader(xmlA)), new StreamResult(System.out));
            if (!expectSuccess) {
                fail("Expected failure to access external resource");
            }
        } catch (TransformerException e) {
            if (expectSuccess) {
                fail("Expected execution to succeed");
            }
        }
    }

    @Test
    public void testExternalProtocolsRunTime001() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{name(doc('birthdays.xml')/*)}",
                                 "all",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime002() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{name(doc('birthdays.xml')/*)}",
                                 "file",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime003() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{name(doc('birthdays.xml')/*)}",
                                 "http, https",
                                 false);
    }

    @Test
    public void testExternalProtocolsRunTime004() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{name(doc('birthdays.xml')/*)}",
                                 "",
                                 false);
    }

    @Test
    public void testExternalProtocolsRunTime005() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{string-length(unparsed-text('birthdays.xml'))}",
                                 "file, http",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime006() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{string-length(unparsed-text('birthdays.xml'))}",
                                 "https, http",
                                 false);
    }

    @Test
    public void testExternalProtocolsRunTime007() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{json-doc('sample.json')?name}",
                                 "file, http",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime008() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "{json-doc('sample.json')?name}",
                                 "https, http",
                                 false);
    }

    @Test
    public void testExternalProtocolsRunTime009() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "<xsl:source-document href='birthdays.xml'>{name(/*)}</xsl:source-document>",
                                 "file, http",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime010() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "<xsl:source-document href='birthdays.xml'>{name(/*)}</xsl:source-document>",
                                 "",
                                 false);
    }

    @Test
    public void testExternalProtocolsRunTime011() {
        externalProtocolsRunTime("data:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "<xsl:sequence select=\"document('data:text/xml;base64,PGRvYy8+')/*/local-name()\"/>",
                                 "file data",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime012() {
        externalProtocolsRunTime("data:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "<xsl:sequence select=\"document('data:text/xml;base64,PGRvYy8+')/*/local-name()\"/>",
                                 "file http https",
                                 false);
    }

    @Test
    public void testExternalProtocolsRunTime013() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "<xsl:sequence select=\"unparsed-text('classpath:saxon-license.lic')\"/>",
                                 "file data classpath",
                                 true);
    }

    @Test
    public void testExternalProtocolsRunTime014() {
        externalProtocolsRunTime("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                                 "<xsl:sequence select=\"unparsed-text('classpath:saxon-license.lic')\"/>",
                                 "file http https",
                                 false);
    }

   

    // TODO: add tests for fn:transform

    private void externalProtocolsDTD(String baseURI, String instruction, String rule, boolean expectSuccess) {
        try {
            String xsl =
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "    version='3.0' expand-text='yes'>\n" +
                            "    <xsl:template match='/'>" + instruction + "</xsl:template>\n" +
                            "</xsl:stylesheet>";
            String xmlA = "<doc>A</doc>";

            TransformerFactoryImpl factory = new EnterpriseTransformerFactory();
            // Apache Xerces does not recognise ACCESS_EXTERNAL_DTD, so use the JDK parser
            factory.setAttribute(FeatureKeys.SOURCE_PARSER_CLASS, "com.sun.org.apache.xerces.internal.jaxp.SAXParserFactoryImpl");
            factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_DTD, rule);
            StreamSource xslS = new StreamSource(new StringReader(xsl), baseURI);
            Templates templates = factory.newTemplates(xslS);
            Transformer transformer = templates.newTransformer();
            transformer.transform(new StreamSource(new StringReader(xmlA)), new StreamResult(System.out));
            if (!expectSuccess) {
                fail("Expected failure to access external resource");
            }
        } catch (TransformerException e) {
            if (expectSuccess) {
                fail("Expected execution to succeed");
            }
        }
    }

    @Test
    public void testExternalProtocolsDTD001() {
        externalProtocolsDTD("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                             "{name(doc('othello.xml')/*)}",
                             "all",
                             true);
    }

    @Test
    public void testExternalProtocolsDTD002() {
        externalProtocolsDTD("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                             "{name(doc('othello.xml')/*)}",
                             "file",
                             true);
    }

    @Test
    public void testExternalProtocolsDTD003() {
        externalProtocolsDTD("file:///" + ConfigTest.DATA_DIR + "dummy.xsl",
                             "{name(doc('othello.xml')/*)}",
                             "http",
                             false);
    }


}





