package com.saxonica.ee.stream;

import java.util.Set;

/**
 * The posture of an expression defines the relationship of the result to a streamed input document,
 * as defined in the XSLT 3.0 specification.
 */
public enum Posture implements PostureGetter {

    GROUNDED,
    CLIMBING,
    STRIDING,
    CRAWLING,
    ROAMING;

    public Posture getPosture() {
        return this;
    }

    /**
     * Test whether this posture is either striding or crawling
     * @return true if this posture is either striding or crawling
     */

    public boolean isIncremental() {
        return this == STRIDING || this == CRAWLING;
    }

    public String toString() {
        return super.toString().toLowerCase();
    }

    /**
     * Get the combined posture of a set of postures, as defined in the XSLT 3.0 specification
     * @param postures the set of postures
     * @return the combined posture
     */

    public static Posture combinedPosture(Set<Posture> postures) {
        if (postures.contains(ROAMING)) {
            return ROAMING;
        }

        if (postures.isEmpty()) {
            return GROUNDED;
        }

        if (postures.contains(GROUNDED) && postures.size() == 1) {
            return GROUNDED;
        }

        if (postures.contains(CLIMBING)) {
            if (postures.size() == 1) {
                return CLIMBING;
            }
            if (postures.size() == 2 && postures.contains(GROUNDED)) {
                return CLIMBING;
            }
        }

        if (postures.contains(STRIDING)) {
            if (postures.size() == 1) {
                return STRIDING;
            }
            if (postures.size() == 2 && postures.contains(GROUNDED)) {
                return STRIDING;
            }
        }

        if (postures.contains(CRAWLING) && !postures.contains(CLIMBING)) {
            return CRAWLING;
        }

        return ROAMING;
    }
}

// Copyright (c) 2018 Saxonica Limited.


