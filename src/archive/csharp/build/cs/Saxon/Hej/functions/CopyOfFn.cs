using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CopyOfFn : Saxon.Hej.functions.SystemFunction {
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getCardinality();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence @in = ((arguments.Length == 0) ? context.getContextItem() : arguments[0]);
            Saxon.Hej.om.SequenceIterator input = @in.iterate();
            Saxon.Hej.om.SequenceIterator output = Saxon.Hej.expr.ItemMappingIterator.map(input, item => {
                if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                    return item;
                } else {
                    Saxon.Hej.tree.wrapper.VirtualCopy vc = Saxon.Hej.tree.wrapper.VirtualCopy.makeVirtualCopy((Saxon.Hej.om.NodeInfo)(item));
                    if (getRetainedStaticContext().getPackageData().isXSLT()) {
                        ((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()).setCopyAccumulators(true);
                    }
                    return vc;
                }
            } );
            return new Saxon.Hej.om.LazySequence(output);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression arg;
            if ((arguments.Length == 0)) {
                arg = new Saxon.Hej.expr.ContextItemExpression();
            } else {
                arg = arguments[0];
            }
            Saxon.Hej.expr.instruct.CopyOf fn = new Saxon.Hej.expr.instruct.CopyOf(arg, true, Saxon.Hej.lib.Validation.PRESERVE, null, false);
            fn.setCopyAccumulators(true);
            fn.setSchemaAware(false);
            return fn;
        }
    
    }
}