////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.hof;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.query.Annotation;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.List;


/**
 * This class implements the function function-annotations(), which is a standard function in XPath 4.0
 */

public class FunctionAnnotations extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        FunctionItem f = (FunctionItem) arguments[0].head();
        AnnotationList annotationList = f.getAnnotations();
        HashTrieMap map = new HashTrieMap();
        for (Annotation a : annotationList) {
            // TODO: need spec clarification on handling duplicate keys
            List<AtomicValue> val = a.getAnnotationParameters();
            GroundedValue gv = SequenceExtent.makeSequenceExtent(val);
            map.initialPut(new QNameValue(a.getAnnotationQName(), BuiltInAtomicType.QNAME), gv);
        }
        return map;
    }
}

// Copyright (c) 2024 Saxonica Limited