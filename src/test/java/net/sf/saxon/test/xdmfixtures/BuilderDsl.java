package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;

import java.util.stream.Stream;

public interface BuilderDsl {
    GroundedValue getValue();

    default GroundedValue sequence(DslLambda<SequenceBuilder> definition) {
        return SequenceBuilder.build(definition);
    }

    default GroundedValue sequence(String... strings) {
        return SequenceBuilder.build(s -> s.append(strings));
    }

    default GroundedValue sequence(Integer... values) {
        return SequenceBuilder.build(s -> s.append(values));
    }

    default GroundedValue sequence() {
        return SequenceBuilder.empty();
    }

    default MapItem map(DslLambda<TestMapBuilder> definition) {
        return TestMapBuilder.build(definition);
    }

    default GroundedValue array(DslLambda<ArrayBuilder> definition) {
        return ArrayBuilder.build(definition);
    }

    default GroundedValue array(String... strings) {
        return ArrayBuilder.build(a -> a.append(strings));
    }

    default GroundedValue array(GroundedValue... values) {
        return ArrayBuilder.build(a -> a.append(values));
    }

    default GroundedValue array(Stream<GroundedValue> stream) {
        return ArrayBuilder.build(a -> a.append(stream));
    }

    default GroundedValue array() {
        return ArrayBuilder.empty();
    }

    default MapItem map() {
        return TestMapBuilder.empty();
    }

}
