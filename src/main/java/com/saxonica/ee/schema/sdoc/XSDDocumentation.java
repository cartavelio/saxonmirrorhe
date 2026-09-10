package com.saxonica.ee.schema.sdoc;

//-- imported classes and packages

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.StandardURIChecker;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.StringView;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.StringConverter;
import net.sf.saxon.type.ValidationFailure;

/**
 * An xs:documentation or xs:appinfo element in an XML Schema document
 */
public class XSDDocumentation extends SchemaElement {

    /**
     * Creates a new documentation or appinfo element
     */
    public XSDDocumentation() {
    }

    /**
     * Process the attributes of this element and all its children
     */

    @Override
    public void processAllAttributes() throws SchemaException {
        prepareAttributes();
        // Suppress validation of the subtree for these elements, except for saxon:param elements within xs:appInfo
        // Technically we should check that if the element contains elements such as xs:element, then
        // these are valid according to the schema for schemas.
        if (getFingerprint() == StandardNames.XS_APPINFO) {
            for (NodeInfo child : children()) {
                if (child instanceof SaxonParam) {
                    ((SchemaElement) child).processAllAttributes();
                }
            }
        }
    }


    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        String[] allowed = {"source"};
        // Note that no id attribute is permitted
        // Note also that we don't handle xml:lang specially. It's permitted automatically
        // by being in a different namespace. However, we need to check that it's a valid xs:language
        allowAttributes(atts, allowed);

        String source = atts.getValue(NamespaceUri.NULL, "source");
        if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
            if (source != null && !StandardURIChecker.getInstance().isValidURI(source)) {
                error("Value of source attribute must be a valid anyURI value");
            }
        }

        String lang = atts.getValue(NamespaceUri.XML, "lang");
        if (lang != null) {
            ValidationFailure vf = StringConverter.StringToLanguage.INSTANCE.validate(StringView.tidy(lang));
            if (vf != null) {
                error("Value of xml:lang attribute is not a valid language code");
            }
        }

    }

    /**
     * Recursive walk through the stylesheet to validate all nodes: this implementation
     * suppresses the subtree validation.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void validateSubtree(SchemaCompiler compiler) {
        // Suppress validation of the subtree for these elements. This has the side-effect of permitting
        // character data where all other elements prohibit it.

        // Technically we should check that if the element contains elements such as xs:element, then
        // these are valid according to the schema for schemas.
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     */

    @Override
    public void validate(SchemaCompiler compiler) {
        // This method is not called, because validateSubtree() suppresses it
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
