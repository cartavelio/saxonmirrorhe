package com.saxonica.ptree;

import net.sf.saxon.Configuration;
import net.sf.saxon.om.GenericTreeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import java.io.BufferedInputStream;
import java.io.DataInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;

/**
 * A JAXP Source that maps to an XML document represented as a PTree on disk.
 */

public class PTreeSource implements Source {

    private String systemId;

    /**
     * Get the system identifier that was set with setSystemId.
     *
     * @return The system identifier that was set with setSystemId, or null
     *         if setSystemId was not called.
     */
    public String getSystemId() {
        return systemId;
    }

    /**
     * Set the system identifier for this Source.
     * <p>The system identifier is optional if the source does not
     * get its data from a URL, but it may still be useful to provide one.
     * The application can use a system identifier, for example, to resolve
     * relative URIs and to include in error messages and warnings.</p>
     *
     * @param systemId The system identifier as a URL string.
     */
    public void setSystemId(String systemId) {
        this.systemId = systemId;
    }

    /**
     * Read the PTree and return the DocumentInfo representing the root node of the resulting
     * in-memory tree
     */

    public TreeInfo load(Configuration config) throws XPathException {
        try {
            InputStream in = new URL(systemId).openStream();
            DataInputStream din = new DataInputStream(
                    new BufferedInputStream(in));

            TreeInfo doc = new PTreeReader().readTree(din, systemId, config);
            ((GenericTreeInfo)doc).setSystemId(systemId);
            return doc;
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }
}

// Copyright (c) 2005 Saxonica Limited.
