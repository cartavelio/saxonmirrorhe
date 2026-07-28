package net.sf.saxon.trans;

import org.junit.jupiter.api.function.Executable;

import java.util.List;
import java.util.function.Supplier;

class FeatureTestGroupDsl {
    private final List<FeatureTestDsl.NamedFeatureTest> tests;
    private final Supplier<FeatureTest.FeatureTestInstance> before;

    FeatureTestGroupDsl(Supplier<FeatureTest.FeatureTestInstance> before, List<FeatureTestDsl.NamedFeatureTest> tests) {
        this.before = before;
        this.tests = tests;
    }

    public void add(String name, FeatureTestDsl.FeatureTestLambda testBody) {
        Executable test = () -> testBody.test(before.get());
        tests.add(new FeatureTestDsl.NamedFeatureTest(name, test));
    }
}
