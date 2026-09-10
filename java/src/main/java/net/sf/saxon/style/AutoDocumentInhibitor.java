package net.sf.saxon.style;

import net.sf.saxon.type.ItemType;

/**
 * A marker interface implemented by XSLT instructions, which, if they appear in a sequence
 * constructor under 4.0, inhibit the automatic creation of a document node by a variable
 * binding element
 */

public interface AutoDocumentInhibitor {

    /**
     * Get the type of the items returned by this instruction
     * @return the static item type
     */
    ItemType getItemType();
}

