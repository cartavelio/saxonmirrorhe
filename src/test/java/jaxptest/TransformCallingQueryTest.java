package jaxptest;

import junit.framework.TestCase;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.FeatureKeys;

import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringWriter;

/**
 * Tests for saxon:import-query
 */
public class TransformCallingQueryTest extends TestCase {

    public void testTransformCallingQuery()
	{
        // Bug reported 2012-03-28 by Michael Hedenus, Continental Corporation. Only fails if running PE.
        try {
            TransformerFactory transformerFactory = new TransformerFactoryImpl();
            transformerFactory.setAttribute(FeatureKeys.GENERATE_BYTE_CODE, false);
            Transformer transformer = transformerFactory.newTransformer(new StreamSource("file:///Users/mike/bugs/2012/hedenus/bug.xslt"));
            transformer.setParameter("myparam", "london");


            StringWriter writer = new StringWriter();
            transformer.transform(new StreamSource("file:///Users/mike/bugs/2012/hedenus/bug.xslt"), new StreamResult(writer));
            System.err.println(writer.toString());
            assertTrue(writer.toString().contains("<london><a/></london>"));

        } catch (TransformerException e) {
            fail();
            e.printStackTrace();
        }
    }


}

