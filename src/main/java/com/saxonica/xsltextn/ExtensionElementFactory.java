package com.saxonica.xsltextn;

import net.sf.saxon.style.StyleElement;

/**
 * Interface ExtensionElementFactory.
 * <p>A "Factory" for user-defined nodes in the stylesheet tree. </p>
 * <p>(Actually, it's not strictly a factory: it doesn't create the nodes,
 * it merely identifies what class they should be.)</p>
 */

public interface ExtensionElementFactory {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     * The returned class must extend {@link net.sf.saxon.style.StyleElement}, and it must have a
     * zero-argument public constructor.
     * @param localname the local name of the element
     *
     * @return null if the local name is not a recognised element type in this
     *         namespace.
     */

    Class<? extends StyleElement> getExtensionClass(String localname);

}

// Copyright (c) 2009-2022 Saxonica Limited