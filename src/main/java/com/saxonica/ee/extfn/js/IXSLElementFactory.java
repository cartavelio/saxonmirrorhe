package com.saxonica.ee.extfn.js;

import com.saxonica.xsltextn.ExtensionElementCreator;

/**
 * Class IXSLElementFactory.
 * A "Factory" for IXSL extension nodes in the stylesheet tree.
 * <p>The standard namespace associated with this extension is "http://saxonica.com/ns/interactiveXSLT".
 * However, it can be registered under a different namespace if required.</p>
 */

public abstract class IXSLElementFactory {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     * The returned class must extend net.sf.saxon.style.StyleElement
     */

    public final static ExtensionElementCreator FACTORY = localname -> {
        switch (localname) {
            case "schedule-action":
                return new IXSLScheduleAction();
            case "set-attribute":
                return new IXSLSetAttribute();
            case "remove-attribute":
                return new IXSLRemoveAttribute();
            case "set-property":
                return new IXSLSetProperty();
            case "remove-property":
                return new IXSLRemoveProperty();
            case "set-style":
                return new IXSLSetStyle();
        }
        return null;
    };

}

// Copyright (c) 2018-2022 Saxonica Limited

