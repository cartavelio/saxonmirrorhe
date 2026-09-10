package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.ExtensionElementFactory;
import net.sf.saxon.style.*;

/**
 * A Factory for Saxon extension nodes in the stylesheet tree. <br>
 */

public class SaxonElementFactory implements ExtensionElementFactory {

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
        if (localname.equals("array")) {
            return XSLArray.class;
        }
        if (localname.equals("array-member")) {
            return XSLArrayMember.class;
        }
        if (localname.equals("assign")) {
            return SaxonAssign.class;
        }
        if (localname.equals("deep-update")) {
            return SaxonDeepUpdate.class;
        }
        if (localname.equals("do")) {
            return SaxonDo.class;
        }
        if (localname.equals("doctype")) {
            return SaxonDoctype.class;
        }
        if (localname.equals("entity-ref")) {
            return SaxonEntityRef.class;
        }
        if (localname.equals("import-query")) {
            return SaxonImportQuery.class;
        }
        if (localname.equals("item-type")) {
            return XSLItemType.class;
        }
        if (localname.equals("tabulate-maps")) {
            return SaxonTabulateMaps.class;
        }
        if (localname.equals("while")) {
            return SaxonWhile.class;
        }
        return null;
    }

}

// Copyright (c) 2009-2022 Saxonica Limited
