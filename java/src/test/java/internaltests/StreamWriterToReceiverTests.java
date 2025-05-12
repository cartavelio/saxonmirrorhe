package internaltests;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.stream.StreamResult;
import java.io.StringWriter;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for the StreamWriterToReciever class
 */
public class StreamWriterToReceiverTests {

    @Test
    public void testStreamWriterToReceiver001() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeEmptyElement("doc");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc/>"));
        } catch (XMLStreamException | XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver002() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeStartElement("doc");
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc/>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver003() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeStartElement("doc");
            sw.writeAttribute("size", "42");
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc size=\"42\"/>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver004() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeEmptyElement("http://saxon.sf.net/", "doc");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc xmlns=\"http://saxon.sf.net/\"/>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver005() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeEmptyElement("p", "doc", "http://saxon.sf.net/");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<p:doc xmlns:p=\"http://saxon.sf.net/\"/>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver006() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeEmptyElement("p", "doc", "http://saxon.sf.net/");
            sw.writeAttribute("size", "42");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<p:doc"));
            assertTrue(out.toString().contains("xmlns:p=\"http://saxon.sf.net/\""));
            assertTrue(out.toString().contains(" size=\"42\""));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver007() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeEmptyElement("p", "doc", "http://saxon.sf.net/");
            sw.writeAttribute("xml", NamespaceConstant.XML, "id", "z123");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<p:doc"));
            assertTrue(out.toString().contains("xmlns:p=\"http://saxon.sf.net/\""));
            assertTrue(out.toString().contains(" xml:id=\"z123\""));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver008() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeStartElement("p", "doc", "http://saxon.sf.net/");
            sw.writeAttribute("xml", NamespaceConstant.XML, "id", "z123");
            sw.writeCharacters("hello");
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<p:doc"));
            assertTrue(out.toString().contains("xmlns:p=\"http://saxon.sf.net/\""));
            assertTrue(out.toString().contains(" xml:id=\"z123\""));
            assertTrue(out.toString().contains("hello</p:doc>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver009() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeStartElement("p", "doc", "http://saxon.sf.net/");
            sw.writeStartElement("p", "el", "http://saxon.sf.net/");
            sw.writeEndElement();
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<p:doc xmlns:p=\"http://saxon.sf.net/\"><p:el/></p:doc>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver010() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.setPrefix("p", "http://saxon.sf.net/");
            sw.writeStartElement("http://saxon.sf.net/", "doc");
            sw.writeStartElement("http://saxon.sf.net/", "el");
            sw.writeEndElement();
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<p:doc xmlns:p=\"http://saxon.sf.net/\"><p:el/></p:doc>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver011() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.setPrefix("p", "http://saxon.sf.net/");
            sw.writeStartElement("http://saxon.sf.net/", "doc");
            sw.setPrefix("q", "http://saxon.sf.net/");
            sw.writeStartElement("http://saxon.sf.net/", "el");
            sw.setPrefix("r", "http://saxon.sf.net/");
            sw.writeEndElement();
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<q:doc xmlns:q=\"http://saxon.sf.net/\"><r:el xmlns:r=\"http://saxon.sf.net/\"/></q:doc>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver012() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.setDefaultNamespace("http://saxon.sf.net/");
            sw.writeEmptyElement("doc");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc/>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver013() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.setDefaultNamespace("http://bar.com/");
            sw.writeStartElement("foo", "doc", "http://foo.com");
            sw.writeStartElement("http://bar.com", "el");
            sw.writeEndElement();
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<foo:doc xmlns:foo=\"http://foo.com\"><el xmlns=\"http://bar.com\"/></foo:doc>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreamWriterToReceiver014() {
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            XMLStreamWriter sw = config.getSerializerFactory().getXMLStreamWriter(new StreamResult(out), new Properties());
            sw.writeStartDocument();
            sw.writeStartElement("doc");
            sw.writeNamespace("saxon", NamespaceConstant.SAXON);
            sw.writeStartElement("el");
            sw.writeNamespace("saxon", NamespaceConstant.SAXON);
            sw.writeEndElement();
            sw.writeEndElement();
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc xmlns:saxon=\"http://saxon.sf.net/\"><el/></doc>"));
        } catch (XMLStreamException e) {
            e.printStackTrace();
            fail();
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }
}




