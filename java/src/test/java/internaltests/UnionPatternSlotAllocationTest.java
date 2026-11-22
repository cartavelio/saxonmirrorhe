package internaltests;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.Serializer;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;

/** Public synthetic regression for local-variable allocation in registered union-pattern branches. */
public class UnionPatternSlotAllocationTest {
    private static final String UNION = "para[matches(@role, $info-blockquote-source-roles)]                            [preceding-sibling::*[1][self::para[matches(@role, $info-blockquote-roles)]                                                               [not(parent::blockquote)]]]                       |*[self::bibliomixed|self::para]/node()[1][self::br]                       |*[self::bibliomixed|self::para]/br[every $p in preceding-sibling::node()                                                            satisfies $p[   not(matches(., '\\P{Zs}'))                                                                         or empty(.)]]";
    private static final String NON_UNION = "*[self::bibliomixed|self::para]/br[every $p in preceding-sibling::node()                                                            satisfies $p[   not(matches(., '\\P{Zs}'))                                                                         or empty(.)]]";
    private static final String INPUT = "<article xmlns=\"http://docbook.org/ns/docbook\"><para> <br/>tail</para></article>";
    private static final String EXPECTED = "<article xmlns=\"http://docbook.org/ns/docbook\"><para> tail</para></article>";

    @Test
    public void allocatedQuantifierInObservedThreeWayUnion() throws Exception {
        assertEquals(EXPECTED, transform(UNION, "hub:clean-hub", "", "clean-hub", INPUT));
    }

    @Test
    public void allocatedQuantifierInNonUnionControl() throws Exception {
        assertEquals(EXPECTED, transform(NON_UNION, "hub:clean-hub", "", "clean-hub", INPUT));
    }

    @Test
    public void separateNamedModesAllocateTheirRegisteredBranches() throws Exception {
        assertEquals(EXPECTED, transform(UNION, "hub:clean-hub hub:other", "", "clean-hub", INPUT));
        assertEquals(EXPECTED, transform(UNION, "hub:clean-hub hub:other", "", "other", INPUT));
    }

    @Test
    public void explicitPriorityControlKeepsTheUnionAllocated() throws Exception {
        assertEquals(EXPECTED, transform(UNION, "hub:clean-hub", " priority=\"2\"", "clean-hub", INPUT));
    }

    @Test
    public void currentItemBindingRemainsAvailableInUnionBranches() throws Exception {
        String match = UNION + "[current()/@keep ne 'yes']";
        String input = "<article xmlns=\"http://docbook.org/ns/docbook\"><para> <br keep=\"no\"/>tail</para></article>";
        assertEquals(EXPECTED, transform(match, "hub:clean-hub", "", "clean-hub", input));
    }

    private static String transform(String match, String modes, String priority, String initialMode, String input) throws Exception {
        String stylesheet = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\""
                + " xmlns:hub=\"http://transpect.io/hub\" xmlns=\"http://docbook.org/ns/docbook\""
                + " xpath-default-namespace=\"http://docbook.org/ns/docbook\" exclude-result-prefixes=\"hub\">"
                + "<xsl:output omit-xml-declaration=\"yes\" indent=\"no\"/>"
                + "<xsl:variable name=\"info-blockquote-source-roles\" select=\"'source-role'\"/>"
                + "<xsl:variable name=\"info-blockquote-roles\" select=\"'quote-role'\"/>"
                + "<xsl:template match=\"@*|node()\" mode=\"#all\"><xsl:copy>"
                + "<xsl:apply-templates select=\"@*, node()\" mode=\"#current\"/>"
                + "</xsl:copy></xsl:template>"
                + "<xsl:template match=\"" + match + "\" mode=\"" + modes + "\"" + priority + "/>"
                + "</xsl:stylesheet>";
        Processor processor = new Processor(false);
        StreamSource styleSource = new StreamSource(new StringReader(stylesheet));
        styleSource.setSystemId("urn:saxon-test:union-pattern-slot-allocation");
        XsltExecutable executable = processor.newXsltCompiler().compile(styleSource);
        XsltTransformer transformer = executable.load();
        transformer.setInitialMode(new QName("http://transpect.io/hub", initialMode));
        StreamSource inputSource = new StreamSource(new StringReader(input));
        inputSource.setSystemId("urn:saxon-test:union-pattern-input");
        transformer.setSource(inputSource);
        StringWriter writer = new StringWriter();
        Serializer serializer = processor.newSerializer(writer);
        transformer.setDestination(serializer);
        transformer.transform();
        return writer.toString();
    }
}
