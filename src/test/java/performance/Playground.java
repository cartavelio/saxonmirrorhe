package performance;

import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.StringView;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;

public class Playground extends PerformanceTestCase {

    // Some experiments to study different ways of testing item types. Results show they're all much the same

    @Test
    @Tag("performance")
    public void testInstanceOf() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i=0; i<5000000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.wrap("" + i)));
        }
        measure(new Measurable() {
            @Override
            public void run() {
                int total = 0;
                for (AtomicValue v : values) {
                    if (v.isUntypedAtomic()) {
                        total++;
                    }
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 50000;
            }

            @Override
            public String name() {
                return "testInstanceOf";
            }
        });
    }

    @Test
    @Tag("performance")
    public void testPrimitiveType() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i = 0; i < 5000000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.wrap ("" + i)));
        }
        measure(new Measurable() {
            @Override
            public void run() {
                int total = 0;
                for (AtomicValue v : values) {
                    if (v.getPrimitiveType() == BuiltInAtomicType.UNTYPED_ATOMIC) {
                        total++;
                    }
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 50000;
            }

            @Override
            public String name() {
                return "testPrimitive";
            }
        });
    }

    @Test
    @Tag("performance")
    public void testActualType() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i = 0; i < 5000000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.wrap ("" + i)));
        }
        measure(new Measurable() {
            @Override
            public void run() {
                int total = 0;
                for (AtomicValue v : values) {
                    if (v.getItemType() == BuiltInAtomicType.UNTYPED_ATOMIC) {
                        total++;
                    }
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 50000;
            }

            @Override
            public String name() {
                return "testActual";
            }
        });
    }

    @Test
    @Tag("performance")
    public void testFingerprint() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i = 0; i < 5000000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.wrap("" + i)));
        }
        measure(new Measurable() {
            @Override
            public void run() {
                int total = 0;
                for (AtomicValue v : values) {
                    if (v.getItemType().getFingerprint() == StandardNames.XS_UNTYPED_ATOMIC) {
                        total++;
                    }
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 50000;
            }

            @Override
            public String name() {
                return "testFingerprint";
            }
        });
    }

}

