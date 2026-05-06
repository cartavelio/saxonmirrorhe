package internaltests;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import static org.junit.Assert.fail;

public class JsonParserTests {
    private static String unixInput = "{\n" +
            "  \"level1\": {\n" +
            "    \"level2A\": \"level2A value\",\n" +
            "    \"x\": \"y\",\n" +
            "    \"level2B\": {\n" +
            "      \"key1\":\"value1\",\n" +
            "      \"key1\":\"value2\"\n" +
            "    }\n" +
            "  }\n" +
            "}\n";

    private static String windowsInput = "{\r\n" +
            "  \"level1\": {\r\n" +
            "    \"level2A\": \"level2A value\",\r\n" +
            "    \"x\": \"y\",\r\n" +
            "    \"level2B\": {\r\n" +
            "      \"key1\":\"value1\",\r\n" +
            "      \"key1\":\"value2\"\r\n" +
            "    }\r\n" +
            "  }\r\n" +
            "}\r\n";

    @Test
    public void testLineNumbersUnix() {
        try {
            // Cheap and cheerful path through to the parser.
            Processor processor = new Processor(false);

            XPathCompiler compiler = processor.newXPathCompiler();
            compiler.declareVariable(new QName("a"));
            compiler.declareVariable(new QName("opts"));

            XPathSelector selector = compiler.compile("parse-json($a, $opts)").load();
            selector.setVariable(new QName("a"), new XdmAtomicValue(unixInput));

            XdmMap opts = new XdmMap();
            opts = opts.put(new XdmAtomicValue("duplicates"), new XdmAtomicValue("reject"));
            selector.setVariable(new QName("opts"), opts);

            XdmValue json = selector.evaluate();
            System.err.println(json);
        } catch (SaxonApiException e) {
            Assertions.assertNotNull(e.getMessage());
            Assertions.assertTrue(e.getMessage().contains("Invalid JSON input on line 7:"));
        }
    }

    @Test
    public void testLineNumbersWindows() {
        try {
            // Cheap and cheerful path through to the parser.
            Processor processor = new Processor(false);

            XPathCompiler compiler = processor.newXPathCompiler();
            compiler.declareVariable(new QName("a"));
            compiler.declareVariable(new QName("opts"));

            XPathSelector selector = compiler.compile("parse-json($a, $opts)").load();
            selector.setVariable(new QName("a"), new XdmAtomicValue(windowsInput));

            XdmMap opts = new XdmMap();
            opts = opts.put(new XdmAtomicValue("duplicates"), new XdmAtomicValue("reject"));
            selector.setVariable(new QName("opts"), opts);

            XdmValue json = selector.evaluate();
            System.err.println(json);
        } catch (SaxonApiException e) {
            Assertions.assertNotNull(e.getMessage());
            Assertions.assertTrue(e.getMessage().contains("Invalid JSON input on line 7:"));
        }
    }
}
