package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.ExtensionElementFactory;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.style.XSLResultDocument;

/**
 * A Factory for the old saxon:output extension instruction (in the old Saxon6 namespace) for the benefit
 * of people running the docbook stylesheets.
 */

public class Saxon6ElementFactory implements ExtensionElementFactory {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     * The returned class must extend net.sf.saxon.style.StyleElement
     *
     * @return null if the local name is not a recognised element type in this
     *         namespace.
     */

    /*@Nullable*/
    @Override
    public Class<? extends StyleElement> getExtensionClass(String localname) {
        if (localname.equals("output")) {
            // reinstated in 9.9 for docbook purposes
            return XSLResultDocument.class;
        }
        return null;
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
