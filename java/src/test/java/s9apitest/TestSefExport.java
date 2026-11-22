package s9apitest;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XsltExecutable;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

class TestSefExport {
    @Test
    void exportLabelsAndReloadInFreshProcessor() throws Exception {
        Processor sourceProcessor = new Processor(false);
        var fixture = new File("../src/test/testdata/compiled-stylesheet/labels.xsl");
        XsltExecutable source = sourceProcessor.newXsltCompiler().compile(new StreamSource(fixture));
        var buffer = new TrackingOutputStream();
        source.export(buffer);

        assertTrue(buffer.closed);
        assertTrue(buffer.toString(StandardCharsets.UTF_8).contains("<?Σ2 "));
        Processor targetProcessor = new Processor(false);
        XsltExecutable reloaded = reload(targetProcessor, buffer);
        assertEquals("[ADA]", transform(sourceProcessor, source));
        assertEquals("[ADA]", transform(targetProcessor, reloaded));
    }

    @Test
    void exportBindingsAndKeepParametersSeparateAfterReload() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                                xmlns:f="urn:functions" default-mode="f:labels">
                  <xsl:output method="text"/>
                  <xsl:param name="label" required="yes"/>
                  <xsl:variable name="upper" select="f:upper($label)"/>
                  <xsl:function name="f:upper">
                    <xsl:param name="value"/>
                    <xsl:sequence select="upper-case($value)"/>
                  </xsl:function>
                  <xsl:template name="emit"><xsl:value-of select="$upper"/></xsl:template>
                  <xsl:template match="/" mode="f:labels"><xsl:call-template name="emit"/></xsl:template>
                </xsl:stylesheet>
                """);
        var buffer = new ByteArrayOutputStream();
        source.export(buffer, "HE");
        Processor targetProcessor = new Processor(false);
        XsltExecutable reloaded = reload(targetProcessor, buffer);
        var first = reloaded.load30();
        var second = reloaded.load30();
        first.setStylesheetParameters(Map.of(new QName("label"), new XdmAtomicValue("Ada")));
        second.setStylesheetParameters(Map.of(new QName("label"), new XdmAtomicValue("Grace")));

        StringWriter firstResult = new StringWriter();
        StringWriter secondResult = new StringWriter();
        second.applyTemplates(input(), targetProcessor.newSerializer(secondResult));
        first.applyTemplates(input(), targetProcessor.newSerializer(firstResult));
        assertEquals("ADA", firstResult.toString());
        assertEquals("GRACE", secondResult.toString());
    }

    @Test
    void rejectUnsupportedTargetBeforeWriting() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"/>
                """);
        var buffer = new ByteArrayOutputStream();
        SaxonApiException error = assertThrows(SaxonApiException.class, () -> source.export(buffer, "JS"));

        assertEquals("Unsupported stylesheet export target in Saxon-HE: JS", error.getMessage());
        assertEquals(0, buffer.size());
    }

    @Test
    void rejectWhitespaceDeclarationsUntilTheyCanBePreserved() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:strip-space elements="*"/>
                </xsl:stylesheet>
                """);
        var buffer = new ByteArrayOutputStream();

        assertThrows(SaxonApiException.class, () -> source.export(buffer));
        assertEquals(0, buffer.size());
    }

    private static XsltExecutable compile(Processor processor, String stylesheet) throws SaxonApiException {
        return processor.newXsltCompiler().compile(
                new StreamSource(new StringReader(stylesheet), "urn:test:sef-export"));
    }

    private static XsltExecutable reload(Processor processor, ByteArrayOutputStream buffer) throws SaxonApiException {
        return processor.newXsltCompiler().loadExecutablePackage(
                new StreamSource(new ByteArrayInputStream(buffer.toByteArray())));
    }

    private static StreamSource input() {
        return new StreamSource(new StringReader("<record name='Ada'/>"));
    }

    private static String transform(Processor processor, XsltExecutable executable) throws SaxonApiException {
        StringWriter result = new StringWriter();
        executable.load30().applyTemplates(input(), processor.newSerializer(result));
        return result.toString();
    }

    private static final class TrackingOutputStream extends ByteArrayOutputStream {
        private boolean closed;

        @Override
        public void close() {
            closed = true;
        }
    }
}
