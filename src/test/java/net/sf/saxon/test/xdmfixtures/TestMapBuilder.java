package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.MapBuilder;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;

public class TestMapBuilder implements BuilderDsl {
    MapBuilder map = DictionaryMap.getBuilder();

    public static MapItem build(DslLambda<TestMapBuilder> definition) {
        TestMapBuilder builder = new TestMapBuilder();
        definition.buildWith(builder);
        return builder.getValue();
    }

    public static MapItem empty() {
        return EmptyMap.getInstance();
    }

    @Override
    public MapItem getValue() {
        return map.getCompletedMap();
    }

    public TestMapBuilder put(String name, DslLambda<SequenceBuilder> definition) {
        map.put(name, SequenceBuilder.build(definition));
        return this;
    }

    public TestMapBuilder put(String name, GroundedValue value) {
        map.put(name, value);
        return this;
    }

    public TestMapBuilder put(String name, String value) {
        map.put(name, AtomicValueBuilder.string(value));
        return this;
    }

    public TestMapBuilder put(String name, Integer value) {
        map.put(name, AtomicValueBuilder.integer(value));
        return this;
    }

}
