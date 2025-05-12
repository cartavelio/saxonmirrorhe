package bugs.bug5388;

import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmDestination;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.FileInputStream;
import java.io.InputStream;

public class Bug5388 {
    @Test
    public void bug() {
        try {
            Processor processor = new Processor(false);
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode document = builder.build(new SAXSource(new InputSource("src/test/java/bugs/bug5388/OUTPUT.xml")));

            InputStream stylesheet = new FileInputStream("src/test/java/bugs/bug5388/forest2dot.xsl");
            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(false);
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylesheet)));
            XsltTransformer transformer = exec.load();
            transformer.setInitialContextNode(document);
            XdmDestination destination = new XdmDestination();
            transformer.setDestination(destination);
            transformer.transform();

            XdmNode text = destination.getXdmNode();
            String dot = text.getStringValue();
            Assertions.assertFalse(dot.contains("\0\0\0\0"));
        } catch (Exception ex) {
            Assertions.fail();
        }
    }
}
