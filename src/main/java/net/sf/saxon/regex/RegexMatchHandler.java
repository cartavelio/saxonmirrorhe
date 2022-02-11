package net.sf.saxon.regex;

import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

/**
 * Interface defining a call-back action for processing captured groups
 */

public interface RegexMatchHandler {

    /**
     * Method to be called with each fragment of text in a matching substring
     *
     * @param s a matching substring, or part thereof that falls within a specific group
     */

    void characters(UnicodeString s) throws XPathException;

    /**
     * Method to be called when the start of a captured group is encountered
     *
     * @param groupNumber the group number of the captured group
     */

    void onGroupStart(int groupNumber) throws XPathException;

    /**
     * Method to be called when the end of a captured group is encountered
     *
     * @param groupNumber the group number of the captured group
     */

    void onGroupEnd(int groupNumber) throws XPathException;
}
