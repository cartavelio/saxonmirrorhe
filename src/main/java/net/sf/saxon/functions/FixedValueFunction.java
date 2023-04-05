////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;

/**
 * A ConstantFunction is a zero-argument anonymous function that always delivers the same result, supplied
 * at the time the function is instantiated.
 */

public class FixedValueFunction extends AbstractFunction  {

    private final GroundedValue value;
    private final static SpecificFunctionType TYPE = new SpecificFunctionType(new SequenceType[]{}, SequenceType.ANY_SEQUENCE);


    public FixedValueFunction(GroundedValue value) {
        this.value = value;
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return value;
    }

    @Override
    public FunctionItemType getFunctionItemType() {
        return TYPE;
    }

    @Override
    public StructuredQName getFunctionName() {
        return null;
    }

    @Override
    public int getArity() {
        return 0;
    }

    @Override
    public String getDescription() {
        return "function(){" + value.toShortString() + "}";
    }
}

