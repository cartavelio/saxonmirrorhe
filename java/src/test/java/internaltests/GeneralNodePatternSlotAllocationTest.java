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

public class GeneralNodePatternSlotAllocationTest {
    private static final String PREDICATES =
            "[every $p in preceding-sibling::node()"
                    + " satisfies $p[not(matches(., '\\P{Zs}')) or empty(.)]][1]";

    @Test
    public void allocatedQuantifierInPromotedPrecondition() throws Exception {
        String match = "other | br" + PREDICATES;
        assertEquals("<para> tail</para>",
                transform(match, "<para> <br/>tail</para>", ""));
        assertEquals("<article><para>text<br/>tail</para><para> <br/>tail</para></article>",
                transform(match,
                        "<article><other/><para>text<br/>tail</para><para> <br/><br/>tail</para></article>",
                        ""));
    }

    @Test
    public void allocatedQuantifierInParentlessTreeAlternative() throws Exception {
        String rootTemplate =
                "<xsl:template match=\"/\" mode=\"clean\">"
                        + "<xsl:variable name=\"para\" as=\"element(para)\">"
                        + "<para><xsl:text> </xsl:text><br/>tail</para>"
                        + "</xsl:variable>"
                        + "<xsl:variable name=\"nonmatching\" as=\"element(para)\">"
                        + "<para>text<br/>tail</para>"
                        + "</xsl:variable>"
                        + "<out><xsl:apply-templates select=\"$para, $nonmatching\" mode=\"clean\"/></out>"
                        + "</xsl:template>";
        assertEquals("<out><para> tail</para><para>text<br/>tail</para></out>",
                transform("para/br" + PREDICATES, "<input/>", rootTemplate));
    }

    private static String transform(String match, String input, String rootTemplate) throws Exception {
        String stylesheet =
                "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">"
                        + "<xsl:output omit-xml-declaration=\"yes\" indent=\"no\"/>"
                        + "<xsl:mode name=\"clean\" on-no-match=\"shallow-copy\"/>"
                        + "<xsl:template match=\"" + match + "\" mode=\"clean\"/>"
                        + rootTemplate
                        + "</xsl:stylesheet>";
        Processor processor = new Processor(false);
        StreamSource styleSource = new StreamSource(new StringReader(stylesheet));
        styleSource.setSystemId("urn:saxon-test:general-node-pattern-slot-allocation");
        XsltExecutable executable = processor.newXsltCompiler().compile(styleSource);
        XsltTransformer transformer = executable.load();
        transformer.setInitialMode(new QName("clean"));
        StreamSource inputSource = new StreamSource(new StringReader(input));
        inputSource.setSystemId("urn:saxon-test:general-node-pattern-input");
        transformer.setSource(inputSource);
        StringWriter writer = new StringWriter();
        Serializer serializer = processor.newSerializer(writer);
        transformer.setDestination(serializer);
        transformer.transform();
        return writer.toString();
    }
}
