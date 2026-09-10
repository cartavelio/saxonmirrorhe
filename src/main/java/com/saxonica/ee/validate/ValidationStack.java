package com.saxonica.ee.validate;

import com.saxonica.ee.schema.Assertion;
import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.UserComplexType;
import net.sf.saxon.Controller;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.wrapper.VirtualCopy;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Whitespace;

import java.util.*;


/**
 * This class manages the receivers that do element content
 * validation. It maintains a stack of Validators, each of which does
 * local element validation of the contents of a single element,
 * adding a new validator to the stack to handle each layer of nesting.
 */

public class ValidationStack extends ValidatingFilter {

    private Stack<ContentValidator> stack = new Stack<>();
    private Stack<Location> locationStack = new Stack<>();
    private boolean contentStarted = false;
    private Receiver outputReceiver;
    private ContentValidator initialValidator;
    private List<ValidationFailure> xsiValidationErrors;
    private int initialValidationMode;
    private SchemaType initialType;
    private boolean isNilled = false;
    private NodeName topLevelElement = null;
    private AssertionTreeBuilder assertionBuilder;
    private TinyBuilder builder;
    private int builderDepth;
    private Map<SchemaComponent, Integer> statistics;

    /**
     * Create a validation stack
     *
     * @param next                  the destination of events after passing through validation
     * @param initialValidationMode the initial validation mode, strict/lax
     * @param initialType           the initial type to validate against, if any
     */

