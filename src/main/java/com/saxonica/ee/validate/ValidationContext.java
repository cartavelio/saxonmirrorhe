package com.saxonica.ee.validate;

import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.Controller;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.InvalidityHandler;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.type.ValidationParams;
import net.sf.saxon.value.EmptySequence;

/**
 * This class defines information about a validation episode
 */

public class ValidationContext extends ConversionRules {

    private PreparedSchema schema;
    private ValidationParams validationParams;
    private Controller controller;
    private InvalidityHandler invalidityHandler;
    private SchemaType xsiType;
    private AttributeInheritor attributeInheritor;
    private ConstraintChecker constraintChecker;
    private String errorCode;
    private int errorCount = 0;
    private int errorLimit = Integer.MAX_VALUE;
    private Location initiatingLocation;
    private ValidationFailure firstFailure = null;
    private AssertionTreeBuilder assertionTreeBuilder = null;

    public ValidationContext(ConversionRules rules) {
        rules.copyTo(this);
    }

    public void setSchema(PreparedSchema ps) {
        this.schema = ps;
    }

    public PreparedSchema getSchema() {
        return schema;
    }

    /**
     * Get the Controller used for this validation episode
     *
     * @return the Controller
     */

    public Controller getController() {
        return controller;
    }

    /**
     * Set the Controller used for this validation episode
     *
     * @param controller the controller to be used
     */

    public void setController(Controller controller) {
        this.controller = controller;
    }

    /**
     * Get the callback for reporting validation errors
     * @return the registered InvalidityHandler
     */

    public InvalidityHandler getInvalidityHandler() {
        return invalidityHandler;
    }

    /**
     * Set the callback for reporting validation errors
     *
     * @param invalidityHandler the InvalidityHandler to be used for reporting validation failures
     */

    public void setInvalidityHandler(InvalidityHandler invalidityHandler) {
        this.invalidityHandler = invalidityHandler;
    }

    /**
     * Get the tree builder used for XSD 1.1 assertion evaluation
     * @return the tree builder used for assertions
     */

    public AssertionTreeBuilder getAssertionTreeBuilder() {
        return assertionTreeBuilder;
    }

    /**
     * Set the tree builder used for XSD 1.1 assertion evaluation
     * @param assertionTreeBuilder the tree builder used for assertions
     */

    public void setAssertionTreeBuilder(AssertionTreeBuilder assertionTreeBuilder) {
        this.assertionTreeBuilder = assertionTreeBuilder;
    }


    public void startValidation(String systemId) {
        if (invalidityHandler != null) {
            try {
                invalidityHandler.startReporting(systemId);
            } catch (XPathException err) {
                invalidityHandler = null;
            }
        }
    }

    /**
     * Get the error object. If an InvalidityReportGenerator was set as the InvalidityHandler,
     * this will be supplied by the InvalidityHandler. The resulting value is accessible as
     * the value of the $err:value variable within a catch clause.
     */

    public Sequence getValidationErrorObject() {
        if (invalidityHandler != null) {
            try {
                return invalidityHandler.endReporting();
            } catch (XPathException e) {
                return EmptySequence.getInstance();
            }
        }
        return null;
    }

    /**
     * Get the validation parameters (as declared using saxon:param)
     *
     * @return the validation parameter values. May be null if none have been defined
     */

    public ValidationParams getValidationParams() {
        return validationParams;
    }

    /**
     * Ask whether there are any validation parameters defined
     * @return true if one or more validation paameters are defined
     */

    public boolean hasValidationParams() {
        return validationParams != null && validationParams.size() != 0;
    }

    /**
     * Set the validation parameters (as declared using saxon:param)
     *
     * @param validationParams the validation parameter values. May be null if none are required.
     */

    public void setValidationParams(ValidationParams validationParams) {
        this.validationParams = validationParams;
    }

    /**
     * Set the constraint checker used in this pipeline
     *
     * @param checker the constraint checker
     */

    public void setConstraintChecker(ConstraintChecker checker) {
        this.constraintChecker = checker;
    }

    /**
     * Get the constraint checker used in this pipeline
     *
     * @return the constraint checker
     */

    protected ConstraintChecker getConstraintChecker() {
        return this.constraintChecker;
    }

    /**
     * Set the StartTagBuffer, which buffers attributes on the element start tag, and also acts as
     * the namespaceResolver to be used for resolving QName-valued attributes
     *
     * @param inheritor the namespace resolver
     */

    public void setAttributeInheritor(AttributeInheritor inheritor) {
        attributeInheritor = inheritor;
    }

    /**
     * Get the AttributeInheritor, which keeps track of inherited attributes for use
     * in conditional type assignment.
     *
     * @return the AttributeInheritor
     */

    public AttributeInheritor getAttributeInheritor() {
        return attributeInheritor;
    }

    /**
     * Set the value of the xsi:type attribute. This is called immediately before
     * the call of startElement on which the xsi:type attribute appears.
     *
     * @param xsiType the schema type identified by the xsi:type attribute
     */

    public void setXSIType(SchemaType xsiType) {
        this.xsiType = xsiType;
    }

    /**
     * Get the value of the xsi:type attribute. This is called immediately before
     * the call of startElement on which the xsi:type attribute appears.
     *
     * @return the schema type identified by the xsi:type attribute
     */

    public SchemaType getXSIType() {
        return xsiType;
    }

    /**
     * Set the error code to be used for error messages
     *
     * @param errorCode the error code
     */

    public void setErrorCode(/*@Nullable*/ String errorCode) {
        this.errorCode = errorCode;
    }

    /*@Nullable*/
    protected String getErrorCode() {
        return errorCode;
    }


    /**
     * Set a limit on the number of errors to be reported before validation is abandoned
     *
     * @param limit the limit on the number of errors
     */

    public void setErrorLimit(int limit) {
        this.errorLimit = limit;
    }

    /**
     * Increment the count of validation errors found
     * @param failure the invalidity that was found
     * @return true if the limit on the number of errors has been exceeded, otherwise false
     */

    public boolean incrementErrorCount(ValidationFailure failure) {
        if (firstFailure == null) {
            firstFailure = failure;
        }
        return ++errorCount >= errorLimit;
    }

    /**
     * Get details of the first failure that was reported
     * @return the first failure reported, if any; otherwise null
     */

    public ValidationFailure getFirstFailure() {
        return firstFailure;
    }

    /**
     * Get the number of validation errors found during the validation episode
     *
     * @return the number of errors
     */

    public int getErrorCount() {
        return errorCount;
    }

    public Location getInitiatingLocation() {
        return initiatingLocation;
    }

    public void setInitiatingLocation(Location initiatingLocation) {
        this.initiatingLocation = initiatingLocation;
    }

}

// Copyright (c) 2012-2023 Saxonica Limited