package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSingletonIterator;

/**
 * Back-reference
 */

public class OpBackReference extends Operation {

    int groupNr;

    OpBackReference(int groupNr) {
        this.groupNr = groupNr;
    }

    /**
     * Ask whether the regular expression is known, after static analysis, to match a
     * zero-length string
     *
     * @return false. Returning true means that
     * the expression is known statically to match ""; returning false means that this cannot
     * be determined statically; it does not mean that the expression does not match "".
     * We cannot do the analysis statically where back-references are involved, so we return false.
     */

    @Override
    public int matchesEmptyString() {
        return 0; // no information available
    }

    @Override
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        // Get the start and end of the backref
        int s = matcher.startBackref[groupNr];
        int e = matcher.endBackref[groupNr];

        // We don't know the backref yet
        if (s == -1 || e == -1) {
            return EmptyIntIterator.getInstance();
        }

        // The backref is empty size
        if (s == e) {
            return new IntSingletonIterator(position);
        }

        // Get the length of the backref
        int l = e - s;

        // If there's not enough input left, give up.
        UnicodeString search = matcher.search;
        if (position + l - 1 >= search.length()) {
            return EmptyIntIterator.getInstance();
        }

        // Case fold the backref?
        if (matcher.program.flags.isCaseIndependent()) {
            // Compare backref to input
            for (int i = 0; i < l; i++) {
                if (!matcher.equalCaseBlind(search.codePointAt(position + i), search.codePointAt(s + i))) {
                    return EmptyIntIterator.getInstance();
                }
            }
        } else {
            // Compare backref to input
            for (int i = 0; i < l; i++) {
                if (search.codePointAt(position + i) != search.codePointAt(s + i)) {
                    return EmptyIntIterator.getInstance();
                }
            }
        }
        return new IntSingletonIterator(position + l);
    }

    /**
     * Display the operation as a regular expression, possibly in abbreviated form
     *
     * @return the operation in a form that is recognizable as a regular expression or abbreviated
     * regular expression
     */
    @Override
    public String display() {
        return "\\" + groupNr;
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
