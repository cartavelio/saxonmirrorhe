package jaxptest;

import net.sf.saxon.TransformerFactoryImpl;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for saxon:import-query
 */
public class TransformCallingQueryTest  {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testTransformCallingQuery()
	{
        // Bug reported 2012-03-28 by Michael Hedenus, Continental Corporation. Only fails if running PE.
        try {
            TransformerFactory transformerFactory = new TransformerFactoryImpl();
            Transformer transformer = transformerFactory.newTransformer(new StreamSource(configTest.getBugsDir() + "hedenus/bug.xslt"));
            transformer.setParameter("myparam", "london");


            StringWriter writer = new StringWriter();
            transformer.transform(new StreamSource(configTest.getBugsDir() +"hedenus/bug.xslt"), new StreamResult(writer));
            System.err.println(writer.toString());
            assertTrue(writer.toString().contains("<london><a/></london>"));

        } catch (TransformerException e) {
            fail();
            e.printStackTrace();
        }
    }


}

