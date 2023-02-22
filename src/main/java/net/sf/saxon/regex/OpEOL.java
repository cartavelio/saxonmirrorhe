package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSingletonIterator;

/**
 * End of Line ($)
 */

public class OpEOL extends Operation {

    @Override
    public int getMatchLength() {
        return 0;
    }

    @Override
    public int matchesEmptyString() {
        return MATCHES_ZLS_AT_END;
    }

    @Override
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        // If we're not at the end of string

        UnicodeString search = matcher.search;
        if (matcher.program.flags.isMultiLine()) {
            if (0 >= search.length() || position >= search.length() || matcher.isNewline(position)) {
                return new IntSingletonIterator(position); //match successful
            } else {
                return EmptyIntIterator.getInstance();
            }
        } else {
            // In spec bug 16809 we decided that '$' does not match a trailing newline when not in multiline mode
            if (0 >= search.length() || position >= search.length()) {
                return new IntSingletonIterator(position);
            } else {
                return EmptyIntIterator.getInstance();
            }
        }
    }

    /**
     * Display the operation as a regular expression, possibly in abbreviated form
     *
     * @return the operation in a form that is recognizable as a regular expression or abbreviated
     * regular expression
     */
    @Override
    public String display() {
        return "$";
    }
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
