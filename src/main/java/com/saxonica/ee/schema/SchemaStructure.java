package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaValidationStatus;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import javax.xml.transform.SourceLocator;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Stack;

/**
 * The base class for all XML Schema stuctures. Generally represents a schema component
 * in the sense of the XML Schema specification, though the correspondence is not always
 * one to one. Maintains information about the location of the schema construct in an
 * XML Schema document, for diagnostic purposes.
 */

public abstract class SchemaStructure implements UserSchemaComponent, Location {

    private String systemId;    // the URI of the XML entity in which the component is defined
    private int lineNumber;     // the line number of the component definition within that entity
    private String generatedId; // a string used to compare node identity, such as a path
    private String schemaDocumentURI;  // the URI of the xs:schema element of the containing schema document
    // may be different from systemId if XML entities are used

    /**
     * The fixup status
     */

    private SchemaValidationStatus fixupStatus = SchemaValidationStatus.UNVALIDATED;

    /**
     * The validation status
     */
    private SchemaValidationStatus validationStatus = SchemaValidationStatus.UNVALIDATED;

    /**
     * The configuration under which this component was created
     */

    private EnterpriseConfiguration config;

    /**
     * The redefinition level of this component
     */

    private int redefinitionLevel = 0;

    /**
     * Get the system identifier (URI) of the schema module (XML document) containing
     * the definition of this component.
     *
     * @return the URI of the containing module.
     */

    @Override
    public String getSystemId() {
        return systemId;
    }

    /**
     * Get the public ID of the containing module, if known.
     *
     * @return This information is not currently maintained, the method
     *         always returns null.
     */
    @Override
    public String getPublicId() {
        return null;
    }

    /**
     * Get the line number of the element defining this schema component, within its
     * module. This will be as delivered by the SAX2 parser: that is, the line number of an element
     * is generally the line number on which its start tag ends.
     *
     * @return the line number where the relevant element can be found.
     */

    @Override
    public int getLineNumber() {
        return lineNumber;
    }

    /**
     * Get the column number of the element defining this schema component.
     *
     * @return -1: this information is not maintained.
     */

    @Override
    public int getColumnNumber() {
        return -1;
    }

    /**
     * Get an immutable copy of this Location object. By default Location objects may be mutable, so they
     * should not be saved for later use. The result of this operation holds the same location information,
     * but in an immutable form.
     */
    @Override
    public Location saveLocation() {
        return this;
    }

    /**
     * Set the location information for this schema component.
     *
     * @param locator the location information.
     */
    public void setLocator(/*@Nullable*/ SourceLocator locator) {
        if (locator != null) {
            systemId = locator.getSystemId();
            lineNumber = locator.getLineNumber();
            if (locator instanceof NodeInfo) {
                generatedId = Navigator.getPath((NodeInfo)locator);
            }
        }
    }

    /**
     * Set the system Id
     *
     * @param systemId the systemId (the URI of the schema document in which the component is declared)
     */

    public void setSystemId(String systemId) {
        this.systemId = systemId;
    }

    /**
     * Set the line number
     *
     * @param lineNumber the line number of the declaration of the component within its schema document
     */

    public void setLineNumber(int lineNumber) {
        this.lineNumber = lineNumber;
    }

    /**
     * Set the base URI of the containing xs:schema element. This will normally be the same as the
     * systemId, unless the schema document is made up of multiple XML entities
     *
     * @param uri the base URI of the schema document
     */

    public void setSchemaDocumentURI(String uri) {
        this.schemaDocumentURI = uri;
    }

    /**
     * Get the base URI of the containing xs:schema element. This will normally be the same as the
     * systemId, unless the schema document is made up of multiple XML entities
     *
     * @return the base URI of the containing schema document
     */

    public String getSchemaDocumentURI() {
        return this.schemaDocumentURI;
    }

    /**
     * Set the generated ID, used to test whether two components are "identical"
     * @param generatedId the ID acting as a unique identifier
     */

    public void setGeneratedId(String generatedId) {
        this.generatedId = generatedId;
    }

    /**
     * Get the generated ID, used to test whether two components are "identical"
     *
     * @return the generated ID
     */

    public String getGeneratedId() {
        return generatedId;
    }

    /**
     * Checks the validity of this Schema component.
     *
     * @param compiler the schema compiler
     * @return true when this Schema definition is valid, otherwise false.
     * @throws SchemaException if the ErrorListener decides that an error is fatal.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        return true;
    }

    @Override
    public FunctionItem getComponentAsFunction() {
        return null;
    }

    /**
     * Get the current validation status
     *
     * @return an integer code indicating whether validation has not yet been started, is underway,
     *         has completed successfully, or has completed unsuccessfully
     */

    public SchemaValidationStatus getValidationStatus() {
        return validationStatus;
    }

    /**
     * Ask if validation is needed
     * @return true if the component has not yet been validated
     */

