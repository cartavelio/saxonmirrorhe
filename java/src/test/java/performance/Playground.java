package performance;

import net.sf.saxon.om.StandardNames;
import net.sf.saxon.serialize.HTMLURIEscaper;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.fail;

public class Playground extends PerformanceTestCase {

    // Some experiments to study different ways of testing item types. Results show they're all much the same

    @Test
    @Tag("performance")
    public void testInstanceOf() {
        final List<AtomicValue> values = new ArrayList<>();
        for (int i=0; i<100000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.tidy("" + i)));
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
                return 500;
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
        for (int i = 0; i < 100000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.tidy("" + i)));
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
                return 500;
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
        for (int i = 0; i < 100000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.tidy("" + i)));
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
                return 400;
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
        for (int i = 0; i < 100000; i++) {
            values.add(new Int64Value(i));
            values.add(StringValue.makeUntypedAtomic(StringView.tidy("" + i)));
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
                return 300;
            }

            @Override
            public String name() {
                return "testFingerprint";
            }
        });
    }

    @Test
    //@Tag("performance")
    public void testURLescaping() {
        final List<String> results = new ArrayList<>();

        measure(new Measurable() {
            @Override
            public void run() {
                int total = 0;
                for (int i = 256; i < 48000; i++) {
                    String uri = "http://my.example.uri/path/" + (char)(i);
                    try {
                        results.add(HTMLURIEscaper.escapeURL(uri, false, null));
                    } catch (XPathException e) {
                        e.printStackTrace();
                    }
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 500000;
            }

            @Override
            public String name() {
                return "testEscapeURI";
            }
        });
    }

    @Test
    @Tag("performance")
    public void testStringJoin() {

        //List<UnicodeString> result = new ArrayList<>();

        measure(new Measurable() {
            @Override
            public void run() {
                UnicodeBuilder qb = new UnicodeBuilder(32);
                for (int i = 256; i < 48000; i++) {
                    String uri = "http://my.example.uri/path/" + (char) (i);
                    qb.append(uri);
                }
                if (qb.toUnicodeString().length() < 1000) {
                    fail();
                }
                //System.err.println("len = " + qb.toUnicodeString().length());
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 30000;
            }

            @Override
            public String name() {
                return "testEscapeURI";
            }
        });
    }

    @Test
    @Tag("performance")
    public void testStringJoinFromTwine8() {

        //List<UnicodeString> result = new ArrayList<>();

        measure(new Measurable() {
            @Override
            public void run() {
                UnicodeBuilder qb = new UnicodeBuilder(32);
                Twine8 base = new Twine8("http://my.example.uri/path/");
                for (int i = 256; i < 48000; i++) {
                    qb.append(base).append(""+ i);
                }
                if (qb.toUnicodeString().length() < 1000) {
                    fail();
                }
                //System.err.println("len = " + qb.toUnicodeString().length());
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 30000;
            }

            @Override
            public String name() {
                return "testEscapeURI";
            }
        });
    }

}

