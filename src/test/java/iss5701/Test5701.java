package iss5701;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Paths;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class Test5701 {
    private final ConfigTest config = new ConfigTest();
    @Test
    public void testTransformXsl() {
        Processor processor = new Processor(false);
        processor.registerExtensionFunction(new HelloWorld());

        try {
            RawDestination result = new RawDestination();
            XsltCompiler compiler = processor.newXsltCompiler();
            InputStream stylef = Files.newInputStream(new File("src/test/java/iss5701/driver.xsl").toPath());
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylef)));
            XsltTransformer transformer = exec.load();
            transformer.setDestination(result);

            InputStream docf = Files.newInputStream(new File("src/test/java/iss5701/input.xml").toPath());
            transformer.setSource(new SAXSource(new InputSource(docf)));

            transformer.transform();
            XdmValue value = result.getXdmValue();
            System.err.println(value.toString());
            assertTrue(value.toString().indexOf("Hello") > 0);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test //- reinstate this to regenerate the SEF file
    public void exportSef() {
        Processor processor = new Processor(true);
        processor.registerExtensionFunction(new HelloWorld());

        try {
            XsltCompiler compiler = processor.newXsltCompiler();
            InputStream stylef = Files.newInputStream(Paths.get("src/test/java/iss5701/style.xsl"));
            compiler.setJustInTimeCompilation(false);
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylef)));
            exec.export(Files.newOutputStream(Paths.get(config.getTempDir() + "style12.sef.xml")));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTransformSef() {
        Processor processor = new Processor(false);
        processor.registerExtensionFunction(new HelloWorld());

        try {
            RawDestination result = new RawDestination();
            XsltCompiler compiler = processor.newXsltCompiler();
            InputStream stylef = new FileInputStream(new File("src/test/java/iss5701/driver.xsl"));
            XsltExecutable exec = compiler.compile(new SAXSource(new InputSource(stylef)));
            XsltTransformer transformer = exec.load();
            transformer.setDestination(result);

            InputStream docf = new FileInputStream(new File("src/test/java/iss5701/input.xml"));
            transformer.setSource(new SAXSource(new InputSource(docf)));

            transformer.setParameter(new QName("use-sef"), new XdmAtomicValue("true", ItemType.STRING));

            transformer.transform();
            XdmValue value = result.getXdmValue();
            System.err.println(value.toString());
            assertTrue(value.toString().indexOf("Hello") > 0);
        } catch (Exception ex) {
            fail();
        }
    }
}
