package net.sf.saxon.s9api.push;

import net.sf.saxon.s9api.SaxonApiException;

/**
 * A {@link Container} representing a document node.
 *
 * <p>If the document is constrained to be well-formed then the permitted sequence of events is
 * {@code (COMMENT | PI)* ELEMENT (COMMENT | PI)* CLOSE}.</p>
 *
 * <p>If the document is NOT constrained to be well-formed then the permitted sequence of events is
 * {@code (COMMENT | PI | TEXT | ELEMENT)* CLOSE}.</p>
 */

public interface Document extends Container {

    @Override
    Document text(CharSequence value) throws SaxonApiException;

    @Override
    Document comment(CharSequence value) throws SaxonApiException;

    @Override
    Document processingInstruction(String name, CharSequence value) throws SaxonApiException;
}

//
// The contents of this file are subject to the Mozilla Public License Version 1.0 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Saxonica Limited
//
// Portions created by (your name) are Copyright (C) (your legal entity). All Rights Reserved.
//
// Contributor(s): none.
//
