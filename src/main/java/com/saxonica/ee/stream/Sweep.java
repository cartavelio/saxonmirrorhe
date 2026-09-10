package com.saxonica.ee.stream;

/**
 * The posture of an expression defines the relationship of the result to a streamed input document,
 * as defined in the XSLT 3.0 specification.
 */
public enum Sweep {

    MOTIONLESS,
    CONSUMING,
    FREE_RANGING;

    /**
     * Return the wider of two sweeps. The order of increasing width is MOTIONLESS, CONSUMING, FREE_RANGING.
     * @param a the first sweep
     * @param b the second sweep
     * @return the wider of the two
     */
    public static Sweep wider(Sweep a, Sweep b) {
        switch (a) {
            case FREE_RANGING:
                return a;
            case CONSUMING:
                return b == FREE_RANGING ? b : a;
            case MOTIONLESS:
                return b;
            default:
                throw new IllegalArgumentException();
        }
    }
}

// Copyright (c) 2018 Saxonica Limited.


