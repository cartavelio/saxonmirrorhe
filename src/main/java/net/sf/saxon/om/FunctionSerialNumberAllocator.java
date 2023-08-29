package net.sf.saxon.om;

public class FunctionSerialNumberAllocator {

    public static AtomicCounter serial = new AtomicCounter(0);
}

