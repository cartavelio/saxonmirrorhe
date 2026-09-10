////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.regex.RegexGroupInfo;
import net.sf.saxon.regex.RegexIterator;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;


public class RegexGroups extends ContextAccessorFunction {

    // Anticipates resolution of spec issue 2801

    private static Shape GROUP_INFO_SHAPE = new Shape(new Twine8("value"), new Twine8("position"), new Twine8("nr"), new Twine8("name"));

    @Override
    public FunctionItem bindContext(XPathContext context) throws XPathException {
        return new ConstantFunction(buildGroupMap(context), SequenceType.one(getResultItemType()));
    }

    /**
     * Build the map containing all the current regex captured groups
     */

    private MapItem buildGroupMap(XPathContext context) throws XPathException {
        RegexIterator ri = context.getCurrentRegexIterator();
        if (ri == null) {
            throw new XPathException("There is no current group", "XTDE1061");
        }

        GeneralMapBuilder builder = AbstractFixedMap.getBuilder(40);
        for (RegexGroupInfo info : ri.getCurrentRegexGroups()) {
            ShapedMap groupInfo = new ShapedMap(GROUP_INFO_SHAPE,
                                              new StringValue(info.content()),
                                              Int64Value.makeIntegerValue(info.startPosition()),
                                              Int64Value.makeIntegerValue(info.index()),
                                              info.groupName() == null ? EmptySequence.getInstance() : new StringValue(info.groupName()));
            if (info.groupName() != null) {
                builder.put(new StringValue(info.groupName()), groupInfo);
            } else {
                builder.put(Int64Value.makeIntegerValue(info.index()), groupInfo);
            }
        }
        return builder.getCompletedMapConfidently();
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences
     * @return the result of the evaluation, in the form of a Sequence
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        return buildGroupMap(context);
    }

}

