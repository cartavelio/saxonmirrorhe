package s9apitest;

import net.sf.saxon.Version;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

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
    void reloadHeExtensionFunctionReferences() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable executable = processor.newXsltCompiler().loadExecutablePackage(exportFixture());

        assertEquals("true", call(processor, executable, "whole-number", new XdmAtomicValue(4.0)));
        assertEquals("number", call(processor, executable, "object-type", new XdmAtomicValue(9)));
    }

    @Test
    void rejectUnavailableAndUnknownCompiledFunctionReferences() throws Exception {
        for (String name : new String[]{"Q{http://expath.org/ns/file}read-text", "Q{http://expath.org/ns/binary}length"}) {
            SaxonApiException error = assertThrows(SaxonApiException.class,
                    () -> new Processor(false).newXsltCompiler().loadExecutablePackage(referenceFixture(name)));
            assertEquals("SXPK0002", error.getErrorCode().getLocalName());
            assertEquals("This build cannot load a compiled reference to " + name + "#1", error.getMessage());
        }

        SaxonApiException unknown = assertThrows(SaxonApiException.class,
                () -> new Processor(false).newXsltCompiler().loadExecutablePackage(referenceFixture("Q{urn:unknown}f")));
        assertEquals("SXPK0002", unknown.getErrorCode().getLocalName());
        assertTrue(unknown.getMessage().contains("Unknown system function Q{urn:unknown}f#1"));
    }

    @Test
    void reloadNestedAndPartiallyAppliedFunctionsInsideMapsAndArrays() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:xs="http://www.w3.org/2001/XMLSchema">
                  <xsl:param name="value" as="xs:integer"/>
                  <xsl:template name="main">
                    <xsl:sequence select="
                      let $base := $value + 10
                      return map {
                        'nested': [function($prefix as xs:string) as function(xs:string) as xs:string {
                          function($arg as xs:string) as xs:string {
                            concat($prefix, $arg, ':', $base, ':', $value)
                          }
                        }],
                        'partial': concat('prefix:', ?, $base),
                        'higher-order': for-each((1, 2), function($i) {$i + $base})
                      }"/>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        Processor target = new Processor(false);
        XsltExecutable reloaded = target.newXsltCompiler().loadExecutablePackage(export(source));
        var sourceResult = makeFunctions(source, 2);
        var first = makeFunctions(reloaded, 2);
        var second = makeFunctions(reloaded, 7);
        var sourceNested = nestedFunction(sourceResult);
        var reloadedNested = nestedFunction(first);
        assertEquals(((FunctionItem) sourceNested.getUnderlyingValue()).getFunctionItemType(),
                ((FunctionItem) reloadedNested.getUnderlyingValue()).getFunctionItemType());
        assertFunctions(sourceProcessor, sourceResult, 2);
        assertFunctions(target, second, 7);
        assertFunctions(target, first, 2);
    }

    @Test
    void reloadFunctionsWithTransformationSpecificGlobalsAndLazyErrors() throws Exception {
        Processor sourceProcessor = new Processor(false);
        var fixture = TestSefFunctionReferences.class.getResource("returned-inline-function.xsl");
        XsltExecutable source = sourceProcessor.newXsltCompiler().compile(new StreamSource(fixture.toExternalForm()));
        Processor target = new Processor(false);
        XsltExecutable reloaded = target.newXsltCompiler().loadExecutablePackage(export(source));
        assertGlobalsAndErrors(sourceProcessor, source);
        assertGlobalsAndErrors(target, reloaded);
    }

    @Test
    void reloadFunctionSignatureForThePackageXPathVersion() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:map="http://www.w3.org/2005/xpath-functions/map">
                  <xsl:template name="main"><xsl:sequence select="map:for-each#2"/></xsl:template>
                </xsl:stylesheet>
                """);
        Processor target = new Processor(false);
        XsltExecutable reloaded = target.newXsltCompiler().loadExecutablePackage(export(source));
        QName main = new QName("main");
        var before = (XdmFunctionItem) source.load30().callTemplate(main);
        var after = (XdmFunctionItem) reloaded.load30().callTemplate(main);

        assertEquals(((FunctionItem) before.getUnderlyingValue()).getFunctionItemType(),
                ((FunctionItem) after.getUnderlyingValue()).getFunctionItemType());
    }

    @Test
    void reloadFunctionReferenceWithStaticBaseUri() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = sourceProcessor.newXsltCompiler().compile(new StreamSource(new StringReader("""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:template name="main"><xsl:sequence select="resolve-uri#1"/></xsl:template>
                </xsl:stylesheet>
                """), "file:/original/style/main.xsl"));
        Processor target = new Processor(false);
        XsltExecutable reloaded = target.newXsltCompiler().loadExecutablePackage(export(source));
        var relative = new XdmAtomicValue("../data.xml");

        assertEquals("file:/original/data.xml", call(sourceProcessor, source, "main", relative));
        assertEquals("file:/original/data.xml", call(target, reloaded, "main", relative));
    }

    @Test
    void loadXQueryModuleReportsMissingCompilerBeforeAndAfterReload() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:template name="main"><xsl:sequence select="load-xquery-module#1"/></xsl:template>
                </xsl:stylesheet>
                """);
        Processor target = new Processor(false);
        XsltExecutable reloaded = target.newXsltCompiler().loadExecutablePackage(export(source));
        XdmAtomicValue argument = new XdmAtomicValue("urn:test:module");
        assertEquals("FOQM0006", assertThrows(SaxonApiException.class,
                () -> call(sourceProcessor, source, "main", argument)).getErrorCode().getLocalName());
        assertEquals("FOQM0006", assertThrows(SaxonApiException.class,
                () -> call(target, reloaded, "main", argument)).getErrorCode().getLocalName());
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
        return export(compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
                    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
                    xmlns:array="http://www.w3.org/2005/xpath-functions/array"
                    xmlns:saxon="http://saxon.sf.net/" xmlns:exsl="http://exslt.org/common">
                    <xsl:template name="absolute"><xsl:sequence select="abs#1"/></xsl:template>
                    <xsl:template name="square-root"><xsl:sequence select="math:sqrt#1"/></xsl:template>
                    <xsl:template name="map-size"><xsl:sequence select="map:size#1"/></xsl:template>
                    <xsl:template name="array-size"><xsl:sequence select="array:size#1"/></xsl:template>
                    <xsl:template name="whole-number"><xsl:sequence select="saxon:is-whole-number#1"/></xsl:template>
                    <xsl:template name="object-type"><xsl:sequence select="exsl:object-type#1"/></xsl:template>
                </xsl:stylesheet>
                """));
    }

    private static XsltExecutable compile(Processor processor, String stylesheet) throws SaxonApiException {
        return processor.newXsltCompiler().compile(
                new StreamSource(new StringReader(stylesheet), "urn:test:sef-function-references"));
    }

    private static StreamSource export(XsltExecutable compiled) throws SaxonApiException {
        var buffer = new ByteArrayOutputStream();
        compiled.export(buffer);
        return new StreamSource(new ByteArrayInputStream(buffer.toByteArray()));
    }

    private static StreamSource referenceFixture(String functionName) throws XPathException {
        // These references cannot be compiled from a source stylesheet in HE.
        var config = new Processor(false).getUnderlyingConfiguration();
        var buffer = new ByteArrayOutputStream();
        var out = config.newExpressionExporter("HE", buffer, config.makeStylesheetPackage());
        out.startElement("package");
        out.emitAttribute("saxonVersion", Version.getProductVersion());
        out.emitAttribute("version", "30");
        out.emitAttribute("packageVersion", "1");
        out.emitAttribute("implicit", "true");
        out.startElement("co");
        out.emitAttribute("id", "0");
        out.emitAttribute("binds", "");
        out.startElement("template");
        out.emitAttribute("name", "main");
        out.emitAttribute("flags", "os");
        out.emitAttribute("slots", "0");
        out.setChildRole("body");
        out.startElement("fnRef");
        out.emitAttribute("name", functionName);
        out.emitAttribute("arity", "1");
        out.endElement();
        out.endElement();
        out.endElement();
        out.endElement();
        out.close();
        return new StreamSource(new ByteArrayInputStream(buffer.toByteArray()));
    }

    private static XdmMap makeFunctions(XsltExecutable executable, long value) throws SaxonApiException {
        var transformer = executable.load30();
        transformer.setStylesheetParameters(Map.of(new QName("value"), new XdmAtomicValue(value)));
        return (XdmMap) transformer.callTemplate(new QName("main"));
    }

    private static XdmFunctionItem nestedFunction(XdmMap map) {
        return (XdmFunctionItem) ((XdmArray) map.get("nested").itemAt(0)).get(0).itemAt(0);
    }

    private static void assertFunctions(Processor processor, XdmMap map, long value) throws SaxonApiException {
        var callback = (XdmFunctionItem) nestedFunction(map).call(processor, new XdmAtomicValue("p")).itemAt(0);
        assertEquals("px:" + (value + 10) + ":" + value,
                callback.call(processor, new XdmAtomicValue("x")).itemAt(0).getStringValue());
        var partial = (XdmFunctionItem) map.get("partial").itemAt(0);
        assertEquals("prefix:x" + (value + 10),
                partial.call(processor, new XdmAtomicValue("x")).itemAt(0).getStringValue());
        assertEquals(value + 11, ((XdmAtomicValue) map.get("higher-order").itemAt(0)).getLongValue());
        assertEquals(value + 12, ((XdmAtomicValue) map.get("higher-order").itemAt(1)).getLongValue());
    }

    private static void assertGlobalsAndErrors(Processor processor, XsltExecutable executable)
            throws SaxonApiException {
        XdmFunctionItem[] callbacks = new XdmFunctionItem[2];
        for (int i = 0; i < callbacks.length; i++) {
            var transformer = executable.load30();
            transformer.setStylesheetParameters(Map.of(new QName("value"), new XdmAtomicValue(i)));
            callbacks[i] = (XdmFunctionItem) transformer.callTemplate(new QName("main"));
        }
        assertEquals("1", callbacks[1].call(processor, new XdmAtomicValue(false)).itemAt(0).getStringValue());
        assertEquals("0", callbacks[0].call(processor, new XdmAtomicValue(false)).itemAt(0).getStringValue());
        assertEquals("FOER0000", assertThrows(SaxonApiException.class,
                () -> callbacks[0].call(processor, new XdmAtomicValue(true))).getErrorCode().getLocalName());
    }
}
