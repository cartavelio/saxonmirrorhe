package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.ExtensionElementCreator;
import net.sf.saxon.style.XSLResultDocument;

/**
 * A Factory for the old saxon:output extension instruction (in the old Saxon6 namespace) for the benefit
 * of people running the docbook stylesheets.
 */

public abstract class Saxon6ElementFactory {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     * The returned class must extend net.sf.saxon.style.StyleElement
     */

    public final static ExtensionElementCreator FACTORY = (localName -> {
        if (localName.equals("output")) {
            return new XSLResultDocument();
        }
        return null;
    });

}

// Copyright (c) 2018-2022 Saxonica Limited
