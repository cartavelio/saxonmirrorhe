package performance;

import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.UntypedAtomicValue;

import java.util.ArrayList;
import java.util.List;

public class Playground extends PerformanceTestCase {

    // Some experiments to study different ways of testing item types. Results show they're all much the same

    public void testInstanceOf() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i=0; i<5000000; i++) {
            values.add(new Int64Value(i));
            values.add(new UntypedAtomicValue("" + i));
        }
        measure(new Measurable() {
            @Override
            public void run() {
                int total = 0;
                for (AtomicValue v : values) {
                    if (v instanceof UntypedAtomicValue) {
                        total++;
                    }
                }
            }

            @Override
            public int expectedTimeMilliseconds() {
                return 50;
            }

            @Override
            public String name() {
                return "testInstanceOf";
            }
        });
    }

    public void testPrimitiveType() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i = 0; i < 5000000; i++) {
            values.add(new Int64Value(i));
            values.add(new UntypedAtomicValue("" + i));
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
            public int expectedTimeMilliseconds() {
                return 50;
            }

            @Override
            public String name() {
                return "testPrimitive";
            }
        });
    }

    public void testActualType() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i = 0; i < 5000000; i++) {
            values.add(new Int64Value(i));
            values.add(new UntypedAtomicValue("" + i));
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
            public int expectedTimeMilliseconds() {
                return 50;
            }

            @Override
            public String name() {
                return "testActual";
            }
        });
    }

    public void testFingerprint() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i = 0; i < 5000000; i++) {
            values.add(new Int64Value(i));
            values.add(new UntypedAtomicValue("" + i));
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
            public int expectedTimeMilliseconds() {
                return 50;
            }

            @Override
            public String name() {
                return "testFingerprint";
            }
        });
    }

}

