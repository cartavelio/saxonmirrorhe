import com.saxonica.config.EnterpriseTransformerFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ExtensionFunctionCall;
import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import javax.xml.transform.Transformer;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

public class SaxonThreads {

    public static void main(String[] args) throws Exception {
        for (int i = 0; i < 10; i++) {
            System.out.println("START " + i);
            try {
                test(i > 4);
            } catch (Exception e) {
                System.out.println(e.getMessage());
            }
            System.out.println("STOP " + i);
            Thread.sleep(1500);
        }
    }

    private static void test(boolean fail) throws Exception {
        EnterpriseTransformerFactory transformerFactory = new EnterpriseTransformerFactory();

        Configuration configuration = Configuration.newConfiguration();
        configuration.registerExtensionFunction(new SimpleExtension(fail));

        transformerFactory.setConfiguration(configuration);

        StreamSource xslt = new StreamSource("test.xslt");

        Transformer transformer = transformerFactory.newTransformer(xslt);

        StreamSource input = new StreamSource("input.xml");
        StreamResult output = new StreamResult("output.xml");

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
            return new SequenceType[] { SequenceType.SINGLE_INTEGER };
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
                        Thread.sleep(1000);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }

                    if (fail)
                        throw new RuntimeException("TEST");
                    else
                        return new StringValue(sequences.toString());
                }
            };
        }

    }

}