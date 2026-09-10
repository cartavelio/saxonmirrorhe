package s9apitest;

import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SystemLogger;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.SourceLocator;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.logging.Logger;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Attempts to reproduce a bug from Stephen P Cuzner where the MessageListener gets an AIOOB exception
 * reading the string value of a node from a TinyTree used to construct a message; the tree has been
 * passed to the listener after endDocument() but before close(), which means there is no stopper node
 */
public class TestMessageListener {

    @Test
    public void testMessage() {
        try {
            // Write lots of empty messages. Each message is a empty document of a single node. All the documents
            // are concatenated in the TinyTree forest; sooner or later one of them will align with the end of the
            // TinyTree array, which in 9.4 triggers an AIOOB doing the getStringValue() on the content of the document
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='main'>" +
                    "<xsl:for-each select='1 to 5'>" +
                    "<xsl:message><xsl:for-each select='1 to 5'><a>[<b c='{.}'>42</b>]</a></xsl:for-each></xsl:message>" +
                    "</xsl:for-each>" +
                    "<xsl:for-each select='1 to 5000'>" +
                    "<xsl:message/>" +
                    "</xsl:for-each>" +
                    "</xsl:template>" +
                    "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageListener(new MessageListener2() {
                @Override
                public void message(XdmNode content, QName errorCode, boolean terminate, SourceLocator locator) {
                    sb.append(content.getStringValue());
                }
            });
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            assertEquals(100, sb.length());
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMessageHandler() {
        // Rewrite of above testMessage() to use current API
        try {
            // Write lots of empty messages. Each message is a empty document of a single node. All the documents
            // are concatenated in the TinyTree forest; sooner or later one of them will align with the end of the
            // TinyTree array, which in 9.4 triggers an AIOOB doing the getStringValue() on the content of the document
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message><xsl:for-each select='1 to 5'><a>[<b c='{.}'>42</b>]</a></xsl:for-each></xsl:message>" +
                            "</xsl:for-each>" +
                            "<xsl:for-each select='1 to 5000'>" +
                            "<xsl:message/>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageHandler(message -> sb.append(message.getContent().getStringValue()));
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            assertEquals(100, sb.length());
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMessageHandlerThrowingException() {
        // Rewrite of above testMessage() to use current API
        try {
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message terminate='{. eq 4}'>Yaboo!</xsl:message>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageHandler(message -> {
                sb.append(message.getContent().getStringValue());
                if (message.isTerminate()) {
                    throw new RuntimeException(message.getStringValue());
                }
            });
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            assertEquals(100, sb.length());
        } catch (RuntimeException rt) {
            assertEquals("Yaboo!", rt.getMessage());
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMessageListenerThrowingException() {
        // Rewrite of above testMessage() to use current API
        try {
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message terminate='{. eq 4}'>Yaboo!</xsl:message>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageListener(new MessageListener2() {
                @Override
                public void message(XdmNode content, QName errorCode, boolean terminate, SourceLocator locator) {
                    throw new RuntimeException("Yaboo!");
                }
            });
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            assertEquals(100, sb.length());
        } catch (RuntimeException rt) {
            assertEquals("Yaboo!", rt.getMessage());
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMessageConsumerRequestingTermination() {
        // Rewrite of above testMessage() to use current API
        try {
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message terminate='no'><xsl:value-of select='\"message \" || .'/></xsl:message>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageHandler(msg -> {
                if (msg.getContent().getStringValue().contains("4")) {
                    msg.raiseDynamicError();
                }
            });
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            fail("Unexpected success");
        } catch (SaxonApiException e1) {
            System.err.println(e1.getMessage());
            assertTrue(e1.getMessage().contains("Processing terminated by user-written message handler"));
        }
    }
    @Test
    public void testMessageLocation() {
        try {
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                            "<xsl:template name='main'>\n" +
                            "<xsl:variable name='e'>banana</xsl:variable>\n" +
                            "<xsl:message><xsl:value-of select=\"concat('The message is: ', $e)\"/></xsl:message>\n" +
                            "</xsl:template>\n" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl), "http://nominal.location/"));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageListener(new MessageListener2() {
                public void message(XdmNode content, QName errorCode, boolean terminate, SourceLocator locator) {
                    //System.err.println("Location = " + locator.getSystemId() + "#" + locator.getLineNumber());
                    assertEquals("http://nominal.location/", locator.getSystemId());
                    assertEquals(4, locator.getLineNumber());
                    sb.append(content.getStringValue());
                }
            });
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            assertEquals(22, sb.length());
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }


    @Test
    public void testMessageTerminate() {
        // 2020-02-10: report on saxon help forum that in 9.9.1.6, the message listener is not called
        // when terminate="yes". Bug 4457.
        try {
            String xsl =
                    "<xsl:transform version='3.0' expand-text='yes' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message terminate=\"{if (.=5) then 'yes' else 'no'}\">GOTCHA{.}!</xsl:message>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            final StringBuilder sb = new StringBuilder();
            t.setMessageListener(new MessageListener2() {
                @Override
                public void message(XdmNode content, QName errorCode, boolean terminate, SourceLocator locator) {
                    System.err.println(content.getStringValue() + terminate);
                    sb.append(content.getStringValue());
                    assertEquals(terminate, content.getStringValue().equals("GOTCHA5!"), "terminate");
                }
            });
            t.setDestination(p.newSerializer(System.out));
            t.transform();
            fail("Expected xsl:messge termination exception");
        } catch (SaxonApiException e1) {
            assertTrue(e1.getMessage().contains("Processing terminated by xsl:message"), "exception");
        }
    }

    @Test
    public void testMessageToLogger() {
        try {
            String xsl =
                    "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='main'>" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:message><a nr='{.}'/></xsl:message>" +
                            "</xsl:for-each>" +
                            "</xsl:template>" +
                            "</xsl:transform>";
            Processor p = new Processor(false);
            p.getUnderlyingConfiguration().setLogger(new SystemLogger(Logger.getAnonymousLogger()));
            p.setConfigurationProperty(Feature.TIMING, true);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable e = c.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer t = e.load();
            t.setInitialTemplate(new QName("main"));
            t.setDestination(p.newSerializer(System.out));
            t.transform();
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }


    @Test
    public void testBug6464() {
        try {
            String xsl =
                    "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n"
                            + "  <xsl:template match=\"/\">\n"
                            + "    <xsl:message terminate=\"true\">\n"
                            + "      Whoooo\n"
                            + "    </xsl:message>\n"
                            + "  </xsl:template>\n"
                            + "</xsl:stylesheet>";
            Processor proc = new Processor(true);
            XdmNode in = proc.newDocumentBuilder().build(new StreamSource(new StringReader("<doc/>")));
            XsltCompiler compiler = proc.newXsltCompiler();

            RealtaMessageListener listener = new RealtaMessageListener();
            XsltExecutable executable = compiler.compile(new StreamSource(new StringReader(xsl)));
            XsltTransformer transformer = executable.load();

            transformer.setMessageListener(listener);

            transformer.setSource(in.asSource());
            transformer.setDestination(proc.newSerializer(System.out));
            transformer.transform();
        } catch (SaxonApiException e1) {
            if (!e1.getErrorCode().getLocalName().equals("XTMM9000")) {
                e1.printStackTrace();
                fail();
            }
        }
    }

    public static class RealtaMessageListener implements MessageListener2 {
        public void message(XdmNode content, QName errorCode, boolean terminate,
                            SourceLocator locator) {
            throw new RuntimeException("Transform aborted by xsl:message #1");
        }

    }


}
