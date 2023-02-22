using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class SharedAppendEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        private Saxon.Hej.expr.instruct.Block.ChainAction[] actions;
        public SharedAppendEvaluator(Saxon.Hej.expr.instruct.Block expr)  {
            actions = new Saxon.Hej.expr.instruct.Block.ChainAction[expr.size()];
            for (int i = 0; (i<expr.size()); i++) {
                Saxon.Hej.expr.Expression child = expr.getOperanda()[i].getChildExpression();
                if ((child is Saxon.Hej.expr.VariableReference)) {
                    Saxon.Hej.expr.elab.SequenceEvaluator eval = child.makeElaborator().eagerly();
                    actions[i] = (chain, context) => chain.appendSequence((Saxon.Hej.om.GroundedValue)(eval.evaluate(context)));
                } else {
                    Saxon.Hej.expr.elab.PullEvaluator pull = child.makeElaborator().elaborateForPull();
                    actions[i] = (chain, context) => {
                        Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/pull(context);
                        for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                            chain = chain.append(item);
                        }
                        return chain;
                    } ;
                }
            }
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            foreach (Saxon.Hej.expr.instruct.Block.ChainAction action in actions) {
                chain = /*Delegate.perform()*/action(chain, context);
            }
            return chain;
        }
    
    }
}