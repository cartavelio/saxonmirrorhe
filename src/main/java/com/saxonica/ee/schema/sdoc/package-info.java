package com.saxonica.ee.schema.sdoc;

/**
 * Internal classes supporting the XDM tree representation of an XSD schema document. The various classes
 * such as {@link com.saxonica.ee.schema.sdoc.XSDAttributeGroup} correspond to similarly-named elements
 * in an XSD schema document, such as in this case <code>xs:attribute-group</code>. There are some
 * differences, for example <code>xs:restriction</code> elements map to different classes depending
 * on their parentage.
 */

// Copyright (c) 2010-2023 Saxonica Limited.