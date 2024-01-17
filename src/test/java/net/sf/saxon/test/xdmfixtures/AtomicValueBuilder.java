package net.sf.saxon.test.xdmfixtures;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

import java.math.BigInteger;
import java.util.Arrays;
import java.util.stream.Stream;

public interface AtomicValueBuilder {
    static AtomicValue integer(Integer num) {
        return IntegerValue.makeIntegerValue(BigInteger.valueOf(num));
    }

    static AtomicValue string(String string) {
        return new StringValue(string);
    }

    static Stream<GroundedValue> strings(String... strings) {
        return Arrays.stream(strings).map(AtomicValueBuilder::string);
    }

    static Stream<GroundedValue> integers(Integer... integers) {
        return Arrays.stream(integers).map(AtomicValueBuilder::integer);
    }
}
