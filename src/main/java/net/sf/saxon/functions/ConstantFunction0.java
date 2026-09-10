////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;

/**
 * A ConstantFunction is a zero-argument function that always delivers the same result, supplied
 * at the time the function is instantiated. It is a subclass of system function, which means there must
 * be a "details" entry giving information about the function name, signature, etc
 */

public class ConstantFunction0 extends AbstractFunction  {

    private final GroundedValue value;
    private final StructuredQName name;
    private final SequenceType resultType;

    public ConstantFunction0(StructuredQName name, GroundedValue value, SequenceType resultType) {
        this.name = name;
        this.value = value;
        this.resultType = resultType;
    }

    /**
     * Get the item type of the function item
     *
     * @return the function item's type
     */
    @Override
    public FunctionItemType getFunctionItemType() {
        return new SpecificFunctionType(new SequenceType[]{}, resultType);
    }

    /**
     * Get the name of the function, or null if it is anonymous
     *
     * @return the function name, or null for an anonymous inline function
     */
    @Override
    public StructuredQName getFunctionName() {
        return name;
    }

    /**
     * Get the arity of the function
     *
     * @return the number of arguments in the function signature
     */
    @Override
    public int getArity() {
        return 0;
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
        return name.getDisplayName() + "#0";
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return value;
    }

    public GroundedValue getConstantValue() {
        return value;
    }


    public static class True extends ConstantFunction0 {
        public True() {
            super(new StructuredQName("fn", NamespaceUri.FN, "true"),
                  BooleanValue.TRUE, SequenceType.SINGLE_BOOLEAN);
        }
    }

    public static class False extends ConstantFunction0 {
        public False() {
            super(new StructuredQName("fn", NamespaceUri.FN, "false"),
                  BooleanValue.FALSE, SequenceType.SINGLE_BOOLEAN);
        }
    }


}

