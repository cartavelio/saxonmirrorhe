////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.regex.charclass.Categories;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSingletonIterator;

/**
 * Boundary assertion (\b or \B) in an XPath 4.0 regular expression
 */

public class OpBoundaryAssertion extends Operation {

    private final boolean positive;

    public OpBoundaryAssertion(boolean positive) {
        this.positive = positive;
    }

    @Override
    public int getMatchLength() {
        return 0;
    }

    @Override
    public int matchesEmptyString() {
        return MATCHES_ZLS_AT_START | MATCHES_ZLS_AT_END;
    }

    @Override
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        UnicodeString search = matcher.search;
        boolean atBoundary = position == 0 || position == search.length();
        if (!atBoundary) {
            int prev = search.codePointAt(position - 1);
            int next = search.codePointAt(position);
            atBoundary = Categories.ESCAPE_w.test(prev) != Categories.ESCAPE_w.test(next);
        }
        if (atBoundary == positive) {
            return new IntSingletonIterator(position);
        } else {
            return EmptyIntIterator.getInstance();
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
        return positive ? "\\b" : "\\B";
    }
}

