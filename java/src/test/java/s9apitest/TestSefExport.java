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
import java.io.IOException;
import java.io.OutputStream;
import java.io.StringReader;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.util.Map;
import java.util.ArrayList;
import java.util.concurrent.Callable;
import java.util.concurrent.Executors;

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
    void reportWriteAndCloseFailures() throws Exception {
        XsltExecutable source = compile(new Processor(false), """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"/>
                """);
        var destination = new FailingOutputStream();
        SaxonApiException writeFailure = assertThrows(SaxonApiException.class, () -> source.export(destination));
        assertTrue(writeFailure.getMessage().contains("destination stopped accepting bytes"), writeFailure::getMessage);
        assertEquals(32, destination.bytes.size());
        assertTrue(destination.closed);

        var cannotClose = new ByteArrayOutputStream() {
            @Override
            public void close() throws IOException {
                throw new IOException("destination could not close");
            }
        };
        SaxonApiException closeFailure = assertThrows(SaxonApiException.class, () -> source.export(cannotClose));
        assertEquals("destination could not close", closeFailure.getMessage());
    }

    @Test
    void concurrentExportsUseIndependentState() throws Exception {
        XsltExecutable source = compile(new Processor(false), """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                                xmlns:f="urn:concurrent">
                  <xsl:param name="value" required="yes"/>
                  <xsl:variable name="label" select="f:label($value)"/>
                  <xsl:function name="f:label">
                    <xsl:param name="value"/>
                    <xsl:sequence select="upper-case($value)"/>
                  </xsl:function>
                  <xsl:template name="main">
                    <xsl:sequence select="function($suffix) { concat($label, $suffix) }('!')"/>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        var tasks = new ArrayList<Callable<String>>();
        for (int i = 0; i < 16; i++) {
            int number = i;
            tasks.add(() -> {
                Processor target = new Processor(false);
                var transformation = roundTrip(target, source).load30();
                transformation.setStylesheetParameters(Map.of(new QName("value"), new XdmAtomicValue("label" + number)));
                return transformation.callTemplate(new QName("main")).itemAt(0).getStringValue();
            });
        }
        var executor = Executors.newFixedThreadPool(4);
        try {
            var completed = executor.invokeAll(tasks);
            for (int i = 0; i < completed.size(); i++) {
                assertEquals("LABEL" + i + "!", completed.get(i).get());
            }
        } finally {
            executor.shutdownNow();
        }
    }

    @Test
    void exportConditionalsWithUnreachableComparisons() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:output method="text"/>
                  <xsl:variable name="include-anchors" select="false()"/>
                  <xsl:template match="/">
                    <xsl:variable name="anchor" as="element(anchor)?"
                                  select="if ($include-anchors)
                                          then root/anchor[not(@role = ('start', 'end'))][1]
                                          else ()"/>
                    <xsl:value-of select="($anchor/@id, 'kept')"/>
                    <xsl:choose>
                      <xsl:when test="root/@enabled = 'yes'">|active</xsl:when>
                      <xsl:when test="true()">|fallback</xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="root/anchor[not(@role = ('start', 'end'))]/@id"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        String active = "<root enabled='yes'><anchor id='hidden'/></root>";
        String fallback = "<root><anchor id='hidden'/></root>";
        assertEquals("kept|active", transform(sourceProcessor, source, active));
        assertEquals("kept|fallback", transform(sourceProcessor, source, fallback));

        Processor target = new Processor(false);
        XsltExecutable reloaded = roundTrip(target, source);
        assertEquals("kept|active", transform(target, reloaded, active));
        assertEquals("kept|fallback", transform(target, reloaded, fallback));
    }

    @Test
    void preservePositionalPatternAcrossNodeKinds() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:output method="text"/>
                  <xsl:template match="/">
                    <xsl:apply-templates select="root/anchor"/>
                  </xsl:template>
                  <xsl:template match="root/node()[1][self::anchor]">first</xsl:template>
                  <xsl:template match="anchor">later</xsl:template>
                </xsl:stylesheet>
                """);
        String first = "<root><anchor/><!-- after --></root>";
        String later = "<root><!-- before --><anchor/></root>";
        assertEquals("first", transform(sourceProcessor, source, first));
        assertEquals("later", transform(sourceProcessor, source, later));

        Processor target = new Processor(false);
        XsltExecutable reloaded = roundTrip(target, source);
        assertEquals("first", transform(target, reloaded, first));
        assertEquals("later", transform(target, reloaded, later));
    }

    @Test
    void preserveDuplicateKeyErrorFromMapInstructions() throws Exception {
        Processor sourceProcessor = new Processor(false);
        XsltExecutable source = compile(sourceProcessor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:template match="/">
                    <xsl:map>
                      <xsl:for-each select="root/entry">
                        <xsl:map-entry key="string(@key)" select="string(.)"/>
                      </xsl:for-each>
                    </xsl:map>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        String input = "<root><entry key='x'>one</entry><entry key='x'>two</entry></root>";
        Processor target = new Processor(false);
        XsltExecutable reloaded = roundTrip(target, source);
        assertEquals("XTDE3365", assertThrows(SaxonApiException.class,
                () -> transform(sourceProcessor, source, input)).getErrorCode().getLocalName());
        assertEquals("XTDE3365", assertThrows(SaxonApiException.class,
                () -> transform(target, reloaded, input)).getErrorCode().getLocalName());
    }

    @Test
    void preserveWhitespaceRulesAndKeys() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:output method="text"/>
                  <xsl:strip-space elements="*"/>
                  <xsl:preserve-space elements="keep"/>
                  <xsl:key name="entry" match="item" use="@id"/>
                  <xsl:template match="/">
                    <xsl:value-of select="count(root/text()), count(root/keep/text()), key('entry', 'a')"
                                  separator="|"/>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        String xml = "<root>  <item id='a'>found</item>  <keep> </keep>  </root>";
        Processor target = new Processor(false);

        assertEquals("0|1|found", transform(processor, source, xml));
        assertEquals("0|1|found", transform(target, roundTrip(target, source), xml));
    }

    @Test
    void preserveNamedOutputCharacterMapsAndDecimalFormats() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                                xmlns:f="urn:formats" exclude-result-prefixes="f">
                  <xsl:output method="text"/>
                  <xsl:output name="f:secondary" method="text" use-character-maps="f:map"/>
                  <xsl:character-map name="f:map">
                    <xsl:output-character character="@" string="[at]"/>
                  </xsl:character-map>
                  <xsl:decimal-format decimal-separator="," grouping-separator="." NaN="123"/>
                  <xsl:decimal-format name="f:format" decimal-separator=":"/>
                  <xsl:template match="/">
                    <xsl:value-of select="format-number(xs:decimal(record/@number), '0,0')"
                                  xmlns:xs="http://www.w3.org/2001/XMLSchema"/>
                    <xsl:text>|</xsl:text>
                    <xsl:value-of select="format-number(number(record/@nan), '0,0')"/>
                    <xsl:result-document href="secondary.txt" format="{record/@format}">
                      <xsl:text>@</xsl:text>
                      <xsl:value-of select="format-number(xs:decimal(record/@number), '0:0', record/@decimal)"
                                    xmlns:xs="http://www.w3.org/2001/XMLSchema"/>
                    </xsl:result-document>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        String xml = "<record number='1.5' nan='NaN' format='f:secondary' decimal='f:format'/>";
        Processor target = new Processor(false);

        assertEquals(Map.of("primary", "1,5|123", "secondary", "[at]1:5"),
                captureResults(processor, source, xml));
        assertEquals(Map.of("primary", "1,5|123", "secondary", "[at]1:5"),
                captureResults(target, roundTrip(target, source), xml));
    }

    @Test
    void preserveAccumulatorModeAssociationAndBindings() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                                xmlns:f="urn:accumulators" default-mode="f:count">
                  <xsl:output method="text"/>
                  <xsl:param name="step" select="2"/>
                  <xsl:mode name="f:count" use-accumulators="f:count"/>
                  <xsl:accumulator name="f:count" initial-value="0"
                                   as="xs:integer" xmlns:xs="http://www.w3.org/2001/XMLSchema">
                    <xsl:accumulator-rule match="item" select="$value + $step"/>
                  </xsl:accumulator>
                  <xsl:template match="/" mode="f:count">
                    <xsl:value-of select="accumulator-after('f:count')"/>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        String xml = "<root><item/><item/><item/></root>";
        Processor target = new Processor(false);

        assertEquals("6", transform(processor, source, xml));
        assertEquals("6", transform(target, roundTrip(target, source), xml));
    }

    @Test
    void preserveRequiredGlobalContextAndLazyGlobals() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:global-context-item as="document-node()" use="required"/>
                  <xsl:param name="use" select="false()"/>
                  <xsl:variable name="bad" select="error(QName('', 'TEST001'))"/>
                  <xsl:template name="entry">
                    <xsl:sequence select="if ($use) then $bad else string(/record/@name)"/>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        Processor target = new Processor(false);
        checkGlobalContextAndGlobals(processor, source);
        checkGlobalContextAndGlobals(target, roundTrip(target, source));
    }

    @Test
    void preserveMissingGlobalParameterError() throws Exception {
        Processor processor = new Processor(false);
        XsltExecutable source = compile(processor, """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:param name="required" required="yes"/>
                  <xsl:template name="entry"><xsl:sequence select="$required"/></xsl:template>
                </xsl:stylesheet>
                """);
        QName entry = new QName("entry");
        SaxonApiException sourceError = assertThrows(SaxonApiException.class,
                () -> source.load30().callTemplate(entry));
        XsltExecutable reloaded = roundTrip(new Processor(false), source);
        SaxonApiException reloadedError = assertThrows(SaxonApiException.class,
                () -> reloaded.load30().callTemplate(entry));

        assertEquals("XTDE0050", sourceError.getErrorCode().getLocalName());
        assertEquals(sourceError.getErrorCode(), reloadedError.getErrorCode());
    }

    @Test
    void preserveStaticBaseUriNamespacesAndCollation() throws Exception {
        Processor processor = new Processor(false);
        String stylesheet = """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                    xmlns:f="urn:retained" default-collation="http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive">
                  <xsl:output method="text"/>
                  <xsl:template match="/">
                    <xsl:value-of select="resolve-uri(record/@relative), namespace-uri-from-QName(xs:QName(record/@name)),
                                          compare(record/@first, record/@second)" separator="|"
                                  xmlns:xs="http://www.w3.org/2001/XMLSchema"/>
                    <xsl:text>|</xsl:text>
                    <xsl:value-of select="compare(record/@first, record/@second)"
                                  default-collation="http://www.w3.org/2005/xpath-functions/collation/codepoint"/>
                  </xsl:template>
                </xsl:stylesheet>
                """;
        XsltExecutable source = processor.newXsltCompiler().compile(new StreamSource(
                new StringReader(stylesheet), "file:/original/styles/stylesheet.xsl"));
        String xml = "<record relative='../data.xml' name='f:local' first='Ada' second='ADA'/>";
        Processor target = new Processor(false);

        assertEquals("file:/original/data.xml|urn:retained|0|1", transform(processor, source, xml));
        assertEquals("file:/original/data.xml|urn:retained|0|1", transform(target, roundTrip(target, source), xml));
    }

    private static Map<String, String> captureResults(Processor processor, XsltExecutable executable, String xml)
            throws SaxonApiException {
        var transformer = executable.load30();
        StringWriter primary = new StringWriter();
        StringWriter secondary = new StringWriter();
        transformer.setBaseOutputURI("file:/output/primary.txt");
        transformer.setResultDocumentHandler(uri -> {
            assertEquals("file:/output/secondary.txt", uri.toString());
            return processor.newSerializer(secondary);
        });
        transformer.applyTemplates(new StreamSource(new StringReader(xml)), processor.newSerializer(primary));
        return Map.of("primary", primary.toString(), "secondary", secondary.toString());
    }

    private static void checkGlobalContextAndGlobals(Processor processor, XsltExecutable executable)
            throws SaxonApiException {
        QName entry = new QName("entry");
        SaxonApiException missingContext = assertThrows(SaxonApiException.class,
                () -> executable.load30().callTemplate(entry));
        assertEquals("XTDE3086", missingContext.getErrorCode().getLocalName());

        var transformer = executable.load30();
        var doc = processor.newDocumentBuilder().build(input());
        transformer.setGlobalContextItem(doc);
        assertEquals("Ada", transformer.callTemplate(entry).itemAt(0).getStringValue());

        var failing = executable.load30();
        failing.setGlobalContextItem(doc);
        failing.setStylesheetParameters(Map.of(new QName("use"), new XdmAtomicValue(true)));
        SaxonApiException badGlobal = assertThrows(SaxonApiException.class, () -> failing.callTemplate(entry));
        assertEquals("TEST001", badGlobal.getErrorCode().getLocalName());
    }

    private static XsltExecutable compile(Processor processor, String stylesheet) throws SaxonApiException {
        return processor.newXsltCompiler().compile(
                new StreamSource(new StringReader(stylesheet), "urn:test:sef-export"));
    }

    private static XsltExecutable reload(Processor processor, ByteArrayOutputStream buffer) throws SaxonApiException {
        return processor.newXsltCompiler().loadExecutablePackage(
                new StreamSource(new ByteArrayInputStream(buffer.toByteArray())));
    }

    private static XsltExecutable roundTrip(Processor processor, XsltExecutable executable) throws SaxonApiException {
        var buffer = new ByteArrayOutputStream();
        executable.export(buffer);
        return reload(processor, buffer);
    }

    private static StreamSource input() {
        return new StreamSource(new StringReader("<record name='Ada'/>"));
    }

    private static String transform(Processor processor, XsltExecutable executable) throws SaxonApiException {
        return transform(processor, executable, "<record name='Ada'/>");
    }

    private static String transform(Processor processor, XsltExecutable executable, String xml) throws SaxonApiException {
        StringWriter result = new StringWriter();
        executable.load30().applyTemplates(new StreamSource(new StringReader(xml)), processor.newSerializer(result));
        return result.toString();
    }

    private static final class TrackingOutputStream extends ByteArrayOutputStream {
        private boolean closed;

        @Override
        public void close() {
            closed = true;
        }
    }

    private static final class FailingOutputStream extends OutputStream {
        private final ByteArrayOutputStream bytes = new ByteArrayOutputStream();
        private boolean closed;

        @Override
        public void write(int value) throws IOException {
            if (bytes.size() == 32) {
                throw new IOException("destination stopped accepting bytes");
            }
            bytes.write(value);
        }

        @Override
        public void close() {
            closed = true;
        }
    }
}
