package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

/**
 * Test the use of schemas with the saxon:param extension from a s9api Validator
 */
public class TestSaxonParam extends TestCase {

    public void testSimpleTypeAssertionOK() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(FeatureKeys.XSD_VERSION, "1.1");
            SchemaManager manager = proc.getSchemaManager();
            manager.load(new StreamSource(new File(ConfigTest.DATA_DIR + "schema-with-param.xsd")));
            SchemaValidator val = manager.newSchemaValidator();
            val.setParameter(new QName("baseLine"), new XdmAtomicValue("2012-12-12", ItemType.DATE));
            val.validate(new StreamSource(new StringReader("<date>2012-10-12</date>")));  //OK
            try {
                val.validate(new StreamSource(new StringReader("<date>2013-12-12</date>")));  //not OK
                fail();
            } catch (SaxonApiException e1) {
                // OK
            }
            val.setParameter(new QName("baseLine"), new XdmAtomicValue("1999-12-12", ItemType.DATE));
            try {
                val.validate(new StreamSource(new StringReader("<date>2012-11-12</date>")));  //not OK
                fail();
            } catch (SaxonApiException e1) {
                // OK
            }
            // Now use the default
            val.setParameter(new QName("baseLine"), null);
            try {
                val.validate(new StreamSource(new StringReader("<date>3012-11-12</date>")));  //not OK
                fail();
            } catch (SaxonApiException e1) {
                // OK
            }
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    public void testSimpleTypeAssertionBadParam() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(FeatureKeys.XSD_VERSION, "1.1");
            SchemaManager manager = proc.getSchemaManager();
            manager.load(new StreamSource(new File(ConfigTest.DATA_DIR + "schema-with-param.xsd")));
            SchemaValidator val = manager.newSchemaValidator();
            try {
                // Supply a parameter of the wrong type
                val.setParameter(new QName("baseLine"), new XdmAtomicValue("2012-12-12", ItemType.STRING));
                val.validate(new StreamSource(new StringReader("<date>2012-10-12</date>")));  //OK
                fail();
            } catch (SaxonApiException e1) {
                System.err.println(e1);
                // OK
            }
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    public void testValidateDocFn() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(FeatureKeys.XSD_VERSION, "1.1");
            SchemaManager manager = proc.getSchemaManager();
            manager.load(new StreamSource(new File(ConfigTest.DATA_DIR + "schema-with-param.xsd")));
            DocumentBuilder b = proc.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader("<date>2001-10-12</date>")));
            XQueryCompiler c = proc.newXQueryCompiler();
            XQueryEvaluator ev = c.compile("saxon:validate(., map{}, map{'baseLine':xs:date('2100-01-01')})('valid')").load();
            ev.setContextItem(doc);
            assertTrue(((XdmAtomicValue)ev.evaluate()).getBooleanValue());
            ev = c.compile("saxon:validate(., map{}, map{'baseLine':xs:date('1900-01-01')})('valid')").load();
            ev.setContextItem(doc);
            assertFalse(((XdmAtomicValue)ev.evaluate()).getBooleanValue());
            ev = c.compile("saxon:validate(., map{}, map{'baseLine':xs:date('1900-01-01')})('errors')!.('message')").load();
            ev.setContextItem(doc);
            assertEquals(ev.evaluate().size(), 1);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

}

