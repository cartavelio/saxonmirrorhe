package jaxptest;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.config.pe.ConfigurationReaderPE;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.ConfigurationReader;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TemplatesHandler;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;


/**
 *  Test use of configuration files
 */
public class ConfigTest extends TestCase {

    public static final String DATA_DIR = System.getProperty("junitTestData");// "/Users/Mike/repo2/tests/junit/testdata/";
    public static final String XMARK_DIR = System.getProperty("junitTestData") + "../../xmark/";  //"/Users/Mike/repo2/tests/junit/testdata/";

    public void testGeneralConfiguration() {
        try {
            Configuration config = new ConfigurationReader().makeConfiguration(
                    new StreamSource(DATA_DIR + "config.xml"));
            assertTrue("lineNumbering?", config.isLineNumbering());
            assertEquals("tvc?", config.getConfigurationProperty(FeatureKeys.USE_TYPED_VALUE_CACHE), Boolean.TRUE);
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    public void testWhitespaceDefault() {
        Configuration config = new Configuration();
        assertEquals("ignorable", config.getConfigurationProperty(Feature.STRIP_WHITESPACE));
    }


    public void testJaxpConfiguration() {
        try {
            TransformerFactoryImpl factory = new TransformerFactoryImpl();
            factory.setAttribute(FeatureKeys.CONFIGURATION_FILE, DATA_DIR + "config.xml");
            Configuration config = factory.getConfiguration();
            assertTrue("lineNumbering?", config.isLineNumbering());
            assertEquals("tvc?", config.getConfigurationProperty(FeatureKeys.USE_TYPED_VALUE_CACHE), Boolean.TRUE);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    public void testCollationConfiguration() {
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration'>\n" +
                        "<global defaultCollation='http://uclc.com/'/>\n" +
                        "<collations>\n" +
                            "<collation uri='http://uclc.com/' case-order='upper-first'/>\n" +
                            "<collation uri='http://lcuc.com/' case-order='lower-first'/>\n" +
                        "</collations>\n" +
                    "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            XPathCompiler compiler = proc.newXPathCompiler();
            XPathExecutable e1 = compiler.compile("compare('Alpha','alpha','http://uclc.com/')");
            XdmAtomicValue result1 = (XdmAtomicValue)e1.load().evaluateSingle();
            assertEquals("e1 result", result1.getLongValue(), -1);
            XPathExecutable e2 = compiler.compile("compare('Alpha','alpha','http://lcuc.com/')");
            XdmAtomicValue result2 = (XdmAtomicValue)e2.load().evaluateSingle();
            assertEquals("e2 result", result2.getLongValue(), +1);

            XQueryCompiler qcompiler = proc.newXQueryCompiler();
            XQueryExecutable e3 = qcompiler.compile("compare('Alpha','alpha','http://uclc.com/')");
            XdmAtomicValue result3 = (XdmAtomicValue)e3.load().evaluateSingle();
            assertEquals("e3 result", result3.getLongValue(), -1);
            XQueryExecutable e4 = qcompiler.compile("compare('Alpha','alpha','http://lcuc.com/')");
            XdmAtomicValue result4 = (XdmAtomicValue)e4.load().evaluateSingle();
            assertEquals("e4 result", result4.getLongValue(), +1);

            String stylesheet = "<a xmlns:x='http://www.w3.org/1999/XSL/Transform' x:version='2.0' \n" +
                    "default=\"{compare('Alpha','alpha')}\" " +
                    "uclc=\"{compare('Alpha','alpha','http://uclc.com/')}\" " +
                    "lcuc=\"{compare('Alpha','alpha','http://lcuc.com/')}\"/>";
            XsltCompiler xc = proc.newXsltCompiler();
            XsltTransformer t = xc.compile(new StreamSource(new StringReader(stylesheet))).load();
            StringWriter out = new StringWriter();
            t.setSource(new StreamSource(new StringReader(stylesheet)));
            Serializer ser = proc.newSerializer(out);
            t.setDestination(ser);
            t.transform();
            assertTrue(out.toString().contains("default=\"-1\""));
            assertTrue(out.toString().contains("uclc=\"-1\""));
            assertTrue(out.toString().contains("lcuc=\"1\""));
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }

    public void testSerializationConfiguration() {
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='PE'>\n" +
                        "<serialization\n" +
                            "        method=\"xml\"\n" +
                            "        indent=\"yes\"\n" +
                            "        omit-xml-declaration='yes'\n" +
                            "        cdata-section-elements='ccc:c' xmlns:ccc='http://c.com'\n" +
                            "        saxon:indent-spaces=\"8\"\n" +
                            "        xmlns:saxon=\"http://saxon.sf.net/\"/>" +
                    "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            XQueryCompiler qcompiler = proc.newXQueryCompiler();
            XQueryExecutable e3 = qcompiler.compile("<a><b/><c:c xmlns:c='http://c.com'>CCC</c:c></a>");
            StringWriter sw3 = new StringWriter();
            Serializer s3 = proc.newSerializer(sw3);
            XQueryEvaluator ev3 = e3.load();
            ev3.setDestination(s3);
            ev3.run();
            assertEquals("e3 result", "<a>\n        <b/>\n        <c:c xmlns:c=\"http://c.com\"><![CDATA[CCC]]></c:c>\n</a>\n", sw3.toString());

            String stylesheet = "<a xmlns:x='http://www.w3.org/1999/XSL/Transform' x:version='2.0' \n" +
                    "><b/><c:c xmlns:c='http://c.com'>CCC</c:c></a>";
            XsltCompiler xc = proc.newXsltCompiler();
            XsltTransformer t = xc.compile(new StreamSource(new StringReader(stylesheet))).load();
            StringWriter out = new StringWriter();
            t.setSource(new StreamSource(new StringReader(stylesheet)));
            Serializer ser = proc.newSerializer(out);
            t.setDestination(ser);
            t.transform();
            System.err.println(out);
            System.err.println(sw3);
            assertEquals("xslt result", out.toString(), sw3.toString());
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }





    public void testSchemaConfiguration() {
        try {
            String config =
                        "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='EE'>\n" +
                            "<global timing='true'/>\n" +
                            "<xsd useXsiSchemaLocation='false'/>\n" +
                            "<resources>\n" +
                                "<schemaDocument>file:///" + DATA_DIR + "books.xsd</schemaDocument>\n" +
                            "</resources>\n" +
                        "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            SchemaManager sm = proc.getSchemaManager();
            SchemaValidator v = sm.newSchemaValidator();
            v.validate(new StreamSource(new File(DATA_DIR + "books.xml")));
            try {
                v.validate(new StreamSource(new File(DATA_DIR + "config.xml")));
                fail("invalidity not detected");
            } catch (SaxonApiException verr) {
                System.err.println("Failed as expected: " + verr.getMessage());
                // OK, failure was expected
            }
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }

    public void testSchemaConfigurationInHE() {
        // Bug 2731
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='HE'>\n" +
                            "<global timing='true'/>\n" +
                            "<xsd useXsiSchemaLocation='false' occurrenceLimits='100,250'/>\n" +
                            "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            fail("invalidity not detected");
        } catch (SaxonApiException err) {
            assertTrue(err.getMessage().contains("Invalid configuration property xsd/@occurrenceLimits"));
            assertTrue(err.getMessage().contains("Property not available in Saxon-HE"));
            System.err.println(err.getMessage());
            // failure expected;
        }
    }

    public void testInvalidPropertyValue() {
        // Bug 2731
        try {
            String config =
                    "<configuration xmlns='http://saxon.sf.net/ns/configuration' edition='HE'>\n" +
                            "<global timing='maybe'/>\n" +
                            "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            fail("invalidity not detected");
        } catch (SaxonApiException err) {
            assertTrue(err.getMessage().contains("Invalid configuration property global/@timing"));
            //assertTrue(err.getMessage().contains("Property not available in Saxon-HE"));
            System.err.println(err.getMessage());
            // failure expected;
        }
    }

    public void testWrongNamespace() {
        // Bug 4064
        try {
            String config =
                    "<configuration xmlns='http://not.saxon.sf.net/ns/configuration' edition='HE'>\n" +
                            "<global timing='maybe'/>\n" +
                            "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            fail("invalidity not detected");
        } catch (SaxonApiException err) {
            assertTrue(err.getMessage().contains("Configuration"));
            assertTrue(err.getMessage().contains("namespace"));
            //assertTrue(err.getMessage().contains("Property not available in Saxon-HE"));
            System.err.println(err.getMessage());
            // failure expected;
        }
    }


    public void testSqlExtensionPE() {
        try {
            javax.xml.transform.TransformerFactory tf = new com.saxonica.config.ProfessionalTransformerFactory();
            tf.setAttribute(FeatureKeys.XSLT_VERSION, "2.0");
            tf.setAttribute(net.sf.saxon.lib.FeatureKeys.CONFIGURATION_FILE, DATA_DIR + "config-sql.xml");
            tf.newTemplates(new StreamSource(new File(DATA_DIR + "books-sql.xsl")));
        } catch (TransformerException err) {
            fail(err.getMessage());
        }
    }

    public void testSqlExtensionPEWorkaround() {
        try {
            javax.xml.transform.TransformerFactory tf = new com.saxonica.config.ProfessionalTransformerFactory();
            tf.setAttribute(FeatureKeys.XSLT_VERSION, "2.0");

            ConfigurationReaderPE reader = new ConfigurationReaderPE();
            Configuration config = reader.makeConfiguration(new StreamSource(new File(DATA_DIR + "config-sql.xml")));
            tf.setAttribute(FeatureKeys.CONFIGURATION, config);
            tf.newTemplates(new StreamSource(new File(DATA_DIR + "books-sql.xsl")));
        } catch (TransformerException err) {
            fail(err.getMessage());
        }
    }

    public void testProfConfProblem() {

        // Bug 2609 - occurred using a TemplatesHandler with a ProfessionalConfiguration

        try {
            ProfessionalConfiguration pConfig = new ProfessionalConfiguration();
            SAXTransformerFactory tf = new com.saxonica.config.ProfessionalTransformerFactory(pConfig);

            //This line is required as our product code uses it deal with multiple XSLT implementations.
            //Here in this program it may be redundant but cannot remove in product code.
            TemplatesHandler templatesHandler = tf.newTemplatesHandler();
            assertNotNull(templatesHandler);
        } catch (TransformerConfigurationException tce) {
            System.out.println("TFC...");
            tce.printStackTrace();
            fail();
        }
    }



}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//

