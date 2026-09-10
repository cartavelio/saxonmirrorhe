package com.saxonica.ee.stream;

/**
 * This class defines a composite property of an expression containing both the posture and sweep of the
 * expression, as defined in the XSLT 3.0 specification. The class is immutable.
 */
public class PostureAndSweep {

    private final Posture posture;
    private final Sweep sweep;

    /**
     * Create a PostureAndSweep with a given posture and sweep
     * @param posture the posture
     * @param sweep the sweep
     */

    public PostureAndSweep(Posture posture, Sweep sweep) {
        this.posture = posture;
        this.sweep = sweep;
    }

    /**
     * Get the posture component
     * @return  the posture component
     */

    public Posture getPosture() {
        return posture;
    }

    /**
     * Get the sweep component
     * @return  the sweep component
     */

    public Sweep getSweep() {
        return sweep;
    }

    @Override
    public int hashCode() {
        return posture.hashCode()<<16 ^ sweep.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof PostureAndSweep &&
                ((PostureAndSweep)obj).posture.equals(posture) &&
                ((PostureAndSweep)obj).sweep.equals(sweep);
    }

    public String toString() {
        return posture.toString() + " and " + sweep.toString();
    }

    /**
     * Constant value for (posture = roaming, sweep = free-ranging)
     */

    public static final PostureAndSweep ROAMING_AND_FREE_RANGING =
            new PostureAndSweep(Posture.ROAMING, Sweep.FREE_RANGING);

    /**
     * Constant value for (posture = grounded, sweep = motionless)
     */

    public static final PostureAndSweep GROUNDED_AND_MOTIONLESS =
            new PostureAndSweep(Posture.GROUNDED, Sweep.MOTIONLESS);

    /**
     * Constant value for (posture = climbing, sweep = motionless)
     */

    public static final PostureAndSweep CLIMBING_AND_MOTIONLESS =
            new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);
}

// Copyright (c) 2018-2022 Saxonica Limited


