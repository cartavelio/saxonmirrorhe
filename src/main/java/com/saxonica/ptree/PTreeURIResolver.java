package com.saxonica.ptree;

import net.sf.saxon.Configuration;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import java.net.URI;
import java.net.URISyntaxException;


/**
 * This class provides the service of converting a URI into an InputSource.
 * This implementation recognizes the Saxon-EE-specific .ptree extension
 * as identifying a source file in PTree format.
 *
 * @author Michael H. Kay
 */

public class PTreeURIResolver extends StandardURIResolver {

    /**
     * Create a PTreeURIResolver, with no reference to a Configuration
     */
    public PTreeURIResolver() {
        this(null);
        recognizeQueryParameters = true;
    }

    /**
     * Create a PTreeURIResolver, with a reference to a Configuration
     *
     * @param config The Configuration object.
     *               This is used to get a SAX Parser for a source XML document
     */

    public PTreeURIResolver(Configuration config) {
        super(config);
        recognizeQueryParameters = true;
    }

    /**
     * Handle a PTree source file (Saxon-EE only)
     */

    protected Source getPTreeSource(String href, String base) throws XPathException {
        URI url;
        try {
            url = ResolveURI.makeAbsolute(href, base);
        } catch (URISyntaxException err) {
            // try to expand it relative to the current working directory
            String expandedBase = ResolveURI.tryToExpand(base);
            if (!expandedBase.equals(base)) { // prevent infinite recursion
                return resolve(href, expandedBase);
            }
            //err.printStackTrace();
            throw new XPathException("Invalid URI " + Err.wrap(href) + " - base " + Err.wrap(base), err);
        }
        PTreeSource ps = new PTreeSource();
        ps.setSystemId(url.toString());
        return ps;
    }

}

// Copyright (c) 2005 Saxonica Limited. All Rights Reserved