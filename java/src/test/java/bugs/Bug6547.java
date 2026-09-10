package bugs;


import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.stream.StreamSource;
import java.io.File;

public class Bug6671 {

    private final static String XML = "/Users/mike/bugs/2025/6671-Uschakov/issue-to-repo/input.xml";
    private final static String XSL = "/Users/mike/bugs/2025/6671-Uschakov/issue-to-repo/sample.xsl";
    private final static String CONFIG = "/Users/mike/bugs/2025/6671-Uschakov/issue-to-repo/saxon-config.xml";


    public static void main(String[] args) throws InterruptedException {

        for (int i = 0; i < 100; i++) {
            try {
                System.err.println("Run " + i);
                Configuration config = Configuration.readConfiguration(new StreamSource(new File(CONFIG)));
                Processor proc = new Processor(config);
                proc.setConfigurationProperty(Feature.TIMING, true);
                DocumentBuilder builder = proc.newDocumentBuilder();
                XdmNode doc = builder.build(new File(XML));
                XsltCompiler compiler = proc.newXsltCompiler();
                XsltExecutable exec = compiler.compile(new StreamSource(new File(XSL)));
                Xslt30Transformer trans = exec.load30();
                trans.applyTemplates(doc.asSource());
            } catch (SaxonApiException | XPathException e) {
                throw new RuntimeException(e);
            }
        }
    }

}



