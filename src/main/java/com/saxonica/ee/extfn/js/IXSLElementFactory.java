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
    @Override
    public Class<? extends StyleElement> getExtensionClass(String localname) {
        switch (localname) {
            case "schedule-action":
                return IXSLScheduleAction.class;
            case "set-attribute":
                return IXSLSetAttribute.class;
            case "remove-attribute":
                return IXSLRemoveAttribute.class;
            case "set-property":
                return IXSLSetProperty.class;
            case "remove-property":
                return IXSLRemoveProperty.class;
            case "set-style":
                return IXSLSetStyle.class;
        }
        return null;
    }

}

// Copyright (c) 2018-2022 Saxonica Limited

