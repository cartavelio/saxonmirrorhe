package bugs.bug6117;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.util.HashMap;

public class Bug6117Test {
    @Test
    public void fails_XsltTransformer() {
        try {
            Processor processor = new Processor(true);

            String stylesheet = "src/test/java/bugs/bug6117/forest2dot.xsl";
            String forest = "src/test/java/bugs/bug6117/forest.xml";

            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode document = builder.build(new SAXSource(new InputSource(forest)));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(false);
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylesheet)));
            XsltTransformer transformer = exec.load();
            transformer.setInitialContextNode(document);
            XdmDestination destination = new XdmDestination();
            transformer.setDestination(destination);

            HashMap<String,String> styleopts = new HashMap<>();
            styleopts.put("show-priority", "true");
            styleopts.put("selected-root", "date");
            styleopts.put("selected-depth", "2");

            for (String key : styleopts.keySet()) {
                XdmValue value = new XdmAtomicValue(styleopts.get(key));
                QName name = new QName(key);
                transformer.setParameter(name, value);
            }

            transformer.transform();

            System.err.println(destination.getXdmNode().getStringValue());

        } catch (Exception ex) {
            Assertions.fail();
        }
    }

    @Test
    public void fails_Xslt30Transformer() {
        try {
            Processor processor = new Processor(true);

            String stylesheet = "src/test/java/bugs/bug6117/forest2dot.xsl";
            String forest = "src/test/java/bugs/bug6117/forest.xml";

            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode document = builder.build(new SAXSource(new InputSource(forest)));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(false);
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylesheet)));
            Xslt30Transformer transformer = exec.load30();
            XdmDestination destination = new XdmDestination();

            HashMap<QName,XdmAtomicValue> styleopts = new HashMap<>();
            styleopts.put(new QName("show-priority"), new XdmAtomicValue("true"));
            styleopts.put(new QName("selected-root"), new XdmAtomicValue("date"));
            styleopts.put(new QName("selected-depth"), new XdmAtomicValue("2"));
            transformer.setStylesheetParameters(styleopts);

            transformer.transform(document.asSource(), destination);

            System.err.println(destination.getXdmNode().getStringValue());

        } catch (Exception ex) {
            Assertions.fail();
        }
    }

    @Test
    public void passes_Xslt30Transformer() {
        try {
            Processor processor = new Processor(true);

            String stylesheet = "src/test/java/bugs/bug6117/forest2dot.xsl";
            String forest = "src/test/java/bugs/bug6117/forest.xml";

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(false);
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylesheet)));
            Xslt30Transformer transformer = exec.load30();
            XdmDestination destination = new XdmDestination();

            HashMap<QName,XdmAtomicValue> styleopts = new HashMap<>();
            styleopts.put(new QName("show-priority"), new XdmAtomicValue("true"));
            styleopts.put(new QName("selected-root"), new XdmAtomicValue("date"));
            styleopts.put(new QName("selected-depth"), new XdmAtomicValue("2"));
            transformer.setStylesheetParameters(styleopts);

            transformer.transform(new SAXSource(new InputSource(forest)), destination);

            System.err.println("pass");
            //System.err.println(destination.getXdmNode().getStringValue());

        } catch (Exception ex) {
            Assertions.fail();
        }
    }

}
