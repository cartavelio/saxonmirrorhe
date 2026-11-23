package internaltests;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class ApplyImportsPatternBindingTest {
    private static final String STYLESHEET =
            "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\""
                    + " xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">";

    @Test
    public void namedTemplateLooksUpImportedMatchInTheCurrentMode() throws Exception {
        String imported = STYLESHEET
                + "<xsl:param name=\"flag\" as=\"xs:boolean\" select=\"true()\"/>"
                + "<xsl:template match=\"a[let $v := $flag return $v] | b\">"
                + "<xsl:value-of select=\"local-name()\"/></xsl:template></xsl:stylesheet>";
        String principal = STYLESHEET
                + "<xsl:import href=\"imported.xsl\"/><xsl:output method=\"text\"/>"
                + "<xsl:template match=\"a|b\"><xsl:call-template name=\"helper\"/></xsl:template>"
                + "<xsl:template name=\"helper\"><xsl:apply-imports/></xsl:template></xsl:stylesheet>";
        Processor processor = new Processor(false);
        XsltCompiler compiler = processor.newXsltCompiler();
        compiler.setResourceResolver(request -> source(imported, "imported.xsl"));
        XsltExecutable sheet = compiler.compile(source(principal, "main.xsl"));
        assertEquals("ab", transform(sheet, true));
        assertEquals("b", transform(sheet, false));
    }

    private static StreamSource source(String text, String name) {
        return new StreamSource(new StringReader(text), "file:/apply-imports-test/" + name);
    }

    private static String transform(XsltExecutable sheet, boolean flag) throws Exception {
        XsltTransformer transformer = sheet.load();
        transformer.setParameter(new QName("flag"), new XdmAtomicValue(flag));
        transformer.setSource(source("<root><a/><b/></root>", "input.xml"));
        StringWriter out = new StringWriter();
        transformer.setDestination(sheet.getProcessor().newSerializer(out));
        transformer.transform();
        return out.toString();
    }
}
