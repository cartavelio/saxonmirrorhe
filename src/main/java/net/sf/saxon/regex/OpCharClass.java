package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.regex.charclass.CharacterClass;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.z.*;

/**
 * A match of a single character in the input against a set of permitted characters
 */

public class OpCharClass extends Operation {
    private final IntPredicateProxy predicate;

    OpCharClass(IntPredicateProxy predicate) {
        this.predicate = predicate;
    }

    public IntPredicateProxy getPredicate() {
        return predicate;
    }

    @Override
    public int getMatchLength() {
        return 1;
    }

    @Override
    public int matchesEmptyString() {
        return MATCHES_ZLS_NEVER;
    }

    @Override
    public CharacterClass getInitialCharacterClass(boolean caseBlind) {
        if (predicate instanceof CharacterClass) {
            return (CharacterClass) predicate;
        } else {
            return super.getInitialCharacterClass(caseBlind);
        }
    }

    @Override
    public IntIterator iterateMatches(REMatcher matcher, int position) {
        UnicodeString in = matcher.search;
        if (position < in.length() && predicate.test(in.codePointAt(position))) {
            return new IntSingletonIterator(position + 1);
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
        if (predicate instanceof IntSetPredicate) {
            IntSet s = ((IntSetPredicate) predicate).getIntSet();
            if (s instanceof IntSingletonSet) {
                return "" + (char) ((IntSingletonSet) s).getMember();
            } else if (s instanceof IntRangeSet) {
                StringBuilder fsb = new StringBuilder(64);
                IntRangeSet irs = (IntRangeSet) s;
                fsb.append("[");
                for (int i = 0; i < irs.getNumberOfRanges(); i++) {
                    fsb.append((char) irs.getStartPoints()[1]);
                    fsb.append("-");
                    fsb.append((char) irs.getEndPoints()[1]);
                }
                fsb.append("[");
                return fsb.toString();
            } else {
                return "[....]";
            }
        } else {
            return "[....]";
        }
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
