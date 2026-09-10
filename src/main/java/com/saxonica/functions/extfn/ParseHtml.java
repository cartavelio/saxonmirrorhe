package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NoElementsSpaceStrippingRule;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.XMLReader;

/**
 * Supports the saxon:parse-html() function, a variant of saxon:parse() that differs
 * only in that it uses the TagSoup HTML parser
 */
public class ParseHtml extends Parse {

    /**
     * Set the XML parser to be used, if different from the default XML parser
     *
     * @param config  the Saxon configuration
     * @param options the ParseOptions object to be updated with the selected parser
     * @throws XPathException if the selected parser cannot be loaded
     */

    protected void setXMLReader(Configuration config, ParseOptions options) throws XPathException {
        Object parser = config.getInstance("org.ccil.cowan.tagsoup.Parser", null);
        if (!(parser instanceof XMLReader)) {
            throw new XPathException("Loaded org.ccil.cowan.tagsoup.Parser, but it is not an XMLReader");
        }
        options.setXMLReader((XMLReader) parser);
        options.setDTDValidationMode(Validation.SKIP);
        options.setSchemaValidationMode(Validation.SKIP);
        options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
    }

}

// Copyright (c) 2009-2016 Saxonica Limited.
