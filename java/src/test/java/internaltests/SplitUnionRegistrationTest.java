package internaltests;

import net.sf.saxon.expr.instruct.TemplateRule;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;
import net.sf.saxon.trans.rules.Rule;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

public class SplitUnionRegistrationTest {
    private static final String UNION = "x[@a] | x[@b]";

    @Test
    public void splitBranchesOwnTheirRegisteredPatterns() throws Exception {
        XsltExecutable sheet = compile("<xsl:template match=\"" + UNION + "\"/>");
        List<Rule> rules = new ArrayList<>();
        sheet.getUnderlyingCompiledStylesheet().getRuleManager().getUnnamedMode().processRules(rules::add);
        assertEquals(2, rules.size());
        assertNotSame(rules.get(0).getAction(), rules.get(1).getAction());
        assertEquals(rules.get(0).getSequence(), rules.get(1).getSequence());
        assertNotEquals(rules.get(0).getPartNumber(), rules.get(1).getPartNumber());
        for (Rule rule : rules) {
            assertSame(rule.getPattern(), ((TemplateRule) rule.getAction()).getMatchPattern());
        }
    }

    @Test
    public void nextMatchTreatsOverlappingBranchesAsOneDeclaration() throws Exception {
        assertEquals("matched", nextMatch("<xsl:template match=\"" + UNION + "\">matched</xsl:template>"));
    }

    @Test
    public void nextMatchReportsAnAmbiguityBetweenDistinctDeclarations() {
        SaxonApiException error = assertThrows(SaxonApiException.class, () -> nextMatch(
                "<xsl:template match=\"x[@a]\"/><xsl:template match=\"x[@b]\"/>"));
        assertEquals("XTDE0540", error.getErrorCode().getLocalName());
    }

    private static String nextMatch(String templates) throws Exception {
        XsltExecutable sheet = compile("<xsl:mode on-multiple-match=\"fail\"/>"
                + "<xsl:template match=\"x\" priority=\"2\"><xsl:next-match/></xsl:template>" + templates);
        XsltTransformer transformer = sheet.load();
        transformer.setSource(new StreamSource(new StringReader("<x a='1' b='2'/>")));
        StringWriter result = new StringWriter();
        transformer.setDestination(sheet.getProcessor().newSerializer(result));
        transformer.transform();
        return result.toString();
    }

    private static XsltExecutable compile(String templates) throws SaxonApiException {
        Processor processor = new Processor(false);
        String style = "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">"
                + "<xsl:output method=\"text\"/>" + templates + "</xsl:stylesheet>";
        return processor.newXsltCompiler().compile(new StreamSource(new StringReader(style)));
    }
}
