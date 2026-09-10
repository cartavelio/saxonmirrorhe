////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.serialize;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.regex.ARegularExpression;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.XPathException;

/**
 * NewlineAdjuster: This ProxyReceiver adjusts newline characters in text nodes, comment nodes, and
 * processing instructions
 */


public class NewlineAdjuster extends ProxyReceiver {

    private final Twine8 lineEnding;
    private final ARegularExpression regex;
    private final IndentWhitespace replacement;


    /**
     * Create a NewlineAdjuster
     *
     * @param next the next receiver in the pipeline, always an XMLEmitter
     * @param lineEnding the string to substitute for newline characters
     */

    public NewlineAdjuster(Receiver next, String lineEnding) {
        super(next);
        this.lineEnding = new Twine8(lineEnding.replace("\\r", "\r").replace("\\n", "\n"));
        this.replacement = IndentWhitespace.of(this.lineEnding, 0,' ');
        try {
            this.regex = new ARegularExpression(StringConstants.NEWLINE, "", "XP20", null, getPipelineConfiguration().getConfiguration());
        } catch (XPathException e) {
            throw new RuntimeException(e);
        }
    }


    /**
     * Output a processing instruction
     */

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) throws XPathException {
        data = regex.replace(data, lineEnding);
        getNextReceiver().processingInstruction(target, data, locationId, properties);
    }

    /**
     * Output character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (chars instanceof WhitespaceString) {
            // implies it was added by the indenter, which will already have handled the newline
            getNextReceiver().characters(chars, locationId, properties);
        } else {
            while (true) {
                long nl = chars.indexOf('\n');
                if (nl == -1) {
                    getNextReceiver().characters(chars, locationId, properties);
                    break;
                } else {
                    getNextReceiver().characters(chars.substring(0, nl), locationId, properties);
                    // The replacement is wrapped in an IndentWhitespace object to prevent escaping
                    getNextReceiver().characters(replacement, locationId, properties);
                    chars = chars.substring(nl + 1);
                }
            }
        }
    }

    /**
     * Output a comment
     */

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) throws XPathException {
        chars = regex.replace(chars, lineEnding);
        getNextReceiver().comment(chars, locationId, properties);
    }

}

