package net.sf.saxon.resource;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.ActiveSource;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.InputSource;
import org.xml.sax.XMLReader;

import javax.xml.transform.stream.StreamSource;

public class ActiveStreamSource extends StreamSource implements ActiveSource {

    public ActiveStreamSource(StreamSource in) {
        setInputStream(in.getInputStream());
        setReader(in.getReader());
        setSystemId(in.getSystemId());
        setPublicId(in.getPublicId());
    }

    /**
     * Deliver the content of the source to a supplied Receiver
     *
     * @param receiver the receiver to which events representing the parsed XML document will be sent
     * @param options  options for parsing the source
     * @throws XPathException if parsing fails for any reason. The detailed diagnostics will
     *                        have been sent to the error reporter.
     */
    @Override
    public void deliver(Receiver receiver, ParseOptions options) throws XPathException {
        Configuration config = receiver.getPipelineConfiguration().getConfiguration();
        String url = getSystemId();
        InputSource is = new InputSource(url);
        is.setCharacterStream(getReader());
        is.setByteStream(getInputStream());
        boolean reuseParser = false;
        XMLReader parser = options.obtainXMLReader();
        if (parser == null) {
            parser = config.getSourceParser();
            if (options.getEntityResolver() != null && parser.getEntityResolver() == null) {
                parser.setEntityResolver(options.getEntityResolver());
            }
            reuseParser = true;
        }
        //System.err.println("Using parser: " + parser.getClass().getName());
        ActiveSAXSource sax = new ActiveSAXSource(parser, is);
        sax.setSystemId(url);
        sax.deliver(receiver, options);
        if (reuseParser) {
            config.reuseSourceParser(parser);
        }
    }
}

