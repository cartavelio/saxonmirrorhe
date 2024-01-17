package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;

public class FixtureBuilder {
    public static MapItem map(DslLambda<MapBuilder> definition) {
        return (MapItem) MapBuilder.build(definition);
    }

    public static GroundedValue sequence(DslLambda<SequenceBuilder> definition) {
        return SequenceBuilder.build(definition);
    }

    public static ArrayItem array(DslLambda<ArrayBuilder> definition) {
        return (ArrayItem) ArrayBuilder.build(definition);
    }
}
