using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class Evaluators {
        public const int UNDECIDED = -1;
        public const int EVALUATE_LITERAL = 0;
        public const int EVALUATE_VARIABLE = 1;
        public const int MAKE_CLOSURE = 3;
        public const int MAKE_MEMO_CLOSURE = 4;
        public const int RETURN_EMPTY_SEQUENCE = 5;
        public const int EVALUATE_AND_MATERIALIZE_VARIABLE = 6;
        public const int CALL_EVALUATE_OPTIONAL_ITEM = 7;
        public const int ITERATE_AND_MATERIALIZE = 8;
        public const int PROCESS = 9;
        public const int LAZY_TAIL_EXPRESSION = 10;
        public const int SHARED_APPEND_EXPRESSION = 11;
        public const int MAKE_INDEXED_VARIABLE = 12;
        public const int MAKE_SINGLETON_CLOSURE = 13;
        public const int EVALUATE_SUPPLIED_PARAMETER = 14;
        public const int STREAMING_ARGUMENT = 15;
        public const int CALL_EVALUATE_SINGLE_ITEM = 16;
        public static Saxon.Hej.expr.parser.Evaluator getEvaluator(int code) {
            switch (code) {
              case UNDECIDED:
                return Saxon.Hej.expr.parser.Evaluator.EagerSequence.INSTANCE;
              case EVALUATE_LITERAL:
                return Saxon.Hej.expr.parser.Evaluator.Literal.INSTANCE;
              case EVALUATE_VARIABLE:
                return Saxon.Hej.expr.parser.Evaluator.Variable.INSTANCE;
              case MAKE_CLOSURE:
                return Saxon.Hej.expr.parser.Evaluator.LazySequence.INSTANCE;
              case MAKE_MEMO_CLOSURE:
                return Saxon.Hej.expr.parser.Evaluator.MemoClosureEvaluator.INSTANCE;
              case RETURN_EMPTY_SEQUENCE:
                return Saxon.Hej.expr.parser.Evaluator.EmptySequenceEvaluator.INSTANCE;
              case EVALUATE_AND_MATERIALIZE_VARIABLE:
                return Saxon.Hej.expr.parser.Evaluator.Variable.INSTANCE;
              case CALL_EVALUATE_OPTIONAL_ITEM:
                return Saxon.Hej.expr.parser.Evaluator.OptionalItem.INSTANCE;
              case ITERATE_AND_MATERIALIZE:
                return Saxon.Hej.expr.parser.Evaluator.EagerSequence.INSTANCE;
              case PROCESS:
                return Saxon.Hej.expr.parser.Evaluator.Process.INSTANCE;
              case LAZY_TAIL_EXPRESSION:
                return Saxon.Hej.expr.parser.Evaluator.LazyTail.INSTANCE;
              case SHARED_APPEND_EXPRESSION:
                return Saxon.Hej.expr.parser.Evaluator.SharedAppend.INSTANCE;
              case MAKE_INDEXED_VARIABLE:
                return Saxon.Hej.expr.parser.Evaluator.MakeIndexedVariable.INSTANCE;
              case MAKE_SINGLETON_CLOSURE:
                return Saxon.Hej.expr.parser.Evaluator.SingletonClosure.INSTANCE;
              case EVALUATE_SUPPLIED_PARAMETER:
                return Saxon.Hej.expr.parser.Evaluator.SuppliedParameter.INSTANCE;
              case STREAMING_ARGUMENT:
                return Saxon.Hej.expr.parser.Evaluator.StreamingArgument.INSTANCE;
              case CALL_EVALUATE_SINGLE_ITEM:
                return Saxon.Hej.expr.parser.Evaluator.SingleItem.INSTANCE;
              default:
                return Saxon.Hej.expr.parser.Evaluator.EagerSequence.INSTANCE;
            }
        }
    
    }
}