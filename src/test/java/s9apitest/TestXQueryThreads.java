package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.lib.StandardErrorReporter;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.IOException;
import java.io.InputStream;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.fail;

/**
 * Created by mike on 23/02/2016.
 */
public class TestXQueryThreads {
    @Test
    public void testForOpenThreads() throws Exception {
        EnterpriseConfiguration configuration = new EnterpriseConfiguration();
        Processor saxonProcessor = new Processor(configuration);
        XQueryCompiler xqueryCompiler = saxonProcessor.newXQueryCompiler();
        final int testCount = 100;

        Set<Thread> threads0 = threads();
        Set<Thread> threads1 = null;

        for (int i = 0; i < testCount; ++i) {
            XQueryExecutable xqueryExecutable = xqueryCompiler.compile("saxon:stream(doc('uriresolver:resolve')/*/*)");
            XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
            xqueryEvaluator.setResourceResolver(request -> new StreamSource(strawberryFieldsForever()));
            StandardErrorReporter reporter = new StandardErrorReporter();
            reporter.suppressWarning("SXQP0001");
            xqueryEvaluator.setErrorReporter(reporter);
            XdmSequenceIterator<?> xdmSequenceIterator = xqueryEvaluator.iterator();
            xdmSequenceIterator.next();

            Thread.sleep(100); // wait some time before closing

            xdmSequenceIterator.close();
            xqueryEvaluator.close();
        }

        Thread.sleep(10000);

        for (int j = 0; j < 10; ++j) {
            threads1 = threads();
            if (threads0.containsAll(threads1))
                return;
            Thread.sleep(1000);
        }

        String msg = "Number of threads increased from " + threads0.size() + " to " + threads1.size();
        System.err.println(msg);
        for (Thread thread : threads1) {
            if (!threads0.contains(thread)) {
                System.err.println("  " + thread.getName());
                for (StackTraceElement stackTraceElement : thread.getStackTrace())
                    System.err.println("    " + stackTraceElement);
            }
        }

        fail(msg);
    }

    private static Set<Thread> threads() {
        return Thread.getAllStackTraces().keySet();
    }

    private static InputStream strawberryFieldsForever() {
        return new InputStream() {
            private static final String intro =
                    "<song>\n";
            private static final String chorus =
                    "  <chorus>\n" +
                            "    <line>Let me take you down</line>\n" +
                            "    <line>Cause I'm going to Strawberry Fields</line>\n" +
                            "    <line>Nothing is real</line>\n" +
                            "    <line>And nothing to get hung about</line>\n" +
                            "    <line>Strawberry Fields forever</line>\n" +
                            "  </chorus>\n";
            private int i = -intro.length();

            @Override
            public int read() throws IOException {
                if (i++ < 0) {
                    return intro.charAt(i + intro.length() - 1);
                } else {
                    i %= chorus.length();
                    return chorus.charAt(i);
                }
            }
        };
    }
}

