package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.StandardModuleURIResolver;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XQueryCompiler;
import net.sf.saxon.s9api.XQueryExecutable;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for XQuery Module URI resolver
 */
public class TestModuleURIResolver {

    @Test
    public void testModuleResolver() {

        // Tests that the two imports of module2 resolve to the same module. If the module is loaded twice,
        // a spurious error occurs about duplicate global variables. Bug report from oXygen, 14 Oct 2011.

        try {
            Processor processor = new Processor(true);
            XQueryCompiler compiler = processor.newXQueryCompiler();
            compiler.setModuleURIResolver(new CustomModuleResolver(processor.getUnderlyingConfiguration()));
            XQueryExecutable exec = compiler.compile(new File(ConfigTest.DATA_DIR + "xq_modules_problem/Process.xquery"));
            exec.load().evaluate();
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        } catch (IOException e) {
            fail(e.getMessage());
        }

    }

    public class CustomModuleResolver extends StandardModuleURIResolver {

        public CustomModuleResolver(Configuration config) {
            super(config);
        }

        /**
         * @see net.sf.saxon.lib.StandardModuleURIResolver#getQuerySource(java.net.URI)
         */
        @Override
        protected StreamSource getQuerySource(URI absoluteURI) throws XPathException {
            if (absoluteURI.toString().startsWith("http://ldueck.librarytest/")) {
                String replace = absoluteURI.toString().replace("http://ldueck.librarytest/", ConfigTest.DATA_DIR + "xq_modules_problem/");
                System.out.println("Load from " + replace);
                return super.getQuerySource(new File(replace).toURI());
            }
            return super.getQuerySource(absoluteURI);
        }

    }

}

