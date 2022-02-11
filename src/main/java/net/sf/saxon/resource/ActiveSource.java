package net.sf.saxon.resource;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;

/**
 * An ActiveSource is a Source that is capable of delivering an XML document to a Receiver;
 */

public interface ActiveSource extends Source {

    /**
     * Deliver the content of the source to a supplied Receiver
     * @param receiver the receiver to which events representing the parsed XML document will be sent
     * @param options options for parsing the source
     * @throws XPathException if parsing fails for any reason. The detailed diagnostics will
     * have been sent to the error reporter.
     */
    void deliver(Receiver receiver, ParseOptions options) throws XPathException;

}

