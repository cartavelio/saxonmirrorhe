package s9apitest;

import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

class TestSefFunctionReferences {
    @Test
    void reloadStandardFunctionReferencesInHe() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable executable = processor.newXsltCompiler().loadExecutablePackage(exportFixture());

        assertEquals("4", call(processor, executable, "absolute", new XdmAtomicValue(-4)));
        assertEquals("3", call(processor, executable, "square-root", new XdmAtomicValue(9)));
        assertEquals("0", call(processor, executable, "map-size", new XdmMap()));
        assertEquals("0", call(processor, executable, "array-size", new XdmArray()));
    }

    @Test
    void rejectDisabledFunctionOnReload() throws Exception {
        Processor processor = new Processor(false);
        processor.getUnderlyingConfiguration().disableFunction(NamespaceUri.FN.qName("abs"));

        SaxonApiException error = assertThrows(SaxonApiException.class,
                () -> processor.newXsltCompiler().loadExecutablePackage(exportFixture()));
        assertEquals("XPST0017", error.getErrorCode().getLocalName());
    }

    @Test
    void rejectUnsupportedExportTargetBeforeWriting() throws Exception {
        Processor processor = new Processor(false);
        var config = processor.getUnderlyingConfiguration();
        var pkg = config.makeStylesheetPackage();
        var buffer = new ByteArrayOutputStream();

        assertThrows(XPathException.class, () -> config.newExpressionExporter("JS", buffer, pkg));
        assertEquals(0, buffer.size());
    }

    private static String call(Processor processor, XsltExecutable executable,
                               String template, XdmItem argument) throws SaxonApiException {
        XdmFunctionItem function = (XdmFunctionItem) executable.load30().callTemplate(new QName(template));
        return function.call(processor, argument).itemAt(0).getStringValue();
    }

    private static StreamSource exportFixture() throws Exception {
        Processor processor = new Processor(false);
        var stylesheet = new StreamSource(new StringReader("""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
                    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
                    xmlns:array="http://www.w3.org/2005/xpath-functions/array">
                    <xsl:template name="absolute"><xsl:sequence select="abs#1"/></xsl:template>
                    <xsl:template name="square-root"><xsl:sequence select="math:sqrt#1"/></xsl:template>
                    <xsl:template name="map-size"><xsl:sequence select="map:size#1"/></xsl:template>
                    <xsl:template name="array-size"><xsl:sequence select="array:size#1"/></xsl:template>
                </xsl:stylesheet>
                """), "urn:test:sef-function-references");
        var compiled = processor.newXsltCompiler().compile(stylesheet);
        var buffer = new ByteArrayOutputStream();
        compiled.export(buffer);
        return new StreamSource(new ByteArrayInputStream(buffer.toByteArray()));
    }
}
