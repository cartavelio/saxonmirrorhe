package com.saxonica.xsltextn;

import net.sf.saxon.Configuration;
import net.sf.saxon.style.StyleElement;

/**
 * Interface ExtensionElementCreator.
 * <p>A Factory for user-defined nodes in the stylesheet tree. </p>
 * <p>An instance of this class is registered with the {@link Configuration}
 * to process all extension elements in a particular namespace.</p>
 * <p>This interface is a replacement for {@link ExtensionElementFactory},
 * which remains available for the time being.</p>
 */

@FunctionalInterface
public interface ExtensionElementCreator {

    /**
     * Create a StyleElement to be used for stylesheet elements with a given local name.
     * @param localname the local name of the element
     *
     * @return null if the local name is not a recognised element type in this
     *         namespace.
     */

    StyleElement createExtensionElement(String localname);

}

// Copyright (c) 2022-2023 Saxonica Limited