using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class GroupStarting : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence input = arguments[0];
            Saxon.Hej.om.FunctionItem function = (Saxon.Hej.om.FunctionItem)(arguments[1].head());
            return new Saxon.Hej.om.LazySequence(new Saxon.Eej.functions.extfn.GroupStarting.GroupStartingFunctionIterator(input.iterate(), function, context));
        }
        private class GroupStartingFunctionIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator baseIterator;
            private readonly Saxon.Hej.om.FunctionItem function;
            private readonly Saxon.Hej.expr.XPathContext context;
            private Saxon.Hej.om.Item pending = null;
            private bool finished = false;
            public GroupStartingFunctionIterator(Saxon.Hej.om.SequenceIterator @in, Saxon.Hej.om.FunctionItem f, Saxon.Hej.expr.XPathContext context)  {
                this.baseIterator = @in;
                this.function = f;
                this.context = context;
            }
            public Saxon.Hej.om.Item next() {
                if (finished) {
                    return null;
                }
                System.Collections.Generic.IList<Saxon.Hej.om.Item> group = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                if ((pending != null)) {
                    group.Add(pending);
                }
                try {
                    for (Saxon.Hej.om.Item item; ((item = baseIterator.next()) != null); ) {
                        Saxon.Hej.om.Sequence test = function.call(context, Saxon.Hej.om.SequenceTool.fromItems(item));
                        bool match = Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(test.head());
                        if (match) {
                            pending = item;
                            return makeSimpleArray(group);
                        } else {
                            group.Add(item);
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
                pending = null;
                finished = true;
                return makeSimpleArray(group);
            }
            private Saxon.Hej.ma.arrays.SimpleArrayItem makeSimpleArray(System.Collections.Generic.IList<Saxon.Hej.om.Item> group) {
                try {
                    return Saxon.Hej.ma.arrays.SimpleArrayItem.makeSimpleArrayItem(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(group));
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            }
            public void close() {
                baseIterator.close();
            }
        
        }
    
    }
}