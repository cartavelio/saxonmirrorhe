package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test XdmMap class, representing an XPath map item
 */
public class TestXdmMap {

    @Test
    public void testMapOperations() {
        XdmMap map = new XdmMap();
        map = map.put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
        map = map.put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
        map = map.put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
        assertEquals(3, map.mapSize());
        try {
            assertEquals(2, ((XdmAtomicValue) map.get(new XdmAtomicValue("b"))).getLongValue());
            assertEquals(2, ((XdmAtomicValue) map.get("b")).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }
        map = map.remove(new XdmAtomicValue("c"));
        map = map.remove(new XdmAtomicValue("d"));
        map = map.put(new XdmAtomicValue("a"), new XdmAtomicValue(4));
        assertEquals(2, map.mapSize());
        try {
            assertEquals(4, ((XdmAtomicValue) map.get(new XdmAtomicValue("a"))).getLongValue());
            assertEquals(4, ((XdmAtomicValue) map.get("a")).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testConversionFromJavaMap() {
        Map<XdmAtomicValue, XdmAtomicValue> jam = new HashMap<XdmAtomicValue, XdmAtomicValue>();
        jam.put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
        jam.put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
        jam.put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
        XdmMap map = new XdmMap(jam);
        assertEquals(3, map.mapSize());
        try {
            assertEquals(2, ((XdmAtomicValue) map.get(new XdmAtomicValue("b"))).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }
        map = map.remove(new XdmAtomicValue("c"));
        map = map.remove(new XdmAtomicValue("d"));
        map = map.put(new XdmAtomicValue("a"), new XdmAtomicValue(4));
        assertEquals(2, map.mapSize());
        try {
            assertEquals(4, ((XdmAtomicValue) map.get(new XdmAtomicValue("a"))).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testMapAsFunction() {
        XdmMap map = new XdmMap();
        map = map.put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
        map = map.put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
        map = map.put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
        assertEquals(3, map.mapSize());
        Processor proc = new Processor(true);
        try {
            XdmValue result = map.call(proc, new XdmAtomicValue("c"));
            assertTrue(result instanceof XdmAtomicValue);
            assertTrue(((XdmAtomicValue)result).getLongValue() == 3);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testMapAsQueryParameter() {
        try {
            XdmMap map = new XdmMap();
            map = map.put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
            map = map.put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
            map = map.put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
            Processor proc = new Processor(true);
            String query =
                    "declare namespace m='http://www.w3.org/2005/xpath-functions/map';" +
                            "declare variable $a as map(*) external; " +
                            "m:size($a)";
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryEvaluator q = comp.compile(query).load();
            q.setExternalVariable(new QName("a"), map);
            XdmAtomicValue result = (XdmAtomicValue) q.evaluateSingle();
            assertEquals(3, result.getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMapAsQueryResult() {
        try {
            Processor proc = new Processor(true);
            String query = "map{1:2, 2:3, 3:4}";
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryEvaluator q = comp.compile(query).load();
            XdmMap result = (XdmMap) q.evaluateSingle();
            assertEquals(3, result.mapSize());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testConversionToJavaMap() {
        try {
            Processor proc = new Processor(true);
            String query = "map{1:2, 2:3, 3:4}";
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryEvaluator q = comp.compile(query).load();
            XdmMap result = (XdmMap) q.evaluateSingle();
            Map<XdmAtomicValue, XdmValue> jmap = result.asMap();
            assertEquals(3, jmap.size());
            assertEquals(4, ((XdmAtomicValue)jmap.get(new XdmAtomicValue(3))).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testConversionFromJavaMap2() {
        Map<Integer, String> in = new HashMap<Integer, String>();
        in.put(1, "a");
        in.put(2, "b");
        XdmMap map = XdmMap.makeMap(in);
        assertEquals(2, map.mapSize());
        assertEquals(XdmValue.makeValue("b"), map.get(2));
        assertTrue(ItemType.ANY_MAP.matches(map));

        Processor proc = new Processor(true);
        ItemTypeFactory factory = new ItemTypeFactory(proc);
        ItemType mapType = factory.getMapType(ItemType.INT,
                                              SequenceType.makeSequenceType(ItemType.STRING, OccurrenceIndicator.ONE));
        assertTrue(mapType.matches(map));
    }

    @Test
    public void testMapToString() {
        XdmMap map = new XdmMap();
        map = map.put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
        map = map.put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
        map = map.put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
        String result = map.toString();
        assertEquals("map{\"a\":1,\"b\":2,\"c\":3}", result); // Could change
    }
}




