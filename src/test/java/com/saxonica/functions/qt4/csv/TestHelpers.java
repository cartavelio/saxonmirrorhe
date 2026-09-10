package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.tree.iter.ArrayIterator;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.function.Consumer;
import java.util.stream.Collectors;

public class TestHelpers {
    public static IntegerValue integerValue(Number num) {
        return new Int64Value(num.longValue());
    }

    static class RowMaker {
        private final List<ArrayItem> rows = new ArrayList<>();

        public RowMaker row(String... values) {
            rows.add(new SimpleArrayItem(Arrays.stream(values).map(StringValue::new).collect(Collectors.toList())));
            return this;
        }


        Sequence get() {
            return SequenceExtent.makeSequenceExtent(rows);
        }
    }

    public static Sequence makeRow(Consumer<RowMaker> consumer) {
        return makeRows(consumer);
    }

    public static Sequence makeRows(Consumer<RowMaker>... consumers) {
        RowMaker maker = new RowMaker();
        Arrays.stream(consumers).forEach (c -> c.accept(maker));
        return maker.get();
    }

    public static StringValue stringValue(String string) {
        return new StringValue(string);
    }
    public static GroundedValue stringValueSequence(String... strings) {
        return SequenceExtent.makeSequenceExtent(Arrays.stream(strings).map(TestHelpers::stringValue).collect(Collectors.toList()));
    }

    public static ArrayItem stringsArrayItem(String... strings) {
        return new SimpleArrayItem(Arrays.stream(strings).map(TestHelpers::stringValue).collect(Collectors.toList()));
    }
}
