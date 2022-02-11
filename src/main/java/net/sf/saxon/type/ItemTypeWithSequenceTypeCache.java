package net.sf.saxon.type;

import net.sf.saxon.value.SequenceType;

/**
 * Extension of the ItemType interface implemented by some item types, to provide
 * a cache of SequenceType objects based on this item type, with different
 * occurrence indicators.
 */

public interface ItemTypeWithSequenceTypeCache extends ItemType {
    /**
     * Get a sequence type representing exactly one instance of this atomic type
     *
     * @return a sequence type representing exactly one instance of this atomic type
     * @since 9.8.0.2
     */

    SequenceType one();

    /**
     * Get a sequence type representing zero or one instances of this atomic type
     *
     * @return a sequence type representing zero or one instances of this atomic type
     * @since 9.8.0.2
     */

    SequenceType zeroOrOne();

    /**
     * Get a sequence type representing one or more instances of this atomic type
     *
     * @return a sequence type representing one or more instances of this atomic type
     * @since 9.8.0.2
     */

    SequenceType oneOrMore();

    /**
     * Get a sequence type representing one or more instances of this atomic type
     *
     * @return a sequence type representing one or more instances of this atomic type
     * @since 9.8.0.2
     */

    SequenceType zeroOrMore();

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
