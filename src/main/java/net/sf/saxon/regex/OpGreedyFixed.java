package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntStepIterator;

/**
 * Handle a greedy repetition (with possible min and max) where the
 * size of the repeated unit is fixed.
 */

public class OpGreedyFixed extends OpRepeat {
    private final int len;

    OpGreedyFixed(Operation op, int min, int max, int len) {
        super(op, min, max, true);
        this.len = len;
    }

    @Override
    public int getMatchLength() {
        return min == max ? min * len : -1;
    }

    @Override
    public int matchesEmptyString() {
        if (min == 0) {
            return MATCHES_ZLS_ANYWHERE;
        }
        return op.matchesEmptyString();
    }

    @Override
    public Operation optimize(REProgram program, REFlags flags) {
        if (max == 0) {
            return new OpNothing();
        }
        if (op.getMatchLength() == 0) {
            return op;
        }
        op = op.optimize(program, flags);
        return this;
    }

    @Override
    public IntIterator iterateMatches(REMatcher matcher, int position) {
        int guard = matcher.search.length32();
        if (max < Integer.MAX_VALUE) {
            guard = Math.min(guard, position + len * max);
        }
        if (position >= guard && min > 0) {
            return EmptyIntIterator.getInstance();
        }

        int p = position;
        int matches = 0;
        while (p <= guard) {
            IntIterator it = op.iterateMatches(matcher, p);
            boolean matched = false;
            if (it.hasNext()) {
                matched = true;
                it.next();
            }
            if (matched) {
                matches++;
                p += len;
                if (matches == max) {
                    break;
                }
            } else {
                break;
            }
        }
        if (matches < min) {
            return EmptyIntIterator.getInstance();
        }

        return new IntStepIterator(p, -len, position + len * min);
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
