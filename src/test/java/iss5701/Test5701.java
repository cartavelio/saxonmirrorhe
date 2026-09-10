package iss5701;

import net.sf.saxon.s9api.*;
import net.sf.saxon.value.StringValue;
import org.junit.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

public class Test5701 {
    @Test
    public void testTransformXsl() {
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

            transformer.transform();
            XdmValue value = result.getXdmValue();
            System.err.println(value.toString());
            assertTrue(value.toString().indexOf("Hello") > 0);
        } catch (Exception ex) {
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
