package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import com.saxonica.ee.schema.UserComplexType;
import com.saxonica.ee.schema.Wildcard;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.functions.registry.ConstructorFunctionLibrary;
import net.sf.saxon.functions.registry.XPath20FunctionSet;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeSelector;
import net.sf.saxon.str.StringView;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.*;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.linked.ElementImpl;
import net.sf.saxon.tree.linked.NodeImpl;
import net.sf.saxon.type.*;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.Whitespace;

import java.util.Arrays;
import java.util.StringTokenizer;

/**
 * An element in an XML Schema document. This is an abstract superclass for the elements
 * in the schema document; many of its methods are designed to be called from the subclasses
 */

public abstract class SchemaElement extends ElementImpl {

    private SchemaNodeFactory factory;
    //private int lineNumber;

    /**
     * Option indicating "use the default element namespace when unprefixed"
     */

    public static final int USE_DEFAULT_NS = 0;

    /**
     * Option indicating "no namespace prefix is allowed; the name is in the targetNamespace"
     */

    public static final int REQUIRE_TARGET_NS = 1;

    /**
     * Option indicating "no namespace prefix is allowed; the name is in the null Namespace"
     */

    public static final int REQUIRE_NULL_NS = 2;

    /**
     * Option indicating "use the default element namespace when unprefixed", but without a requirement
     * to have an xs:import
     */

    public static final int USE_DEFAULT_NS_WITHOUT_IMPORT = 3;
    protected static final String NAMESPACE_ANY = "##any";
    protected static final String NAMESPACE_LOCAL = "##local";
    protected static final String NAMESPACE_OTHER = "##other";
    protected static final String NAMESPACE_TARGET = "##targetNamespace";

    /**
     * Construct a SchemaElement
     */

    public SchemaElement() {
    }

    /**
     * Return true if this element is a child of xs:schema or xs:redefine
     *
     * @return true if this element is a child of xs:schema or xs:redefine
     */

    protected boolean isTopLevel() {
        NodeInfo parent = getParent();
        return (parent instanceof XSDSchema || parent instanceof XSDRedefine || parent instanceof XSDOverride);
    }

    /**
     * Set the nodeFactory used to create this element.
     *
     * @param factory the schema node factory
     */

    public void setSchemaNodeFactory(SchemaNodeFactory factory) {
        this.factory = factory;
    }

    /**
     * Get the nodeFactory used to create this element. This nodeFactory is used to
     * keep track of any errors encountered during construction of the schema document
     *
     * @return factory the schema node factory
     */

    public SchemaNodeFactory getSchemaNodeFactory() {
        return factory;
    }

    /**
     * Set the line number
     */

//    public void setLineNumber(int line) {
//        lineNumber = line;
//    }

    /**
     * Get the line number
     */

//    public int getLineNumber() {
//        return lineNumber;
//    }

    /**
     * Process the attributes of this element and all its children
     */

    public void processAllAttributes() throws SchemaException {
        prepareAttributes();
        for (NodeInfo child : children(NodeSelector.of(SchemaElement.class::isInstance))) {
            ((SchemaElement) child).processAllAttributes();
        }
    }

    /**
     * Set the attribute list for the element. This is called to process the attributes (note
     * the distinction from processAttributes in the superclass).
     * Must be supplied in a subclass
     */

    protected abstract void prepareAttributes() throws SchemaException;

    /**
     * Check the attributes of an element against a list of allowed attributes.
     * Report an error if any extraneous attribute is present
     *
     * @param atts    the attributes actually present
     * @param allowed the attributes that are allowed for this element. They must
     *                be listed in alphabetical order
     */

    public void allowAttributes(AttributeMap atts, String[] allowed) throws SchemaException {
        for (AttributeInfo att : atts) {
            NodeName attName = att.getNodeName();
            if (attName.hasURI(NamespaceUri.NULL)) {
                if (Arrays.binarySearch(allowed, attName.getLocalPart()) < 0) {
                    error("Attribute " + Err.wrap(attName.getLocalPart(), Err.ATTRIBUTE) +
                            " is not allowed on element " + Err.wrap(getDisplayName(), Err.ELEMENT));
                }
            } else if (attName.hasURI(NamespaceUri.SCHEMA)) {
                error("Attributes in the XML Schema namespace are not allowed");
            }
        }
    }

    /**
     * Indicate that a particular attribute is required
     *
     * @param atts     the attribtue collection
     * @param required the local name of the required attribute, which must be in the null namespace
     */

    public void requireAttribute(AttributeMap atts, String required) throws SchemaException {
        if (atts.getValue(NamespaceUri.NULL, required) == null) {
            missingAttribute(required);
        }
    }

    /**
     * Recursive walk through the schema to validate all nodes
     *
     * @param compiler used for error reporting
     */

    public void validateSubtree(SchemaCompiler compiler) throws SchemaException {
        checkNoTextNodes();
        validate(compiler);
        validateChildren(compiler);
        postValidate(compiler);
    }

    /**
     * Check that the element has no text node children
     *
     * @throws SchemaException if the element has text node children
     */

    private void checkNoTextNodes() throws SchemaException {
        SchemaElement prev = this;
        for (NodeInfo n : children()) {
            if (n instanceof SchemaElement) {
                // This is to get a reasonable line number for the error
                prev = (SchemaElement) n;
            }
            if (n.getNodeKind() == Type.TEXT) {
                prev.error("Character data is not allowed as a child of "
                        + Err.wrap(getDisplayName()));
            }
        }
    }

