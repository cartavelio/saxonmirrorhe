using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class ValidationStack : Saxon.Eej.ee.validate.ValidatingFilter {
        private readonly System.Collections.Generic.Stack<Saxon.Eej.ee.validate.ContentValidator> stack = new System.Collections.Generic.Stack<Saxon.Eej.ee.validate.ContentValidator>();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.s9api.Location> locationStack = new System.Collections.Generic.Stack<Saxon.Hej.s9api.Location>();
        private readonly Saxon.Hej.@event.Receiver outputReceiver;
        private Saxon.Eej.ee.validate.ContentValidator initialValidator;
        private System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> xsiValidationErrors;
        private readonly int initialValidationMode;
        private readonly Saxon.Hej.type.SchemaType initialType;
        private bool isNilled = false;
        private Saxon.Hej.om.NodeName topLevelElement = null;
        private readonly Saxon.Eej.ee.validate.AssertionTreeBuilder assertionBuilder;
        private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private Saxon.Hej.tree.tiny.TinyBuilder builder;
        private int builderDepth;
        private System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaComponent,int> statistics;
        public ValidationStack(Saxon.Hej.@event.Receiver next, int initialValidationMode, Saxon.Hej.type.SchemaType initialType) : base (next) {
            outputReceiver = next;
            this.initialValidationMode = initialValidationMode;
            this.initialType = initialType;
            Saxon.Hej.@event.PipelineConfiguration pipe = next.getPipelineConfiguration();
            setPipelineConfiguration(pipe);
            assertionBuilder = (Saxon.Eej.ee.validate.AssertionTreeBuilder)(pipe.getComponent("com.saxonica.ee.validate.AssertionTreeBuilder"));
            watchManager = (Saxon.Eej.ee.stream.watch.WatchManager)(pipe.getComponent("com.saxonica.ee.stream.watch.WatchManager"));
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            if ((pipe.getController() == null)) {
                Saxon.Hej.Controller controller = new Saxon.Hej.Controller(pipe.getConfiguration());
                controller.getExecutable().setSchemaAware(true);
                pipe.setController(controller);
            }
            if ((pipe.getParseOptions().getValidationStatisticsRecipient() != null)) {
                setKeepStatistics(true);
            }
        }
        public void setTopLevelElement(Saxon.Hej.om.NodeName name) {
            topLevelElement = name;
        }
        public Saxon.Hej.om.NodeName getTopLevelElement() {
            return topLevelElement;
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
            base.setUnparsedEntity(name, uri, publicId);
            Saxon.Eej.ee.validate.ConstraintChecker cc = getConstraintChecker();
            if ((cc != null)) {
                cc.setUnparsedEntity(name, uri, publicId);
            }
        }
        public void setIsNilled(bool nilled) {
            isNilled = nilled;
        }
        public void setXsiValidationErrors(System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> errors) {
            xsiValidationErrors = errors;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            locationStack.Push(location.saveLocation());
            Saxon.Hej.type.SchemaType xsiType = getValidationContext().getXSIType();
            if (isNilled) {
                properties |= Saxon.Hej.@event.ReceiverOption.NILLED_ELEMENT;
            }
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            try {
                int fp = elemName.obtainFingerprint(getNamePool());
                if ((stack.Count==0)) {
                    Saxon.Eej.ee.validate.ContentValidator initial;
                    if (((initialValidator != null) && (xsiType == null))) {
                        initial = initialValidator;
                    } else {
                        if ((topLevelElement != null)) {
                            if (!(topLevelElement.Equals(elemName))) {
                                string required = Saxon.Hej.trans.Err.wrap(topLevelElement.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT);
                                string actual = Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT);
                                if (required.Equals(actual)) {
                                    required = topLevelElement.getStructuredQName().getEQName();
                                    actual = elemName.getStructuredQName().getEQName();
                                }
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Top-level element name is "+actual)+", required name is ")+required));
                                reportValidationError(ve, false, location);
                            }
                        }
                        Saxon.Hej.@event.Receiver initialReceiver = getNextReceiver();
                        System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>(1);
                        if ((xsiType != null)) {
                            if ((((initialReceiver is Saxon.Eej.ee.validate.EmptyContentValidator) || (initialReceiver is Saxon.Eej.ee.validate.AnyTypeValidator)) || (initialReceiver is Saxon.Eej.ee.validate.LaxValidator))) {
                                initial = /*134*/Saxon.Eej.ee.validate.ContentValidator.makeValidatorForType(null, xsiType, pipe, outputReceiver, failures);
                            } else {
                                if ((initialReceiver is Saxon.Eej.ee.validate.SimpleContentValidator)) {
                                    Saxon.Hej.type.SchemaType type = ((Saxon.Eej.ee.validate.SimpleContentValidator)(initialReceiver)).getSchemaType();
                                    try {
                                        getConfiguration().checkTypeDerivationIsOK(xsiType, type, 0);
                                    } catch (Saxon.Hej.type.SchemaException e) {
                                        throw new Saxon.Hej.trans.XPathException(e);
                                    }
                                    ((Saxon.Eej.ee.validate.SimpleContentValidator)(initialReceiver)).setSchemaType(xsiType);
                                    initial = (Saxon.Eej.ee.validate.ContentValidator)(getNextReceiver());
                                } else {
                                    if ((initialReceiver is Saxon.Eej.ee.validate.ComplexContentValidator)) {
                                        Saxon.Hej.type.SchemaType type = ((Saxon.Eej.ee.validate.ComplexContentValidator)(initialReceiver)).getSchemaType();
                                        try {
                                            getConfiguration().checkTypeDerivationIsOK(xsiType, type, 0);
                                        } catch (Saxon.Hej.type.SchemaException e) {
                                            throw new Saxon.Hej.trans.XPathException(e);
                                        }
                                        ((Saxon.Eej.ee.validate.ComplexContentValidator)(initialReceiver)).setSchemaType((Saxon.Eej.ee.schema.UserComplexType)(xsiType));
                                        initial = (Saxon.Eej.ee.validate.ContentValidator)(getNextReceiver());
                                    } else {
                                        Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getConfiguration().getElementDeclaration(fp));
                                        initial = /*134*/Saxon.Eej.ee.validate.ContentValidator.makeValidator(decl, elemName.getStructuredQName(), location, getValidationContext(), xsiType, initialValidationMode, pipe, outputReceiver, failures);
                                    }
                                }
                            }
                        } else {
                            if ((initialReceiver is Saxon.Eej.ee.validate.ContentValidator)) {
                                initial = (Saxon.Eej.ee.validate.ContentValidator)(getNextReceiver());
                            } else {
                                if ((initialType != null)) {
                                    initial = /*134*/Saxon.Eej.ee.validate.ContentValidator.makeValidatorForType(null, initialType, pipe, outputReceiver, failures);
                                } else {
                                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getConfiguration().getElementDeclaration(fp));
                                    initial = /*134*/Saxon.Eej.ee.validate.ContentValidator.makeValidator(decl, elemName.getStructuredQName(), location, getValidationContext(), null, initialValidationMode, pipe, outputReceiver, failures);
                                    if ((((decl != null) && !(decl.isNillable())) && (attributes.get(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE, "nil") != null))) {
                                        string message = "The element is not nillable, so xsi:nil must not be present";
                                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                                        ve.setConstraintReference(1, "cvc-elt", "3.1");
                                        xsiValidationErrors.Add(ve);
                                    }
                                }
                            }
                        }
                        if (!(failures.Count==0)) {
                            reportValidationError(failures[0], false, location);
                            initial = new Saxon.Eej.ee.validate.LaxValidator(getNextReceiver());
                        }
                        System.Diagnostics.Debug.Assert((initial != null));
                        initial.setValidationContext(getValidationContext());
                    }
                    Saxon.Hej.type.SchemaType annotation = initial.getAnnotation();
                    initial.setConstraintChecker(getConstraintChecker());
                    initial.setNamespaceResolver(namespaces);
                    initial.setNilled(isNilled);
                    if ((!(((initial is Saxon.Eej.ee.validate.SkipValidator))) && !(xsiValidationErrors.Count==0))) {
                        foreach (Saxon.Hej.type.ValidationFailure err in xsiValidationErrors) {
                            reportValidationError(err, false, location);
                        }
                    }
                    Saxon.Hej.type.SchemaType initialSchemaType = initial.getSchemaType();
                    System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> additionalNamespaces = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
                    Saxon.Hej.om.AttributeMap validatedAttributes = initial.validateAttributes(attributes, additionalNamespaces);
                    foreach (Saxon.Hej.om.NamespaceBinding ns in additionalNamespaces) {
                        namespaces = namespaces.put(ns.getPrefix(), ns.getURI());
                    }
                    if (((initial is Saxon.Eej.ee.validate.SimpleContentValidator) && initialSchemaType.isIdType())) {
                        pipe.getErrorReporter().report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident("During validation, using an ID element at the outermost level has no effect").asWarning()));
                    }
                    Saxon.Eej.ee.validate.ConstraintChecker constraintChecker = getConstraintChecker();
                    if ((constraintChecker != null)) {
                        Saxon.Eej.ee.schema.ElementDecl element = initial.getElementDeclaration();
                        if ((element != null)) {
                            constraintChecker.setElementDeclaration(element);
                        }
                    }
                    outputReceiver.startElement(elemName, annotation, validatedAttributes, namespaces, location, properties);
                    if (((initialSchemaType is Saxon.Eej.ee.schema.UserComplexType) && ((Saxon.Eej.ee.schema.UserComplexType)(initialSchemaType)).hasAssertions())) {
                        prepareForAssertions(elemName, initialSchemaType, validatedAttributes, namespaces, location, properties, pipe);
                    }
                    stack.Push(initial);
                } else {
                    Saxon.Eej.ee.validate.ContentValidator v0 = stack.Peek();
                    v0.startElement(elemName, typeCode, attributes, namespaces, location, properties);
                    Saxon.Eej.ee.validate.ContentValidator v1 = v0.getChildValidator();
                    v1.setValidationContext(getValidationContext());
                    v1.setUnderlyingReceiver(outputReceiver);
                    v1.setNamespaceResolver(namespaces);
                    if ((statistics != null)) {
                        incrementStatistics(v1.getElementDeclaration());
                        incrementStatistics(v1.getSchemaType());
                    }
                    setUnderlyingReceiver(v1);
                    stack.Push(v1);
                    if ((!(((v1 is Saxon.Eej.ee.validate.SkipValidator))) && !(xsiValidationErrors.Count==0))) {
                        foreach (Saxon.Hej.type.ValidationFailure err in xsiValidationErrors) {
                            reportValidationError(err, false, location);
                        }
                    }
                    Saxon.Hej.type.SchemaType type = v1.getSchemaType();
                    if (((type is Saxon.Eej.ee.schema.UserComplexType) && ((Saxon.Eej.ee.schema.UserComplexType)(type)).hasAssertions())) {
                        System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> additionalNamespaces = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
                        Saxon.Hej.om.AttributeMap validatedAtts = v1.validateAttributes(attributes, additionalNamespaces);
                        foreach (Saxon.Hej.om.NamespaceBinding ns in additionalNamespaces) {
                            namespaces = namespaces.put(ns.getPrefix(), ns.getURI());
                        }
                        prepareForAssertions(elemName, type, validatedAtts, namespaces, location, properties, pipe);
                    }
                }
                if ((builder != null)) {
                    builderDepth++;
                }
            } catch (Saxon.Hej.type.ValidationException ve) {
                if (!(ve.hasBeenReported())) {
                    reportValidationError(ve.getValidationFailure(), true, location);
                    ve.setHasBeenReported(true);
                }
                if (!(pipe.isRecoverFromValidationErrors())) {
                    throw;
                }
            }
        }
        private void prepareForAssertions(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap validatedAttributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties, Saxon.Hej.@event.PipelineConfiguration pipe) {
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Assertion> assertions = ((Saxon.Eej.ee.schema.UserComplexType)(type)).getAssertions();
            bool buildTree = false;
            Saxon.Hej.tree.linked.ElementImpl stub = null;
            foreach (Saxon.Eej.ee.schema.Assertion a in assertions) {
                if ((a.getSweep() == Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                    if ((stub == null)) {
                        Saxon.Hej.tree.linked.LinkedTreeBuilder stubBuilder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe);
                        stubBuilder.open();
                        stubBuilder.startElement(elemName, Saxon.Hej.type.Untyped.INSTANCE, validatedAttributes, namespaces, location, properties);
                        stubBuilder.endElement();
                        stubBuilder.close();
                        stub = (Saxon.Hej.tree.linked.ElementImpl)(stubBuilder.getCurrentRoot());
                    }
                    Saxon.Hej.Controller controller = getValidationContext().getController();
                    Saxon.Hej.sxpath.XPathDynamicContext context = a.getCondition().createDynamicContext(controller, stub);
                    bool ok;
                    try {
                        ok = a.getCondition().effectiveBooleanValue(context);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        ok = false;
                    }
                    if (!(ok)) {
                        string message = (("Element "+elemName)+" does not satisfy assertion");
                        if ((a.getMessage() != null)) {
                            message += (". "+a.getMessage());
                        } else {
                            message += (" "+Saxon.Hej.value.Whitespace.collapseWhitespace(Saxon.Hej.str.StringView.of(a.getConditionText()).tidy()));
                        }
                        Saxon.Hej.type.ValidationFailure error = new Saxon.Hej.type.ValidationFailure(message);
                        error.setLocator(location);
                        error.setConstraintReference(1, "sec-cvc-assertion", "0");
                        reportValidationError(error, true, location);
                    }
                } else {
                    if (((a.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (builder == null))) {
                        Saxon.Hej.Controller controller = getValidationContext().getController();
                        Saxon.Hej.expr.XPathContext context = controller.newXPathContext();
                        context.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator(watchManager.getCurrentNode()));
                        Saxon.Eej.ee.stream.Inversion inversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(a.getCondition().getInternalExpression(), false);
                        Saxon.Eej.ee.stream.feed.ItemFeed assertionResultHandler = new Saxon.Eej.ee.validate.ValidationStack.AssertionResultHandler(this, a, elemName, context);
                        assertionResultHandler.setPipelineConfiguration(pipe);
                        Saxon.Eej.ee.stream.feed.ItemFeed booleanCapture = new Saxon.Eej.ee.stream.adjunct.BooleanFnFeed.Positive(null, assertionResultHandler, context);
                        Saxon.Eej.ee.stream.watch.Trigger assertionWatch = inversion.getWatch(watchManager, booleanCapture, context);
                        watchManager.addWatch(assertionWatch, true);
                        watchManager.restartElement(assertionWatch);
                    } else {
                        buildTree = true;
                    }
                }
            }
            if ((buildTree && (builder == null))) {
                builder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
                builder.setStatistics(pipe.getConfiguration().getTreeStatistics().ASSERTION_TREE_STATISTICS);
                builder.setSystemId(location.getSystemId());
                builder.setBaseURI(location.getSystemId());
                builder.open();
                builder.startElement(elemName, Saxon.Hej.type.Untyped.INSTANCE, validatedAttributes, namespaces, location, properties);
                builderDepth = 0;
                assertionBuilder.activate(builder);
            }
        }
        private class AssertionResultHandler : Saxon.Eej.ee.stream.feed.SinkFeed {
            private readonly Saxon.Eej.ee.validate.ValidationStack validationStack;
            private readonly Saxon.Eej.ee.schema.Assertion assertion;
            private readonly Saxon.Hej.om.NodeName elementName;
            public AssertionResultHandler(Saxon.Eej.ee.validate.ValidationStack validationStack, Saxon.Eej.ee.schema.Assertion assertion, Saxon.Hej.om.NodeName elementName, Saxon.Hej.expr.XPathContext context) : base (context) {
                this.validationStack = validationStack;
                this.assertion = assertion;
                this.elementName = elementName;
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (((item is Saxon.Hej.value.BooleanValue) && !(((Saxon.Hej.value.BooleanValue)(item)).getBooleanValue()))) {
                    string message = (("Element "+elementName)+" does not satisfy assertion");
                    if ((assertion.getMessage() != null)) {
                        message += (". "+assertion.getMessage());
                    } else {
                        message += (" "+Saxon.Hej.value.Whitespace.collapseWhitespace(Saxon.Hej.str.StringView.of(assertion.getConditionText()).tidy()));
                    }
                    Saxon.Hej.type.ValidationFailure error = new Saxon.Hej.type.ValidationFailure(message);
                    error.setLocator(assertion);
                    error.setConstraintReference(1, "sec-cvc-assertion", "0");
                    validationStack.reportValidationError(error, true, assertion);
                }
            }
            public override bool handlesAppend() {
                return true;
            }
        
        }
        public void setInitialValidator(Saxon.Eej.ee.validate.ContentValidator validator) {
            initialValidator = validator;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Eej.ee.validate.ContentValidator v0 = stack.Peek();
            v0.characters(chars, locationId, properties);
        }
        public override void endElement() {
            Saxon.Hej.s9api.Location location = locationStack.Pop();
            Saxon.Eej.ee.validate.ContentValidator v0 = stack.Pop();
            v0.endElement();
            if ((builder != null)) {
                builderDepth--;
                Saxon.Hej.type.SchemaType type = v0.getSchemaType();
                if (((type is Saxon.Eej.ee.schema.UserComplexType) && ((Saxon.Eej.ee.schema.UserComplexType)(type)).hasAssertions())) {
                    Saxon.Hej.om.NodeInfo root = builder.getLastCompletedElement();
                    if ((builderDepth != 0)) {
                        root = Saxon.Hej.tree.wrapper.VirtualCopy.makeVirtualCopy(root);
                    } else {
                    }
                    testAssertions(root, (Saxon.Eej.ee.schema.UserComplexType)(type), location);
                }
                if ((builderDepth == 0)) {
                    builder.close();
                    assertionBuilder.deactivate();
                    builder = null;
                }
            }
            if (!((stack.Count==0))) {
                setUnderlyingReceiver(stack.Peek());
            } else {
                setUnderlyingReceiver(outputReceiver);
                reportIfInvalid();
            }
        }
        public override void endDocument() {
            try {
                nextReceiver.endDocument();
                reportIfInvalid();
                if ((statistics != null)) {
                    getPipelineConfiguration().getParseOptions().getValidationStatisticsRecipient().notifyValidationStatistics(statistics);
                }
            } catch (Saxon.Api.SaxonApiException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        private void testAssertions(Saxon.Hej.om.NodeInfo root, Saxon.Eej.ee.schema.UserComplexType type, Saxon.Hej.s9api.Location location) {
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Assertion> assertions = type.getAssertions();
            Saxon.Hej.om.AtomicSequence value = null;
            if (type.isSimpleContent()) {
                Saxon.Hej.str.UnicodeString s = root.getUnicodeStringValue();
                Saxon.Hej.type.SimpleType st = type.getSimpleContentType();
                Saxon.Hej.om.NamespaceResolver ns = (st.isNamespaceSensitive() ? root.getAllNamespaces() : null);
                value = st.getTypedValue(s, ns, getConfiguration().getConversionRules());
            }
            foreach (Saxon.Eej.ee.schema.Assertion assertion in assertions) {
                if ((assertion.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                    testAssertion(assertion, root, value, location);
                }
            }
        }
        private void testAssertion(Saxon.Eej.ee.schema.Assertion assertion, Saxon.Hej.om.NodeInfo root, Saxon.Hej.om.AtomicSequence value, Saxon.Hej.s9api.Location location) {
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> offendingItems = assertion.testComplex(root, value, getValidationContext());
            if (!(offendingItems.Count==0)) {
                string message = (("Element "+root.getDisplayName())+" does not satisfy assertion");
                if ((assertion.getMessage() != null)) {
                    message += (". "+assertion.getMessage());
                } else {
                    message += (" "+Saxon.Hej.value.Whitespace.collapseWhitespace(Saxon.Hej.str.StringView.tidy(assertion.getConditionText())));
                }
                Saxon.Hej.type.ValidationFailure error = new Saxon.Hej.type.ValidationFailure(message);
                error.setLocator(location);
                error.setConstraintReference(1, "sec-cvc-assertion", "0");
                foreach (Saxon.Hej.om.NodeInfo item in offendingItems) {
                    if ((item != root)) {
                        error.addOffendingNode(item);
                    }
                }
                reportValidationError(error, true, location);
            }
        }
        private void incrementStatistics(Saxon.Hej.type.SchemaComponent component) {
            Saxon.Impl.Helpers.MapUtils.Merge(statistics, component, 1, (a, b) => (a+b));
        }
        public void setKeepStatistics(bool keep) {
            if (keep) {
                statistics = new System.Collections.Generic.Dictionary<Saxon.Hej.type.SchemaComponent, int>();
            } else {
                statistics = null;
            }
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaComponent,int> getCoverageStatistics() {
            return statistics;
        }
    
    }
}