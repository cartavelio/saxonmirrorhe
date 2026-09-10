package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.ExtensionElementCreator;
import net.sf.saxon.style.XSLArray;
import net.sf.saxon.style.XSLArrayMember;
import net.sf.saxon.style.XSLItemType;

/**
 * A Factory for Saxon extension nodes in the stylesheet tree. <br>
 */

public class SaxonElementFactory {

    /**
     * Identify the class to be used for stylesheet elements with a given local name.
     */


    public final static ExtensionElementCreator FACTORY = (localname -> {
        if (localname.equals("array")) {
            return new XSLArray();
        }
        if (localname.equals("array-member")) {
            return new XSLArrayMember();
        }
        if (localname.equals("assign")) {
            return new SaxonAssign();
        }
        if (localname.equals("deep-update")) {
            return new SaxonDeepUpdate();
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
        if (localname.equals("item-type")) {
            return new XSLItemType();
        }
        if (localname.equals("tabulate-maps")) {
            return new SaxonTabulateMaps();
        }
        if (localname.equals("while")) {
            return new SaxonWhile();
        }
        return null;
    });


}

// Copyright (c) 2009-2023 Saxonica Limited
