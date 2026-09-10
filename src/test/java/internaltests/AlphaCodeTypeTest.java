package internaltests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.extfn.ExperimentalSequenceTypeMarshaller;
import com.saxonica.ee.extfn.ExperimentalXPathCompiler;
import com.saxonica.expr.SaxonSyntaxExtension;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AlphaCode;
import net.sf.saxon.value.SequenceType;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *  Tests of the NodeInfo interface as implemented by the tinytree
 */
public class AlphaCodeTypeTest {

    @Test
    public void testSequenceTypes() {
        try {
            Configuration config = new EnterpriseConfiguration();
            IndependentContext ic = new IndependentContext(config);
            ic.declareNamespace("foo", "http://foo.com/");
            ic.setXPathLanguageLevel(31);
            XPathParser parser = new XPathParser();
            parser.setParserExtension(new SaxonSyntaxExtension());
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
}
