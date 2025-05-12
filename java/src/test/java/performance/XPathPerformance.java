package performance;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class XPathPerformance extends PerformanceTestCase {

    @Test
    @Tag("performance")
    public void testAakashPerf1() {
        String path = "//premis:event\n" +
                "        [premis:eventType != 'normalize' and premis:eventType != 'migrate' ]\n" +
                "        [premis:linkingObjectIdentifier/premis:linkingObjectIdentifierValue = 'info:fda/ED5UQP1LG_4F4AKG/file/197']";
        aakashPerf(path, 400, 800);
    }

    @Test
    @Tag("performance")
    public void testAakashPerf2() {
        String path = "/mets:mets/mets:amdSec/mets:digiprovMD//premis:event\n" +
                "        [premis:eventType != 'normalize' and premis:eventType != 'migrate' ]\n" +
                "        [premis:linkingObjectIdentifier/premis:linkingObjectIdentifierValue = 'info:fda/ED5UQP1LG_4F4AKG/file/197']";
        aakashPerf(path, 250, 700);
    }

    private void aakashPerf(String path, int compileTime, int runTime) {
        try {
            Processor p = new Processor(false);
            Configuration config = p.getUnderlyingConfiguration();
            DocumentBuilder b = p.newDocumentBuilder();
            long start = System.nanoTime();
            XdmNode foo = b.build(new StreamSource(new File(configTest.getDataDir() + "aip_520.xml")));
            long buildTime = (System.nanoTime() - start) / 1000000;
            assertTrue(buildTime < 500, "buildTime " + buildTime);
            System.err.println("Build time: " + buildTime + "ms");
            XPathCompiler c = p.newXPathCompiler();
            c.declareNamespace("premis", "info:lc/xmlns/premis-v2");
            c.declareNamespace("mets", "http://www.loc.gov/METS/");

            Measurable task = new Measurable() {
                @Override
                public void run() {
                    try {
                        c.compile(path);
                    } catch (SaxonApiException saxonApiException) {
                        fail();
                    }
                }

                @Override
                public int expectedTimeMicroseconds() {
                    return compileTime;
                }

                @Override
                public String name() {
                    return "AakashPerf-Compile";
                }
            };
            measure(task);

            final XPathExecutable e = c.compile(path);
            Measurable task2 = new Measurable() {
                @Override
                public void run() {
                    try {
                        XPathSelector s = e.load();
                        s.setContextItem(foo);
                        s.evaluate().size();
                    } catch (SaxonApiException saxonApiException) {
                        fail();
                    }

                }

                @Override
                public int expectedTimeMicroseconds() {
                    return runTime;
                }

                @Override
                public String name() {
                    return "AakashPerf-Evaluation";
                }
            };
            measure(task2);

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    @Tag("performance")
    public void testXPathOptimization() {
        try {
            Processor proc = new Processor(true);
            String xml="<a><b c='1'/><b c='2'/><b c='3'/><b c='4'/><b c='5'/><b c='6'/><b c='7'/><b c='8'/><b c='9'/></a>";
            XdmNode doc = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));
            XPathCompiler xpc = proc.newXPathCompiler();
            XdmValue val1 = xpc.evaluateSingle("/a/b[@c='6']/@c!string()", doc);
            assertEquals("6", val1.itemAt(0).getStringValue());
            XPathCompiler xpc2 = proc.newXPathCompiler();
            XdmValue val2 = xpc2.evaluateSingle("/a/b[@c='2']/@c!string()", doc);
            assertEquals("2", val2.itemAt(0).getStringValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

}

