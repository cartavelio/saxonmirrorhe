package net.sf.saxon.regex;

import net.sf.saxon.regex.charclass.CharacterClass;
import net.sf.saxon.regex.charclass.EmptyCharacterClass;
import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.transpile.CSharpSuppressWarnings;
import net.sf.saxon.z.IntIterator;

import java.util.Iterator;
import java.util.List;

/**
 * A choice of several branches
 */

public class OpChoice extends Operation {

    List<Operation> branches;

    OpChoice(List<Operation> branches) {
        this.branches = branches;
    }

    @Override
    public int getMatchLength() {
        int fixed = branches.get(0).getMatchLength();
        for (int i = 1; i < branches.size(); i++) {
            if (branches.get(i).getMatchLength() != fixed) {
                return -1;
            }
        }
        return fixed;
    }

    @Override
    public int getMinimumMatchLength() {
        int min = branches.get(0).getMinimumMatchLength();
        for (int i = 1; i < branches.size(); i++) {
            int m = branches.get(i).getMinimumMatchLength();
            if (m < min) {
                min = m;
            }
        }
        return min;
    }

    @Override
    public int matchesEmptyString() {
        int m = 0;
        for (Operation branch : branches) {
            int b = branch.matchesEmptyString();
            if (b != MATCHES_ZLS_NEVER) {
                m |= b;
            }
        }
        return m;
    }

    @Override
    public boolean containsCapturingExpressions() {
        for (Operation o : branches) {
            if (o instanceof OpCapture || o.containsCapturingExpressions()) {
                return true;
            }
        }
        return false;
    }

    @Override
    public CharacterClass getInitialCharacterClass(boolean caseBlind) {
        CharacterClass result = EmptyCharacterClass.getInstance();
        for (Operation o : branches) {
            result = RECompiler.makeUnion(result, o.getInitialCharacterClass(caseBlind));
        }
        return result;
    }


    @Override
    public Operation optimize(REProgram program, REFlags flags) {
        for (int i = 0; i < branches.size(); i++) {
            Operation o1 = branches.get(i);
            Operation o2 = o1.optimize(program, flags);
            if (o1 != o2) {
                branches.set(i, o2);
            }
        }
        return this;
    }

    @Override
    @CSharpInnerClass(outer = true, extra = {"Saxon.Hej.regex.REMatcher matcher", "int position"})
    @CSharpSuppressWarnings("UnsafeIteratorConversion")
    public IntIterator iterateMatches(final REMatcher matcher, final int position) {
        return new IntIterator() {
            final Iterator<Operation> branchIter = branches.iterator();
            IntIterator currentIter = null;
            Operation currentOp = null;

            @Override
            public boolean hasNext() {
                while (true) {
                    if (currentIter == null) {
                        if (branchIter.hasNext()) {
                            matcher.clearCapturedGroupsBeyond(position);
                            currentOp = branchIter.next();
                            currentIter = currentOp.iterateMatches(matcher, position);
                        } else {
                            return false;
                        }
                    }
                    if (currentIter.hasNext()) {
                        return true;
                    } else {
                        currentIter = null;
                        //continue;
                    }
                }
            }

            @Override
            public int next() {
                return currentIter.next();
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
        StringBuilder fsb = new StringBuilder(64);
        fsb.append("(?:");
        boolean first = true;
        for (Operation branch : branches) {
            if (first) {
                first = false;
            } else {
                fsb.append('|');
            }
            fsb.append(branch.display());
        }
        fsb.append(")");
        return fsb.toString();
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
