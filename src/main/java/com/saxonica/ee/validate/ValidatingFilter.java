package com.saxonica.ee.validate;

import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.lib.InvalidityHandler;
import net.sf.saxon.lib.InvalidityHandlerWrappingErrorReporter;
import net.sf.saxon.om.*;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.AttributeLocation;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.type.ValidationFailure;

/**
 * This class is used for a filter on the validation pipeline. It provides error reporting services to each
 * of the filters.
 */

public class ValidatingFilter extends ProxyReceiver {

    private ValidationContext validationContext;

    boolean locallyInvalid = false; // indicates that the content model of this element is not satisfied


    ValidatingFilter(Receiver next) {
        super(next);
    }

    /**
     * Get the validation context for this validation episode
     *
     * @return the validation context
     */

    public ValidationContext getValidationContext() {
        return validationContext;
    }

    /**
     * Set the validation context for this validation episode
     *
     * @param validationContext the validation context
     */

    public void setValidationContext(ValidationContext validationContext) {
        this.validationContext = validationContext;
    }

    /**
     * Set the constraint checker used in this pipeline
     *
     * @param checker the constraint checker
     */

    public void setConstraintChecker(ConstraintChecker checker) {
        validationContext.setConstraintChecker(checker);
    }

    /**
     * Get the constraint checker used in this pipeline
     *
     * @return the constraint checker
     */

    protected ConstraintChecker getConstraintChecker() {
        return validationContext.getConstraintChecker();
    }

    /**
     * Set the StartTagBuffer, which buffers attributes on the element start tag, and also acts as
     * the namespaceResolver to be used for resolving QName-valued attributes
     *
     * @param startTag the namespace resolver
     */

    public void setStartTagBuffer(AttributeInheritor startTag) {
        validationContext.setAttributeInheritor(startTag);
    }

    /**
     * Get the AttributeInheritor which maintains details of inherited attributes for use in
     * conditional type assignment
     *
     * @return the AttributeInheritor
     */

    public AttributeInheritor getAttributeInheritor() {
        return validationContext.getAttributeInheritor();
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation. Validation
     *         filters overwrite any type annotations arriving in the input event stream,
     *         so this instance of the method returns false.
     */

    @Override
    public boolean usesTypeAnnotations() {
        return false;
    }


    @Override
    public void append(Item item, Location locationId, int copyNamespaces) throws XPathException {
        decompose(item, locationId, copyNamespaces);
    }

    /**
     * Report a validation error
     *
     * @param err          The validation failure
     * @param issueComment true if the validation error message is to be written as an XML comment to the
     *                     result document
     * @param locationId   identifies the location in the source document or stylesheet/query where the
     *                     validation error was detected
     * @throws net.sf.saxon.trans.XPathException if too many invalidities have been reported
     */
    protected void reportValidationError(ValidationFailure err, boolean issueComment, Location locationId)
            throws XPathException {

        if (err.hasBeenReported()) {
            return;
        }

        PipelineConfiguration pipe = getPipelineConfiguration();

        if (Loc.isUnknown(locationId)) {
            err.setSourceLocator(validationContext.getInitiatingLocation());
        } else {
            err.setSourceLocator(locationId);
        }
        if (err.getErrorCodeQName() == null) {
            err.setErrorCode(validationContext.getErrorCode());
        }
        PathMaintainer pm = (PathMaintainer) pipe.getComponent("net.sf.saxon.event.PathMaintainer");
        if (pm != null) {
            AbsolutePath path = pm.getAbsolutePath();
            if (locationId instanceof AttributeLocation) {
                NodeName attName = new FingerprintedQName(((AttributeLocation)locationId).getAttributeName(), getNamePool());
                path.appendAttributeName(attName);
            }
            err.setPath(path);
        } else if (locationId instanceof AttributeLocation) {
            String attLoc = "@" + ((AttributeLocation) locationId).getAttributeName().getDisplayName();
            StructuredQName elName = ((AttributeLocation)locationId).getElementName();
            if (elName != null) {
                attLoc = elName.getDisplayName() + "/" + attLoc;
            }
            String message = "At " + attLoc + ": " + err.getMessage();
            err.setMessage(message);
        }

        XPathContext context = pipe.getXPathContext();
        while (context != null) {
            Item current = context.getContextItem();
            if (current instanceof NodeInfo) {
                err.setContextPath(Navigator.getAbsolutePath((NodeInfo) current));
                break;
            }
            context = context.getCaller();
        }

        InvalidityHandler handler = validationContext.getInvalidityHandler();
        if (handler == null) {
            handler = new InvalidityHandlerWrappingErrorReporter(pipe.getErrorReporter());
        }

        handler.reportInvalidity(err);
        err.setHasBeenReported(true);

        AssertionTreeBuilder atb = validationContext.getAssertionTreeBuilder();
        if (atb != null) {
            atb.markAncestorNodesUntyped();
        }

        if (validationContext.incrementErrorCount(err)) {
            throw new XPathException("Validation terminated - too many errors");
        }
        if (issueComment && pipe.getParseOptions().isAddCommentsAfterValidationErrors()) {
            comment(StringView.of("\nVALIDATION ERROR: " + err.getMessage() + '\n'), locationId, ReceiverOption.NONE);
        }
    }


    protected void reportIfInvalid() throws ValidationException {
        if (validationContext != null && validationContext.getErrorCount() > 0
                && !getPipelineConfiguration().isRecoverFromValidationErrors()) {
            String message;
            if (validationContext.getErrorCount() == 1) {
                message = "One validation error was reported: ";
            } else {
                message = new Numberer_en().toWords(validationContext.getErrorCount()) + " validation errors were reported. First error: ";
            }
            message += validationContext.getFirstFailure().getMessage();
            ValidationFailure err = new ValidationFailure(message);
            String firstErrorCode = validationContext.getFirstFailure().getErrorCode();
            if (firstErrorCode != null && !firstErrorCode.startsWith("FO")) { // horribly ad-hoc
                err.setErrorCode(firstErrorCode);
            } else if (validationContext.getErrorCode() != null) {
                err.setErrorCode(validationContext.getErrorCode());
            } else {
                err.setErrorCode(getPipelineConfiguration().isXSLT() ? "XTTE1510" : "XQDY0027");
            }
            ValidationException ve = err.makeException();
            ve.setErrorObject(validationContext.getValidationErrorObject());
            throw ve;
        }
    }
}

// Copyright (c) 2005-2022 Saxonica Limited No warranty.
