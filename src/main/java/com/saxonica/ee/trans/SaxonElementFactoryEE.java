package com.saxonica.ee.trans;

import com.saxonica.ee.xtupdate.*;
import com.saxonica.xsltextn.ExtensionElementCreator;
import com.saxonica.xsltextn.style.*;
import net.sf.saxon.style.*;

/**
 * A Factory for Saxon extension nodes in the stylesheet tree. <br>
 */

public abstract class SaxonElementFactoryEE {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     * The returned class must extend net.sf.saxon.style.StyleElement
     */

    public final static ExtensionElementCreator FACTORY = localname -> {
        if (localname.equals("array")) {
            return new XSLArray();
        }
        if (localname.equals("array-member")) {
            return new XSLArrayMember();
        }
        if (localname.equals("assign")) {
            return new SaxonAssign();
        }
        if (localname.equals("change")) {
            return new SaxonChange();
        }
        if (localname.equals("deep-update")) {
            return new SaxonDeepUpdate();
        }
        if (localname.equals("delete")) {
            return new SaxonDelete();
        }
        if (localname.equals("do")) {
            return new SaxonDo();
        }
        if (localname.equals("doctype")) {
            return new SaxonDoctype();
        }
        if (localname.equals("entity-ref")) {
            return new SaxonEntityRef();
        }
        if (localname.equals("import-query")) {
            return new SaxonImportQuery();
        }
        if (localname.equals("insert")) {
            return new SaxonInsert();
        }
        if (localname.equals("item-type")) {
            return new net.sf.saxon.style.XSLItemType();
        }
        if (localname.equals("rename")) {
            return new SaxonRename();
        }
        if (localname.equals("replace")) {
            return new SaxonReplace();
        }
        if (localname.equals("tabulate-maps")) {
            return new SaxonTabulateMaps();
        }
        if (localname.equals("update")) {
            return new SaxonUpdate();
        }
        if (localname.equals("while")) {
            return new SaxonWhile();
        }
        return SaxonElementFactory.FACTORY.createExtensionElement(localname);
    };

}

// Copyright (c) 2009-2022 Saxonica Limited
