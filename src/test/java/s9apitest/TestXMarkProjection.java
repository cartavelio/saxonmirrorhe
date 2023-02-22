package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.IOException;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test the XMark queries with and without document projection, comparing the results
 */
public class TestXMarkProjection {
    private final ConfigTest configTest = new ConfigTest();

    private void doTest(String query) {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, true);
            //proc.setConfigurationProperty(Feature.DEBUG_BYTE_CODE, true);
            //proc.setConfigurationProperty(Feature.DISPLAY_BYTE_CODE, true);
            //proc.setConfigurationProperty(Feature.DEBUG_BYTE_CODE_DIR, ConfigTest.TEMP_DIR);
            File queryFile = new File(configTest.getDataDir() + "../xmark/" + query + ".xq");
            File source = new File(configTest.getDataDir() + "../xmark/xmark4.xml");
            DocumentBuilder b1 = proc.newDocumentBuilder();
            XdmNode fullDoc = b1.build(source);
            XQueryCompiler c1 = proc.newXQueryCompiler();
            XQueryExecutable e1 = c1.compile(queryFile);
            XQueryEvaluator ev1 = e1.load();
            ev1.setContextItem(fullDoc);
            XdmValue v1 = ev1.evaluate();

            b1.setDocumentProjectionQuery(e1);
            XdmNode projectedDoc = b1.build(source);
            ev1.setContextItem(projectedDoc);
            XdmValue v2 = ev1.evaluate();

            Serializer s = proc.newSerializer();
            assertEquals(serialize(s, v1), serialize(s, v2), "same result");
        } catch (SaxonApiException | IOException e) {
            e.printStackTrace();
            fail();
        }


    }

    private static String serialize(Serializer s, XdmValue v) throws SaxonApiException {
        StringWriter sw = new StringWriter();
        s.setOutputWriter(sw);
        s.serializeXdmValue(v);
        return s.toString();
    }

    @Test
    @Tag("bytecode")
    public void testQ1() {
        doTest("q1");
    }

    @Test
    @Tag("bytecode")
    public void testQ2() {
        doTest("q2");
    }

    @Test
    @Tag("bytecode")
    public void testQ3() {
        doTest("q3");
    }

    @Test
    @Tag("bytecode")
    public void testQ4() {
        doTest("q4");
    }

    @Test
    @Tag("bytecode")
    public void testQ5() {
        doTest("q5");
    }

    @Test
    @Tag("bytecode")
    public void testQ6() {
        doTest("q6");
    }

    @Test
    @Tag("bytecode")
    public void testQ7() {
        doTest("q7");
    }

    @Test
    @Tag("bytecode")
    public void testQ8() {
        doTest("q8");
    }

    @Test
    @Tag("bytecode")
    public void testQ9() {
        doTest("q9");
    }

    @Test
    @Tag("bytecode")
    public void testQ10() {
        doTest("q10");
    }

    @Test
    @Tag("bytecode")
    public void testQ11() {
        doTest("q11");
    }

    @Test
    @Tag("bytecode")
    public void testQ12() {
        doTest("q12");
    }

    @Test
    @Tag("bytecode")
    public void testQ13() {
        doTest("q13");
    }

    @Test
    @Tag("bytecode")
    public void testQ14() {
        doTest("q14");
    }

    @Test
    @Tag("bytecode")
    public void testQ15() {
        doTest("q15");
    }

    @Test
    @Tag("bytecode")
    public void testQ16() {
        doTest("q16");
    }

    @Test
    @Tag("bytecode")
    public void testQ17() {
        doTest("q17");
    }

    @Test
    @Tag("bytecode")
    public void testQ18() {
        doTest("q18");
    }

    @Test
    @Tag("bytecode")
    public void testQ19() {
        doTest("q19");
    }

    @Test
    @Tag("bytecode")
    public void testQ20() {
        doTest("q20");
    }
}

