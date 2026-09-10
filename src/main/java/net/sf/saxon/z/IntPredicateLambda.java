package net.sf.saxon.z;

import java.util.function.IntPredicate;

public class IntPredicateLambda implements IntPredicateProxy {

    IntPredicate lambda;
    public IntPredicateLambda(IntPredicate lambda) {
        this.lambda = lambda;
    }

    public static IntPredicateProxy of(IntPredicate lambda) {
        return new IntPredicateLambda(lambda);
    }

    @Override
    public boolean test(int value) {
        return lambda.test(value);
    }
}

