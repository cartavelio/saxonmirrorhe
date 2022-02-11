using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public abstract class Evaluator {
        private int code;
        internal void setCode(int code) {
            this.code = code;
            Saxon.Hej.expr.parser.Evaluators.index(this, code);
        }
        public int getCode() {
            return code;
        }
        public abstract Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context);
        public sealed class EmptySequenceEvaluator : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.EmptySequenceEvaluator INSTANCE = new Saxon.Hej.expr.parser.Evaluator.EmptySequenceEvaluator();
            private EmptySequenceEvaluator()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.RETURN_EMPTY_SEQUENCE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public sealed class Literal : Saxon.Hej.expr.parser.Evaluator {
            public static Saxon.Hej.expr.parser.Evaluator.Literal INSTANCE = new Saxon.Hej.expr.parser.Evaluator.Literal();
            private Literal()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.EVALUATE_LITERAL);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return ((Saxon.Hej.expr.Literal)(expr)).getGroundedValue();
            }
        
        }
        public sealed class Variable : Saxon.Hej.expr.parser.Evaluator {
            public static Saxon.Hej.expr.parser.Evaluator.Variable INSTANCE = new Saxon.Hej.expr.parser.Evaluator.Variable();
            private Variable()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.EVALUATE_VARIABLE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                try {
                    return ((Saxon.Hej.expr.VariableReference)(expr)).evaluateVariable(context);
                } catch (System.InvalidCastException e) {
                    System.Diagnostics.Debug.Assert(false);
                    return Saxon.Hej.expr.parser.Evaluator.LazySequence.INSTANCE.evaluate(expr, context);
                }
            }
        
        }
        public sealed class SuppliedParameter : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.SuppliedParameter INSTANCE = new Saxon.Hej.expr.parser.Evaluator.SuppliedParameter();
            private SuppliedParameter()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.EVALUATE_SUPPLIED_PARAMETER);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                try {
                    return ((Saxon.Hej.expr.SuppliedParameterReference)(expr)).evaluateVariable(context);
                } catch (System.InvalidCastException e) {
                    System.Diagnostics.Debug.Assert(false);
                    return Saxon.Hej.expr.parser.Evaluator.LazySequence.INSTANCE.evaluate(expr, context);
                }
            }
        
        }
        public sealed class SingleItem : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.SingleItem INSTANCE = new Saxon.Hej.expr.parser.Evaluator.SingleItem();
            private SingleItem()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.CALL_EVALUATE_SINGLE_ITEM);
            }
            public override Saxon.Hej.om.Item evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return expr.evaluateItem(context);
            }
        
        }
        public sealed class OptionalItem : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.OptionalItem INSTANCE = new Saxon.Hej.expr.parser.Evaluator.OptionalItem();
            public OptionalItem()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.CALL_EVALUATE_OPTIONAL_ITEM);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.om.Item item = expr.evaluateItem(context);
                if ((item == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return item;
                }
            }
        
        }
        public sealed class LazySequence : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.LazySequence INSTANCE = new Saxon.Hej.expr.parser.Evaluator.LazySequence();
            public LazySequence()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.MAKE_CLOSURE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.om.SequenceIterator iter = expr.iterate(context);
                return new Saxon.Hej.om.LazySequence(iter);
            }
        
        }
        public sealed class MemoClosureEvaluator : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.MemoClosureEvaluator INSTANCE = new Saxon.Hej.expr.parser.Evaluator.MemoClosureEvaluator();
            public MemoClosureEvaluator()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.MAKE_MEMO_CLOSURE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return new Saxon.Hej.value.MemoClosure(expr, context);
            }
        
        }
        public sealed class SingletonClosure : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.SingletonClosure INSTANCE = new Saxon.Hej.expr.parser.Evaluator.SingletonClosure();
            public SingletonClosure()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.MAKE_SINGLETON_CLOSURE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return new Saxon.Hej.value.SingletonClosure(expr, context);
            }
        
        }
        public sealed class EagerSequence : Saxon.Hej.expr.parser.Evaluator {
            public static Saxon.Hej.expr.parser.Evaluator.EagerSequence INSTANCE = new Saxon.Hej.expr.parser.Evaluator.EagerSequence();
            public EagerSequence()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.ITERATE_AND_MATERIALIZE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                try {
                    Saxon.Hej.om.SequenceIterator iter = expr.iterate(context);
                    return Saxon.Hej.om.SequenceTool.toGroundedValue(iter);
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            }
        
        }
        public sealed class SharedAppend : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.SharedAppend INSTANCE = new Saxon.Hej.expr.parser.Evaluator.SharedAppend();
            public SharedAppend()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.SHARED_APPEND_EXPRESSION);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                try {
                    if ((expr is Saxon.Hej.expr.instruct.Block)) {
                        Saxon.Hej.expr.instruct.Block block = (Saxon.Hej.expr.instruct.Block)(expr);
                        Saxon.Hej.expr.Operand[] children = block.getOperanda();
                        Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
                        foreach (Saxon.Hej.expr.Operand o in children) {
                            Saxon.Hej.expr.Expression child = o.getChildExpression();
                            if (Saxon.Hej.value.Cardinality.allowsMany(child.getCardinality())) {
                                chain.appendSequence(Saxon.Hej.om.SequenceTool.toGroundedValue(child.iterate(context)));
                            } else {
                                Saxon.Hej.om.Item j = child.evaluateItem(context);
                                if ((j != null)) {
                                    chain.append(j);
                                }
                            }
                        }
                        return chain;
                    } else {
                        return Saxon.Hej.om.SequenceTool.toGroundedValue(expr.iterate(context));
                    }
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            }
        
        }
        public sealed class StreamingArgument : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.StreamingArgument INSTANCE = new Saxon.Hej.expr.parser.Evaluator.StreamingArgument();
            public StreamingArgument()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.STREAMING_ARGUMENT);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return context.getConfiguration().obtainOptimizer().evaluateStreamingArgument(expr, context);
            }
        
        }
        public sealed class MakeIndexedVariable : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.MakeIndexedVariable INSTANCE = new Saxon.Hej.expr.parser.Evaluator.MakeIndexedVariable();
            public MakeIndexedVariable()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.MAKE_INDEXED_VARIABLE);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                return context.getConfiguration().obtainOptimizer().makeIndexedValue(expr.iterate(context));
            }
        
        }
        public sealed class Process : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.Process INSTANCE = new Saxon.Hej.expr.parser.Evaluator.Process();
            public Process()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.PROCESS);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.Controller controller = context.getController();
                Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter();
                Saxon.Hej.@event.ComplexContentOutputter @out = new Saxon.Hej.@event.ComplexContentOutputter(seq);
                @out.open();
                expr.process(@out, context);
                @out.close();
                Saxon.Hej.om.Sequence val = seq.getSequence();
                seq.reset();
                return val;
            }
        
        }
        public sealed class LazyTail : Saxon.Hej.expr.parser.Evaluator {
            public readonly static Saxon.Hej.expr.parser.Evaluator.LazyTail INSTANCE = new Saxon.Hej.expr.parser.Evaluator.LazyTail();
            public LazyTail()  {
                setCode(Saxon.Hej.expr.parser.Evaluators.LAZY_TAIL_EXPRESSION);
            }
            public override Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.expr.TailExpression tail = (Saxon.Hej.expr.TailExpression)(expr);
                Saxon.Hej.expr.VariableReference vr = (Saxon.Hej.expr.VariableReference)(tail.getBaseExpression());
                Saxon.Hej.om.Sequence @base = Saxon.Hej.expr.parser.Evaluator.Variable.INSTANCE.evaluate(vr, context);
                if ((@base is Saxon.Hej.value.MemoClosure)) {
                    Saxon.Hej.om.SequenceIterator it = @base.iterate();
                    @base = Saxon.Hej.om.SequenceTool.toGroundedValue(it);
                }
                if ((@base is Saxon.Hej.value.IntegerRange)) {
                    long step = ((Saxon.Hej.value.IntegerRange)(@base)).getStep();
                    long start = (((Saxon.Hej.value.IntegerRange)(@base)).getStart()+(((tail.getStart()-1))*step));
                    long end = ((Saxon.Hej.value.IntegerRange)(@base)).getEnd();
                    if ((start == end)) {
                        return Saxon.Hej.value.Int64Value.makeIntegerValue(end);
                    } else {
                        if ((start>end)) {
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        } else {
                            return new Saxon.Hej.value.IntegerRange(start, step, end);
                        }
                    }
                }
                if ((@base is Saxon.Hej.om.GroundedValue)) {
                    Saxon.Hej.om.GroundedValue baseSeq = (Saxon.Hej.om.GroundedValue)(@base);
                    return baseSeq.subsequence((tail.getStart()-1), ((baseSeq.getLength()-tail.getStart())+1));
                }
                return new Saxon.Hej.value.MemoClosure(tail, context);
            }
        
        }
    
    }
}