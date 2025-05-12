package pulltests;

import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.s9api.*;
import net.sf.saxon.stax.XMLStreamWriterDestination;
import org.junit.jupiter.api.Test;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.*;
import javax.xml.transform.stax.StAXResult;
import javax.xml.transform.stax.StAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.StringReader;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.nio.charset.StandardCharsets;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on StaxResults
 */
public class TestStaxResult {

     /**
     * Test use of StaxResult with s9api
     */

    @Test
    public void testStaxResults1()  {
        Processor proc = new Processor(true);
            XsltCompiler compiler = proc.newXsltCompiler();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
                    "    <xsl:template match=\"/\" >  \n" +
                    "        <p>test ok</p>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet>";

         XMLStreamReader reader = null;
        try{
            byte[] byteArray = stylesheet.getBytes(StandardCharsets.UTF_8);
            ByteArrayInputStream inputStream = new ByteArrayInputStream(byteArray);
            reader = XMLInputFactory.newInstance().createXMLStreamReader(inputStream);
        } catch(XMLStreamException e) {
            fail(e.getMessage());
        }
        StAXSource source = new StAXSource(reader);
        Serializer serializer = proc.newSerializer();
        serializer.setOutputStream(System.out);
        StAXResult result = null;
        try {
            result = new StAXResult(serializer.getXMLStreamWriter());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

        XMLStreamWriter writer2 = result.getXMLStreamWriter();
        XMLStreamWriterDestination dest = new XMLStreamWriterDestination(writer2);

        try {
            XsltTransformer trans = compiler.compile(source).load();
            trans.setSource(source);
            trans.setDestination(dest);
            trans.transform();
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test use of StaxResult with JAXP
     */

    @Test
    public void testStaxResults2() {
        try {
            TransformerFactory factory = new TransformerFactoryImpl();
            String stylesheet = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
                    "    <xsl:template match=\"/\" >  \n" +
                    "        <p>test ok</p>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet>";
            Templates templates = factory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            XMLStreamReader reader = null;
            try {
                byte[] byteArray = stylesheet.getBytes(StandardCharsets.UTF_8);
                ByteArrayInputStream inputStream = new ByteArrayInputStream(byteArray);
                reader = XMLInputFactory.newInstance().createXMLStreamReader(inputStream);
            } catch (XMLStreamException e) {
                fail(e.getMessage());
            }
            StAXSource source = new StAXSource(reader);
            Serializer serializer = new Processor(false).newSerializer();
            StringWriter sw = new StringWriter();
            serializer.setOutputWriter(sw);
            StAXResult result = null;
            try {
                result = new StAXResult(serializer.getXMLStreamWriter());
            } catch (SaxonApiException e) {
                fail(e.getMessage());
            }

            XMLStreamWriter writer2 = result.getXMLStreamWriter();

            Result dest = new StAXResult(writer2);
            Transformer trans = templates.newTransformer();
            trans.transform(source, dest);
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><p>test ok</p>", sw.toString().trim());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }
}
