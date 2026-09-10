package s9apitest;

import net.sf.saxon.Configuration;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.s9api.*;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.QNameValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api XsltTransformer
 */
public class TestXsltTransformerEE {
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
    public void testExtensionWithSchemaAccess() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty("http://saxon.sf.net/feature/timing", "true");

            XsdCompiler sm = proc.newXsdCompiler();
            XsdSchema schema = sm.compile(new StreamSource(Testdata.file("books.xsd")));
            proc.registerExtensionFunction(new TestXslt30TransformerEE.IsOptionalTest(schema));

            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                    "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                    "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer transformer = exec.load();

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(schema.newValidator());
            XdmNode books = builder.build(Testdata.file("books.xml"));
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
        int cardinality = ((ComplexType) parentType).getElementParticleCardinality(fp, null, true);
        return Cardinality.allowsZero(cardinality);
    }

}

