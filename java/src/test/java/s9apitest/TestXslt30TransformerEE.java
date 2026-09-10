////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import net.sf.saxon.Configuration;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.s9api.*;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.QNameValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer
 *
 */
public class TestXslt30TransformerEE {

    private static class IsOptionalTest implements ExtensionFunction {

        private final XsdSchema schema;

        public IsOptionalTest(XsdSchema schema) {
            this.schema = schema;
        }

        @Override
        public QName getName() {
            return new QName("java:s9apitest.TestXslt30Transformer", "isOptional");
        }

        @Override
        public net.sf.saxon.s9api.SequenceType getResultType() {
            return ItemType.BOOLEAN.one();
        }

        @Override
        public net.sf.saxon.s9api.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.s9api.SequenceType[]{ItemType.ANY_NODE.one(), ItemType.QNAME.one()};
        }

        @Override
        public XdmValue call(XdmValue[] arguments) throws SaxonApiException {
            NodeInfo parent = (NodeInfo) arguments[0].getUnderlyingValue().head();
            QNameValue childName = (QNameValue) arguments[1].getUnderlyingValue().head();
            Configuration config = parent.getConfiguration();
            SchemaType parentType = parent.getSchemaType();
            int fp = config.getNamePool().allocateFingerprint(childName.getNamespaceURI(), childName.getLocalName());
            int cardinality = ((ComplexType) parentType).getElementParticleCardinality(fp, schema.getUnderlyingSchema(), true);
            return new XdmAtomicValue(Cardinality.allowsZero(cardinality));
        }
    }



    @Test
    public void testExtensionWithSchemaAccess() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);

            XsdCompiler sm = proc.newXsdCompiler();
            XsdSchema schema = sm.compile(new StreamSource(Testdata.file("books.xsd")));

            proc.registerExtensionFunction(new IsOptionalTest(schema));
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                                                       "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                                                       "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(schema.newValidator());
            XdmNode books = builder.build(Testdata.file("books.xml"));
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

    /**
     * Load and run a SEF file that specifies relocatable="yes" and calls the doc() function
     */

    @Test
    public void testRelocatableExport() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            XsltExecutable executable = xsltCompiler.compile(new StreamSource(Testdata.file("booksWithMain10.sef")));

            Xslt30Transformer transformer = executable.load30();

            Serializer dest = processor.newSerializer();
            dest.setOutputFile(Testdata.file("/sandpit/OutputStep2.xml"));

            transformer.callTemplate(new QName("main"), dest);


        } catch (Exception e) {
            e.printStackTrace();
            fail();

        }

    }

    @Test
    public void testDiagnosticsOnAtomizationFailureSansExport() {
        testDiagnosticsOnAtomizationFailure(false);
    }

    @Test
    public void testDiagnosticsOnAtomizationFailureWithExport() {
        testDiagnosticsOnAtomizationFailure(true);
    }

    private void testDiagnosticsOnAtomizationFailure(boolean export) {
        // Bug 4086

        String xsl = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f = 'http://functions/' " +
                "                version='3.0'>" +
                "                <xsl:param name='p' select='map{2:5}'/>" +
                "                <xsl:template name='main' as='xs:integer+'>" +
                "                     <xsl:sequence select=\"f:do($p)\" />        " +
                "                </xsl:template>" +
                "                <xsl:function name='f:do' as='xs:integer+'>" +
                "                     <xsl:param name='x' as='xs:integer'/>" +
                "                     <xsl:sequence select='$x+3' />        " +
                "                </xsl:function>" +
                "                </xsl:stylesheet>";

        try {
            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            c.setJustInTimeCompilation(false);
            XsltExecutable exec = c.compile(new StreamSource(new StringReader(xsl)));
            if (export) {
                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                exec.export(baos);
                try {
                    baos.close();
                } catch (IOException e) {
                    fail();
                }
                ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
                Processor p2 = new Processor(true);
                XsltCompiler c2 = p2.newXsltCompiler();
                exec = c2.loadLibraryPackage(new StreamSource(bais)).link();
            }
            XdmValue val = exec.load30().callTemplate(new QName("main"));
            fail();
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("atomiz"));
            assertTrue(e.getMessage().contains("first argument of f:do()"));
            assertEquals("FOTY0013", e.getErrorCode().getLocalName());
            System.err.println(e.getMessage());
        }
    }


    @Test
    public void test4301() {
        try {
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <xsl:param name='code' as='xs:string'/> "
                    + "           <xsl:sequence select=\"if ($code = ('test')) then $code else error(QName('http://www.ctk.cz/error-code/', 'NotExists'))\"/>"
                    + "        </xsl:template>"
                    + " </xsl:transform>";
            Processor processor = new Processor(true);
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            Map<QName, XdmValue> p = new HashMap<>();
            p.put(new QName("code"), new XdmAtomicValue("abc"));
            transformer.setInitialTemplateParameters(p, false);
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            if (err.getErrorCode().getNamespaceUri().toString().equals("http://www.ctk.cz/error-code/")) {
                //return;
            } else {
                err.printStackTrace();
                fail();
            }
        }
    }

    @Test
    public void test3929inlineSchema() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, true);
            XsdCompiler sm = processor.newXsdCompiler();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            XsdSchema schema = sm.compile(new StreamSource(new StringReader(xsd0)));
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
            comp.useSchema(schema);
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }


    @Test
    public void test3929externalSchema() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, false);
            XsdCompiler sm = processor.newXsdCompiler();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            String xsd1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='b'/>\n" +
                    "</xs:schema>";
            XsdSchema schema = sm.compile(new StreamSource(new StringReader(xsd0)));
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:import-schema namespace='ns:a' schema-location='ns:a'/>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <b xmlns='ns:a'>12</b>"
                    + "        </xsl:template>"
                    + "        </xsl:transform>";
            XsltCompiler comp = processor.newXsltCompiler();
            comp.useSchema(schema);
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



