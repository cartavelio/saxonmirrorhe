package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.IOException;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test XSD processing
 */
public class TestSchemaProcessing {

    private ConfigTest configTest = new ConfigTest();

    @Test
    public void testBug6050() {
        try {
            Processor processor = new Processor(true);
            SchemaManager mgr = processor.getSchemaManager();
            mgr.load(new File(configTest.getDataDir() + "bug6050/person.xsd"));
            //mgr.load(new File("src/test/java/bugs/bug6050/simple.xsd"));

            XQueryCompiler compiler = processor.newXQueryCompiler();
            XQueryExecutable exec = compiler.compile(new File(configTest.getDataDir() + "bug6050/common.xq"));
            XQueryEvaluator eval = exec.load();
            eval.setExternalVariable(new QName("phoneNumber"), new XdmAtomicValue("1189482766"));
            StringWriter sw = new StringWriter();
            eval.setDestination(processor.newSerializer(sw));
            eval.run();
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?>1189482766", sw.toString());
        } catch (SaxonApiException | IOException e) {
            fail(e.getMessage());
        }
    }

}

