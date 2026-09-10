package net.sf.saxon.test;

import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapBuilder;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.test.xdmfixtures.SequenceBuilder;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

import java.util.Map;
import java.util.stream.Stream;

public class TestXdmHelpers {
    /**
     * Build a MapItem from a Map.
     *
     * @param options the map to convert
     * @return
     */
    public static MapItem buildXdmMap(Map<String, GroundedValue> options) {
        MapBuilder resultMap = DictionaryMap.getBuilder(options.size());
        options.forEach(resultMap::put);
        return resultMap.getCompletedMap();
    }

    /**
     * Build a MapItem from a stream of key value pairs
     *
     * @param entries the stream to convert
     * @return
     */
    public static MapItem buildXdmMap(Stream<Object[]> entries) {
        MapBuilder resultMap = DictionaryMap.getBuilder();
        entries.forEach((pair) -> {
            resultMap.put((String) pair[0], (GroundedValue) pair[1]);
        });
        return resultMap.getCompletedMap();
    }

    public static IntegerValue integerValue(Number num) {
        return new Int64Value(num.longValue());
    }

    public static StringValue stringValue(String string) {
        return new StringValue(string);
    }

    public static GroundedValue simpleValueSequence(String... strings) {
        return SequenceBuilder.build(s -> s.append(strings));
    }

    public static GroundedValue simpleValueSequence(Integer... ints) {
        return SequenceBuilder.build(s -> s.append(ints));
    }
}
