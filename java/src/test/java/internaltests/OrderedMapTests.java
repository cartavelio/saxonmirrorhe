package internaltests;

import net.sf.saxon.ma.map.OrderedMap;
import net.sf.saxon.ma.map.OrderedStringMap;
import net.sf.saxon.Configuration;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmMap;
import net.sf.saxon.s9api.XdmValue;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;

import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests to test different implementations of ordered XDM maps (interface MapItem)
 */
public class OrderedMapTests {

    /**
     *  This method must be called with an initial map containing the values
     *  {"a":1.0e0, "b":2.0e0, "c":3.0e0, "d":4.0e0}
     *  It tests maps with string-valued keys.
     *
     */
    private void testOrderedStringMapImplementation(MapItem initial) {
        try {
            Configuration config = new Configuration();
            MapType stringToDouble = new MapType(BuiltInAtomicType.STRING,BuiltInAtomicType.DOUBLE.one());
            testKeys("a-b-c-d-", initial);
            assertTrue(stringToDouble.matches(initial, config.getTypeHierarchy()));
            assertEquals(1, ((NumericValue) initial.get(new StringValue("a"))).getDoubleValue());
            assertEquals(2, ((NumericValue) initial.get(new StringValue("b"))).getDoubleValue());
            assertEquals(3, ((NumericValue) initial.get(new StringValue("c"))).getDoubleValue());
            assertEquals(4, ((NumericValue) initial.get(new StringValue("d"))).getDoubleValue());
            assertEquals(4, initial.size());
            assertNull(initial.get(new StringValue("#")));
            testKeys("a-b-c-d-", initial);

            MapItem minus3 = initial.remove(new StringValue("c"));
            assertEquals(1, ((NumericValue) minus3.get(new StringValue("a"))).getDoubleValue());
            assertEquals(2, ((NumericValue) minus3.get(new StringValue("b"))).getDoubleValue());
            assertEquals(4, ((NumericValue) minus3.get(new StringValue("d"))).getDoubleValue());
            assertNull(minus3.get(new StringValue("c")));
            assertNull(minus3.get(new StringValue("#")));
            assertEquals(3, minus3.size());
            assertTrue(stringToDouble.matches(minus3, config.getTypeHierarchy()));
            assertEquals(stringToDouble, minus3.getItemType(config.getTypeHierarchy()));
            testKeys("a-b-d-", minus3);

            MapItem plus5 = initial.put(new StringValue("*"), new DoubleValue(5));
            assertEquals(1, ((NumericValue) plus5.get(new StringValue("a"))).getDoubleValue());
            assertEquals(2, ((NumericValue) plus5.get(new StringValue("b"))).getDoubleValue());
            assertEquals(3, ((NumericValue) plus5.get(new StringValue("c"))).getDoubleValue());
            assertEquals(4, ((NumericValue) plus5.get(new StringValue("d"))).getDoubleValue());
            assertEquals(5, ((NumericValue) plus5.get(new StringValue("*"))).getDoubleValue());
            assertNull(plus5.get(new StringValue("#")));
            assertEquals(5, plus5.size());
            assertTrue(stringToDouble.matches(plus5, config.getTypeHierarchy()));
            testKeys("a-b-c-d-*-", plus5);
            assertEquals(stringToDouble, plus5.getItemType(config.getTypeHierarchy()));

            MapItem plus20 = initial.put(new StringValue("twas"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("brillig"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("and"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("the"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("slithy"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("toves"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("did"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("gyre"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("and"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("gimble"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("in"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("the"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("wabe"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("all"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("mimsy"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("were"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("the"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("borogroves"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("and"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("the"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("nome"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("wraths"), new DoubleValue(5));
            plus20 = plus20.put(new StringValue("outgrabe"), new DoubleValue(5));
            testKeys("a-b-c-d-twas-brillig-and-the-slithy-toves-did-gyre-gimble-in-wabe-all-mimsy-were-borogroves-nome-wraths-outgrabe-", plus20);
            assertTrue(stringToDouble.matches(plus20, config.getTypeHierarchy()));

            MapItem replace2 = plus5.put(new StringValue("b"), new DoubleValue(81));
            assertEquals(1, ((NumericValue) replace2.get(new StringValue("a"))).getDoubleValue());
            assertEquals(81, ((NumericValue) replace2.get(new StringValue("b"))).getDoubleValue());
            assertEquals(3, ((NumericValue) replace2.get(new StringValue("c"))).getDoubleValue());
            assertEquals(4, ((NumericValue) replace2.get(new StringValue("d"))).getDoubleValue());
            assertEquals(5, ((NumericValue) replace2.get(new StringValue("*"))).getDoubleValue());
            assertNull(replace2.get(new StringValue("#")));
            assertEquals(5, replace2.size());
            testKeys("a-b-c-d-*-", plus5);
            assertTrue(stringToDouble.matches(plus5, config.getTypeHierarchy()));

            MapItem nonInteger = plus5.put(new StringValue("z"), new StringValue("banana"));
            assertFalse(stringToDouble.matches(nonInteger, config.getTypeHierarchy()));
            assertTrue(stringToDouble.matches(plus5, config.getTypeHierarchy()));
            MapItem allInteger = nonInteger.remove(new StringValue("z"));
            assertTrue(stringToDouble.matches(allInteger, config.getTypeHierarchy()));

            MapItem empty=initial.remove(new StringValue("a"))
                    .remove(new StringValue("b"))
                    .remove(new StringValue("c"))
                    .remove(new StringValue("d"));
            testKeys("", empty);
            assertEquals(0, empty.size());
            MapType intToString = new MapType(BuiltInAtomicType.INTEGER, BuiltInAtomicType.STRING.one());
            // empty map matches anything
            assertTrue(intToString.matches(empty, config.getTypeHierarchy()));
            
        } catch (Exception e) {
            fail(e);
        }

    }

    private void testKeys(String expected, MapItem map) {
        StringBuilder sb = new StringBuilder();
        for (KeyValuePair kvp : map.keyValuePairs()) {
            sb.append(kvp.key.getStringValue());
            sb.append("-");
        }
        assertEquals(expected, sb.toString());
    }

    @Test
    public void testOrderedMap() {
        MapBuilder builder = OrderedMap.getBuilder();
        builder.put(new StringValue("a"), new DoubleValue(1));
        builder.put(new StringValue("b"), new DoubleValue(2));
        builder.put(new StringValue("c"), new DoubleValue(3));
        builder.put(new StringValue("d"), new DoubleValue(4));
        testOrderedStringMapImplementation(builder.getCompletedMap());
    }

    @Test
    public void testOrderedStringMap() {
        MapItem o = new OrderedStringMap();
        o = o.put(new StringValue("a"), new DoubleValue(1));
        o = o.put(new StringValue("b"), new DoubleValue(2));
        o = o.put(new StringValue("c"), new DoubleValue(3));
        o = o.put(new StringValue("d"), new DoubleValue(4));
        testOrderedStringMapImplementation(o);
    }

    @Test
    public void testBuildFromEmpty() {
        MapItem o = new EmptyMap();
        o = o.put(new StringValue("a"), new DoubleValue(1));
        o = o.put(new StringValue("b"), new DoubleValue(2));
        o = o.put(new StringValue("c"), new DoubleValue(3));
        o = o.put(new StringValue("d"), new DoubleValue(4));
        testOrderedStringMapImplementation(o);
    }

    @Test
    public void testBuildFromSingleton() {
        MapItem o = new SingleEntryMap(new StringValue("a"), new DoubleValue(1));
        o = o.put(new StringValue("b"), new DoubleValue(2));
        o = o.put(new StringValue("c"), new DoubleValue(3));
        o = o.put(new StringValue("d"), new DoubleValue(4));
        testOrderedStringMapImplementation(o);
    }

    @Test
    public void testShapedMap() {
        Shape shape = new Shape(new Twine8("a"), new Twine8("b"), new Twine8("c"), new Twine8("d"));
        ShapedMap map = new ShapedMap(shape,
                                      new DoubleValue(1),
                                      new DoubleValue(2),
                                      new DoubleValue(3),
                                      new DoubleValue(4));
        testOrderedStringMapImplementation(map);
    }

    @Test
    public void testJsonMap() {
        try {
            XdmValue val = new Processor(false).newJsonBuilder().parseJson(
                    new StringReader("{\"a\":1, \"b\":2, \"c\":3, \"d\":4}"));
            assertTrue(val instanceof XdmMap);
            XdmMap m = (XdmMap) val;
            MapItem item = m.getUnderlyingValue();
            testOrderedStringMapImplementation(item);
        } catch (SaxonApiException e) {
            fail();
        }
    }


}
