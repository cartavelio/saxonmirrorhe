package net.sf.saxon.expr.freq;

/**
 * A {@code Ping} represents an event with no associated information.
 */

public class Ping {

    private Ping() {}

    public final static Ping INSTANCE = new Ping();
}

