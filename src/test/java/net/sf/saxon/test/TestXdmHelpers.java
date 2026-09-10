package net.sf.saxon.test;

import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;

import java.util.Map;
import java.util.stream.Stream;

public class TestXdmHelpers {
    /**
     * Build a MapItem from a Map.
     *
     * @param options the map to convert
     * @return
     */
    public static MapItem buildXdmMap(Map<String, Sequence> options) {
        DictionaryMap resultMap = new DictionaryMap();
        options.forEach((k, v) -> {
            resultMap.initialPut(k, (GroundedValue) v);
        });
        return resultMap;
    }

    /**
     * Build a MapItem from a stream of key value pairs
     *
     * @param entries the stream to convert
     * @return
     */
    public static MapItem buildXdmMap(Stream<Object[]> entries) {
        DictionaryMap resultMap = new DictionaryMap();
        entries.forEach((pair) -> {
            resultMap.initialPut((String) pair[0], (GroundedValue) pair[1]);
        });
        return resultMap;
    }
}
