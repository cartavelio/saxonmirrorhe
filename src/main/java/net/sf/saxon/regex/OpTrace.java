package net.sf.saxon.regex;

import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.z.IntIterator;

/**
 * Operation that wraps a base operation and traces its execution
 */

public class OpTrace extends Operation {

    private Operation base;
    private static int counter = 0;

    OpTrace(Operation base) {
        this.base = base;
    }

    @Override
    @CSharpInnerClass(outer = true,
            extra = {
                    "Saxon.Hej.z.IntIterator baseIter",
                    "int iterNr"
            })
    public IntIterator iterateMatches(REMatcher matcher, int position) {
        final IntIterator baseIter = base.iterateMatches(matcher, position);
        final int iterNr = counter++;
        String clName = baseIter.getClass().getName();
        int lastDot = clName.lastIndexOf(".");
        String iterName = clName.substring(lastDot + 1);
        System.err.println("Iterating over " + base.getClass().getSimpleName() + " " +
                                   base.display() + " at position " + position + " returning " +
                                   iterName + " " + iterNr);
        return new IntIterator() {
            @Override
            public boolean hasNext() {
                boolean b = baseIter.hasNext();
                System.err.println("IntIterator " + iterNr + " hasNext() = " + b);
                return b;
            }

            @Override
            public int next() {
                int n = baseIter.next();
                System.err.println("IntIterator " + iterNr + " next() = " + n);
                return n;
            }
        };
    }

    @Override
    public int getMatchLength() {
        return base.getMatchLength();
    }

    @Override
    public int matchesEmptyString() {
        return base.matchesEmptyString();
    }

    @Override
    public Operation optimize(REProgram program, REFlags flags) {
        base = base.optimize(program, flags);
        return this;
    }

    /**
     * Display the operation as a regular expression, possibly in abbreviated form
     *
     * @return the operation in a form that is recognizable as a regular expression or abbreviated
     * regular expression
     */
    @Override
    public String display() {
        return base.display();
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