    /**
     * Validate the children of this node, recursively.
     *
     * @param compiler used for error reporting
     * @throws SchemaException if the subtree is not valid
     */

    protected void validateChildren(SchemaCompiler compiler) throws SchemaException {
        for (NodeInfo child : children(NodeSelector.of(SchemaElement.class::isInstance))) {
            ((SchemaElement) child).validateSubtree(compiler);
        }
    }


    /**
     * Check that the schema element is valid. This is called once for each element, after
     * the entire tree has been built. As well as validation, it can perform first-time
     * initialisation. The default implementation does nothing; it is normally overriden
     * in subclasses.
     *
     * @param compiler used for error reporting
     * @throws SchemaException if the subtree is not valid
     */

    public void validate(SchemaCompiler compiler) throws SchemaException {
    }

    /**
     * Hook to allow additional validation of a parent element immediately after its
     * children have been validated.
     *
     * @param compiler used for error reporting
     * @throws SchemaException if errors are found
     */

    public void postValidate(SchemaCompiler compiler) throws SchemaException {
    }

    /**
     * Get the owning XSDSchema element.
     *
     * @return the containing xs:schema element
     * @throws SchemaException if none is found (which means the schema is invalid)
     */

    public XSDSchema getXSDSchema() throws SchemaException {
        NodeInfo n = this;
        while (true) {
            if (n instanceof XSDSchema) {
                return (XSDSchema) n;
            }
            n = n.getParent();
            if (n == null) {
                throw new SchemaException("Node does not have an xs:schema ancestor");
            }
        }
    }

    /**
     * Get the schema corresponding to the contents of the schema document containing this element
     *
     * @return the PreparedSchema object used to assemble the schema components defined in the schema
     *         document that contains this element.
     * @throws SchemaException if the containing PreparedSchema cannot be found
     */

    public SingleNamespaceSchema getSchema() throws SchemaException {
        return getXSDSchema().getSchema();
    }

    /**
     * Get the complexType associated with the containing xs:complexType element
     *
     * @return the UserComplexType object representing the containing complex type
     * @throws SchemaException if the element is not contained in an xs:complexType
     */

    protected UserComplexType getContainingComplexType() throws SchemaException {
        NodeInfo parent = getParent();
        while (true) {
            if (parent instanceof XSDComplexType) {
                return ((XSDComplexType) parent).getComplexType();
            }
            if (parent == null) {
                throw new SchemaException("Found " + Err.wrap(getDisplayName(), Err.ELEMENT) +
                        " with no xs:complexType ancestor");
            }
            parent = parent.getParent();
        }
    }

    /**
     * Get the simpleTypeDefinition associated with the containing xs:simpleType element
     *
     * @return the schema component representing the containing simple type
     * @throws SchemaException if not contained in an xs:simpleType
     */

    protected SimpleTypeDefinition getContainingSimpleType() throws SchemaException {
        NodeInfo parent = getParent();
        while (true) {
            if (parent instanceof XSDSimpleType) {
                return ((XSDSimpleType) parent).getSimpleTypeDefinition();
            }
            if (parent == null) {
                throw new SchemaException("Found " + Err.wrap(getDisplayName()) + " with no xs:simpleType ancestor");
            }
            parent = parent.getParent();
        }
    }

    /**
     * Make a NamespaceContext object representing the list of in-scope namespaces. The NamePool
     * used for numeric codes in the NamespaceContext will be the target name pool.
     *
     * @return a copy of the namespace context for this element
     */

    protected NamespaceResolver makeNamespaceContext() {
        return getAllNamespaces();
    }

    /**
     * Check that a given type is a permitted type
     */

    public void checkPermittedType(int fp, String name, String role) throws SchemaException {
        if (fp == StandardNames.XS_DAY_TIME_DURATION ||
                fp == StandardNames.XS_YEAR_MONTH_DURATION) {
            if (getConfiguration().getXsdVersion() == Configuration.XSD10) {
                warning("Type " + name + " is recognized by Saxon but is not interoperable");
            }
        } else if (fp == StandardNames.XS_DATE_TIME_STAMP) {
            if (getConfiguration().getXsdVersion() == Configuration.XSD10) {
                error("Type xs:dateTimeStamp requires XSD 1.1 support to be enabled");
            }
        } else if (fp == StandardNames.XS_UNTYPED_ATOMIC || fp == StandardNames.XS_UNTYPED) {
            error("Type " + name + " cannot be used as " + role);
        } else if (fp == StandardNames.XS_ANY_ATOMIC_TYPE) {
            if (getConfiguration().getXsdVersion() == Configuration.XSD10) {
                error("Type xs:anyAtomicType requires XSD 1.1 support to be enabled");
            }
        }
    }

    /**
     * Parse a boolean attribute value
     *
     * @param name  the name of the attribute (for diagnostics)
     * @param value the string value of the attribute (need not be trimmed)
     * @return the boolean value of the attribute if it is valid
     * @throws SchemaException if the value is not a valid boolean
     */

    public boolean parseBooleanAttribute(String name, String value) throws SchemaException {
        value = Whitespace.trim(value);
        if (value.equals("true") || value.equals("1")) {
            return true;
        } else if (value.equals("false") || value.equals("0")) {
            return false;
        } else {
            invalidAttributeValue(name, value, "Must be true(1) or false(0)");
            return false; // dummy to keep the compiler happy
        }
    }

