package internaltests;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.PatternParser;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XsdCompiler;
import net.sf.saxon.s9api.XsdSchema;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class PriorityTests {

    private final Configuration config = new EnterpriseConfiguration();

    private void testPriority(String pattern, double expectedPriority) {
        try {
            IndependentContext env = new IndependentContext(config);
            env.setXPathLanguageLevel(40);
            env.setSchemaAware(true);
            PatternParser parser = new PatternParser(env);
            Pattern pat = parser.parsePattern(pattern, env);
            assertEquals(expectedPriority, pat.getDefaultPriority());
        } catch (XPathException e) {
            fail(e);
        }
    }

    private void testPrioritySchemaAware(String pattern, double expectedPriority) {
        try {
            String schema = "<schema xmlns='http://www.w3.org/2001/XMLSchema'>" +
                    "<element name='e'/>" +
                    "<attribute name='a'/>" +
                    "</schema>";
            Processor proc = new Processor(config);
            XsdCompiler comp = proc.newXsdCompiler();
            XsdSchema xsd = comp.compile(new StreamSource(new StringReader(schema)));
            IndependentContext env = new IndependentContext(config);
            env.setXPathLanguageLevel(40);
            env.setSchemaAware(true);
            env.setImportedSchema(xsd.getUnderlyingSchema());
            PatternParser parser = new PatternParser(env);
            Pattern pat = parser.parsePattern(pattern, env);
            assertEquals(expectedPriority, pat.getDefaultPriority());
        } catch (XPathException | SaxonApiException e) {
            fail(e);
        }
    }

    @Test
    public void testPriority000() {
        testPriority("/", -0.5);
        testPriority("text()", -0.5);
        testPriority("comment()", -0.5);
        testPriority("document-node()", -0.5);
        testPriority("processing-instruction()", -0.5);
        testPriority("namespace-node()", -0.5);
        testPriority("attribute()", -0.5);
        testPriority("node()", -0.5);
        testPriority("child::text()", -0.5);
        testPriority("descendant::text()", -0.5);
        testPriority("descendant-or-self::text()", -0.5);
    }

    @Test
    public void testPriority001() {
        testPriority("*", -0.5);
        testPriority("element()", -0.5);
        testPriority("element(*)", -0.5);
        testPriority("attribute()", -0.5);
        testPriority("attribute(*)", -0.5);
        testPriority("@*", -0.5);
    }

    @Test
    public void testPriority002() {
        testPriority("xml:*", -0.25);
        testPriority("element(xml:*)", -0.25);
        testPriority("element(*:local)", -0.25);
        testPriority("attribute(xml:*)", -0.25);
        testPriority("attribute(*:local)", -0.25);
        testPriority("@xml:*", -0.25);
        testPriority("@*:local", -0.25);
    }

    @Test
    public void testPriority003() {
        testPriority("e", 0);
        testPriority("element(e)", 0);
        testPriority("@code", 0);
        testPriority("attribute(code)", 0);
        testPriority("element(*, xs:anyType)", 0);
        testPriority("element(*, xs:anyType?)", 0);
        testPriority("element(*, xs:untyped)", 0);
        testPriority("attribute(*, xs:untypedAtomic)", 0);
        testPriority("attribute(*, xs:integer)", 0);
        testPriority("processing-instruction('x')", 0);
        testPriority("processing-instruction(x)", 0);
        testPriority("child::e", 0);
        testPriority("descendant::e", 0);
        testPriority("descendant-or-self::e", 0);
    }

    @Test
    public void testPriority004() {
        testPriority("element(xml:*, xs:integer)", 0.125);
        testPriority("element(*:local, xs:integer)", 0.125);
        testPriority("element(xml:*, xs:integer?)", 0.125);
        testPriority("element(Q{http://www.example.com/}*, xs:integer?)", 0.125);
        testPriority("element(*:local, xs:integer?)", 0.125);
        testPriority("child::element(*:local, xs:integer?)", 0.125);
        testPriority("attribute(xml:*, xs:integer)", 0.125);
        testPriority("attribute(*:local, xs:integer)", 0.125);
        testPriority("attribute::attribute(*:local, xs:integer)", 0.125);
        testPriority("@attribute(*:local, xs:integer)", 0.125);
    }

    @Test
    public void testPriority005() {
        testPriority("element(e, xs:integer)", 0.25);
        testPriority("element(e, xs:integer?)", 0.25);
        testPriority("element(xml:e, xs:integer?)", 0.25);
        testPriority("element(Q{http://ns.example.com/}e, xs:integer?)", 0.25);
        testPriority("descendant::element(e, xs:integer?)", 0.25);
        testPriority("attribute(xml:space, xs:integer)", 0.25);
        testPriority("@attribute(xml:space, xs:integer)", 0.25);
    }

    @Test
    public void testPriority006() {
        testPriority("e union f", 0);
        testPriority("@a union @b", 0);
        testPriority("* except e", -0.5);
        testPriority("xml:* except xml:space", -0.25);
    }

    @Test
    public void testPriority007() {
        testPriority("/e", 0.5);
        testPriority("e/f", 0.5);
        testPriority("e//f", 0.5);
        testPriority("e[@x = 3]", 0.5);
    }

    @Test
    public void testPriority008() {
        testPriority("document-node(a)", 0);
        testPriority("document-node(*:a)", -0.25);
        testPriority("document-node(element(a))", 0);
        testPriority("document-node(element(a, xs:anyType?))", 0.25);
        testPriority("document-node(element(*:a))", -0.25);
        testPriority("document-node(element(*:a, xs:untyped))", 0.125);
        testPriority("document-node(*)", -0.5);
    }
    
    @Test
    public void testPriority020() {
        testPriority("jnode(*)[?x = 2]", 0.5);
        testPriority("jnode(*)", -1);
        testPriority("jnode(*, item()*)", -1);
        testPriority("jnode('xxx')", 0);
        testPriority("jnode('xxx', item()*)", 0);
        testPriority("jnode(*, xs:integer)", 0);
        testPriority("jnode('xxx', xs:integer)", 0.25);
        testPriority("jnode(23, xs:integer)", 0.25);
        testPriority("jnode(-17.6e0, xs:integer)", 0.25);
        testPriority("jnode(true(), xs:integer)", 0.25);
        testPriority("jnode(false(), xs:integer?)", 0.25);
        testPriority("jnode(#xml:space, array(*))", 0.25);
        testPriority("jnode(#Q{uri}local, (xs:integer|xs:string)*)", 0.25);
    }

    @Test
    public void testPriority021() {
        testPriority("~array(*)[?x = 2]", 0.5);
        testPriority("~array(*)", 0);
        testPriority("~array(xs:integer)", 0);
        testPriority("~map(*)[?x = 2]", 0.5);
        testPriority("~map(*)", 0);
        testPriority("~map(xs:integer, xs:boolean)", 0);
    }

    @Test
    public void testPriority022() {
        testPriority(".", -1);
        testPriority(".[. instance of xs:integer]", 1);
    }

    @Test
    public void testPriority050() {
        testPrioritySchemaAware("schema-element(e)", 0.25);
        testPrioritySchemaAware("schema-attribute(a)", 0.25);
        testPrioritySchemaAware("child::schema-element(e)", 0.25);
        testPrioritySchemaAware("descendant-or-self::schema-element(e)", 0.25);
        testPrioritySchemaAware("attribute::schema-attribute(a)", 0.25);
        testPrioritySchemaAware("//schema-element(e)", 0.5);
        testPrioritySchemaAware("schema-element(e)/schema-attribute(a)", 0.5);
    }
}




