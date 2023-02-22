package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSingletonIterator;

/**
 * Beginning of Line (^)
 */

public class OpBOL extends Operation {

    @Override
    public int getMatchLength() {
        return 0;
    }

    @Override
    public int matchesEmptyString() {
        return MATCHES_ZLS_AT_START;
    }

    @Override
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        // Fail if we're not at the start of the string
        if (position != 0) {
            // If we're multiline matching, we could still be at the start of a line
            if (matcher.program.flags.isMultiLine()) {
                // Continue if at the start of a line
                if (matcher.isNewline(position - 1) && !(position >= matcher.search.length())) {
                    return new IntSingletonIterator(position);
                }
            }
            return EmptyIntIterator.getInstance();
        }
        return new IntSingletonIterator(position);
    }

    /**
     * Display the operation as a regular expression, possibly in abbreviated form
     *
     * @return the operation in a form that is recognizable as a regular expression or abbreviated
     * regular expression
     */
    @Override
    public String display() {
        return "^";
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
