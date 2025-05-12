package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.IOException;
import java.io.InputStream;

import static org.junit.jupiter.api.Assertions.fail;

public class ThreadCountTest {
    private Processor saxonProcessor;
    private XQueryCompiler xqueryCompiler;

    // Bug 2549

    @Test
    public void testOpenThreads() throws Exception {
        EnterpriseConfiguration configuration = new EnterpriseConfiguration();
        saxonProcessor = new Processor(configuration);
        xqueryCompiler = saxonProcessor.newXQueryCompiler();
        int threadCount0 = threadCount();

        for (int i = 0; i < 100; ++i) {
            XQueryExecutable xqueryExecutable = xqueryCompiler.compile("saxon:stream(doc('uriresolver:resolve')/*/*)");
            XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
            xqueryEvaluator.setURIResolver(new StandardURIResolver() {
                @Override
                public Source resolve(String href, String base) throws XPathException {
                    return new StreamSource(strawberryFieldsForever());
                }
            });
            XdmSequenceIterator<?> xdmSequenceIterator = xqueryEvaluator.iterator();
            xdmSequenceIterator.next();
            xdmSequenceIterator.close();
            xqueryEvaluator.close();
        }

        String msg = null;
        for (int j = 0; j < 10; ++j) {
            msg = "number of threads increased from " + threadCount0 + " to " + threadCount();
            System.err.println(msg);
            if (threadCount() <= threadCount0 + 10)
                return;
            Thread.sleep(1000);
        }
        fail(msg);
    }

    private static int threadCount() {
        return Thread.getAllStackTraces().keySet().size();
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
