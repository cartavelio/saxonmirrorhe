package com.saxonica.functions.qt4.pin;

import net.sf.saxon.expr.ContextOriginator;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatcher;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.*;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.value.AtomicValue;

/**                                                   n
 * A <code>LabeledMapItem</code> represents an XDM map together with a label, which is a map:
 * the map typically contains retained information about how the map was selected within a JSON-like tree.
 */

public class LabeledFunctionItem implements FunctionItem, LabeledItem {

    private final FunctionItem f;
    private final StringMap label;


    /**
     * Create a LabeledFunctionItem with a supplied label
     * @param subject the subject function item
     * @param label the label
     */

    public LabeledFunctionItem(FunctionItem subject, StringMap label) {
        this.f = subject;
        this.label = label;
    }


    /**
     * Get the subject item without its label
     *
     * @return the subject item
     */
    @Override
    public Item getSubject() {
        return f;
    }

    /**
     * Get the label of this value, typically, information about
     * the containing maps and arrays in the containing tree
     *
     * @return the associated {@code Pedigree}.
     */
    @Override
    public StringMap getLabel() {
        return label;
    }

    /**
     * Call the Callable.
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences.
     *                  <p>Generally it is advisable, if calling iterate() to process a supplied sequence, to
     *                  call it only once; if the value is required more than once, it should first be converted
     *                  to a {@link GroundedValue} by calling the utility method
     *                  SequenceTool.toGroundedValue().</p>
     *                  <p>If the expected value is a single item, the item should be obtained by calling
     *                  Sequence.head(): it cannot be assumed that the item will be passed as an instance of
     *                  {@link Item} or {@link AtomicValue}.</p>
     *                  <p>It is the caller's responsibility to perform any type conversions required
     *                  to convert arguments to the type expected by the callee. An exception is where
     *                  this Callable is explicitly an argument-converting wrapper around the original
     *                  Callable.</p>
     * @return the result of the evaluation, in the form of a Sequence. It is the responsibility
     * of the callee to ensure that the type of result conforms to the expected result type.
     * @throws XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return f.call(context, arguments);
    }

    /**
     * Ask whether this function item is a map
     *
     * @return true if this function item is a map, otherwise false
     */
    @Override
    public boolean isMap() {
        return false;
    }

    /**
     * Ask whether this function item is an array
     *
     * @return true if this function item is an array, otherwise false
     */
    @Override
    public boolean isArray() {
        return false;
    }

    /**
     * Get the item type of the function item
     *
     * @return the function item's type
     */
    @Override
    public FunctionItemType getFunctionItemType() {
        return f.getFunctionItemType();
    }

    /**
     * Get the name of the function, or null if it is anonymous
     *
     * @return the function name, or null for an anonymous inline function
     */
    @Override
    public StructuredQName getFunctionName() {
        return f.getFunctionName();
    }

    /**
     * Get the arity of the function
     *
     * @return the number of arguments in the function signature
     */
    @Override
    public int getArity() {
        return f.getArity();
    }

    /**
     * Get the roles of the arguments, for the purposes of streaming
     *
     * @return an array of OperandRole objects, one for each argument
     */
    @Override
    public OperandRole[] getOperandRoles() {
        return f.getOperandRoles();
    }

    /**
     * Get the function annotations (as defined in XQuery). Returns an empty
     * list if there are no function annotations.
     *
     * @return the function annotations
     */
    @Override
    public AnnotationList getAnnotations() {
        return f.getAnnotations();
    }

    /**
     * Prepare an XPathContext object for evaluating the function
     *
     * @param callingContext the XPathContext of the function calling expression
     * @param originator     identifies the location of the caller for diagnostics
     * @return a suitable context for evaluating the function (which may or may
     * not be the same as the caller's context)
     */
    @Override
    public XPathContext makeNewContext(XPathContext callingContext, ContextOriginator originator) {
        return f.makeNewContext(callingContext, originator);
    }

    /**
     * Test whether this FunctionItem is deep-equal to another function item,
     * under the rules of the deep-equal function
     *
     * @param other    the other function item
     * @param context  the dynamic evaluation context
     * @param comparer the object to perform the comparison
     * @param flags    options for how the comparison is performed
     * @return true if the two function items are deep-equal
     * @throws XPathException if the comparison cannot be performed
     */
    @Override
    public boolean deepEquals(FunctionItem other, XPathContext context, AtomicMatcher comparer, int flags) throws XPathException {
        return f.deepEquals(other, context, comparer, flags);
    }

    @Override
    public boolean deepEqual40(FunctionItem other, XPathContext context, DeepEqual.DeepEqualOptions options) throws XPathException {
        return f.deepEqual40(other, context, options);
    }

    /**
     * Get a description of this function for use in error messages. For named functions, the description
     * is the function name (as a lexical QName). For others, it might be, for example, "inline function",
     * or "partially-applied ends-with function".
     *
     * @return a description of the function for use in error messages
     */
    @Override
    public String getDescription() {
        return f.getDescription();
    }

    /**
     * Output information about this function item to the diagnostic explain() output
     *
     * @param out the destination for the information
     * @throws XPathException if things go wrong
     */
    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        f.export(out);
    }

    /**
     * Ask if the function can be trusted to return a result of the correct type
     *
     * @return true if the implementation can be trusted
     */
    @Override
    public boolean isTrustedResultType() {
        return f.isTrustedResultType();
    }

    /**
     * Get the value of the item as a Unicode string. For nodes, this is the string value of the
     * node as defined in the XPath 2.0 data model, except that all nodes are treated as being
     * untyped: it is not an error to get the string value of a node with a complex type.
     * For atomic values, the method returns the result of casting the atomic value to a string.
     *
     * @return the string value of the item
     * @throws UnsupportedOperationException if the item is a function item (an unchecked exception
     *                                       is used here to avoid introducing exception handling to a large number of paths where it is not
     *                                       needed)
     * @since 8.4
     */
    @Override
    public UnicodeString getUnicodeStringValue() {
        return f.getUnicodeStringValue();
    }

    /**
     * Atomize the item.
     *
     * @return the result of atomization
     * @throws XPathException if atomization is not allowed
     *                        for this kind of item
     */
    @Override
    public AtomicSequence atomize() throws XPathException {
        return f.atomize();
    }
}

// Copyright (c) 2024 Saxonica Limited

