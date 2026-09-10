package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.StringMapBuilder;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import org.junit.Assert;

public class TestMapBuilder implements BuilderDsl {
    StringMapBuilder map = new StringMapBuilder();

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
        try {
            return map.getCompletedMap();
        } catch (XPathException e) {
            Assert.fail(e.getMessage());
            return null;
        }
    }

    public TestMapBuilder put(String name, DslLambda<SequenceBuilder> definition) {
        map.put(StringView.of(name), SequenceBuilder.build(definition));
        return this;
    }

    public TestMapBuilder put(String name, GroundedValue value) {
        map.put(StringView.of(name), value);
        return this;
    }

    public TestMapBuilder put(String name, String value) {
        map.put(StringView.of(name), AtomicValueBuilder.string(value));
        return this;
    }

    public TestMapBuilder put(String name, Integer value) {
        map.put(StringView.of(name), AtomicValueBuilder.integer(value));
        return this;
    }

}
