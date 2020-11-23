package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;

import java.io.File;
import java.io.IOException;
import java.io.StringWriter;

/**
 * Test the XMark queries with and without document projection, comparing the results
 */
public class TestXMarkProjection extends TestCase {

    private void doTest(String query) {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);
            File queryFile = new File(ConfigTest.DATA_DIR + "../../xmark/" + query + ".xq");
            File source = new File(ConfigTest.DATA_DIR + "../../xmark/xmark4.xml");
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
            assertEquals("same result", serialize(s, v1), serialize(s, v2));
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

    public void testQ1() {
        doTest("q1");
    }

    public void testQ2() {
        doTest("q2");
    }

    public void testQ3() {
        doTest("q3");
    }

    public void testQ4() {
        doTest("q4");
    }

    public void testQ5() {
        doTest("q5");
    }

    public void testQ6() {
        doTest("q6");
    }

    public void testQ7() {
        doTest("q7");
    }

    public void testQ8() {
        doTest("q8");
    }

    public void testQ9() {
        doTest("q9");
    }

    public void testQ10() {
        doTest("q10");
    }

    public void testQ11() {
        doTest("q11");
    }

    public void testQ12() {
        doTest("q12");
    }

    public void testQ13() {
        doTest("q13");
    }

    public void testQ14() {
        doTest("q14");
    }

    public void testQ15() {
        doTest("q15");
    }

    public void testQ16() {
        doTest("q16");
    }

    public void testQ17() {
        doTest("q17");
    }

    public void testQ18() {
        doTest("q18");
    }

    public void testQ19() {
        doTest("q19");
    }

    public void testQ20() {
        doTest("q20");
    }
}

