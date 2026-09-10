package net.sf.saxon.regex;

import net.sf.saxon.expr.sort.EmptyIntIterator;
import net.sf.saxon.regex.charclass.CharacterClass;
import net.sf.saxon.regex.charclass.EmptyCharacterClass;
import net.sf.saxon.regex.charclass.IntSetCharacterClass;
import net.sf.saxon.regex.charclass.SingletonCharacterClass;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntSingletonIterator;

/**
 * A match against a fixed string of any length
 */

public class OpAtom extends Operation {
    private final UnicodeString atom;
    private final int len;

    OpAtom(UnicodeString atom) {
        this.atom = atom;
        this.len = atom.length32();
    }

    UnicodeString getAtom() {
        return atom;
    }

    @Override
    public int getMatchLength() {
        return len;
    }

    @Override
    public int matchesEmptyString() {
        return len == 0 ? MATCHES_ZLS_ANYWHERE : MATCHES_ZLS_NEVER;
    }

    @Override
    public CharacterClass getInitialCharacterClass(boolean caseBlind) {
        if (len == 0) {
            return EmptyCharacterClass.getInstance();
        } else if (caseBlind) {
            IntSet set;
            int ch = atom.codePointAt(0);
            int[] variants = CaseVariants.getCaseVariants(ch);
            if (variants.length > 0) {
                set = new IntHashSet(variants.length);
                set.add(ch);
                for (int v : variants) {
                    set.add(v);
                }
                return new IntSetCharacterClass(set);
            }
        }
        return new SingletonCharacterClass(atom.codePointAt(0));
    }

    @Override
    public IntIterator iterateMatches(REMatcher matcher, int position) {
        UnicodeString in = matcher.search;
        if (position + len > in.length()) {
            return EmptyIntIterator.getInstance();
        }
        if (matcher.program.flags.isCaseIndependent()) {
            for (int i = 0; i < len; i++) {
                if (!matcher.equalCaseBlind(in.codePointAt(position + i), atom.codePointAt(i))) {
                    return EmptyIntIterator.getInstance();
                }
            }
        } else {
            for (int i = 0; i < len; i++) {
                if (in.codePointAt(position + i) != atom.codePointAt(i)) {
                    return EmptyIntIterator.getInstance();
                }
            }
        }
        return new IntSingletonIterator(position + len);
    }

    @Override
    public String display() {
        return atom.toString();
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
