package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.StringReader;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * test Processor.newJsonBuilder() (new in 11)
 */
public class TestJsonBuilder {

    @Test
    public void testNumber() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(new StringReader("17"));
            assertTrue(val instanceof XdmAtomicValue);
            assertEquals(((XdmAtomicValue)val).getTypeName().getLocalName(), "double");
            assertEquals(17, ((XdmAtomicValue) val).getDoubleValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testString() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(new StringReader("\"banana\""));
            assertTrue(val instanceof XdmAtomicValue);
            assertEquals(((XdmAtomicValue) val).getTypeName().getLocalName(), "string");
            assertEquals("banana", ((XdmAtomicValue) val).getStringValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testBoolean() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(new StringReader("true"));
            assertTrue(val instanceof XdmAtomicValue);
            assertEquals(((XdmAtomicValue) val).getTypeName().getLocalName(), "boolean");
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testNull() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(new StringReader("null"));
            assertTrue(val instanceof XdmEmptySequence);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testArray() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(new StringReader("[1,2,3]"));
            assertTrue(val instanceof XdmArray);
            XdmArray a = (XdmArray)val;
            assertTrue(a.get(0) instanceof XdmAtomicValue);
            assertEquals(1, ((XdmAtomicValue)a.get(0)).getDoubleValue());
            assertEquals(3, a.arrayLength());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testObject() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(new StringReader("{\"a\":1, \"b\":2}"));
            assertTrue(val instanceof XdmMap);
            XdmMap m = (XdmMap) val;
            assertTrue(m.get("a") instanceof XdmAtomicValue);
            assertEquals(1, ((XdmAtomicValue) m.get("a")).getDoubleValue());
            assertEquals(2, m.mapSize());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testLiberal() {
        try {
            JsonBuilder jsonBuilder = new Processor(false).newJsonBuilder();
            jsonBuilder.setLiberal(true);
            XdmValue val = jsonBuilder.parseJson(new StringReader("{\"a\":1, \"b\":2,}"));
            assertTrue(val instanceof XdmMap);
            XdmMap m = (XdmMap) val;
            assertTrue(m.get("a") instanceof XdmAtomicValue);
            assertEquals(1, ((XdmAtomicValue) m.get("a")).getDoubleValue());
            assertEquals(2, m.mapSize());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testStrict() {
        try {
            JsonBuilder jsonBuilder = new Processor(false).newJsonBuilder();
            jsonBuilder.setLiberal(false);
            XdmValue val = jsonBuilder.parseJson(new StringReader("{\"a\":1, \"b\":2,}"));
            assertTrue(val instanceof XdmMap);
            XdmMap m = (XdmMap) val;
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertEquals("FOJS0001", e.getErrorCode().getLocalName());
        }
    }

    @Test
    public void testParseFromString() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson("{\"a\":1, \"b\":2}");
            assertTrue(val instanceof XdmMap);
            XdmMap m = (XdmMap) val;
            assertTrue(m.get("a") instanceof XdmAtomicValue);
            assertEquals(1, ((XdmAtomicValue) m.get("a")).getDoubleValue());
            assertEquals(2, m.mapSize());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testParseFromURI() {
        try {
            URI uri = new File(new ConfigTest().getDataDir() + "sample.json").toURI();
            XdmValue val = new Processor(false).newJsonBuilder().loadJson(uri);
            assertTrue(val instanceof XdmMap);
            XdmMap m = (XdmMap) val;
            assertTrue(m.get("id") instanceof XdmAtomicValue);
            assertEquals(1, ((XdmAtomicValue) m.get("id")).getDoubleValue());
            assertEquals(4, m.mapSize());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


}

