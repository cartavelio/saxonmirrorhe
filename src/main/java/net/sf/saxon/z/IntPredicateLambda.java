package net.sf.saxon.z;

import java.util.function.IntPredicate;

/**
 * This class allows an integer predicate (a boolean function of an integer) to be written as a lambda
 * expression, in a way that works both in Java and C#. It is needed because interfaces and delegates
 * are interchangeable in Java (through the mechanism of "functional interfaces), but the same is not
 * the case in C#.
 */

public class IntPredicateLambda implements IntPredicateProxy {

    IntPredicate lambda;
    private IntPredicateLambda(IntPredicate lambda) {
        this.lambda = lambda;
    }

    /**
     * Implement an integer predicate as a lambda expression
     * @param lambda an implementation of the functional interface {@link IntPredicate}, generally
     *               supplied as a lambda expression (for example <code>i -> i > 0</code> for a predicate
     *               that matches all positive integers).
     * @return the integer predicate
     */
    public static IntPredicateProxy of(IntPredicate lambda) {
        return new IntPredicateLambda(lambda);
    }

    @Override
    public boolean test(int value) {
        return lambda.test(value);
    }
}

