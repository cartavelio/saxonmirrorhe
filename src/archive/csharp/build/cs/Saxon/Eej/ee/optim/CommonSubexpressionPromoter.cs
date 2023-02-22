using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class CommonSubexpressionPromoter {
        private readonly Saxon.Eej.ee.optim.OptimizerEE optimizer;
        public CommonSubexpressionPromoter(Saxon.Eej.ee.optim.OptimizerEE opt)  {
            this.optimizer = opt;
        }
        public Saxon.Hej.expr.Expression promoteCommonSubexpressions(Saxon.Hej.expr.Expression @in) {
            System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Operand>> eligibles = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Operand>>();
            gatherEligibles(@in, eligibles);
            double topCost = 0;
            Saxon.Hej.expr.Expression topCandidate = null;
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Operand>> entry in eligibles) {
                int count = entry.Value.Count;
                if ((count>1)) {
                    double cost = (count*entry.Key.getCost());
                    if ((cost>topCost)) {
                        topCost = cost;
                        topCandidate = entry.Key;
                    }
                }
            }
            if ((topCost>10)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Operand> equivalents = Saxon.Impl.Helpers.MapUtils.Get(eligibles, topCandidate);
                optimizer.trace("Identified commmon subexpression", topCandidate);
                int cardinality = 0;
                Saxon.Hej.type.UType itemType = Saxon.Hej.type.UType.VOID;
                foreach (Saxon.Hej.expr.Operand o in equivalents) {
                    Saxon.Hej.expr.Expression e = o.getChildExpression();
                    cardinality = Saxon.Hej.value.Cardinality.union(cardinality, e.getCardinality());
                    itemType = itemType.union(e.getItemType().getUType());
                }
                Saxon.Hej.value.SequenceType type = Saxon.Hej.value.SequenceType.makeSequenceType(itemType.toItemType(), cardinality);
                Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                let.setSequence(topCandidate);
                let.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE, ("cc"+topCandidate.GetHashCode())));
                let.setRequiredType(type);
                let.setNeedsLazyEvaluation(true);
                foreach (Saxon.Hej.expr.Operand o in equivalents) {
                    o.setChildExpression(new Saxon.Hej.expr.LocalVariableReference(let));
                }
                let.setAction(@in);
                Saxon.Hej.expr.parser.ExpressionTool.resetPropertiesWithinSubtree(@in);
                let.rebuildReferenceList(true);
                return let;
            }
            return @in;
        }
        private static bool containsExact(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list, Saxon.Hej.expr.Expression exp) {
            foreach (Saxon.Hej.expr.Expression e in list) {
                if ((e == exp)) {
                    return true;
                }
            }
            return false;
        }
        private void gatherEligibles(Saxon.Hej.expr.Expression @in, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Expression,System.Collections.Generic.IList<Saxon.Hej.expr.Operand>> eligibles) {
            if ((@in.getCost()<2)) {
                return ;
            }
            if ((@in is Saxon.Eej.ee.stream.StreamInstr)) {
            } else {
                if (!(@in.isLiftable(false))) {
                } else {
                    if (((!(@in.allowExtractingCommonSubexpressions()) || (@in is Saxon.Hej.expr.LocalBinding)) || isBlockWithLocalParams(@in))) {
                        processChildren(@in);
                    } else {
                        foreach (Saxon.Hej.expr.Operand o in @in.operands()) {
                            Saxon.Hej.expr.Expression child = o.getChildExpression();
                            if ((o.hasSameFocus() && !(o.getOperandRole().isConstrainedClass()))) {
                                if (eligibles.ContainsKey(child)) {
                                    Saxon.Impl.Helpers.MapUtils.Get(eligibles, child).Add(o);
                                } else {
                                    System.Collections.Generic.IList<Saxon.Hej.expr.Operand> equivalents = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(2);
                                    equivalents.Add(o);
                                    Saxon.Impl.Helpers.MapUtils.Put(eligibles, child, equivalents);
                                }
                                gatherEligibles(child, eligibles);
                            } else {
                                o.setChildExpression(promoteCommonSubexpressions(child));
                            }
                        }
                    }
                }
            }
        }
        private static bool isBlockWithLocalParams(Saxon.Hej.expr.Expression @in) {
            if ((@in is Saxon.Hej.expr.instruct.Block)) {
                Saxon.Hej.expr.Operand[] operands = ((Saxon.Hej.expr.instruct.Block)(@in)).getOperanda();
                return ((operands.Length>0) && (operands[0].getChildExpression() is Saxon.Hej.expr.instruct.LocalParam));
            } else {
                return false;
            }
        }
        private void processChildren(Saxon.Hej.expr.Expression @in) {
            foreach (Saxon.Hej.expr.Operand o in @in.operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                o.setChildExpression(promoteCommonSubexpressions(child));
            }
        }
    
    }
}