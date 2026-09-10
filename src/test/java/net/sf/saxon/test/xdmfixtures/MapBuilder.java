package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.StringValue;

import static com.saxonica.functions.qt4.csv.TestHelpers.integerValue;

public class MapBuilder implements BuilderDsl {
    DictionaryMap map = new DictionaryMap();

    public static MapItem build(DslLambda<MapBuilder> definition) {
        MapBuilder builder = new MapBuilder();
        definition.buildWith(builder);
        return builder.getValue();
    }

    public static MapItem empty() {
        return new HashTrieMap();
    }

    @Override
    public MapItem getValue() {
        return map;
    }

    public MapBuilder put(String name, DslLambda<SequenceBuilder> definition) {
        map.initialPut(name, SequenceBuilder.build(definition));
        return this;
    }

    public MapBuilder put(String name, GroundedValue value) {
        map.initialPut(name, value);
        return this;
    }

    public MapBuilder put(String name, String value) {
        map.initialPut(name, new StringValue(value));
        return this;
    }

    public MapBuilder put(String name, Number value) {
        map.initialPut(name, integerValue(value));
        return this;
    }

}
