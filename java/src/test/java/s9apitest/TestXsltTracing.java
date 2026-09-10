////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.function.Function;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer
 *
 */
public class TestXsltTracing {
    private final ConfigTest configTest = new ConfigTest();
    
    /**
     * Simple test, tracing with level="normal"
     */

    @Test
    public void testNormalLevel() {
        try {
            String xml = "<a/>";
            String xsl = "<xsl:transform xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"3.0\">\n"
                    + "   <xsl:template match=\"/\">\n"
                    + "      <out x=\"2\">\n"
                    + "         <xsl:value-of select=\"name(*)\"/>\n"
                    + "      </out>\n"
                    + "   </xsl:template>\n"
                    + "</xsl:transform>";
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            final XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setCompileWithTracing(true);
            Xslt30Transformer transformer = compiler.compile(
                    new StreamSource(new StringReader(xsl), "dummy.xsl")).load30();
            XSLTTraceListener tracer = new XSLTTraceListener();
            AssertingLogger logger = new AssertingLogger()
                    //.withDisabled()   // toggle this comment to run without checking
                    .expect(out -> out.contains("<trace saxon-version"))
                    .expect(out -> out.contains("<source node=\"/\" line=\"0\""))
                    .expect(out -> out.contains("<LRE name=\"out\" line=\"3\""))
                    .expect(out -> out.contains("<xsl:value-of line=\"4\""))
                    .expect(out -> out.contains("</xsl:value-of>"))
                    .expect(out -> out.contains("</LRE>"))
                    .expect(out -> out.contains("</source>"))
                    .expect(out -> out.contains("</trace>"));
            tracer.setOutputDestination(logger);
            transformer.setTraceListener(tracer);
            XdmDestination serializer = new XdmDestination();
            XdmNode input = processor.newDocumentBuilder().build(new StreamSource(new StringReader(xml), "dummy.xml"));
            transformer.applyTemplates(input, serializer);


        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test tracing with level="low" (meaning low level of detail)
     */

    @Test
    public void testLowLevel() {
        try {
            String xml = "<a/>";
            String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<xsl:transform xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"3.0\" xmlns:local=\"local\">\n"
                    + "   <xsl:template match=\"/\">\n"
                    + "      <out x=\"2\">\n"
                    + "         <xsl:call-template name=\"one\"/>\n"
                    + "      </out>\n"
                    + "   </xsl:template>\n"
                    + "   <xsl:template name=\"one\">\n"
                    + "      <xsl:value-of select=\"local:f(*)\"/>\n"
                    + "   </xsl:template>\n"
                    + "   <xsl:function name=\"local:f\">\n"
                    + "      <xsl:param name=\"e\"/>\n"
                    + "      <xsl:sequence select=\"name($e)\"/>\n"
                    + "   </xsl:function>\n"
                    + "</xsl:transform>";
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            final XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setCompileWithTracing(true);
            //((XSLTTraceCodeInjector)compiler.getUnderlyingCompilerInfo().getCodeInjector()).setTraceLevel(TraceLevel.LOW);
            Xslt30Transformer transformer = compiler.compile(
                    new StreamSource(new StringReader(xsl), "dummy.xsl")).load30();
            XSLTTraceListener tracer = new XSLTTraceListener();
            tracer.setLevelOfDetail(TraceLevel.LOW);
            AssertingLogger logger = new AssertingLogger()
                    //.withDisabled()  // toggle comment to run without checking
                    .expect(out -> out.contains("<trace saxon-version"))
                    .expect(out -> out.contains("<source node=\"/\" line=\"0\""))
                    .expect(out -> out.contains("<xsl:template match=\"/\" line=\"3\""))
                    .expect(out -> out.contains("<xsl:call-template line=\"5\""))
                    .expect(out -> out.contains("<xsl:template name=\"one\" line=\"8\""))
                    .expect(out -> out.contains("<xsl:function arity=\"1\" name=\"local:f\" line=\"11\""))
                    .expect(out -> out.contains("</xsl:function>"))
                    .expect(out -> out.contains("</xsl:template>"))
                    .expect(out -> out.contains("</xsl:call-template>"))
                    .expect(out -> out.contains("</xsl:template>"))
                    .expect(out -> out.contains("</source>"))
                    .expect(out -> out.contains("</trace>"));

            tracer.setOutputDestination(logger);
            transformer.setTraceListener(tracer);
            XdmDestination serializer = new XdmDestination();
            XdmNode input = processor.newDocumentBuilder().build(new StreamSource(new StringReader(xml), "dummy.xml"));
            transformer.applyTemplates(input, serializer);


        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test tracing with level="high" (meaning high level of detail)
     */

    @Test
    public void testHighLevel() {
        try {
            String xml = "<a/>";
            String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0' "
                    + "       xmlns:local='local' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n"
                    + "   <xsl:template match=\"/\">\n"
                    + "      <out x=\"#{count(//*)}\" y=\"{current-date() + xs:dayTimeDuration('P1D')}\"/>\n"
                    + "   </xsl:template>\n"
                    + "</xsl:transform>";
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            final XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setCompileWithTracing(true);
            ((XSLTTraceCodeInjector)compiler.getUnderlyingCompilerInfo().getCodeInjector()).setTraceLevel(TraceLevel.HIGH);
            Xslt30Transformer transformer = compiler.compile(
                    new StreamSource(new StringReader(xsl), "dummy.xsl")).load30();
            XSLTTraceListener tracer = new XSLTTraceListener();
            tracer.setLevelOfDetail(TraceLevel.HIGH);
            AssertingLogger logger = new AssertingLogger()
                    .withDisabled()  // toggle comment to run without checking
                    .expect(out -> out.contains("<trace saxon-version"))
                    .expect(out -> out.contains("<source node=\"/\" line=\"0\""))
                    .expect(out -> out.contains("<xsl:template match=\"/\" line=\"3\""))
                    .expect(out -> out.contains("<LRE"))
                    .expect(out -> out.contains("<sequence"))
                    .expect(out -> out.contains("<ATTR"))
                    .expect(out -> out.contains("<fn:concat"))
                    .expect(out -> out.contains("<literal>"))
                    .expect(out -> out.contains("</literal>"))
                    .expect(out -> out.contains("<trace>"))
                    .expect(out -> out.contains("<convert>"))
                    .expect(out -> out.contains("<fn:count>"))
                    .expect(out -> out.contains("<axisStep>"))
                    .expect(out -> out.contains("</axisStep>"))
                    .expect(out -> out.contains("</fn:count>"))
                    .expect(out -> out.contains("</convert>"))
                    .expect(out -> out.contains("</trace>"))
                    .expect(out -> out.contains("</fn:concat>"))
                    .expect(out -> out.contains("</ATTR>"))
                    .expect(out -> out.contains("<ATTR name=\"y\">"));
                    // etc

            tracer.setOutputDestination(logger);
            transformer.setTraceListener(tracer);
            XdmDestination serializer = new XdmDestination();
            XdmNode input = processor.newDocumentBuilder().build(new StreamSource(new StringReader(xml), "dummy.xml"));
            transformer.applyTemplates(input, serializer);


        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    private class AssertingLogger extends Logger {

        private final List<Function<String, Boolean>> assertions = new ArrayList<>();
        private int count = 0;
        private boolean disabled;

        public AssertingLogger() {
        }

        public AssertingLogger expect(Function<String, Boolean> test) {
            assertions.add(test);
            return this;
        }

        private AssertingLogger withDisabled() {
            disabled = true;
            return this;
        }

        /**
         * Log a message with level {@link Logger#INFO}
         *
         * @param message the message to be logged
         */
        @Override
        public void info(String message) {
            System.err.println(message);
            if (!disabled) {
                if (count >= assertions.size()) {
                    return;
                }
                boolean ok = assertions.get(count).apply(message);
                if (!ok) {
                    fail("Unexpected trace message " + count + ": " + message);
                }
            }
            count++;
        }

        /**
         * Log a message. To be implemented in a concrete subclass
         *
         * @param message  The message to be output
         * @param severity The severity level. One of {@link Logger#INFO}, {@link Logger#WARNING}, {@link Logger#ERROR},
         *                 {@link Logger#DISASTER}
         */
        @Override
        public void println(String message, int severity) {
            System.err.println(message);
        }
    }


    /**
     * Test that open and close are called once each per transformation. Bug 6688.
     */

    @Test
    public void testOpenClose() {
        try {
            String xml = "<a/>";
            String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0' "
                    + "       xmlns:local='local' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n"
                    + "   <xsl:template match=\"/\">\n"
                    + "      <out x=\"#{count(//*)}\" y=\"{current-date() + xs:dayTimeDuration('P1D')}\"/>\n"
                    + "   </xsl:template>\n"
                    + "   <xsl:template name='t' visibility='public'><out/></xsl:template>\n"
                    + "   <xsl:function visibility='public' name='local:f'><out/></xsl:function>\n"
                    + "</xsl:transform>";
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);
            XdmNode doc = processor.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));
            // Create a transformer for the stylesheet.
            final XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setCompileWithTracing(true);
            XsltExecutable exec = compiler.compile(
                    new StreamSource(new StringReader(xsl), "dummy.xsl"));
            OpenCloseTraceListener tracer = new OpenCloseTraceListener();

            Xslt30Transformer trans = exec.load30();
            trans.setTraceListener(tracer);
            trans.applyTemplates(doc);
            System.err.println("opens " + tracer.opens + " closes " + tracer.closes);
            trans.applyTemplates(doc);
            System.err.println("opens " + tracer.opens + " closes " + tracer.closes);
            assertEquals(2, tracer.opens);
            assertEquals(2, tracer.closes);
            assertEquals(tracer.enters, tracer.leaves);
            assertEquals(tracer.starts, tracer.ends);

            trans.callTemplate(new QName("", "t"));
            System.err.println("opens " + tracer.opens + " closes " + tracer.closes);
            trans.callTemplate(new QName("", "t"));
            System.err.println("opens " + tracer.opens + " closes " + tracer.closes);
            assertEquals(4, tracer.opens);
            assertEquals(4, tracer.closes);
            assertEquals(tracer.enters, tracer.leaves);
            assertEquals(tracer.starts, tracer.ends);

            trans.callFunction(new QName("local", "f"), new XdmValue[]{});
            System.err.println("opens " + tracer.opens + " closes " + tracer.closes);
            trans.callFunction(new QName("local", "f"), new XdmValue[]{});
            System.err.println("opens " + tracer.opens + " closes " + tracer.closes);
            assertEquals(6, tracer.opens);
            assertEquals(6, tracer.closes);
            assertEquals(tracer.enters, tracer.leaves);
            assertEquals(tracer.starts, tracer.ends);


        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    private static class OpenCloseTraceListener extends AbstractTraceListener {

        public int opens, closes, enters, leaves, starts, ends;

        @Override
        public void open(Controller controller) {
            opens++;
        }

        @Override
        public void startCurrentItem(Item item) {
            starts++;
        }

        @Override
        public void endCurrentItem(Item item) {
            ends++;
        }

        @Override
        public void enter(Traceable info, Map<String, Object> properties, XPathContext context) {
            enters++;
        }

        @Override
        public void leave(Traceable info) {
            leaves++;
        }

        @Override
        public void close() {
            closes++;
        }
    }


}



