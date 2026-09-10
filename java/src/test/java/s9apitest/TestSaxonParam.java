package s9apitest;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Test the use of schemas with the saxon:param extension from a s9api Validator
 */
public class TestSaxonParam {
    @Test
    public void testSimpleTypeAssertionOK() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.XSD_VERSION, "1.1");
            XsdCompiler manager = proc.newXsdCompiler();
            XsdSchema schema = manager.compile(new StreamSource(Testdata.file("schema-with-param.xsd")));
            SchemaValidator val = schema.newValidator();
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
}

