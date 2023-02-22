package net.sf.saxon.expr.parser;

import net.sf.saxon.z.IntHashMap;

public class Evaluators {
    
    private static IntHashMap<Evaluator> map = new IntHashMap<>(20);

    private static void bind(int code, Evaluator evaluator) {
        evaluator.setCode(code);
        map.put(code, evaluator);
    }

    // These numeric constants must be stable as they are held in the SEF file

    public static final int UNDECIDED = -1;
    public static final int EVALUATE_LITERAL=0;
    public static final int EVALUATE_VARIABLE=1;
    public static final int MAKE_CLOSURE = 3;
    public static final int MAKE_MEMO_CLOSURE=4;
    public static final int RETURN_EMPTY_SEQUENCE = 5;
    public static final int EVALUATE_AND_MATERIALIZE_VARIABLE = 6;
    public static final int CALL_EVALUATE_OPTIONAL_ITEM = 7;
    public static final int ITERATE_AND_MATERIALIZE = 8;
    public static final int PROCESS = 9;
    public static final int LAZY_TAIL_EXPRESSION = 10;
    public static final int SHARED_APPEND_EXPRESSION = 11;
    public static final int MAKE_INDEXED_VARIABLE = 12;
    public static final int MAKE_SINGLETON_CLOSURE = 13;
    public static final int EVALUATE_SUPPLIED_PARAMETER = 14;
    public static final int STREAMING_ARGUMENT = 15;
    public static final int CALL_EVALUATE_SINGLE_ITEM = 16;

    static {
        bind(UNDECIDED, Evaluator.EagerSequence.INSTANCE);
        bind(EVALUATE_LITERAL, Evaluator.Literal.INSTANCE);
        bind(EVALUATE_VARIABLE, Evaluator.Variable.INSTANCE);
        bind(MAKE_CLOSURE, Evaluator.LazySequence.INSTANCE);
        bind(MAKE_MEMO_CLOSURE, Evaluator.MemoClosure.INSTANCE);
        bind(RETURN_EMPTY_SEQUENCE, Evaluator.EmptySequence.INSTANCE);
        bind(EVALUATE_AND_MATERIALIZE_VARIABLE, Evaluator.Variable.INSTANCE);
        bind(CALL_EVALUATE_OPTIONAL_ITEM, Evaluator.OptionalItem.INSTANCE);
        bind(ITERATE_AND_MATERIALIZE, Evaluator.EagerSequence.INSTANCE);
        bind(PROCESS, Evaluator.Process.INSTANCE);
        bind(LAZY_TAIL_EXPRESSION, Evaluator.LazyTail.INSTANCE);
        bind(SHARED_APPEND_EXPRESSION, Evaluator.SharedAppend.INSTANCE);
        bind(MAKE_INDEXED_VARIABLE, Evaluator.MakeIndexedVariable.INSTANCE);
        bind(MAKE_SINGLETON_CLOSURE, Evaluator.SingletonClosure.INSTANCE);
        bind(EVALUATE_SUPPLIED_PARAMETER, Evaluator.SuppliedParameter.INSTANCE);
        bind(STREAMING_ARGUMENT, Evaluator.StreamingArgument.INSTANCE);
        bind(CALL_EVALUATE_SINGLE_ITEM, Evaluator.SingleItem.INSTANCE);
    }
    
    public static Evaluator getEvaluator(int code) {
        return map.get(code);
    }
}