    /**
     * This method is called for a general error. The location of this message is the current element.
     *
     * @param err the error message to report
     */

    protected void error(String err) throws SchemaException {
        ErrorReporter reporter = getSchemaNodeFactory().getPipelineConfiguration().getErrorReporter();
        try {
            getXSDSchema().incrementErrorCount();
        } catch (SchemaException e2) {
            throw new IllegalStateException("Schema document contains no xs:schema element. " +
                                                    "Problem occurred while reporting error: " + e2.getMessage());
        }

        if (reporter != null) {
            XmlProcessingIncident incident = new XmlProcessingIncident(err, SaxonErrorCode.SXSQ0001, this);
            reporter.report(incident);
            if (incident.getTerminationMessage() != null) {
                throw new SchemaException(incident.getTerminationMessage());
            }
        }
    }

    /**
     * This method is called for a general error. The location of this message is the current element.
     *
     * @param exception the error message to report
     * @throws SchemaException if the user-written error handler chooses to treat the error as fatal
     */

    protected void error(SchemaException exception) throws SchemaException {
        ErrorReporter reporter = getSchemaNodeFactory().getPipelineConfiguration().getErrorReporter();
        try {
            getXSDSchema().incrementErrorCount();
        } catch (SchemaException e2) {
            throw new IllegalStateException("Schema document contains no xs:schema element. " +
                    "Problem occurred while reporting error: " + exception.getMessage());
        }

        if (reporter != null) {
            XmlProcessingException incident = new XmlProcessingException(exception);
            reporter.report(incident);
            if (incident.getTerminationMessage() != null) {
                throw new SchemaException(incident.getTerminationMessage());
            }
        } else {
            throw exception;
        }
    }

    /**
     * Report a warning
     *
     * @param err the warning message
     * @throws SchemaException if the user-supplied error handler chooses to treat the warning as fatal
     */

    protected void warning(String err) throws SchemaException {
        XmlProcessingIncident exception = new XmlProcessingIncident(err, SaxonErrorCode.SXWN9018, this).asWarning();
        ErrorReporter reporter = getSchemaNodeFactory().getPipelineConfiguration().getErrorReporter();
        reporter.report(exception);
        if (exception.getTerminationMessage() != null) {
            throw new SchemaException(exception.getTerminationMessage());
        }
    }

    /**
     * This method is called to check that a QName is valid and allocate a
     * fingerprint for the name in the namePool. If the QName isn't valid,
     * the method throws an exception: it is the caller's responsibility to call
     * the error handler and recover from the error. The method makes use of the
     * namespace context information for the schema element.
     *
     * @param name   The QName
     * @param option The option for handling unprefixed names. Options are
     *               to use the default namespace, the target namespace, or the null namespace
     *               Note that this method
     *               assumes the name, if unprefixed, is in the default namespace: NOT
     *               the targetNamespace of the schema.
     * @return a fingerprint
     * @throws net.sf.saxon.type.SchemaException
     *          if the QName is invalid.
     */

    protected int getFingerprint(String name, int option) throws SchemaException {
        StructuredQName qname = getComponentName(name, option);
        return getNamePool().allocateFingerprint(qname.getNamespaceUri(), qname.getLocalPart());
    }

    /**
     * This method is called to check that a lexical QName is valid and allocate a
     * nameCode for the name in the namePool. If the QName isn't valid,
     * the method throws an exception: it is the caller's responsibility to call
     * the error handler and recover from the error. The method makes use of the
     * namespace context information for the schema element.
     *
     * @param name   The QName as written in the schema
     * @param option The option for handling unprefixed names. Options are
     *               to use the default namespace {@link #USE_DEFAULT_NS},
     *               the target namespace {@link #REQUIRE_TARGET_NS}, or the null namespace
     *               {@link #REQUIRE_NULL_NS}
     * @return an integer name code
     * @throws SchemaException if the QName is invalid.
     */

    protected StructuredQName getComponentName(String name, int option)
        throws SchemaException {
        try {
            XSDSchema root = getXSDSchema();
            String[] parts = NameChecker.getQNameParts(Whitespace.trim(name));
            NamespaceUri uri = null;
            if (parts[0].isEmpty()) {
                if (option == USE_DEFAULT_NS || option == USE_DEFAULT_NS_WITHOUT_IMPORT) {
                    // This option is used for references that need to be resolved, e.g. ref="name"
                    uri = getURIForPrefix("", true);
                    if (uri == NamespaceUri.NULL && root.isChameleon()) {
                        uri = root.getTargetNamespace();
                    }
                } else if (option == REQUIRE_TARGET_NS) {
                    // This option is used for global elements and attributes, and those with form="qualified"
                    String target = getAttributeValue(NamespaceUri.NULL, "targetNamespace");
                    if (target == null) {
                        uri = root.getTargetNamespace();
                    } else {
                        uri = NamespaceUri.of(target);
                    }
                } else if (option == REQUIRE_NULL_NS) {
                    // This option is used for local elements and attributes with form="unqualified".
                    uri = NamespaceUri.NULL;
                }
            } else {
                if (option == REQUIRE_TARGET_NS || option == REQUIRE_NULL_NS) {
                    throw new SchemaException("No namespace prefix is allowed in this name " + Err.wrap(name));
                }
                uri = getURIForPrefix(parts[0], true);
            }
            if (uri == null) {
                throw new SchemaException("Undeclared namespace prefix in " + Err.wrap(name));
            }
            if (option == USE_DEFAULT_NS && !root.getTargetNamespace().equals(uri)
                && !root.isImportedNamespace(uri)
                && !uri.equals(NamespaceUri.SCHEMA)
                && !uri.equals(NamespaceUri.SCHEMA_INSTANCE)) {
                throw new SchemaException("Cannot reference schema components in " +
                    (uri.isEmpty() ? "the null namespace" : "namespace " + uri) +
                    " as it has not been imported");
            }
            return new StructuredQName(parts[0], uri, parts[1]);
        } catch (QNameException err) {
            throw new SchemaException(err.getMessage());
        }
    }


