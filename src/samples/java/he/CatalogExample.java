package he;

import net.sf.saxon.Configuration;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmDestination;
import net.sf.saxon.s9api.Xslt30Transformer;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import org.xml.sax.InputSource;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.sax.SAXSource;
import java.util.Collections;

public class CatalogExample {
    public static void main(String[] argv) throws Exception {
        Processor processor = new Processor(false);
        Configuration config = processor.getUnderlyingConfiguration();

        String cwd = System.getProperty("user.dir");
        String datapath = cwd + "/src/test/testdata/resolvers";

        String catalog = datapath + "/catalog.xml";

        config.getResourceResolver().setFeature(ResolverFeature.CATALOG_FILES, Collections.singletonList(catalog));
        System.err.println(config.getResourceResolver().getFeature(ResolverFeature.CATALOG_FILES));

        InputSource docsrc = new InputSource(datapath + "/input.xml");
        InputSource xslsrc = new InputSource(datapath + "/stylesheet.xsl");

        XsltCompiler compiler = processor.newXsltCompiler();
        XsltExecutable exec = compiler.compile(new SAXSource(xslsrc));
        Xslt30Transformer transformer = exec.load30();

        XdmDestination destination = new XdmDestination();

        transformer.transform(new SAXSource(docsrc), destination);

        System.err.println(destination.getXdmNode());
    }
}