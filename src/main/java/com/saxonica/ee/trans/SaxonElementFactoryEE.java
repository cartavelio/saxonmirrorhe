package com.saxonica.ee.trans;

import com.saxonica.ee.xtupdate.*;
import com.saxonica.xsltextn.style.*;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.style.XSLItemType;

/**
 * A Factory for Saxon extension nodes in the stylesheet tree. <br>
 */

public class SaxonElementFactoryEE extends SaxonElementFactory {

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
            return SaxonArray.class;
        }
        if (localname.equals("array-member")) {
            return SaxonArrayMemberInst.class;
        }
        if (localname.equals("assign")) {
            return SaxonAssign.class;
        }
        if (localname.equals("change")) {
            return SaxonChange.class;
        }
        if (localname.equals("deep-update")) {
            return SaxonDeepUpdate.class;
        }
        if (localname.equals("delete")) {
            return SaxonDelete.class;
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
        if (localname.equals("for-each-member")) {
            return SaxonForEachMember.class;
        }
        if (localname.equals("import-query")) {
            return SaxonImportQuery.class;
        }
        if (localname.equals("insert")) {
            return SaxonInsert.class;
        }
        if (localname.equals("item-type")) {
            return XSLItemType.class;
        }
        if (localname.equals("rename")) {
            return SaxonRename.class;
        }
        if (localname.equals("replace")) {
            return SaxonReplace.class;
        }
        if (localname.equals("tabulate-maps")) {
            return SaxonTabulateMaps.class;
        }

        if (localname.equals("update")) {
            return SaxonUpdate.class;
        }
        if (localname.equals("while")) {
            return SaxonWhile.class;
        }
        return null;
    }

}

// Copyright (c) 2009-2020 Saxonica Limited