    /**
     * Check that the only child element of this element is an
     * (optional and non-repeatable) xs:annotation element
     */

    protected void checkOnlyChildIsAnnotation() throws SchemaException {
        boolean foundAnnotation = false;
        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            if (fp == StandardNames.XS_ANNOTATION) {
                if (foundAnnotation) {
                    duplicateElement("annotation");
                }
                foundAnnotation = true;
            } else {
                illegalElement(child);
            }
        }
    }


    /**
     * This method is called when an illegal Element is encountered.
     *
     * @param child The child element whose presence is an error
     * @throws SchemaException thrown if the errorListener
     *                         decides that it's a fatal error.
     */

    protected void illegalElement(NodeInfo child) throws SchemaException {
        if (child.getNodeKind() == Type.TEXT) {
            // Invalid character data has already been reported
        } else if (child instanceof XSDSchema) {
            // this one needs care; if there is a misplaced XSDSchema element the normal error reporting
            // would increment the error count on this wrong element, leading to the error being undetected later.
            // See test mgO026
            error("Element " + Err.wrap(child.getDisplayName(), Err.ELEMENT) +
                    " is not allowed as a child of " + Err.wrap(getDisplayName(), Err.ELEMENT));
        } else if (child instanceof SchemaElement) {
            // These convolutions are to get the line number right
            ((SchemaElement) child).error("Element " + Err.wrap(child.getDisplayName(), Err.ELEMENT) +
                    " is not allowed as a child of " + Err.wrap(getDisplayName(), Err.ELEMENT));
        } else {
            error("Element " + Err.wrap(child.getDisplayName(), Err.ELEMENT) +
                    " is not allowed as a child of " + Err.wrap(getDisplayName(), Err.ELEMENT));
        }
    }

    /**
     * This method is called when two occurrences of a child element are found
     * and only one is allowed.
     *
     * @param name the name of the illegal element
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */

    protected void duplicateElement(String name) throws SchemaException {
        error("Only one " + Err.wrap(name, Err.ELEMENT) +
                " is allowed as a child of " + Err.wrap(getDisplayName(), Err.ELEMENT));
    }

    /**
     * This method is called when two objects in the same symbol space within
     * a schema have duplicate names
     *
     * @param category the name of the first attribute
     * @param name     the name of the second attribute
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */

    protected void duplicate(String category, String name) throws SchemaException {
        error("The schema already contains " +
                indefiniteArticle(category, "a") +
                ' ' + category +
                " named " + Err.wrap(name));
    }


    /**
     * This method is called when two child elements may not coexist
     *
     * @param name1 the name of the first element
     * @param name2 the name of the second element
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */

    protected void mutuallyExclusive(String name1, String name2) throws SchemaException {
        if (name1.equals(name2)) {
            duplicateElement(name1);
        } else {
            error("The " + Err.wrap(name1, Err.ELEMENT) + " and " + Err.wrap(name2, Err.ELEMENT) + " elements " +
                    "must not both appear as children of the same " + Err.wrap(getDisplayName(), Err.ELEMENT));
        }
    }

    /**
     * This method is called when an attribute and a child elements may not coexist
     *
     * @param elem the name of the element
     * @param att  the name of the attribute
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    protected void mutuallyExclusiveElementAndAttribute(String elem, String att) throws SchemaException {
        error(indefiniteArticle(elem, "A") + ' ' + Err.wrap(elem, Err.ELEMENT) +
                " element must not appear as a child of " +
                indefiniteArticle(getDisplayName(), "a") + ' ' +
                Err.wrap(getDisplayName(), Err.ELEMENT) + " with " +
                indefiniteArticle(att, "a") + ' ' + Err.wrap(att, Err.ATTRIBUTE) + " attribute");
    }

    /**
     * This method is called when two attributes may not coexist
     *
     * @param name1 the name of the first attribute
     * @param name2 the name of the second attribute
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    protected void mutuallyExclusiveAttributes(String name1, String name2) throws SchemaException {
        error("The " + Err.wrap(name1, Err.ATTRIBUTE) + " and " + Err.wrap(name2, Err.ATTRIBUTE) + " attributes " +
                "must not both appear on the same " + Err.wrap(getDisplayName(), Err.ELEMENT) + " element");
    }

    /**
     * This method tests whether two attributes coexist and reports an error if they do
     *
     * @param name1 the local name of the first attribute (must be in the null namespace)
     * @param name2 the local name of the second attribute (must be in the null namespace)
     */

    protected void checkMutuallyExclusiveAttributes(String name1, String name2) throws SchemaException {
        AttributeMap atts = attributes();
        if (atts.getValue(NamespaceUri.NULL, name1) != null && atts.getValue(NamespaceUri.NULL, name2) != null) {
            mutuallyExclusiveAttributes(name1, name2);
        }
    }

    /**
     * This method is called when two child elements are found
     * in the wrong order.
     *
     * @param name1 the name of the element that should be first
     * @param name2 the name of the element that should be second
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    protected void mustPrecede(String name1, String name2) throws SchemaException {
        error("The " + Err.wrap(name1, Err.ELEMENT) + " must come before the " +
                Err.wrap(name2, Err.ELEMENT) + " element");
    }

    /**
     * This method is called when a child element is required to be the first
     * child, but is found elsewhere
     *
     * @param name the name of the illegal element
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    protected void mustBeFirstElement(String name) throws SchemaException {

        error(indefiniteArticle(name, "A") + ' ' + Err.wrap(name, Err.ELEMENT) +
                " element is allowed only as the first child of " + Err.wrap(getDisplayName(), Err.ELEMENT));
    }

    /**
     * Construct an indefinite article in English
     *
     * @param word  The word to follow the article
     * @param start Typically "a" or "A"
     * @return the indefinite article
     */

    protected String indefiniteArticle(String word, String start) {
        String article = start;
        if ("aeiouAEIOU".indexOf(word.charAt(0)) >= 0) {
            article += "n";
        } else if (word.startsWith("xs") || word.startsWith("xml")) {
            article += "n";
        }
        return article;
    }

    /**
     * This method is called when an invalid attribute value is found
     *
     * @param name    the name of the attribute
     * @param value   the supplied value of the attribute
     * @param message additional explanation of why it's wrong. May be null.
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    public void invalidAttributeValue(String name, String value, /*@Nullable*/ String message) throws SchemaException {
        String s = "The value " + Err.wrap(value, Err.VALUE) + " is not valid for the " +
                Err.wrap(name, Err.ATTRIBUTE) + " attribute of " + Err.wrap(getDisplayName(), Err.ELEMENT);
        if (message != null) {
            s += ". " + message;
        }
        error(s);
    }

    /**
     * This method is called when a required attribute is not found
     *
     * @param name the name of the attribute
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    public void missingAttribute(String name) throws SchemaException {
        String s = "The required attribute " + Err.wrap(name, Err.ATTRIBUTE) +
                " is missing from the " + Err.wrap(getDisplayName(), Err.ELEMENT) + " element";
        error(s);
    }

    /**
     * This method is called when a required child element is not found
     *
     * @param name the name of the required child element
     * @throws net.sf.saxon.type.SchemaException
     *          thrown if the errorListener
     *          decides that it's a fatal error.
     */
    public void missingChildElement(String name) throws SchemaException {
        String s = "The " + Err.wrap(getDisplayName(), Err.ELEMENT) +
                " element must have a child " + Err.wrap(name, Err.ELEMENT) + " element";
        error(s);
    }

    /**
     * Parse a list of blocked or final derivations. Used to process the block,
     * final, blockDefault, and finalDefault attributes.
     *
     * @param attrName   the name of the attribute being parsed
     * @param finalValue A list of values to be parsed, as a string,
     *                   for example "extension restriction"
     * @param allowed    The permitted values, as a bit-significant integer. For example
     *                   the value (DERIVE_BY_EXTENSION | DERIVE_BY_RESTRICTION) allows the values
     *                   "extension" and "restriction" to appear.
     * @return The final/blocked values, as a bit-significant integer
     * @throws net.sf.saxon.type.SchemaException
     *          if the value of the attribute is invalid
     */

    public int parseFinalOrBlock(String attrName, String finalValue, int allowed) throws SchemaException {
        if (Whitespace.trim(finalValue).equals("#all")) {
            return allowed;
        } else {
            int result = 0;
            StringTokenizer tokenizer = new StringTokenizer(finalValue, " \t\n\r", false);
            while (tokenizer.hasMoreTokens()) {
                String token = tokenizer.nextToken();
                int v;
                switch (token) {
                    case "union":
                        v = Derivation.DERIVATION_UNION;
                        break;
                    case "list":
                        v = Derivation.DERIVATION_LIST;
                        break;
                    case "restriction":
                        v = Derivation.DERIVATION_RESTRICTION;
                        break;
                    case "extension":
                        v = Derivation.DERIVATION_EXTENSION;
                        break;
                    case "substitution":
                        v = Derivation.DERIVE_BY_SUBSTITUTION;
                        break;
                    default:
                        String err = "Invalid " + Err.wrap(attrName, Err.ATTRIBUTE) + " value " +
                                Err.wrap(token, Err.VALUE);
                        error(err);
                        return 0;
                }
                if ((v & ~allowed) != 0) {
                    String err = "The " + Err.wrap(attrName, Err.ATTRIBUTE) + " value " +
                            Err.wrap(token, Err.VALUE) + " is not allowed here";
                    error(err);
                    return 0;
                }
                result |= v;
            }
            return result;
        }
    }

    /**
     * Process an @id attribute on the schema element: if present, check that it is a valid
     * ID, and that it is unique in the schema document
     *
     * @throws SchemaException if the id is present and is invalid or is a duplicate
     */
    public void processId() throws SchemaException {
        String id = attributes().getValue(NamespaceUri.NULL, "id");
        if (id != null) {
            SchemaException err = getSchemaNodeFactory().registerId(id, this);
            if (err != null) {
                err.setLocation(this);
                error(err);
            }
        }
    }


    /**
     * Allocate a name to an anonymous type. It's important that the name allocated
     * should be repeatable, because it can be referenced as a type annotation in a SEF document.
     *
     * @return the name code of the allocated name
     */

    protected NodeName allocateAnonymousTypeCode() {
        AxisIterator iter = iterateAxis(AxisInfo.PRECEDING_OR_ANCESTOR, NodeKindTest.ELEMENT);
        String moduleName = getSystemId();
        while (true) {
            int slash = moduleName.indexOf('/');
            if (slash < 0) {
                break;
            } else if (slash == moduleName.length() - 1) {
                moduleName = moduleName.substring(0, slash);
            } else {
                moduleName = moduleName.substring(slash + 1);
            }
        }
        if (!NameChecker.isValidNmtoken(StringView.of(moduleName))) {
            moduleName = "module" + moduleName.hashCode();
        }
        int count = 0;
        String topname = "";
        while (true) {
            SchemaElement node = (SchemaElement) iter.next();
            count++;
            if (node == null) {
                break;
            }
            if (node.isTopLevel()) {
                AttributeMap atts = node.attributes();
                topname = atts.getValue(NamespaceUri.NULL, "name");
                if (topname == null) {
                    topname = "";
                }
                break;
            }
        }
        int tries = 0;
        NamePool pool = getNamePool();
        String baseName = topname + "_anonymous_type_" + count + "_at_line_" + getLineNumber() + "_of_" + moduleName;
        String name = baseName;
        // Make sure the name has not been allocated before
        while (pool.getFingerprint(NamespaceUri.ANONYMOUS, name) != -1) {
            tries++;
            name = baseName + "_" + tries;
        }
        int fp = getNamePool().allocateFingerprint(NamespaceUri.ANONYMOUS, name);
        return new FingerprintedQName("anon", NamespaceUri.ANONYMOUS, name, fp);
    }

    /**
     * Process the value of a maxOccurs attribute on any element where it appears
     *
     * @param attValue the value of the attribute as written, either "unbounded" or an integer
     * @return the value of the attribute as an integer, or -1 indicating "unbounded"
     * @throws SchemaException if the value is invalid (in which case the error will have been reported).
     *                         If the value is a very high integer (greater than Integer.MAX_VALUE), the method outputs a warning
     *                         and treats the value as "unbounded".
     */

    public int processMaxOccurs(String attValue) throws SchemaException {
        attValue = Whitespace.trim(attValue);
        if (attValue.equals("unbounded")) {
            return -1;
        }
        try {
            ConversionResult maxOccursValue = IntegerValue.stringToInteger(attValue);
            if (maxOccursValue instanceof ValidationFailure) {
                invalidAttributeValue("maxOccurs", attValue, "Must be numeric or 'unbounded'");
            } else if (((IntegerValue) maxOccursValue).compareTo(Integer.MAX_VALUE) > 0) {
                warning("maxOccurs value exceeds Saxon limits: taken as unbounded");
            } else if (((IntegerValue) maxOccursValue).compareTo(0) < 0) {
                invalidAttributeValue("maxOccurs", attValue, "The value is negative");
            } else {
                return (int) ((IntegerValue) maxOccursValue).longValue();
            }
        } catch (XPathException e) {
            invalidAttributeValue("maxOccurs", attValue, "Must be numeric or 'unbounded'");
        }
        return -1;
    }

    /**
     * Process the value of a minOccurs attribute on any element where it appears
     *
     * @param attValue the value of the attribute as written, an integer
     * @return the value of the attribute as an integer
     * @throws SchemaException if the value is invalid (in which case the error will have been reported).
     *                         If the value is a very high integer (greater than Integer.MAX_VALUE), the method outputs a warning
     *                         and treats the value as Integer.MAX_VALUE.
     */

    public int processMinOccurs(String attValue) throws SchemaException {
        attValue = Whitespace.trim(attValue);
        try {
            ConversionResult minOccursValue = IntegerValue.stringToInteger(attValue);
            if (minOccursValue instanceof ValidationFailure) {
                invalidAttributeValue("minOccurs", attValue, "Must be numeric");
            } else if (((IntegerValue) minOccursValue).compareTo(Integer.MAX_VALUE) > 0) {
                warning("minOccurs value exceeds Saxon limits: taken as " + Integer.MAX_VALUE);
                return Integer.MAX_VALUE;
            } else if (((IntegerValue) minOccursValue).compareTo(0) < 0) {
                invalidAttributeValue("minOccurs", attValue, "The value is negative");
            } else {
                return (int) ((IntegerValue) minOccursValue).longValue();
            }
        } catch (XPathException e) {
            invalidAttributeValue("minOccurs", attValue, "Must be numeric");
        }
        return -1;
    }

    /**
     * Check the content model of an element in the schema. This uses a finite state machine generated into
     * the Java source code by applying an XSLT transformation to the schema component model for the Schema
     * for Schemas.
     * <p>The finite state machine is generated as follows: First process the S4S to create an SCM model,
     * then apply the stylesheet generate-S4S-code in the build directory with type=complexType, for example.
     * The value of the type parameter can be either the name of the type or its ID in the SCM file.
     *
     * @param stateTerms     an array of states, and for each state, an array containing the fingerprints
     *                       of elements allowed to appear in that state
     * @param stateTargets   an array of states, and for each state, an array containing the destinations
     *                       of transitions when the corresponding term from stateTerms is encountered
     * @param allowWildcards  true if wildcard transitions are allowed
     * @throws SchemaException if the schema being processed does not conform to the required content model
     */

    protected void checkContentModel(int[][] stateTerms, int[][] stateTargets, boolean allowWildcards)
            throws SchemaException {
        NamePool pool = getNamePool();
        int state = 0;
        AxisIterator iter = iterateAxis(AxisInfo.CHILD);
        NodeImpl child;

        while ((child = (NodeImpl)iter.next()) != null) {
            final int fp = child.getFingerprint();
            int[] terms = stateTerms[state];
            int[] targets = stateTargets[state];
            boolean continueOuter = false;
            for (int i = 0; i < terms.length; i++) {
                if (fp == terms[i]) {
                    state = targets[i];
                    continueOuter = true;
                    break;
                }
            }
            if (continueOuter) {
                continue;
            }
            if (allowWildcards && !NamespaceUri.SCHEMA.equals(child.getNamespaceUri())) {
                for (int i = 0; i < terms.length; i++) {
                    if (terms[i] == -1) {
                        state = targets[i];
                        continueOuter = true;
                        break;
                    }
                }
                if (continueOuter) {
                    continue;
                }
            }

            StringBuilder msg = new StringBuilder(64);
            if (child.getNodeKind() == Type.ELEMENT) {
                msg.append("Element " + Err.wrap(child.getDisplayName(), Err.ELEMENT) + " cannot appear here: expected ");
                if (terms.length == 0) {
                    msg.append("no more child elements");
                } else if (terms.length == 1) {
                    msg.append(terms[0] == -1 ? "*" : pool.getEQName(terms[0]));
                } else {
                    msg.append("one of {");
                    for (int i = 0; i < terms.length; i++) {
                        msg.append(terms[i] == -1 ? "*" : pool.getEQName(terms[i]));
                        msg.append(i == terms.length - 1 ? "}" : ", ");
                    }
                }

                if (isFinalState(state) && terms.length > 0) {
                    msg.append(" or nothing");
                }
            } else {
                msg.append("Text node '" + Err.wrap(child.getUnicodeStringValue()) + "' cannot appear here");
            }

            if (child instanceof SchemaElement) {
                ((SchemaElement) child).error(msg.toString());
            } else {
                error(msg.toString());
            }
        }
        if (!isFinalState(state)) {
            throw new SchemaException("Content of element is incomplete");
        }

    }

    /**
     * For elements whose content model is parsed using a finite state machine, test whether
     * a given state is a final state. The default implementation of this method is never used; it exists
     * for the benefit of subclasses that do not use this mechanism
     *
     * @param state the state to be tested
     * @return true if the given state is a final state
     */

    protected boolean isFinalState(int state) {
        return false;
    }

    /**
     * Process the namespaces attribute of XSDAny or XSDAnyAttribute
     *
     * @param attValue the value of the attribute
     * @param wildcard the wildcard object being constructed
     * @throws SchemaException if the attribute is in error
     */

    protected void processAllowedNamespaces(String attValue, Wildcard wildcard) throws SchemaException {
        // check if there is more than one namespace
        StringTokenizer tokenizer = new StringTokenizer(attValue, " \t\n\r", false);
        int count = tokenizer.countTokens();
        if (count == 0) {
            // the namespace attribute is present but empty: indicates no namespaces allowed
            wildcard.setNoNamespacesAllowed();
        } else {
            while (tokenizer.hasMoreTokens()) {
                //need to retrieve all the namespaces
                String ns = tokenizer.nextToken();
                //if there is more than one namespace ##any or ##other should not appear
                //appear
                if (count > 1 &&
                        (NAMESPACE_ANY.equals(ns) ||
                                NAMESPACE_OTHER.equals(ns))) {
                    error(ns + " is not valid when multiple namespaces are listed.");
                    return;
                }
                if (ns.equals(NAMESPACE_TARGET)) {
                    wildcard.addAllowedNamespace(getXSDSchema().getTargetNamespace());
                } else if (ns.equals(NAMESPACE_LOCAL)) {
                    wildcard.addAllowedNamespace(NamespaceUri.NULL);
                } else if (ns.equals(NAMESPACE_OTHER)) {
                    wildcard.addDisallowedNamespace(NamespaceUri.NULL);
                    wildcard.addDisallowedNamespace(getXSDSchema().getTargetNamespace());
                } else if (ns.equals(NAMESPACE_ANY)) {
                    // do nothing, this is the default
                } else if (ns.contains("##")) {
                    // the rules say it must be an anyURI. But we don't want to be
                    // over-restrictive. But we do want to catch mis-spellings (and
                    // pass the test suite...)
                    error("Invalid namespace URI " + Err.wrap(ns));
                } else {
                    wildcard.addAllowedNamespace(NamespaceUri.of(ns));
                }
            }
        }
    }

    /**
     * Process the notNamespaces attribute of XSDAny or XSDAnyAttribute
     *
     * @param attValue the value of the attribute
     * @param wildcard the wildcard object being constructed
     * @throws SchemaException if the attribute is in error
     */

    protected void processDisallowedNamespaces(String attValue, Wildcard wildcard) throws SchemaException {
        // check if there is more than one namespace
        StringTokenizer tokenizer = new StringTokenizer(attValue, " \t\n\r", false);
        while (tokenizer.hasMoreTokens()) {
            //need to retrieve all the namespaces
            String ns = tokenizer.nextToken();
            if (ns.equals(XSDAny.NAMESPACE_TARGET)) {
                wildcard.addDisallowedNamespace(getXSDSchema().getTargetNamespace());
            } else if (ns.equals(XSDAny.NAMESPACE_LOCAL)) {
                wildcard.addDisallowedNamespace(NamespaceUri.NULL);
            } else if (ns.contains("##")) {
                // the rules say it must be an anyURI. But we don't want to be
                // over-restrictive. But we do want to catch mis-spellings (and
                // pass the test suite...)
                error("Invalid namespace URI " + Err.wrap(ns));
            } else {
                wildcard.addDisallowedNamespace(NamespaceUri.of(ns));
            }
        }
    }

    /**
     * Process the notQNames attribute of XSDAny or XSDAnyAttribute
     *
     * @param attValue the value of the attribute
     * @param wildcard the wildcard object being constructed
     * @throws SchemaException if the attribute is in error
     */

    protected void processDisallowedQNames(String attValue, Wildcard wildcard) throws SchemaException {
        // check if there is more than one namespace
        StringTokenizer tokenizer = new StringTokenizer(attValue, " \t\n\r", false);
        while (tokenizer.hasMoreTokens()) {
            String qname = tokenizer.nextToken();
            if (qname.equals("##defined")) {
                wildcard.setDisallowDefinedNames(true);
            } else if (qname.equals("##definedSibling")) {
                wildcard.setDisallowDefinedSiblings(true);
            } else {
                int fp = -1;
                try {
                    fp = getFingerprint(qname, USE_DEFAULT_NS_WITHOUT_IMPORT);
                } catch (SchemaException e) {
                    error(e.getMessage());
                    continue;
                }
                NamespaceUri uri = getNamePool().getURI(fp);
                if (!wildcard.matches(uri)) {
                    error("QName " + Err.wrap(qname, Err.GENERAL) +
                            " is in a namespace that is not allowed by the wildcard");
                }
                wildcard.addDisallowedQName(fp);
            }
        }
    }

    /**
     * For an element or attribute declaration with a targetNamespace attribute (XSD 1.1), check the
     * highly-paternalistic rules for when this attribute is permitted.
     *
     * @param elementOrAttribute the string "element" or "attribute" for error messages
     * @throws SchemaException if the rules are not satisfied
     */

    public void checkLocalTargetNamespace(String elementOrAttribute) throws SchemaException {
        // only allowed for a descendant of xs:restriction
        AxisIterator ancestors = iterateAxis(AxisInfo.ANCESTOR,
                new NameTest(Type.ELEMENT, StandardNames.XS_RESTRICTION, getNamePool()));
        NodeInfo restriction = ancestors.next();
        if (restriction == null) {
            error("A local targetNamespace is allowed only for an " +
                    elementOrAttribute + " declared within an xs:restriction");
        } else {
            ancestors = iterateAxis(AxisInfo.ANCESTOR,
                    new NameTest(Type.ELEMENT, StandardNames.XS_COMPLEX_TYPE, getNamePool()));
            NodeInfo complexType = ancestors.next();
            if (complexType == null) {
                error("A local targetNamespace is allowed only within a complex type restriction");
            } else {
                UserComplexType ct = ((XSDComplexType) complexType).getComplexType();
                if (ct != null && ct.getBaseTypeFingerprint() == StandardNames.XS_ANY_TYPE) {
                    error("A local targetNamespace is not allowed on an " +
                            elementOrAttribute + " within a restriction of xs:anyType");
                }
            }
        }
    }

    protected static class XPathStaticContext extends IndependentContext {

        private final SchemaElement schemaElement;

        public XPathStaticContext(SchemaElement schemaElement) {
            super(schemaElement.getConfiguration());
            this.schemaElement = schemaElement;
            Configuration config = schemaElement.getConfiguration();
            getPackageData().setSchemaAware(true);

            FunctionLibraryList lib = new FunctionLibraryList();
            lib.addFunctionLibrary(XPath20FunctionSet.getInstance());
            lib.addFunctionLibrary(getConfiguration().getBuiltInExtensionLibraryList(31));
            lib.addFunctionLibrary(new ConstructorFunctionLibrary(getConfiguration()));
            lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(lib);
            setFunctionLibrary(lib);

            setXPathLanguageLevel(20);
        }

        /**
         * Get a Namespace resolver that can be used for run-time resolution of QNames appearing
         * in the XPath expression
         *
         * @return a namespace resolver that uses the namespace context of the containing element
         *         in the schema document
         */

        @Override
        public NamespaceResolver getNamespaceResolver() {
            return schemaElement.getAllNamespaces();
        }

        /*@Nullable*/
        @Override
        public Executable getExecutable() {
            try {
                return schemaElement.getXSDSchema().getExecutable();
            } catch (SchemaException e) {
                return null;
            }
        }
    }
}

// Copyright (c) 2004-2023 Saxonica Limited