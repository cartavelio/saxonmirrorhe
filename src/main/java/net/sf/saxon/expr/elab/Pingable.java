package net.sf.saxon.expr.elab;

/**
 * A {@code Ping} represents an object that can receive an event with no associated information.
 */

public interface Pingable {
    void ping();
}

