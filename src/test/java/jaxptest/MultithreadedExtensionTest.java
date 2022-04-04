package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ExtensionFunctionCall;
import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Transformer;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

import static org.junit.Assert.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * tests for handling of exceptions from extension functions while multithreading.
 */
public class MultithreadedExtensionTest {
    private final ConfigTest configTest = new ConfigTest();

    private final String bugs = configTest.getBugsDir() + "4788/";
    private final String temp = configTest.getTempDir();

    @Test
    public void testFailingMultithreadedExtension() { // Bug 4788
        int threads = java.lang.Thread.activeCount();
        try {
            for (int i = 0; i < 10; i++) {
                System.out.println("START " + i);
                try {
                    test(i > 4);
                } catch (Exception e) {
                    System.err.println("Failed (as expected): " + e.getMessage());
                }
                System.out.println("STOP " + i);
                Thread.sleep(800);
            }
        } catch (InterruptedException e) {
            fail("Interrupted");
        }
        int finalThreads = java.lang.Thread.activeCount();
        assertEquals(threads, finalThreads);
    }

    private void test(boolean fail) throws Exception {
        EnterpriseTransformerFactory transformerFactory = new EnterpriseTransformerFactory();

        Configuration configuration = Configuration.newConfiguration();
        configuration.registerExtensionFunction(new SimpleExtension(fail));

        transformerFactory.setConfiguration(configuration);

        StreamSource xslt = new StreamSource(bugs + "test.xslt");

        Transformer transformer = transformerFactory.newTransformer(xslt);

        StreamSource input = new StreamSource(bugs + "input.xml");
        StreamResult output = new StreamResult(temp + "4788output.xml");

        transformer.transform(input, output);
    }

    static class SimpleExtension extends ExtensionFunctionDefinition {

        private final boolean fail;

        public SimpleExtension(boolean fail) {
            this.fail = fail;
        }

        @Override
        public StructuredQName getFunctionQName() {
            return StructuredQName.fromClarkName("{http://test.com/extension}test");
        }

        @Override
        public SequenceType[] getArgumentTypes() {
            return new SequenceType[]{SequenceType.SINGLE_INTEGER};
        }

        @Override
        public SequenceType getResultType(SequenceType[] sequenceTypes) {
            return SequenceType.SINGLE_STRING;
        }

        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new ExtensionFunctionCall() {

                @Override
                public Sequence call(XPathContext xPathContext, Sequence[] sequences) {
                    System.out.println(Thread.currentThread().getName());
                    try {
                        Thread.sleep(500);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }

                    if (fail)
                        throw new RuntimeException("FAILED_ON_PURPOSE");
                    else
                        try {
                            return new StringValue(sequences[0].head().getStringValue());
                        } catch (XPathException e) {
                            return new StringValue("ERR: " + e.getMessage());
                        }
                }
            };
        }

    }

}
