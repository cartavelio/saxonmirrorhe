////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;

/**
 * Implements the XPath 2.0 subsequence() function with two arguments
 */

public class ItemsAt extends SystemFunction {

    /**
     * Get the static properties of this expression (other than its type). The result is
     * bit-significant. These properties are used for optimizations. In general, if
     * property bit is set, it is true, but if it is unset, the value is unknown.
     *
     * @param arguments the actual arguments to the function call
     */

    @Override
    public int getSpecialProperties(Expression[] arguments) {
        return arguments[0].getSpecialProperties();
    }


    /**
     * Call the function
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences.
     * @return the result of the function call
     * @throws XPathException if a dynamic error occurs
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
         GroundedValue input = arguments[0].materialize();
         if (input.getLength() == 0) {
             return EmptySequence.getInstance();
         }
         ZenoSequence result = new ZenoSequence();
         SequenceIterator positions = arguments[1].iterate();
         for (Item it; (it = positions.next()) != null; ) {
              int offset = (int)((IntegerValue)it).longValue();
              Item selected = input.itemAt(offset - 1);
              if (selected != null) {
                  result = result.append(selected);
              }
         }
         return result;
    };

}