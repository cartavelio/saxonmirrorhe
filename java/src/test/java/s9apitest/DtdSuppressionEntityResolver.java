package s9apitest;

import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.ext.EntityResolver2;

import java.io.IOException;
import java.io.StringReader;

public class DtdSuppressionEntityResolver implements EntityResolver2 {

    @Override
    public InputSource getExternalSubset(String name, String baseURI) throws SAXException, IOException {
        return new InputSource(new StringReader(""));
    }

    @Override
    public InputSource resolveEntity(String name, String publicId, String baseURI, String systemId) throws SAXException, IOException {
        return new InputSource(new StringReader(""));
    }

    @Override
    public InputSource resolveEntity(String publicId, String systemId) throws SAXException, IOException {
        return new InputSource(new StringReader(""));
    }
}

