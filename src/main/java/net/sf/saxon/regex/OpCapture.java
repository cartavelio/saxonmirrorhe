package net.sf.saxon.regex;

import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.z.IntIterator;

/**
 * Open paren (captured group)
 */

public class OpCapture extends Operation {

    int groupNr;
    Operation childOp;

    OpCapture(Operation childOp, int group) {
        this.childOp = childOp;
        this.groupNr = group;
    }

    @Override
    public int getMatchLength() {
        return childOp.getMatchLength();
    }

    @Override
    public int getMinimumMatchLength() {
        return childOp.getMinimumMatchLength();
    }

    @Override
    public int matchesEmptyString() {
        return childOp.matchesEmptyString();
    }

    @Override
    public Operation optimize(REProgram program, REFlags flags) {
        childOp = childOp.optimize(program, flags);
        return this;
    }

    @Override
    @CSharpInnerClass(outer = true, extra = {
            "Saxon.Hej.regex.REMatcher matcher",
            "int position",
            "Saxon.Hej.z.IntIterator basis"
    })
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        if ((matcher.program.optimizationFlags & REProgram.OPT_HASBACKREFS) != 0) {
            matcher.startBackref[groupNr] = position;
        }
        final IntIterator basis = childOp.iterateMatches(matcher, position);
        return new IntIterator() {
            @Override
            public boolean hasNext() {
                return basis.hasNext();
            }

            @Override
            public int next() {
                int next = basis.next();
                // Increase valid paren count
                if (groupNr >= matcher._captureState.parenCount) {
                    matcher._captureState.parenCount = groupNr + 1;
                }

                // Don't set paren if already set later on
                //if (matcher.getParenStart(groupNr) == -1) {
                matcher.setParenStart(groupNr, position);
                matcher.setParenEnd(groupNr, next);
                //}
                if ((matcher.program.optimizationFlags & REProgram.OPT_HASBACKREFS) != 0) {
                    matcher.startBackref[groupNr] = position;
                    matcher.endBackref[groupNr] = next;
                }
                return next;
            }
        };
    }

    /**
     * Display the operation as a regular expression, possibly in abbreviated form
     *
     * @return the operation in a form that is recognizable as a regular expression or abbreviated
     * regular expression
     */
    @Override
    public String display() {
        return "(" + childOp.display() + ")";
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
