package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.ext.EntityResolver2;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.IOException;

public class EntityResolverWrappingResourceResolver implements EntityResolver2 {

    private final ResourceResolver resourceResolver;

    public EntityResolverWrappingResourceResolver(ResourceResolver resolver) {
        this.resourceResolver = resolver;
    }

    @Override
    public InputSource getExternalSubset(String name, String baseURI) throws SAXException {
        ResourceRequest request = new ResourceRequest();
        request.baseUri = baseURI;
        request.entityName = name;
        request.nature = ResourceRequest.DTD_NATURE;
        request.purpose = "getExternalSubset";
        Source resolved;
        try {
            resolved = resourceResolver.resolve(request);
        } catch (XPathException e) {
            throw new SAXException(e);
        }
        if (resolved == null) {
            return null;
        }
        if (resolved instanceof StreamSource) {
            InputSource is = new InputSource(resolved.getSystemId());
            if (((StreamSource) resolved).getInputStream() != null) {
                is.setByteStream(((StreamSource) resolved).getInputStream());
            }
            if (((StreamSource) resolved).getReader() != null) {
                is.setCharacterStream(((StreamSource) resolved).getReader());
            }
            return is;
        }
        throw new SAXException("Unexpected Source type in getExternalSubset()");
    }

    @Override
    public InputSource resolveEntity(String name, String publicId, String baseURI, String systemId) throws SAXException, IOException {
        ResourceRequest request = new ResourceRequest();
        request.baseUri = baseURI;
        request.entityName = name;
        request.publicId = publicId;
        request.uri = systemId;
        request.nature = ResourceRequest.EXTERNAL_ENTITY_NATURE;
        request.purpose = "resolveEntity()";
        Source resolved;
        try {
            resolved = resourceResolver.resolve(request);
        } catch (XPathException e) {
            throw new SAXException(e);
        }
        if (resolved == null) {
            return null;
        }
        if (resolved instanceof StreamSource) {
            InputSource is = new InputSource(resolved.getSystemId());
            if (((StreamSource) resolved).getInputStream() != null) {
                is.setByteStream(((StreamSource) resolved).getInputStream());
            }
            if (((StreamSource) resolved).getReader() != null) {
                is.setCharacterStream(((StreamSource) resolved).getReader());
            }
            return is;
        }
        throw new SAXException("Unexpected Source type in resolveEntity()");
    }

    @Override
    public InputSource resolveEntity(String publicId, String systemId) throws SAXException, IOException {
        return resolveEntity(null, publicId, null, systemId);
    }
}

