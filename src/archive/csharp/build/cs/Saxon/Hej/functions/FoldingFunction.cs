using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class FoldingFunction : Saxon.Hej.functions.SystemFunction {
        public abstract Saxon.Hej.functions.Fold getFold(Saxon.Hej.expr.XPathContext context, params Saxon.Hej.om.Sequence[]  additionalArguments);
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence[] additionalArgs = new Saxon.Hej.om.Sequence[(arguments.Length-1)];
            Saxon.Ejava.lang.System.arraycopy(arguments, 1, additionalArgs, 0, additionalArgs.Length);
            Saxon.Hej.functions.Fold fold = getFold(context, additionalArgs);
            Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                fold.processItem(item);
                if (fold.isFinished()) {
                    break ;
                }
            }
            return fold.result();
        }
        public override string getStreamerName() {
            return "Fold";
        }
    
    }
}