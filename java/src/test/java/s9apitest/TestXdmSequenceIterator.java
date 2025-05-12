package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import java.io.ByteArrayInputStream;
import java.nio.charset.StandardCharsets;
import java.util.NoSuchElementException;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Test XdmSequenceIterator class, representing a sequence of items returned (for example) by a query
 */
public class TestXdmSequenceIterator {

    @Test
    public void testIteratorWithHasNext() {
        String docStr = "<?xml version='1.0'?>\n"
                + "<!-- comment1 -->"
                + "<!-- comment2 -->"
                + "<doc/>";

        Processor processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        ByteArrayInputStream bais = new ByteArrayInputStream(docStr.getBytes(StandardCharsets.UTF_8));
        Source source = new SAXSource(new InputSource(bais));

        try {
            XdmNode doc = builder.build(source);

            XdmSequenceIterator<XdmNode> iter = doc.axisIterator(Axis.CHILD);
            assertTrue(iter.hasNext());
            assertEquals("<!-- comment1 -->", iter.next().toString());
            assertTrue(iter.hasNext());
            assertEquals("<!-- comment2 -->", iter.next().toString());
            assertTrue(iter.hasNext());
            assertEquals("<doc/>", iter.next().toString());
            assertFalse(iter.hasNext());
        } catch (SaxonApiException sae) {
            fail();
        }
    }

    @Test
    public void testIteratorWithRepeatedHasNext() {
        String docStr = "<?xml version='1.0'?>\n"
                + "<!-- comment1 -->"
                + "<!-- comment2 -->"
                + "<doc/>";

        Processor processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        ByteArrayInputStream bais = new ByteArrayInputStream(docStr.getBytes(StandardCharsets.UTF_8));
        Source source = new SAXSource(new InputSource(bais));

        try {
            XdmNode doc = builder.build(source);

            XdmSequenceIterator<XdmNode> iter = doc.axisIterator(Axis.CHILD);
            assertTrue(iter.hasNext());
            assertTrue(iter.hasNext());
            assertTrue(iter.hasNext());
            assertEquals("<!-- comment1 -->", iter.next().toString());
            assertTrue(iter.hasNext());
            assertTrue(iter.hasNext());
            assertTrue(iter.hasNext());
            assertEquals("<!-- comment2 -->", iter.next().toString());
            assertTrue(iter.hasNext());
            assertTrue(iter.hasNext());
            assertTrue(iter.hasNext());
            assertEquals("<doc/>", iter.next().toString());
            assertFalse(iter.hasNext());
            assertFalse(iter.hasNext());
            assertFalse(iter.hasNext());
        } catch (SaxonApiException sae) {
            fail();
        }
    }

    @Test
    public void testIteratorWithoutHasNext() {
        String docStr = "<?xml version='1.0'?>\n"
                + "<!-- comment1 -->"
                + "<!-- comment2 -->"
                + "<doc/>";

        Processor processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        ByteArrayInputStream bais = new ByteArrayInputStream(docStr.getBytes(StandardCharsets.UTF_8));
        Source source = new SAXSource(new InputSource(bais));

        try {
            XdmNode doc = builder.build(source);

            XdmSequenceIterator<XdmNode> iter = doc.axisIterator(Axis.CHILD);
            assertEquals("<!-- comment1 -->", iter.next().toString());
            assertEquals("<!-- comment2 -->", iter.next().toString());
            assertEquals("<doc/>", iter.next().toString());
        } catch (SaxonApiException sae) {
            fail();
        }
    }

    @Test
    public void testIteratorReadsBeyondEnd() {
        String docStr = "<?xml version='1.0'?>\n"
                + "<!-- comment1 -->"
                + "<!-- comment2 -->"
                + "<doc/>";

        Processor processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        ByteArrayInputStream bais = new ByteArrayInputStream(docStr.getBytes(StandardCharsets.UTF_8));
        Source source = new SAXSource(new InputSource(bais));

        try {
            XdmNode doc = builder.build(source);

            XdmSequenceIterator<XdmNode> iter = doc.axisIterator(Axis.CHILD);
            assertTrue(iter.hasNext());
            assertEquals("<!-- comment1 -->", iter.next().toString());
            assertTrue(iter.hasNext());
            assertEquals("<!-- comment2 -->", iter.next().toString());
            assertTrue(iter.hasNext());
            assertEquals("<doc/>", iter.next().toString());
            assertFalse(iter.hasNext());
            try {
                iter.next();
                fail("Exception not thrown");
            } catch (NoSuchElementException err) {
                // no action, expected
            }
            try {
                iter.next();
                fail("Exception not thrown");
            } catch (NoSuchElementException err) {
                // no action, expected
            }
        } catch (SaxonApiException sae) {
            fail();
        }

        bais = new ByteArrayInputStream(docStr.getBytes(StandardCharsets.UTF_8));
        source = new SAXSource(new InputSource(bais));

        try {
            XdmNode doc = builder.build(source);

            XdmSequenceIterator<XdmNode> iter = doc.axisIterator(Axis.CHILD);
            assertEquals("<!-- comment1 -->", iter.next().toString());
            assertEquals("<!-- comment2 -->", iter.next().toString());
            assertEquals("<doc/>", iter.next().toString());
            try {
                iter.next();
                fail("Exception not thrown");
            } catch (NoSuchElementException err) {
                // no action, expected
            }
            try {
                iter.next();
                fail("Exception not thrown");
            } catch (NoSuchElementException err) {
                // no action, expected
            }
        } catch (SaxonApiException sae) {
            fail();
        }
    }

    @Test
    public void testBaseIteratorFails() {
        try {
            Processor processor = new Processor(false);
            String expr = "for $i in 1 to 5 return (if ($i eq 3) then error() else $i)";
            XPathCompiler comp = processor.newXPathCompiler();
            XPathExecutable exec = comp.compile(expr);
            XPathSelector selector = exec.load();
            XdmSequenceIterator<?> iter = selector.iterator();
            assertTrue(iter.hasNext());
            assertEquals("1", iter.next().getStringValue());
            assertTrue(iter.hasNext());
            assertEquals("2", iter.next().getStringValue());
            // beyond this, it becomes unpredictable exactly when the error will be thrown
            if (iter.hasNext()) {
                iter.next().getStringValue();
            }
        } catch (SaxonApiUncheckedException err) {
            assertTrue(err.getMessage().contains("application call on error()"));
            // OK, exception expected

        } catch (SaxonApiException sae) {
            fail(sae.getMessage());
        }

    }


    @Test
    public void testValueIterator() {
        XdmValue value = new XdmAtomicValue("banana");
        value = value.append(new XdmAtomicValue("pineapple"));
        XdmSequenceIterator<?> iter = value.iterator();
        assertTrue(iter.hasNext());
        assertEquals("banana", iter.next().getStringValue());
        assertTrue(iter.hasNext());
        assertEquals("pineapple", iter.next().getStringValue());
        assertFalse(iter.hasNext());
    }

    @Test
    public void testEmptyIterator() {
        XdmSequenceIterator<?> iter = XdmEmptySequence.getInstance().iterator();
        assertFalse(iter.hasNext());
    }

}




