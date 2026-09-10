package com.saxonica.ee.validate;

import com.saxonica.ee.schema.Assertion;
import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.UserComplexType;
import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.adjunct.BooleanFnFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.SinkFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Controller;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.sxpath.XPathDynamicContext;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingIncident;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.tree.linked.ElementImpl;
import net.sf.saxon.tree.linked.LinkedTreeBuilder;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.wrapper.VirtualCopy;
import net.sf.saxon.type.*;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Whitespace;

import java.util.*;


/**
 * This class manages the receivers that do element content
 * validation. It maintains a stack of Validators, each of which does
 * local element validation of the contents of a single element,
 * adding a new validator to the stack to handle each layer of nesting.
 */

public class ValidationStack extends ValidatingFilter {

    private final Stack<ContentValidator> stack = new Stack<>();
    private final Stack<Location> locationStack = new Stack<>();
//    private boolean contentStarted = false;
    private final Receiver outputReceiver;
    private ContentValidator initialValidator;
    private List<ValidationFailure> xsiValidationErrors;
    private final int initialValidationMode;
    private final SchemaType initialType;
    private boolean isNilled = false;
    private NodeName topLevelElement = null;
    private final AssertionTreeBuilder assertionBuilder;
    private final WatchManager watchManager;
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
        assertionBuilder = (AssertionTreeBuilder) pipe.getComponent("com.saxonica.ee.validate.AssertionTreeBuilder");
        watchManager = (WatchManager) pipe.getComponent("com.saxonica.ee.stream.watch.WatchManager");
    }

    @Override
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

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        locationStack.push(location.saveLocation());
        SchemaType xsiType = getValidationContext().getXSIType();
        if (isNilled) {
            properties |= ReceiverOption.NILLED_ELEMENT;
        }
        PipelineConfiguration pipe = getPipelineConfiguration();
        try {
            int fp = elemName.obtainFingerprint(getNamePool());
            if (stack.isEmpty()) {

                // Handle the top-level element of the tree being validated

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
                                    initialValidationMode, pipe,
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
                        if (decl != null && !decl.isNillable() && attributes.get(NamespaceUri.SCHEMA_INSTANCE, "nil") != null) {
                            String message = "The element is not nillable, so xsi:nil must not be present";
                            ValidationFailure ve = new ValidationFailure(message);
                            ve.setConstraintReference(1, "cvc-elt", "3.1");
                            xsiValidationErrors.add(ve);
                        }

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
                initial.setNamespaceResolver(namespaces);
                initial.setNilled(isNilled);

                if (!(initial instanceof SkipValidator) && !xsiValidationErrors.isEmpty()) {
                    // Errors in xsi attributes are not reported when doing skip validation
                    for (ValidationFailure err : xsiValidationErrors) {
                        reportValidationError(err, false, location);
                    }
                }

                SchemaType initialSchemaType = initial.getSchemaType();

                List<NamespaceBinding> additionalNamespaces = new ArrayList<>();
                AttributeMap validatedAttributes = initial.validateAttributes(attributes, additionalNamespaces);
                for (NamespaceBinding ns : additionalNamespaces) {
                    namespaces = namespaces.put(ns.getPrefix(), ns.getNamespaceUri());
                }

                if (initial instanceof SimpleContentValidator && initialSchemaType.isIdType()) {
                    pipe.getErrorReporter().report(
                            new XmlProcessingIncident("During validation, using an ID element at the outermost level has no effect",
                                                      SaxonErrorCode.SXWN9048).asWarning());
                }

                ConstraintChecker constraintChecker = getConstraintChecker();
                if (constraintChecker != null) {
                    ElementDecl element = initial.getElementDeclaration();
                    if (element != null) {
                        constraintChecker.setElementDeclaration(element);
                    }
                }
                outputReceiver.startElement(elemName, annotation, validatedAttributes, namespaces, location, properties);

                if (initialSchemaType instanceof UserComplexType && ((UserComplexType) initialSchemaType).hasAssertions()) {
                    prepareForAssertions(elemName, initialSchemaType, validatedAttributes, namespaces, location, properties, pipe);
                }

                stack.push(initial);

            } else {
                // validate any element except the outermost one

//                if (!contentStarted) {
//                    startContent();
//                }
                // v0 is the validator for the current element (the parent of the one just starting)
                ContentValidator v0 = stack.peek();
                v0.startElement(elemName, typeCode, attributes, namespaces, location, properties);

                // v1 is the validator for the new element (the one just starting)
                ContentValidator v1 = v0.getChildValidator();
                v1.setValidationContext(getValidationContext());
                v1.setUnderlyingReceiver(outputReceiver);
                v1.setNamespaceResolver(namespaces);
                if (statistics != null) {
                    incrementStatistics(v1.getElementDeclaration());
                    incrementStatistics(v1.getSchemaType());
                }
                setUnderlyingReceiver(v1);
                stack.push(v1);
//                contentStarted = false;

                if (!(v1 instanceof SkipValidator) && !xsiValidationErrors.isEmpty()) {
                    // Errors in xsi attributes are not reported when doing skip validation
                    for (ValidationFailure err : xsiValidationErrors) {
                        reportValidationError(err, false, location);
                    }
                }

                SchemaType type = v1.getSchemaType();
                if (type instanceof UserComplexType && ((UserComplexType) type).hasAssertions()) {
                    List<NamespaceBinding> additionalNamespaces = new ArrayList<>();
                    AttributeMap validatedAtts = v1.validateAttributes(attributes, additionalNamespaces);
                    for (NamespaceBinding ns : additionalNamespaces) {
                        namespaces = namespaces.put(ns.getPrefix(), ns.getNamespaceUri());
                    }
                    prepareForAssertions(elemName, type, validatedAtts, namespaces, location, properties, pipe);
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

    private void prepareForAssertions(NodeName elemName, SchemaType type, AttributeMap validatedAttributes, NamespaceMap namespaces, Location location, int properties, PipelineConfiguration pipe) throws XPathException {
        Set<Assertion> assertions = ((UserComplexType) type).getAssertions();
        boolean buildTree = false;
        ElementImpl stub = null;
        for (Assertion a : assertions) {
            if (a.getSweep() == Sweep.MOTIONLESS) {
                if (stub == null) {
                    LinkedTreeBuilder stubBuilder = new LinkedTreeBuilder(pipe, Durability.TEMPORARY);
                    stubBuilder.open();
                    stubBuilder.startElement(elemName, Untyped.INSTANCE, validatedAttributes, namespaces, location, properties);
                    stubBuilder.endElement();
                    stubBuilder.close();
                    stub = (ElementImpl)stubBuilder.getCurrentRoot();
                }
                Controller controller = getValidationContext().getController();
                XPathDynamicContext context = a.getCondition().createDynamicContext(controller, stub);
                boolean ok;
                try {
                    ok = a.getCondition().effectiveBooleanValue(context);
                } catch (XPathException err) {
                    ok = false;
                }
                if (!ok) {
                    String message = "Element " + elemName + " does not satisfy assertion";
                    if (a.getMessage() != null) {
                        message += ". " + a.getMessage();
                    } else {
                        message += " " + Whitespace.collapseWhitespace(StringView.of(a.getConditionText()).tidy());
                    }

                    ValidationFailure error = new ValidationFailure(message);
                    error.setLocator(location);
                    error.setConstraintReference(1, "sec-cvc-assertion", "0");
                    reportValidationError(error, true, location);
                }
            } else if (a.getSweep() == Sweep.CONSUMING && builder == null) {
                Controller controller = getValidationContext().getController();
                XPathContext context = controller.newXPathContext();
                context.setCurrentIterator(new ManualIterator(watchManager.getCurrentNode()));
                Inversion inversion = Inversion.invertExpression(a.getCondition().getInternalExpression(), false);
                    // TODO: precompute the inversion

                ItemFeed assertionResultHandler = new AssertionResultHandler(this, a, elemName, context);

                assertionResultHandler.setPipelineConfiguration(pipe);
                ItemFeed booleanCapture = new BooleanFnFeed.Positive(watchManager, null, assertionResultHandler, context);
                Trigger assertionWatch = inversion.getWatch(watchManager, booleanCapture, context);
                watchManager.addWatch(assertionWatch, true);
                watchManager.restartElement(assertionWatch);
            } else {
                buildTree = true;
            }
        }
        if (buildTree && builder == null) {
            builder = new TinyBuilder(pipe);
            builder.setStatistics(pipe.getConfiguration().getTreeStatistics().ASSERTION_TREE_STATISTICS);
            //builder.setLineNumbering(true);
            builder.setSystemId(location.getSystemId());
            builder.setBaseURI(location.getSystemId());
            builder.open();
            builder.startElement(elemName, Untyped.INSTANCE, validatedAttributes, namespaces, location, properties);
            builderDepth = 0;
            assertionBuilder.activate(builder);
        }
    }

    private static class AssertionResultHandler extends SinkFeed {

        private final ValidationStack validationStack;
        private final Assertion assertion;
        private final NodeName elementName;

        public AssertionResultHandler(ValidationStack validationStack,
                                      Assertion assertion,
                                      NodeName elementName,
                                      XPathContext context) {
            super(context);
            this.validationStack = validationStack;
            this.assertion = assertion;
            this.elementName = elementName;
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append (Item item) throws XPathException {
            if (item instanceof BooleanValue && !((BooleanValue) item).getBooleanValue()) {
                String message = "Element " + elementName + " does not satisfy assertion";
                if (assertion.getMessage() != null) {
                    message += ". " + assertion.getMessage();
                } else {
                    message += " " + Whitespace.collapseWhitespace(StringView.of(assertion.getConditionText()).tidy());
                }

                ValidationFailure error = new ValidationFailure(message);
                error.setLocator(assertion);
                error.setConstraintReference(1, "sec-cvc-assertion", "0");
                validationStack.reportValidationError(error, true, assertion);
            }
        }

        @Override
        public boolean handlesAppend () {
            return true;
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

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
//        if (!contentStarted) {
//            startContent();
//        }
        ContentValidator v0 = stack.peek();
        v0.characters(chars, locationId, properties);
    }

    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        Location location = locationStack.pop();
//        if (!contentStarted) {
//            startContent();
//        }
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
                    //builder.endElement();
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

    @Override
    public void endDocument() throws XPathException {
        try {
            nextReceiver.endDocument();
            reportIfInvalid();
            if (statistics != null) {
                getPipelineConfiguration().getParseOptions().
                        getValidationStatisticsRecipient().notifyValidationStatistics(statistics);
            }
        } catch (SaxonApiException e) {
            throw XPathException.makeXPathException(e);
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
            UnicodeString s = root.getUnicodeStringValue();
            SimpleType st = type.getSimpleContentType();
            NamespaceResolver ns = st.isNamespaceSensitive() ? root.getAllNamespaces() : null;
            value = st.getTypedValue(s, ns, getConfiguration().getConversionRules());
        }
        for (Assertion assertion : assertions) {
            if (assertion.getSweep() != Sweep.MOTIONLESS) {
                testAssertion(assertion, root, value, location);
            }
        }
    }

    private void testAssertion(Assertion assertion, NodeInfo root, AtomicSequence value, Location location) throws XPathException {
        List<NodeInfo> offendingItems = assertion.testComplex(root, value, getValidationContext());
        if (!offendingItems.isEmpty()) {
            String message = "Element " + root.getDisplayName() + " does not satisfy assertion";
            if (assertion.getMessage() != null) {
                message += ". " + assertion.getMessage();
            } else {
                message += " " + Whitespace.collapseWhitespace(StringView.tidy(assertion.getConditionText()));
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

// Copyright (c) 2004-2023 Saxonica Limited


