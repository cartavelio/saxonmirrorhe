package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.stream.Stream;

public class ArrayBuilder implements BuilderDsl {
    public static GroundedValue build(DslLambda<ArrayBuilder> definition) {
        ArrayBuilder builder = new ArrayBuilder();
        definition.buildWith(builder);
        return builder.getValue();
    }

    private final ArrayList<GroundedValue> arrayList = new ArrayList<>();

    public static GroundedValue empty() {
        return new SimpleArrayItem(new ArrayList<>());
    }

    public ArrayBuilder append(String... strings) {
        return append(Arrays.stream(strings).map(StringValue::new));
    }

    public ArrayBuilder append(GroundedValue... values) {
        return append(Arrays.stream(values));
    }

    public ArrayBuilder append(Stream<GroundedValue> stream) {
        stream.forEachOrdered(value -> arrayList.add(value));
        return this;
    }

    public ArrayBuilder append(List<GroundedValue> items) {
        arrayList.addAll(items);
        return this;
    }

    public GroundedValue getValue() {
        return new SimpleArrayItem(arrayList);
    }
}
