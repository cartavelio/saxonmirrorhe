package performance;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

public class QueryGroupingPerformance extends PerformanceTestCase {

    public void measurePut(final int n) {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryExecutable exec = comp.compile(new File("/Users/mike/bugs/2024/6446_KayL/testgroupby.xquery"));
            measure(new Measurable() {

                @Override
                public void run() {
                    try {
                        DocumentBuilder builder = proc.newDocumentBuilder();
                        XdmNode doc = builder.build(new File("/Users/mike/bugs/2024/6446_KayL/testgroupby.xml"));
                        XQueryEvaluator eval = exec.load();
                        eval.setContextItem(doc);
                        XdmValue reault = eval.evaluate();
                    } catch (SaxonApiException e) {
                        throw new RuntimeException(e);
                    }
                }

                @Override
                public int expectedTimeMicroseconds() {
                    return n;
                }

                @Override
                public String name() {
                    return "testXQ" + n;
                }
            });
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    //@Tag("performance")
    public void testXQGrouping() {
        measurePut(1000000);
    }


}

