package net.sf.saxon.om;

import net.sf.saxon.transpile.CSharpModifiers;

/**
 * This class serves no useful purpose in the Java product, but in C#, it serves
 * to capture the default methods defined on the {@link SequenceIterator} interface
 * so they don't have to be replicated on every subclass.
 */

public abstract class AbstractSequenceIterator implements SequenceIterator {
    
    @CSharpModifiers(code={"public", "virtual"})
    public void close() {}

}

