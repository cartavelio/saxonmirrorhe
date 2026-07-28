package net.sf.saxon.trans;

import net.sf.saxon.Configuration;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Supplier;
import java.util.stream.Stream;

class FeatureTest {
    private final List<FeatureTestDsl.NamedFeatureTest> tests;
    private String feature_uri;

    public FeatureTest(String feature_uri, List<FeatureTestDsl.NamedFeatureTest> tests) {
        this.feature_uri = feature_uri;
        this.tests = tests;
    }

    public String getName() {
        return feature_uri;
    }

    public Stream<FeatureTestDsl.NamedFeatureTest> tests() {
        return this.tests.stream();
    }

    static Map<String, FeatureTest> tests(Supplier<FeatureTestInstance> before, FeatureTestDsl.GroupBuilder builder) {

        Map<String, FeatureTest> tests = new HashMap<>();
        final FeatureTestDsl dsl = new FeatureTestDsl(before, tests);
        builder.invoke(dsl);
        return tests;
    }

    public static class FeatureTestInstance {
        public Configuration configuration;
    }
}
