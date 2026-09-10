package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.util.Arrays;
import java.util.List;
import java.util.function.BiFunction;
import java.util.stream.Stream;

public class SequenceBuilder implements BuilderDsl {
    ZenoSequence seq = new ZenoSequence();

    public static GroundedValue build(DslLambda<SequenceBuilder> definition) {
        SequenceBuilder builder = new SequenceBuilder();
        definition.buildWith(builder);
        return builder.getValue();
    }

    public static GroundedValue empty() {
        return EmptySequence.getInstance();
    }

    public SequenceBuilder append(String... strings) {
        return append(Arrays.stream(strings).map(StringValue::new));
    }

    public SequenceBuilder append(Stream<GroundedValue> stream) {
        appendStream(stream);
        return this;
    }

    public SequenceBuilder append(GroundedValue... values) {
        return append(Arrays.stream(values));
    }

    @FunctionalInterface
    interface GroundedValuesToZenoSequence extends BiFunction<ZenoSequence, GroundedValue, ZenoSequence> { }

    private void appendStream(Stream<GroundedValue> stream) {
        seq = stream.reduce(seq,
                (GroundedValuesToZenoSequence) (seq, value) -> seq.appendSequence(value),
                (seq1, seq2) -> seq1.appendSequence(seq2));
    }

    public SequenceBuilder append(List<Item> items) {
        appendList(items);
        return this;
    }

    private void appendList(List<Item> items) {
        seq = seq.appendSequence(SequenceExtent.makeSequenceExtent(items));
    }

    public GroundedValue getValue() {
        if (seq.getLength() == 0) return empty();
        return seq;
    }
}
