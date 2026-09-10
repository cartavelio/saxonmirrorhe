package net.sf.saxon.trans;

import net.sf.saxon.lib.FeatureData;
import net.sf.saxon.lib.FeatureIndex;
import org.junit.jupiter.api.DynamicNode;
import org.junit.jupiter.api.DynamicTest;
import org.junit.jupiter.api.TestFactory;

import java.util.Map;
import java.util.function.Predicate;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assumptions.assumeTrue;
import static org.junit.jupiter.api.DynamicContainer.dynamicContainer;
import static org.junit.jupiter.api.DynamicTest.dynamicTest;

public class ConfigurationFeatureTesting {
    DynamicTest unimplemented(String name) {
        return dynamicTest(name, () -> { assumeTrue(false, "No test for: " + name); });
    }

    Stream<DynamicNode> buildTests(String edition, Map<String, FeatureTest> featureTests) {
        return buildTests(edition, featureTests, f -> true);
    }

    Stream<DynamicNode> buildTests(String edition, Map<String, FeatureTest> featureTests, Predicate<FeatureData> finalFilter) {
        return FeatureIndex.stream().filter(f -> f.editions != null && f.editions.contains(edition)).filter(finalFilter).map(feature -> {
            if (!featureTests.containsKey(feature.uri)) {
                return unimplemented(feature.uri);
            } else {
                FeatureTest t = featureTests.get(feature.uri);
                return dynamicContainer(t.getName(),
                        t.tests().map(test -> dynamicTest(test.name(), test.executable()))
                );
            }
        });
    }
}
