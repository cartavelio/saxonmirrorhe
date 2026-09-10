package net.sf.saxon.trans;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.sapling.Saplings;
import net.sf.saxon.sapling.dsl.ElementDsl;
import org.junit.jupiter.api.DynamicNode;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.TestFactory;

import javax.xml.transform.Source;
import java.util.Map;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.assertEquals;

class ConfigurationReaderTest {
    class ReaderTestInstance extends FeatureTest.FeatureTestInstance {
        public final ConfigurationReader subject = new ConfigurationReader();
        public final Configuration configuration = new Configuration();

        ReaderTestInstance() {
            subject.setBaseConfiguration(configuration);
        }
    }

    Source configFile(String section, ElementDsl builder) {
        return Saplings.doc(doc -> {
            doc.defaultNs("http://saxon.sf.net/ns/configuration");
            doc.elem("configuration", conf -> {
                conf.elem(section, builder);
            });
        });
    }

    Source configFileWithFeature(String section, String feature, String value) {
        return configFile(section, global -> {
            global.attr(feature, value);
        });
    }

    @Nested
    class GlobalSection extends ConfigurationFeatureTesting {
        public Map<String, FeatureTest> featureConfigFileTests = FeatureTest.tests(ReaderTestInstance::new, features -> {
            features.add(FeatureKeys.ALLOWED_PROTOCOLS, tests -> {
                tests.add("Setting allowedProtocols to #none creates the right ProtocolRestrictor", (ft) -> {
                    ReaderTestInstance t = (ReaderTestInstance) ft;
                    Source config = configFileWithFeature("global", "allowedProtocols", "#none");

                    Configuration readConfiguration = t.subject.makeConfiguration(config);

                    assertEquals("", readConfiguration.getProtocolRestrictor().toString(), "protocol restrictor should be set to empty string");
                });

                tests.add("Setting allowedProtocols to '' creates a default ProtocolRestrictor", (ft) -> {
                    ReaderTestInstance t = (ReaderTestInstance) ft;
                    Source config = configFileWithFeature("global", "allowedProtocols", "");

                    Configuration readConfiguration = t.subject.makeConfiguration(config);

                    assertEquals("all", readConfiguration.getProtocolRestrictor().toString(), "protocol restrictor should be set to all");
                });
            });
        });

        @TestFactory
        Stream<DynamicNode> tests() {
            return buildTests("HE", featureConfigFileTests, f -> {
                return f.configFileDetails.section != null &&
                        f.configFileDetails.section.equals("global");
            });
        }
    }
}
