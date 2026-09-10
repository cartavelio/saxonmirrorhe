package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomizingIterator;
import net.sf.saxon.value.AnyExternalObject;
import net.sf.saxon.value.AtomicValue;

/**
 * This class supports the extension function saxon:index, which returns an indexed sequence
 */

public class Index extends SystemFunction {


        /**
         * Evaluate this function call at run-time
         *
         * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call.
         * @return the results of the function.
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs during evaluation of the function. The Saxon run-time
         *          code will add information about the error location.
         */

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Item arg1 = arguments[1].head();
            if (arg1 instanceof AnyExternalObject && ((AnyExternalObject) arg1).getWrappedObject() instanceof Evaluate.PreparedExpression) {
                throw new XPathException("The function saxon:index() has changed to expect a function as its second argument");
            } else if (arg1 instanceof FunctionItem) {
                FunctionItem function = (FunctionItem) arg1;
                HashTrieMap imap = new HashTrieMap();
                Sequence[] functionArgs = SequenceTool.makeSequenceArray(1);
                SequenceIterator iter = arguments[0].iterate();
                Item next;
                while ((next = iter.next()) != null) {
                    functionArgs[0] = next;
                    Sequence keys = dynamicCall(function, context, functionArgs);
                    SequenceIterator keyIterator = new AtomizingIterator(keys.iterate());
                    AtomicValue key;
                    while ((key = (AtomicValue)keyIterator.next()) != null) {
                        GroundedValue existingVal = imap.get(key);
                        if (existingVal == null) {
                            imap.initialPut(key, next);
                        } else if (existingVal instanceof ZenoSequence) {
                            imap.initialPut(key, ((ZenoSequence) existingVal).append(next));
                        } else {
                            ZenoSequence zenoSeq = new ZenoSequence();
                            zenoSeq = zenoSeq.appendSequence(existingVal);
                            zenoSeq = zenoSeq.append(next);
                            imap.initialPut(key, zenoSeq);
                        }
                    }
                }
                return imap;
            } else {
                throw new XPathException("Second argument of saxon:index must be a function item")
                    .withXPathContext(context);
            }
        }




}

// Copyright (c) 2005-2023 Saxonica Limited
