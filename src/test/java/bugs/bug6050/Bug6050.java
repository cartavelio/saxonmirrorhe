package bugs.bug6024;

// This is a main class so that we can run it with a completely different classpath.
// If there's a clever way to run a single unit test with a different classpath,
// I wasn't able to find it.

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;

import java.io.File;

public class Bug6024 {
    public static void main(String[] args) throws SaxonApiException {
        Bug6024 testcase = new Bug6024();
        testcase.run();
    }

    private void run() throws SaxonApiException {
        Processor processor = new Processor(true);
        Configuration config = processor.getUnderlyingConfiguration();
        QName initialTemplateName = new QName("xsl", NamespaceConstant.XSLT, "initial-template");

        XsltCompiler compiler = processor.newXsltCompiler();
        XsltExecutable exec = compiler.compile(new File("src/test/java/bugs/bug6024/batch.xslt"));
        XsltTransformer transformer = exec.load();
        transformer.setInitialTemplate(initialTemplateName);

        XdmDestination destination = new XdmDestination();
        transformer.setDestination(destination);
        transformer.transform();
        String result = destination.getXdmNode().getStringValue();

        // If we got this far, result won't be null, but just to have some use of result...
        if (result == null) {
            throw new RuntimeException("Null result from bug6024?");
        }
    }
}
