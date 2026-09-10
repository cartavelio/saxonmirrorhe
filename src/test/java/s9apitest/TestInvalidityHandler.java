package s9apitest;

import net.sf.saxon.lib.Invalidity;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.SchemaValidator;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests on s9api operations on QName class
 */
public class TestInvalidityHandler {

    /**
     * Test 3-arg constructor
     */

    @Test
    public void testHandler() {
        String INVALID_INPUT = "<element><child1/><!-- missing child --><child3/></element>";

        String SCHEMA = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                + "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n"
                + "\n"
                + "  <xs:element name=\"element\">\n"
                + "    <xs:complexType>\n"
                + "      <xs:sequence>\n"
                + "        <xs:element name=\"child1\"/>\n"
                + "        <xs:element name=\"child2\"/>\n"
                + "        <xs:element name=\"child3\"/>\n"
                + "      </xs:sequence>\n"
                + "      <xs:attribute name=\"requiredAttribute\" use=\"required\"/>\n"
                + "    </xs:complexType>\n"
                + "  </xs:element>\n"
                + "</xs:schema>\n";

        try {
            Processor processor = new Processor(true);
            processor.getSchemaManager().load(new StreamSource(new StringReader(SCHEMA)));

            processor.getUnderlyingConfiguration().setLogger(new Logger() {
                @Override
                public void println(String message, int severity) {
                    fail();
                }
            });

            SchemaValidator validator = processor.getSchemaManager().newSchemaValidator();
            validator.setInvalidityHandler(new CustomInvalidityHandler());

            validator.validate(new StreamSource(new StringReader(INVALID_INPUT)));
        } catch (SaxonApiException e) {
            // Ignore
        }

    }

    private static class CustomInvalidityHandler implements net.sf.saxon.lib.InvalidityHandler {

        public int count;
        @Override
        public void startReporting(String s) throws XPathException {
        }

        @Override
        public void reportInvalidity(Invalidity invalidity) throws XPathException {
            count++;
        }

        @Override
        public Sequence endReporting() throws XPathException {
            return null;
        }
    }
}
