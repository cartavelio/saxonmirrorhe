using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Index : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item arg1 = arguments[1].head();
            if (((arg1 is Saxon.Hej.value.AnyExternalObject) && (((Saxon.Hej.value.AnyExternalObject)(arg1)).getWrappedObject() is Saxon.Eej.functions.extfn.Evaluate.PreparedExpression))) {
                throw new Saxon.Hej.trans.XPathException("The function saxon:index() has changed to expect a function as its second argument");
            } else {
                if ((arg1 is Saxon.Hej.om.FunctionItem)) {
                    Saxon.Hej.om.FunctionItem function = (Saxon.Hej.om.FunctionItem)(arg1);
                    Saxon.Impl.DataModel.ImmMap imap = new Saxon.Impl.DataModel.ImmMap();
                    Saxon.Hej.om.Sequence[] functionArgs = Saxon.Hej.om.SequenceTool.makeSequenceArray(1);
                    Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
                    Saxon.Hej.om.Item next;
                    while (((next = iter.next()) != null)) {
                        functionArgs[0] = next;
                        Saxon.Hej.om.Sequence keys = dynamicCall(function, context, functionArgs);
                        Saxon.Hej.om.SequenceIterator keyIterator = new Saxon.Hej.tree.iter.AtomizingIterator(keys.iterate());
                        Saxon.Hej.value.AtomicValue key;
                        while (((key = (Saxon.Hej.value.AtomicValue)(keyIterator.next())) != null)) {
                            Saxon.Hej.om.GroundedValue existingVal = imap.get(key);
                            if ((existingVal == null)) {
                                imap.initialPut(key, next);
                            } else {
                                if ((existingVal is Saxon.Hej.ma.zeno.ZenoSequence)) {
                                    imap.initialPut(key, ((Saxon.Hej.ma.zeno.ZenoSequence)(existingVal)).append(next));
                                } else {
                                    Saxon.Hej.ma.zeno.ZenoSequence zenoSeq = new Saxon.Hej.ma.zeno.ZenoSequence();
                                    zenoSeq = zenoSeq.appendSequence(existingVal);
                                    zenoSeq = zenoSeq.append(next);
                                    imap.initialPut(key, zenoSeq);
                                }
                            }
                        }
                    }
                    return imap;
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Second argument of saxon:index must be a function item");
                    err.setXPathContext(context);
                    throw err;
                }
            }
        }
    
    }
}