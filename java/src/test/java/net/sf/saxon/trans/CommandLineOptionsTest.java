package net.sf.saxon.trans;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.Processor;
import org.junit.jupiter.api.*;

import java.util.Map;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.*;

class CommandLineOptionsTest {
    class OptionsTestInstance extends FeatureTest.FeatureTestInstance {
        public Processor processor;
        public Configuration configuration;
        public CommandLineOptions subject;

        OptionsTestInstance() {
            processor = new Processor(false);
            configuration = processor.getUnderlyingConfiguration();
            subject = new CommandLineOptions();
        }
    }

    @Nested
    @DisplayName("Test --featureName:value options common to Query and Transform")
    class CommonFeatures extends ConfigurationFeatureTesting {
        public String[] actualOptions(String option) {
            return new String[]{option};
        }

        public Map<String, FeatureTest> featureCommandLineTests = FeatureTest.tests(OptionsTestInstance::new, features -> {
            features.add(FeatureKeys.ALLOWED_PROTOCOLS, tests -> {
                tests.add("Setting allowedProtocols to #none creates the right ProtocolRestrictor", ft -> {
                    OptionsTestInstance t = (OptionsTestInstance) ft;
                    t.subject.setActualOptions(actualOptions("--allowedProtocols:#none"));

                    t.subject.applyToConfiguration(t.processor);

                    assertEquals("", t.configuration.getProtocolRestrictor().toString());
                });

                tests.add("Setting allowedProtocols to '' creates a default ProtocolRestrictor", ft -> {
                    OptionsTestInstance t = (OptionsTestInstance) ft;
                    t.subject.setActualOptions(actualOptions("--allowedProtocols:"));

                    t.subject.applyToConfiguration(t.processor);

                    assertEquals("all", t.configuration.getProtocolRestrictor().toString());
                });
            });
        });

        @TestFactory
        Stream<DynamicNode> tests() {
            return buildTests("HE", featureCommandLineTests);
        }
    }
}
