using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class Evaluators {
        private readonly static Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.parser.Evaluator> map = new Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.parser.Evaluator>(20);
        private static void bind(int code, Saxon.Hej.expr.parser.Evaluator evaluator) {
            evaluator.setCode(code);
            map.put(code, evaluator);
        }
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
            return map.get(code);
        }
        public static void index(Saxon.Hej.expr.parser.Evaluator evaluator, int code) {
            map.put(code, evaluator);
        }
    
    }
}