package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;

public class FixtureBuilder {
    public static MapItem map(DslLambda<MapBuilder> definition) {
        return (MapItem) MapBuilder.build(definition);
    }

    public static MapItem map() {
        return MapBuilder.empty();
    }

    public static GroundedValue sequence(DslLambda<SequenceBuilder> definition) {
        return SequenceBuilder.build(definition);
    }

    public static GroundedValue sequence(String... values) {
        return SequenceBuilder.build(s -> s.append(values));
    }

    public static GroundedValue sequence(Integer... values) {
        return SequenceBuilder.build(s -> s.append(values));
    }

    public static GroundedValue sequence() {
        return SequenceBuilder.empty();
    }

    public static ArrayItem array(DslLambda<ArrayBuilder> definition) {
        return (ArrayItem) ArrayBuilder.build(definition);
    }

    public static ArrayItem array(String... values) {
        return (ArrayItem) ArrayBuilder.build(s -> s.append(values));
    }

    public static ArrayItem array(Integer... values) {
        return  (ArrayItem) ArrayBuilder.build(s -> s.append(values));
    }

}
