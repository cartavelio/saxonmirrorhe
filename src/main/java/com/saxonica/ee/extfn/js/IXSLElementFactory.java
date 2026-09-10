package com.saxonica.ee.extfn.js;

import com.saxonica.xsltextn.ExtensionElementFactory;
import net.sf.saxon.style.StyleElement;

/**
 * Class IXSLElementFactory.
 * A "Factory" for IXSL extension nodes in the stylesheet tree.
 * <p>The standard namespace associated with this extension is "http://saxonica.com/ns/interactiveXSLT".
 * However, it can be registered under a different namespace if required.</p>
 */

public class IXSLElementFactory implements ExtensionElementFactory {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     * The returned class must extend net.sf.saxon.style.StyleElement
     *
     * @return null if the local name is not a recognised element type in this
     *         namespace.
     */

    /*@Nullable*/
    public Class<? extends StyleElement> getExtensionClass(String localname) {
        if (localname.equals("schedule-action")) {
            return IXSLScheduleAction.class;
        } else if (localname.equals("set-attribute")) {
            return IXSLSetAttribute.class;
        } else if (localname.equals("remove-attribute")) {
            return IXSLRemoveAttribute.class;
        } else if (localname.equals("set-property")) {
            return IXSLSetProperty.class;
        } else if (localname.equals("set-style")) {
            return IXSLSetStyle.class;
        }
        return null;
    }

}

// Copyright (c) 2018 Saxonica Limited

