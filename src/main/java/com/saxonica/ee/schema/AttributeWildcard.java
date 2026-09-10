package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.om.Function;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaValidationStatus;


/**
 * This class represents an xs:anyAttribute element in the schema.
 */
public class AttributeWildcard extends SchemaStructure implements UserSchemaComponent {

    private Wildcard wildcard;

    public AttributeWildcard(EnterpriseConfiguration config) {
        wildcard = new Wildcard();
        wildcard.setConfiguration(config);
        setConfiguration(config);
    }

    public AttributeWildcard(Wildcard wc) {
        wildcard = wc;
        setConfiguration(wc.getConfiguration());
    }

    public void setWildcard(Wildcard wildcard) {
        this.wildcard = wildcard;
    }

    public Wildcard getWildcard() {
        return wildcard;
    }

    /**
     * Validate that this AttributeWildcard obeys the schema consistency rules. Any
     * errors are notified to the ErrorListener, and result in the method returning false.
     *
     * @param compiler used for error reporting
     * @return false if any errors were found, true if everything is OK
     * @throws SchemaException if the error listener decided to treat
     *                         the error as fatal.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (!wildcard.validate(compiler)) {
            return false;
        }
        if (wildcard.isInexpressible()) {
            compiler.error("The intersection or union of this wildcard with other wildcards is inexpressible", this);
            return false;
        }
        return true;
    }

    /**
     * Serialize this schema component
     *
     * @param serializer the object that does the serialization
     * @throws XPathException if serialization fails
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(wildcard, false);
        serializer.startElement("attributeWildcard");
        serializer.emitAttribute("ref", id);
        serializer.endElement();
    }

    /**
     * Get the schema component in the form of a function item. This allows schema information
     * to be made visible to XSLT or XQuery code. The function makes available the contents of the
     * schema component as defined in the XSD specification. The function takes a string as argument
     * representing a property name, and returns the corresponding property of the schema component.
     * There is also a property "class" which returns the kind of schema component, for example
     * "Attribute Declaration".
     *
     * @return the schema component represented as a function from property names to property values.
     */
    @Override
    public Function getComponentAsFunction() {
        return wildcard.getComponentAsFunction();
    }

    /**
     * Get the validation status of this component.
     *
     * @return the value {@link SchemaValidationStatus#UNVALIDATED}
     */

    @Override
    public SchemaValidationStatus getValidationStatus() {
        return SchemaValidationStatus.UNVALIDATED;
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
