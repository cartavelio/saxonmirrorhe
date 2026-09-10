package jaxptest;

import net.sf.saxon.serialize.MessageEmitter;
import net.sf.saxon.trans.XPathException;

import java.io.StringWriter;

/**
 * Created by mike on 07/01/2015.
 */
public class MyMessageEmitter extends MessageEmitter {

    public MyMessageEmitter() throws XPathException {
    }

    public void setStringWriter(StringWriter sw) throws XPathException {
        setWriter(sw);
    }

}