    public boolean isValidationNeeded() {
        return validationStatus == SchemaValidationStatus.UNVALIDATED;
    }

    /**
     * Set the validation status
     *
     * @param status an integer code indicating whether validation has not yet been started, is underway,
     *               has completed successfully, or has completed unsuccessfully
     */
    public void setValidationStatus(SchemaValidationStatus status) {
        validationStatus = status;
    }

    /**
     * Get the current fixup status
     *
     * @return an integer code indicating whether fixup of component references has not yet been started, is underway,
     *         has completed successfully, or has completed unsuccessfully
     */

    public SchemaValidationStatus getFixupStatus() {
        return fixupStatus;
    }

    /**
     * Set the fixup status
     *
     * @param status an integer code indicating whether fixup of component references has not yet been started, is underway,
     *               has completed successfully, or has completed unsuccessfully
     */
    public void setFixupStatus(SchemaValidationStatus status) {
        fixupStatus = status;
    }

    /**
     * Set the configuration containing this structure
     *
     * @param config the Saxon configuration
     */

    public void setConfiguration(EnterpriseConfiguration config) {
        this.config = config;
    }

    /**
     * Get the schema containing this structure
     *
     * @return the Saxon configuration
     */

    public EnterpriseConfiguration getConfiguration() {
        return config;
    }

    /**
     * Set the redefinition level. This is zero for a component that has not been redefined;
     * for a redefinition of a level-0 component, it is 1; for a redefinition of a level-N
     * component, it is N+1. This concept is used to support the notion of "pervasive" redefinition:
     * if a component is redefined at several levels, the top level wins, but it is an error to have
     * two versions of the component at the same redefinition level.
     *
     * @param level the redefinition level
     */

    public void setRedefinitionLevel(int level) {
        redefinitionLevel = level;
    }

    /**
     * Get the redefinition level. This is zero for a component that has not been redefined;
     * for a redefinition of a level-0 component, it is 1; for a redefinition of a level-N
     * component, it is N+1. This concept is used to support the notion of "pervasive" redefinition:
     * if a component is redefined at several levels, the top level wins, but it is an error to have
     * two versions of the component at the same redefinition level.
     *
     * @return the redefinition level
     */

    public int getRedefinitionLevel() {
        return redefinitionLevel;
    }

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     * This is a dummy implementation that does nothing; it must be overridden for
     * components that can participate in cycles.
     *
     * @param references A stack of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler   the schema compiler
     * @throws net.sf.saxon.type.SchemaException if cycles are found
     *
     */

    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler)
            throws SchemaException {
        // no action
    }

    /**
     * Fixup references from this schema component to others
     *
     * @param compiler the SchemaCompiler, used primarily for reporting errors
     * @return true if all references were fixed up successfully, false if dangling references were found
     */

    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        return true;
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     */

    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        // no action
    }

    /**
     * Helper method to make an XPathExpression property record
     * @return a function that delivers the information in this component
     */

    protected static FunctionItem makeXPathExpressionPropertyRecord(
            final String conditionText,
            final NamespaceResolver nsResolver,
            final String baseURI) {
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "namespace bindings":
                    List<Item> bindings = new ArrayList<>();
                    Iterator<String> prefixes = nsResolver.iteratePrefixes();
                    while (prefixes.hasNext()) {
                        final String prefix = prefixes.next();
                        CallableDelegate binding = new CallableDelegate((context1, arguments1) -> {
                            String key1 = arguments1[0].head().getStringValue();
                            switch (key1) {
                                case "prefix":
                                    return new StringValue(prefix);
                                case "namespace":
                                    return new AnyURIValue((nsResolver.getURIForPrefix(prefix, false)).toUnicodeString());
                                default:
                                    return EmptySequence.getInstance();
                            }
                        });
                        bindings.add(new CallableFunction(1, binding, SpecificFunctionType.COMPONENT_FUNCTION_TYPE));
                    }
                    return SequenceExtent.makeSequenceExtent(bindings);
                case "default namespace":
                    return new AnyURIValue((nsResolver.getURIForPrefix("", true)).toUnicodeString());
                case "base URI":
                    return new AnyURIValue((baseURI));
                case "expression":
                    return new StringValue(conditionText);
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

    /**
     * Ask whether this schema component should be considered identical to another schema component
     *
     * @param other the other schema component
     * @return true if (on balance of probabilities...) these two structures represent the same schema
     * component. This is considered to be the case if they appear to derive from the same source
     * declaration
     */

    public boolean hasSameLocation(SchemaStructure other) {
        if (!UserDefinedType.isSameURI(this.getSystemId(), other.getSystemId())) {
            return false;
        }
        if (this.getLineNumber() >= 0 && other.getLineNumber() >= 0) {
            return this.getLineNumber() == other.getLineNumber() &&
                    this.getColumnNumber() == other.getColumnNumber();
        }
        return this.generatedId != null && other.generatedId != null && this.generatedId.equals(other.generatedId);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
