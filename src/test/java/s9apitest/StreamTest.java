package s9apitest;

import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Step;
import net.sf.saxon.s9api.streams.Steps;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.Iterator;
import java.util.List;
import java.util.Optional;
import java.util.function.Predicate;

import static net.sf.saxon.s9api.streams.Predicates.eq;
import static net.sf.saxon.s9api.streams.Predicates.isElement;
import static net.sf.saxon.s9api.streams.Steps.*;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test stream-based navigation on the XdmValue class.
 *
 * Needs Java 8.
 */
public class StreamTest {

    private final String xml1 = "<doc id='a'><section id='b1'><para id='c1.1'>One</para><para id='c1.2'>Two</para></section>"
            + "<section id='b2'><para id='c2.1'>One</para><para id='c2.2'>Two</para></section></doc>";

    private final String xml2 = "<doc><thing xml:id='t1' contains='t2 t3'>1</thing>"
            + "<thing xml:id='t2' contains='t4'>2</thing>"
            + "<thing xml:id='t3' contains=''>3</thing>"
            + "<thing xml:id='t4' contains='t5'>4</thing>"
            + "<thing xml:id='t5' contains=''>5</thing></doc>";

    @Test
    public void test001() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
            Step<XdmNode> ids = descendant("para").then(attribute("id"));
            Step<XdmAtomicValue> z = ids.then(atomize());
            List<XdmAtomicValue> resultList = doc.select(z).asList();
            assertEquals("c1.1", resultList.get(0).toString());
            assertEquals("c1.2", resultList.get(1).toString());
            assertEquals("c2.1", resultList.get(2).toString());
            assertEquals("c2.2", resultList.get(3).toString());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void test002() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
            Optional<XdmNode> firstPara = doc.select(descendant("para")).first().asOptionalNode();
            assertTrue(firstPara.isPresent());
            List<XdmAtomicValue> resultList = firstPara.get().select(
                    path(followingSibling(isElement()), attribute("id")).then(atomize())).asListOfAtomic();
            assertEquals("c1.2", resultList.get(0).toString());
            assertEquals(1, resultList.size());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void test003() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
            Optional<XdmNode> firstPara = doc.select(descendant("para")).first().asOptionalNode();
            assertTrue(firstPara.isPresent());
            List<XdmAtomicValue> resultList = firstPara.get().select(
                    Steps.following("para").then(attribute("id")).then(Steps.atomize())).asListOfAtomic();
            assertEquals("c1.2", resultList.get(0).toString());
            assertEquals("c2.1", resultList.get(1).toString());
            assertEquals("c2.2", resultList.get(2).toString());
            assertEquals(3, resultList.size());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void test004() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
            Optional<XdmNode> firstPara = doc.select(descendant("para")).first().asOptionalNode();
            assertTrue(firstPara.isPresent());
            List<XdmAtomicValue> resultList = firstPara.get().select(Steps.following("para")
                                                                             .then(attribute("id")).then(Steps.atomize())).last().asListOfAtomic();
            assertEquals("c2.2", resultList.get(0).toString());
            assertEquals(1, resultList.size());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void test005() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
            Optional<XdmNode> firstPara = doc.select(descendant("para")).first().asOptionalNode();
            assertTrue(firstPara.isPresent());
            List<XdmAtomicValue> otherIds = firstPara.get().select(Steps.following("para").at(2)
                           .then(attribute("id")).then(Steps.atomize())).asListOfAtomic();
            assertEquals("c2.2", otherIds.get(0).toString());
            assertEquals(1, otherIds.size());
        } catch (SaxonApiException err) {
            fail();
        }
    }

//    public void test006() {
//        try {
//            Processor p = new Processor(false);
//            DocumentBuilder b = p.newDocumentBuilder();
//            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
//            Optional<XdmNode> lastPara = doc.select(descendant("para")).last().asOptionalNode();
//            assertTrue(lastPara.isPresent());
//            Predicate<XdmNode> is11 = eq(attribute("id"), "c1.1");
//            Stream<XdmAtomicValue> otherIds = lastPara.get().select(preceding("para").where(is11)
//                                                                             .then(attribute("id")).then(Steps.atomize()));
//            List<XdmAtomicValue> resultList = otherIds.collect(Collectors.toList());
//            assertEquals("c1.1", resultList.get(0).toString());
//            assertEquals(1, resultList.size());
//        } catch (SaxonApiException err) {
//            fail();
//        }
//    }

    @Test
    public void test007() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml1)));
            List<XdmNode> paras = doc.select(descendant("para")).asList();
            Iterator<XdmNode> iter = paras.iterator();
            String id = "id";
            assertEquals("c1.1", iter.next().attribute(id));
            assertEquals("c1.2", iter.next().attribute(id));
            assertEquals("c2.1", iter.next().attribute(id));
            assertEquals("c2.2", iter.next().attribute(id));
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testEarlyExitInclusive() {
        Predicate<XdmItem> over1000 = item -> {
            try {
                return ((XdmAtomicValue) item).getLongValue() > 1000;
            } catch (SaxonApiException e) {
                return false;
            }
        };
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XPathCompiler c = p.newXPathCompiler();
            XPathSelector s = c.compile("1 to 100000000").load();
            XdmAtomicValue last = s.stream().untilFirstInclusive(over1000).last().asAtomic();
            assertEquals(1001, last.getLongValue());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testEarlyExitExclusive() {
        Predicate<XdmItem> over1000 = item -> {
            try {
                return ((XdmAtomicValue) item).getLongValue() > 1000;
            } catch (SaxonApiException e) {
                return false;
            }
        };
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XPathCompiler c = p.newXPathCompiler();
            XPathSelector s = c.compile("1 to 100000000").load();
            XdmAtomicValue last = s.stream().untilFirstExclusive(over1000).last().asAtomic();
            assertEquals(1000, last.getLongValue());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testIdStep() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml2))).select(child(isElement())).asNode();
            XdmNode thing1 = doc.select(child("thing").where(eq("1"))).asNode();
            XdmValue idrefs = thing1.select(attribute("contains").then(tokenize())).asXdmValue();
            List<XdmNode> contents = idrefs.select(id(doc.getParent())).asListOfNodes();
            Iterator<XdmNode> iter = contents.iterator();
            assertEquals("2", contents.get(0).getStringValue());
            assertEquals("3", contents.get(1).getStringValue());
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testXdmFunctionAsStep() {
        try {
            Processor p = new Processor(true);
            XdmFunctionItem f = (XdmFunctionItem)p.newXPathCompiler().evaluateSingle("function($x){$x/*}", null);
            Step<XdmItem> kids = f.asStep(p);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode doc = b.build(new StreamSource(new StringReader(xml2))).select(child("doc")).asNode();
            XdmNode thing1 = doc.select(kids.where(eq("1"))).asNode();
            assertEquals("1", thing1.getStringValue());
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }


}




