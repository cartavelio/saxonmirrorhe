package com.saxonica.ee.stream;

import net.sf.saxon.regex.RegexIterator;
import net.sf.saxon.regex.RegexMatchHandler;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.value.StringValue;

/**
 * The class ManualRegexIterator represents an iterator that is never repositioned, but can
 * be used to represent the current regex groups in the dynamic context. It is used as a snapshot copy of a real
 * regex iterator for use when asynchronous threads are forked, as in xsl:result-document; the
 * purpose of taking a snapshot is to ensure that changes to the base iterator in one thread do not
 * affect the result of the current-group() function in another thread.
 */
public class ManualRegexIterator extends ManualIterator implements RegexIterator {

    private final boolean currentItemMatches;
    private final UnicodeString[] groups;

    public ManualRegexIterator(RegexIterator base) {
        currentItemMatches = base.isMatching();
        groups = new UnicodeString[base.getNumberOfGroups()+1];
        for (int i=0; i<groups.length; i++) {
            groups[i] = base.getRegexGroup(i);
        }
    }

    @Override
    public StringValue next() {
        return (StringValue)super.next();
    }

    /**
     * Get the number of captured groups
     */
    @Override
    public int getNumberOfGroups() {
        return groups.length;
    }

    /**
     * Determine whether the current item in the sequence is a matching item or a non-matching item
     *
     * @return true if the current item is a matching item
     */
    @Override
    public boolean isMatching() {
        return currentItemMatches;
    }

    /**
     * Get a substring that matches a parenthesised group within the regular expression
     *
     * @param number the number of the group to be obtained
     * @return the substring of the current item that matches the n'th parenthesized group
     * within the regular expression
     */
    @Override
    public UnicodeString getRegexGroup(int number) {
        if (number >= 0 && number < groups.length) {
            return groups[number];
        } else {
            return EmptyUnicodeString.getInstance();
        }
    }

    /**
     * Process a matching substring, performing specified actions at the start and end of each matching
     * group
     *
     * @param action the action to be performed on a matching substring
     */
    @Override
    public void processMatchingSubstring(RegexMatchHandler action) {
        throw new UnsupportedOperationException();
    }

}


// Copyright (c) 2018-2023 Saxonica Limited
