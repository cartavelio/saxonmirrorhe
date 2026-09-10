package net.sf.saxon.trans;

import net.sf.saxon.s9api.SaxonApiException;
import org.junit.jupiter.api.function.Executable;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.function.Supplier;

class FeatureTestDsl {
    private final Map<String, FeatureTest> featureTests;
    private final Supplier<FeatureTest.FeatureTestInstance> before;

    public FeatureTestDsl(Supplier<FeatureTest.FeatureTestInstance> before, Map<String, FeatureTest> featureTests) {
        this.before = before;
        this.featureTests = featureTests;
    }

    @FunctionalInterface
    interface FeatureTestLambda {
        void test(FeatureTest.FeatureTestInstance td) throws XPathException, SaxonApiException;
    }

    @FunctionalInterface
    interface GroupBuilder {
        void invoke(FeatureTestDsl builder);
    }

    record NamedFeatureTest(String name, Executable executable) {}

    @FunctionalInterface
    interface TestsBuilder {
        void invoke(FeatureTestGroupDsl builder);
    }

    FeatureTestDsl add(String featureUri, TestsBuilder builder) {
        List<NamedFeatureTest> tests = new ArrayList<>();

        FeatureTestGroupDsl testGroup = new FeatureTestGroupDsl(before, tests);
        builder.invoke(testGroup);
        FeatureTest ft = new FeatureTest(featureUri, tests);
        this.featureTests.put(featureUri, ft);
        return this;
    }
}
