package com.saxonica.ee.bytecode;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;

/**
 * This abstract class is extended by the compiled code representing an Expression. The concrete subclasses
 * of this class are generated dynamically by the expression compiler.
 */
public abstract class GeneratedCode {

    private Configuration config;

    public GeneratedCode() {
    }

    /**
     * Set the configuration. Note that the same Configuration is used when this code is generated (compiled)
     * and when it is evaluated.
     *
     * @param config the Saxon configuration
     */

    public final void setConfiguration(Configuration config) {
        this.config = config;
    }

    /**
     * Get the configuration. Note that the same Configuration is used when this code is generated (compiled)
     * and when it is evaluated.
     *
     * @return the Saxon configuration
     */

    public final Configuration getConfiguration() {
        return config;
    }

    /**
     * Helper method for use at run-time allowing the generated code to throw a dynamic error
     *
     * @param message the error message
     * @param code    the error code
     * @throws XPathException the exception representing the dynamic error
     */

    public final void dynamicError(String message, String code) throws XPathException {
        throw new XPathException(message, code);
    }

    /**
     * Method that can be implemented in each generated subclass, to actually evaluate the compiled code.
     * This class provides a fallback implementation that relies on the process() method being implemented.
     *
     * @param context the XPath dynamic context
     * @return a SequenceIterator representing the results of evaluating the expression
     * @throws XPathException if any dynamic error occurs during the evaluation
     */

    public SequenceIterator iterate(XPathContext context) throws XPathException {
        Controller controller = context.getController();
        SequenceCollector seq = controller.allocateSequenceOutputter();
        process(new ComplexContentOutputter(seq), context);
        seq.close();
        return seq.iterate();
    }

    /**
     * Method that can be implemented in each generated subclass, to actually evaluate the compiled code.
     * This class provides a fallback implementation that relies on the iterate() method being implemented.
     *
     * @param context the XPath dynamic context
     * @throws XPathException if any dynamic error occurs during the evaluation
     */

    public void process(Outputter output, XPathContext context) throws XPathException {
        SequenceTool.supply(iterate(context), (ItemConsumer<? super Item>) it -> output.append(it, Loc.NONE, ReceiverOption.ALL_NAMESPACES));
    }

    /**
     * Method that can be implemented in each generated subclass, to actually evaluate the compiled code.
     * This class provides a fallback implementation that relies on the iterate() method being implemented.
     *
     * @param context the XPath dynamic context
     * @throws XPathException if any dynamic error occurs during the evaluation
     */

    public Item evaluateItem(XPathContext context) throws XPathException {
        return iterate(context).next();
    }

    /**
     * Method that can be implemented in each generated subclass, to actually evaluate the compiled code.
     * This class provides a fallback implementation that relies on the iterate() method being implemented.
     *
     * @param context the XPath dynamic context
     * @throws XPathException if any dynamic error occurs during the evaluation
     */


    public boolean effectiveBooleanValue(XPathContext context) throws XPathException {
        return ExpressionTool.effectiveBooleanValue(iterate(context));
    }

}

// Copyright (c) 2011-2022 Saxonica Limited

