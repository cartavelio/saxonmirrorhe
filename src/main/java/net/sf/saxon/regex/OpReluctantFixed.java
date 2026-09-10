package net.sf.saxon.regex;

import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.z.IntIterator;

/**
 * Handle a reluctant repetition (with possible min and max) where the
 * size of the repeated unit is fixed.
 */

public class OpReluctantFixed extends OpRepeat {
    private final int len;

    OpReluctantFixed(Operation op, int min, int max, int len) {
        super(op, min, max, false);
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
        op = op.optimize(program, flags);
        return this;
    }

    @Override
    @CSharpInnerClass(outer = true, extra = {
            "Saxon.Hej.regex.REMatcher matcher",
            "int position"})
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        return new IntIterator() {
            private int pos = position;
            private int count = 0;
            private boolean started = false;

            @Override
            public boolean hasNext() {
                if (!started) {
                    started = true;
                    while (count < min) {
                        IntIterator child = op.iterateMatches(matcher, pos);
                        if (child.hasNext()) {
                            pos = child.next();
                            count++;
                        } else {
                            return false;
                        }
                    }
                    return true;
                }
                if (count < max) {
                    matcher.clearCapturedGroupsBeyond(pos);
                    IntIterator child = op.iterateMatches(matcher, pos);
                    if (child.hasNext()) {
                        pos = child.next();
                        count++;
                        return true;
                    }
                }
                return false;
            }

            @Override
            public int next() {
                return pos;
            }
        };
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
