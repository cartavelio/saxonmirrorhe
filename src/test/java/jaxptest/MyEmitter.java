package jaxptest;

import net.sf.saxon.serialize.XMLEmitter;
import net.sf.saxon.trans.XPathException;

import java.io.IOException;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;

/**
 * User-written emitter used by SerializerTest.
 */
public class MyEmitter extends XMLEmitter {

    @Override
    public void setOutputProperties(Properties props) throws XPathException {
        assertEquals("Q{http://my.com/}jaxptest.MyEmitter", props.getProperty("method"), "method");
        assertEquals("no", props.getProperty("indent"), "indent");
        assertNull(props.getProperty("doctype-system"), "doctype-system");
    }

    @Override
    public void endDocument() throws XPathException {
        super.endDocument();
        try {
            writer.write("<!--custom-emitter-->");
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }
}
