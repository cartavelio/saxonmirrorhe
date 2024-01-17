package internaltests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.extfn.ExperimentalSequenceTypeMarshaller;
import com.saxonica.ee.extfn.ExperimentalXPathCompiler;
import com.saxonica.expr.SaxonSyntaxExtension;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AlphaCode;
import net.sf.saxon.value.SequenceType;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *  Test round-trip conversion between sequence types and alpha-codes
 */
public class AlphaCodeTypeTest {

    @Test
    public void testSequenceTypesJS() {
        try {
            Configuration config = new EnterpriseConfiguration();
            IndependentContext ic = new IndependentContext(config);
            ic.declareNamespace("foo", NamespaceUri.of("http://foo.com/"));
            ic.setXPathLanguageLevel(31);
            XPathParser parser = new XPathParser(ic);
            String[] testCases = new String[]{
                    "item()",
                    "item()*",
                    "xs:anyAtomicType?",
                    "xs:string*",
                    "xs:integer+",
                    "node()",
                    "element(foo)",
                    "element(foo:bar)",
                    "xs:numeric*",
                    "xs:dayTimeDuration",
                    "map(*)",
                    "map(xs:string, node()*)",
                    "array(*)",
                    "array(array(xs:integer*))",
                    "empty-sequence()",
                    "function(*)",
                    "function(xs:int, xs:int) as xs:date",
                    "function(xs:int) as (function() as xs:date*)*",
                    "document-node(element(foo, xs:integer))?"
            };
            for (String s : testCases) {
                SequenceType t = parser.parseSequenceType(s, ic);
                MapItem map = ExperimentalXPathCompiler.makeMap(t);
                String json = Err.depict(map).toString();
                String lex = ExperimentalSequenceTypeMarshaller.unmarshal(map).getStringValue();
                MapItem map2 = AlphaCode.toXdmMap(lex);
                String json2 = Err.depict(map2).toString();
                String lex2 = ExperimentalSequenceTypeMarshaller.unmarshal(map2).getStringValue();
                assertEquals(lex, lex2, s);
                //System.out.println(s + " => " + json + " => " + lex + " => " + json2 + " => " + lex2);
            }
        } catch (XPathException e) {
            fail();
        }

    }

    @Test
    public void testSequenceTypes() {
        try {
            Configuration config = new EnterpriseConfiguration();
            IndependentContext ic = new IndependentContext(config);
            ic.declareNamespace("foo", NamespaceUri.of("http://foo.com/"));
            ic.setXPathLanguageLevel(31);
            XPathParser parser = new XPathParser(ic);
            String[] testCases = new String[]{
                    "item()",
                    "item()*",
                    "xs:anyAtomicType?",
                    "xs:string*",
                    "xs:integer+",
                    "node()",
                    "element(foo)",
                    "element(foo:bar)",
                    "xs:numeric*",
                    "xs:dayTimeDuration",
                    "map(*)",
                    "map(xs:string, node()*)",
                    "array(*)",
                    "array(array(xs:integer*))",
                    "empty-sequence()",
                    "function(*)",
                    "function(xs:int, xs:int) as xs:date",
                    "function(xs:int) as (function() as xs:date*)*",
                    "document-node(element(foo, xs:integer))?"
            };
            for (String s : testCases) {
                System.err.println("Processing " + s);
                SequenceType t = parser.parseSequenceType(s, ic);
                String alpha = AlphaCode.fromSequenceType(t);
                SequenceType t2 = AlphaCode.toSequenceType(alpha, config);
                assertEquals(t, t2);
                String alpha2 = AlphaCode.fromSequenceType(t2);
                assertEquals(alpha, alpha2); // ?? component order may differ?
            }
        } catch (XPathException e) {
            fail();
        }

    }

    @Test
    public void testRecordTypes() {
        try {
            Configuration config = new EnterpriseConfiguration();
            IndependentContext ic = new IndependentContext(config);
            ic.setXPathLanguageLevel(40);
            XPathParser parser = new XPathParser(ic);
            parser.setParserExtension(new SaxonSyntaxExtension());
            String[] testCases = new String[]{
                    "record(x, y)",
                    "record(x, y, *)",
                    "record('date of birth'?, 'date of death'?, 'married?'?, *)",
                    "record(x as xs:int, y as xs:string)*",
                    "record(x, y as xs:string*, *)",
                    "record(xxx?, yyy?)+",
                    "record(xxx? as xs:boolean?, yyy? as xs:QName?, *)"
            };
            for (String s : testCases) {
                SequenceType t = parser.parseSequenceType(s, ic);
                String alpha = AlphaCode.fromSequenceType(t);
                SequenceType t2 = AlphaCode.toSequenceType(alpha, config);
                assertEquals(t, t2);
                String alpha2 = AlphaCode.fromSequenceType(t2);
                assertEquals(alpha, alpha2); // ?? component order may differ?
            }
        } catch (XPathException e) {
            fail();
        }

    }
}