    public ValidationStack(Receiver next, int initialValidationMode, SchemaType initialType) {
        super(next);
        outputReceiver = next;
        this.initialValidationMode = initialValidationMode;
        this.initialType = initialType;
        final PipelineConfiguration pipe = next.getPipelineConfiguration();
        setPipelineConfiguration(pipe);
        assertionBuilder = (AssertionTreeBuilder) pipe.getComponent(AssertionTreeBuilder.class.getName());
    }

    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        if (pipe.getController() == null) {
            // create a controller so that all assertions in the validation episode share the same
            // document pool
            Controller controller = new Controller(pipe.getConfiguration());
            controller.getExecutable().setSchemaAware(true);
            pipe.setController(controller);
        }
        if (pipe.getParseOptions().getValidationStatisticsRecipient() != null) {
            setKeepStatistics(true);
        }
    }

    /**
     * Set the name of the required top-level element (if any)
     *
     * @param name the name of the element that must appear at the root of the subtree
     *             being validated
     */

    public void setTopLevelElement(NodeName name) {
        topLevelElement = name;
    }

    /**
     * Get the name of the required top-level element (if any)
     *
     * @return the name of the element that must appear at the root of the subtree
     *         being validated, or null if none has been set
     */

    public NodeName getTopLevelElement() {
        return topLevelElement;
    }

    /**
     * Set the URI for an unparsed entity in the document.
     */
    @Override
    public void setUnparsedEntity(String name, String uri, String publicId) throws XPathException {
        super.setUnparsedEntity(name, uri, publicId);
        ConstraintChecker cc = getConstraintChecker();
        if (cc != null) {
            cc.setUnparsedEntity(name, uri, publicId);
        }
    }

    /**
     * Indicate whether the current element is nilled
     *
     * @param nilled true if xsi:nil is set
     */

    public void setIsNilled(boolean nilled) {
        isNilled = nilled;
    }

    /**
     * Supply a list of errors found in attributes in the XSI namespace. This is called
     * immediately before calling startElement on the element containing these erroneous
     * attributes
     * @param errors the list of errors
     */

    public void setXsiValidationErrors(List<ValidationFailure> errors) {
        xsiValidationErrors = errors;
    }

    /**
     * Process element start tag
     */

    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties)
            throws XPathException {
        locationStack.push(location.saveLocation());
        SchemaType xsiType = getValidationContext().getXSIType();
        if (isNilled) {
            properties |= ReceiverOptions.NILLED_ELEMENT;
        }
        PipelineConfiguration pipe = getPipelineConfiguration();
        try {
            int fp = elemName.obtainFingerprint(getNamePool());
            if (stack.isEmpty()) {

                ContentValidator initial;
                if (initialValidator != null && xsiType == null) {
                    initial = initialValidator;

                } else {
                    // get a validator to validate the outermost element

                    if (topLevelElement != null) {
                        if (!topLevelElement.equals(elemName)) {
                            String required = Err.wrap(topLevelElement.getDisplayName(), Err.ELEMENT);
                            String actual = Err.wrap(elemName.getDisplayName(), Err.ELEMENT);
                            if (required.equals(actual)) {
                                // means the mismatch is in the namespace URIs
                                required = topLevelElement.getStructuredQName().getEQName();
                                actual = elemName.getStructuredQName().getEQName();
                            }
                            ValidationFailure ve = new ValidationFailure("Top-level element name is " +
                                    actual + ", required name is " + required);
                            reportValidationError(ve, false, location);
                        }
                    }

                    Receiver initialReceiver = getNextReceiver();
                    List<ValidationFailure> failures = new ArrayList<>(1);
                    if (xsiType != null) {
                        if (initialReceiver instanceof EmptyContentValidator ||
                                initialReceiver instanceof AnyTypeValidator ||
                                initialReceiver instanceof LaxValidator) {
                            initial = ContentValidator.makeValidatorForType(
                                    null, xsiType, pipe, outputReceiver, failures);
                        } else if (initialReceiver instanceof SimpleContentValidator) {
                            SchemaType type = ((SimpleContentValidator) initialReceiver).getSchemaType();
                            try {
                                getConfiguration().checkTypeDerivationIsOK(xsiType, type, 0);
                            } catch (SchemaException e) {
                                throw new XPathException(e);
                            }
                            ((SimpleContentValidator) initialReceiver).setSchemaType(xsiType);
                            initial = (ContentValidator) getNextReceiver();
                        } else if (initialReceiver instanceof ComplexContentValidator) {
                            SchemaType type = ((ComplexContentValidator) initialReceiver).getSchemaType();
                            try {
                                getConfiguration().checkTypeDerivationIsOK(xsiType, type, 0);
                            } catch (SchemaException e) {
                                throw new XPathException(e);
                            }
                            ((ComplexContentValidator) initialReceiver).setSchemaType((UserComplexType) xsiType);
                            initial = (ContentValidator) getNextReceiver();
                        } else {
                            ElementDecl decl = (ElementDecl) getConfiguration().getElementDeclaration(fp);
                            initial = ContentValidator.makeValidator(decl, elemName.getStructuredQName(), location,
                                    getValidationContext(), xsiType,
                                    initialValidationMode,
                                                                     pipe,
                                    outputReceiver, failures);
                        }
                    } else if (initialReceiver instanceof ContentValidator) {
                        initial = (ContentValidator) getNextReceiver();
                    } else if (initialType != null) {
                        initial = ContentValidator.makeValidatorForType(
                                null, initialType, pipe, outputReceiver, failures);
                    } else {
                        ElementDecl decl = (ElementDecl) getConfiguration().getElementDeclaration(fp);
                        initial = ContentValidator.makeValidator(
                                decl, elemName.getStructuredQName(), location, getValidationContext(), null, initialValidationMode,
                                pipe, outputReceiver, failures);
                    }
                    if (!failures.isEmpty()) {
                        reportValidationError(failures.get(0), false, location);
                        initial = new LaxValidator(getNextReceiver());
                    }
                    assert initial != null;
                    initial.setValidationContext(getValidationContext());
                }
                SchemaType annotation = initial.getAnnotation();
                initial.setConstraintChecker(getConstraintChecker());
                if (isNilled) {
                    properties |= ReceiverOptions.NILLED_ELEMENT;
                }

                if (!(initial instanceof SkipValidator) && !xsiValidationErrors.isEmpty()) {
                    // Errors in xsi attributes are not reported when doing skip validation
                    for (ValidationFailure err : xsiValidationErrors) {
                        reportValidationError(err, false, location);
                    }
                }

                SchemaType type = initial.getSchemaType();

                if (initial instanceof SimpleContentValidator && type.isIdType()) {
                    pipe.getErrorListener().warning(
                            new XPathException("During validation, using an ID element at the outermost level has no effect"));
                }

                ConstraintChecker constraintChecker = getConstraintChecker();
                if (constraintChecker != null) {
                    ElementDecl element = initial.getElementDeclaration();
                    if (element != null) {
                        constraintChecker.setElementDeclaration(element);
                    }
                }
                outputReceiver.startElement(elemName, annotation, location, properties);

                if (type instanceof UserComplexType && ((UserComplexType) type).hasAssertions()) {

                    builder = new TinyBuilder(pipe);
                    builder.setStatistics(pipe.getConfiguration().getTreeStatistics().ASSERTION_TREE_STATISTICS);
                    builder.setLineNumbering(true);
                    builder.open();
                    builder.startElement(elemName, typeCode, location, properties);
                    NamespaceResolver nr = getValidationContext().getStartTagBuffer();
                    for (Iterator<String> iter = nr.iteratePrefixes(); iter.hasNext(); ) {
                        String prefix = iter.next();
                        String uri = nr.getURIForPrefix(prefix, true);
                        builder.namespace(new NamespaceBinding(prefix, uri), 0);
                    }
                    builderDepth = 0;
                    assertionBuilder.activate(builder);
                }

                stack.push(initial);

            } else {
                // validate any element except the outermost one

                if (!contentStarted) {
                    startContent();
                }
                // v0 is the validator for the current element (the parent of the one just starting)
                ContentValidator v0 = stack.peek();
                //v0.setXSIType(xsiType);
                //v0.setStartTagBuffer(getStartTagBuffer());
                v0.startElement(elemName, typeCode, location, properties);

                // v1 is the validator for the new element (the one just starting)
                ContentValidator v1 = v0.getChildValidator();
                v1.setValidationContext(getValidationContext());
                v1.setUnderlyingReceiver(outputReceiver);
                if (statistics != null) {
                    incrementStatistics(v1.getElementDeclaration());
                    incrementStatistics(v1.getSchemaType());
                }
                setUnderlyingReceiver(v1);
                stack.push(v1);
                contentStarted = false;

                if (!(v1 instanceof SkipValidator) && !xsiValidationErrors.isEmpty()) {
                    // Errors in xsi attributes are not reported when doing skip validation
                    for (ValidationFailure err : xsiValidationErrors) {
                        reportValidationError(err, false, location);
                    }
                }

                if (builder == null) {
                    SchemaType type = v1.getSchemaType();
                    if (type instanceof UserComplexType && ((UserComplexType) type).hasAssertions()) {
                        builder = new TinyBuilder(pipe);
                        builder.setStatistics(pipe.getConfiguration().getTreeStatistics().ASSERTION_TREE_STATISTICS);
                        builder.open();
                        builder.startElement(elemName, typeCode, location, properties);
                        NamespaceResolver nr = getValidationContext().getStartTagBuffer();
                        for (Iterator<String> iter = nr.iteratePrefixes(); iter.hasNext(); ) {
                            String prefix = iter.next();
                            String uri = nr.getURIForPrefix(prefix, true);
                            builder.namespace(new NamespaceBinding(prefix, uri), 0);
                        }
                        builderDepth = 0;
                        assertionBuilder.activate(builder);
                    }
                }

            }
            if (builder != null) {
                builderDepth++;
            }
        } catch (ValidationException ve) {
            if (!ve.hasBeenReported()) {
                reportValidationError(ve.getValidationFailure(), true, location);
                ve.setHasBeenReported(true);
            }
            if (!pipe.isRecoverFromValidationErrors()) {
                throw ve;
            }
        }
    }

    /**
     * Set the initial validator to be used for the outermost element
     *
     * @param validator the validator to be used for the outermost element being validated
     */

    public void setInitialValidator(ContentValidator validator) {
        initialValidator = validator;
    }

    /**
     * Handle an attribute
     *
     * @param nameCode   integer identifying the name of the attribute
     * @param typeCode   integer identifying the pre-validation type annotation (typically untypedAtomic)
     * @param value      the value of the attribute
     * @param locationId location in the instance document of the element being validated
     * @param properties additional properties of the attribute  @throws net.sf.saxon.trans.XPathException
     *          On any failure to write the attribute
     */

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties)
            throws XPathException {
        ContentValidator v0 = stack.peek();
        v0.setStartTagBuffer(getStartTagBuffer());
        v0.attribute(nameCode, typeCode, value, locationId, properties);
        if (statistics != null && v0 instanceof AttributeValidator) {
            SimpleType type = ((AttributeValidator) v0).getMostRecentAttributeType();
            if (type != null) {
                incrementStatistics(type);
            }

        }
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */

    public final void startContent() throws XPathException {
        contentStarted = true;
        ContentValidator v0 = stack.peek();
        v0.startContent();
    }

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        if (!contentStarted) {
            startContent();
        }
        ContentValidator v0 = stack.peek();
        v0.characters(chars, locationId, properties);
    }

    /**
     * End of element
     */

    public void endElement() throws XPathException {
        Location location = locationStack.pop();
        if (!contentStarted) {
            startContent();
        }
        ContentValidator v0 = stack.pop();
        v0.endElement();

        // If we're building a tree for assertion checking, then
        //   (a) keep building the tree, or finish building it if the outermost assertion is reached
        //   (b) test the value of any assertions defined at this level.

        if (builder != null) {
            //builder.endElement();
            builderDepth--;
            SchemaType type = v0.getSchemaType();
            if (type instanceof UserComplexType && ((UserComplexType) type).hasAssertions()) {
                NodeInfo root = builder.getLastCompletedElement();
                if (builderDepth != 0) {
                    // make a logical copy of the node to prevent straying outside the subtree
                    root = VirtualCopy.makeVirtualCopy(root);
                } else {
                    //builder.endElement();   // bug 4124
                }
                testAssertions(root, (UserComplexType) type, location);
            }
            if (builderDepth == 0) {
                builder.close();
                assertionBuilder.deactivate();
                builder = null; // enables garbage collection of the tree
            }
        }

        if (!stack.isEmpty()) {
            setUnderlyingReceiver(stack.peek());
        } else {
            setUnderlyingReceiver(outputReceiver);
            reportIfInvalid();
        }
    }

    /**
     * Notify the end of a document node
     */

    public void endDocument() throws XPathException {
        nextReceiver.endDocument();
        reportIfInvalid();
        if (statistics != null) {
            getPipelineConfiguration().getParseOptions().
                    getValidationStatisticsRecipient().notifyValidationStatistics(statistics);
        }
    }


    /**
     * Test whether all the assertions on a given complex type are true
     *
     *
     * @param root the element whose assertions are to be tested
     * @param type the type on which the assertions are defined
     * @param location location in instance document of the element being validated
     * @throws ValidationException if any of the assertions are not true
     */

    private void testAssertions(NodeInfo root, UserComplexType type, Location location) throws XPathException {
        Set<Assertion> assertions = type.getAssertions();
        AtomicSequence value = null;
        if (type.isSimpleContent()) {
            CharSequence s = root.getStringValueCS();
            SimpleType st = type.getSimpleContentType();
            NamespaceResolver ns = st.isNamespaceSensitive() ? new InscopeNamespaceResolver(root) : null;
            value = st.getTypedValue(s, ns, getConfiguration().getConversionRules());
        }
        for (Assertion assertion : assertions) {
            List<NodeInfo> offendingItems = assertion.testComplex(root, value, getValidationContext());
            if (!offendingItems.isEmpty()) {
                String message = "Element " + root.getDisplayName() + " does not satisfy assertion";
                if (assertion.getMessage() != null) {
                    message += ". " + assertion.getMessage();
                } else {
                    message += " " + Whitespace.collapseWhitespace(assertion.getConditionText());
                }

                ValidationFailure error = new ValidationFailure(message);
                error.setLocator(location);
                error.setConstraintReference(1, "sec-cvc-assertion", "0");
                for (NodeInfo item : offendingItems) {
                    if (item != root) {
                        error.addOffendingNode(item);
                    }
                }
                reportValidationError(error, true, location);
            }
        }
    }

    /**
     * Increment statistics for the usage of schema components during a validation episode. Currently
     * we maintain statistics for the use of element declarations and types (both user-defined types
     * and built-in types)
     *
     * @param component the schema component whose usage count is to be incremented
     */

    private void incrementStatistics(SchemaComponent component) {
        statistics.merge(component, 1, (a, b) -> a + b);
    }

    /**
     * Ask that statistics be maintained for the duration of the validation episode. The statistics
     * indicate how often particular schema components (currently, element declarations and types)
     * have been used.
     *
     * @param keep true if statistics are to be kept, false if not
     */

    public void setKeepStatistics(boolean keep) {
        if (keep) {
            statistics = new HashMap<>();
        } else {
            statistics = null;
        }
    }

    /**
     * Get the statistics showing the usage of individual schema components during a validation episode
     *
     * @return a table showing for each component that has been used, the number of times it has been used.
     *         If statistics are not being maintained, returns null.
     */

    /*@Nullable*/
    public Map<SchemaComponent, Integer> getCoverageStatistics() {
        return statistics;
    }


}

// Copyright � 2004 - 2008 Saxonica Limited. No warranty.


