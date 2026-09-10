package net.sf.saxon.trans;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.FeatureData;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.sapling.Saplings;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;

import javax.xml.transform.Source;
import java.util.AbstractMap;
import java.util.Map;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.*;
import static org.junit.jupiter.api.Assumptions.assumeTrue;

class ConfigurationReaderTest {
    public final ConfigurationReader configReader = new ConfigurationReader();
    public final Configuration configuration = new Configuration();
    public Configuration readConfiguration;

    @BeforeEach
    void setUp() {
        configReader.setBaseConfiguration(configuration);
    }

    public static AbstractMap.SimpleImmutableEntry<String, FeatureTestLambda> fixture(String featureUri, FeatureTestLambda test) {
        return new AbstractMap.SimpleImmutableEntry<>(featureUri, test);
    }

    @FunctionalInterface
    interface FeatureTestLambda {
        void test(ConfigurationReader configReader) throws XPathException;
    }

    @Nested
    class GlobalSection {
        private static Stream<Arguments> globalConfigFileOptions() {
            FeatureData.init();
            return FeatureData.featureList.stream().filter(feat -> { return feat.configFileDetails.section == "global"; }).map(Arguments::of);
        }

        public Map<String, FeatureTestLambda> featureConfigFileTests = Map.ofEntries(
            fixture(FeatureKeys.ALLOWED_PROTOCOLS, configReader -> {
                Source config = Saplings.doc(builder -> {
                    builder.defaultNs("http://saxon.sf.net/ns/configuration");
                    builder.elem("configuration", conf -> {
                        conf.attr("edition", "EE");
                        conf.elem("global", global -> {
                            global.attr("allowedProtocols", "");
                        });
                    });
                });

                readConfiguration = configReader.makeConfiguration(config);

                assertEquals("", readConfiguration.getProtocolRestrictor().toString(), "protocol restrictor should be set to empty string");
            })
        );

        @ParameterizedTest
        @MethodSource("globalConfigFileOptions")
        void configFileOptionCanBeRead(FeatureData feature) throws XPathException {
            assumeTrue(featureConfigFileTests.containsKey(feature.uri), () -> "No test for " + feature.uri);

            featureConfigFileTests.get(feature.uri).test(configReader);
        }
    }
}
