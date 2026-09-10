package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.test.xdmfixtures.SequenceBuilder;
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
        static RowMaker build(Consumer<RowMaker> definition) {
            RowMaker maker = new RowMaker();
            return maker.add(definition);
        }

        public RowMaker add(Consumer<RowMaker> definition) {
            definition.accept(this);
            return this;
        }

        private final SequenceBuilder builder = new SequenceBuilder();

        public RowMaker row(String... values) {
            builder.append(builder.array(values));
            return this;
        }

        public GroundedValue getValue() {
            return builder.getValue();
        }
    }

    static class ExpectedRowMaker {
        private final List<List<String>> expectedRows = new ArrayList<>();

        static List<List<String>> build(Consumer<ExpectedRowMaker> definition) {
            ExpectedRowMaker maker = new ExpectedRowMaker();
            return maker.add(definition).getValue();
        }

        public ExpectedRowMaker add(Consumer<ExpectedRowMaker> definition) {
            definition.accept(this);
            return this;
        }

        public ExpectedRowMaker row(String... values) {
            List<String> expectedRow = Arrays.asList(values);
            expectedRows.add(expectedRow);
            return this;
        }

        public List<List<String>> getValue() {
            return expectedRows;
        }
    }

    public static List<List<String>> expectedRows(Consumer<ExpectedRowMaker> definition) {
        return ExpectedRowMaker.build(definition);
    }

    public static GroundedValue makeRow(String... strings) {
        return FixtureBuilder.array(ab -> ab.append(strings));
    }

    public static GroundedValue makeRows(Consumer<RowMaker> definition) {
        return RowMaker.build(definition).getValue();
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
