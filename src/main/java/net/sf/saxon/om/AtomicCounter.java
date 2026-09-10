package net.sf.saxon.om;

import net.sf.saxon.transpile.CSharpInjectMembers;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.concurrent.atomic.AtomicInteger;

/**
 * An integer that can be incremented atomically with thread safety
 */

@CSharpInjectMembers(code="private int counter = 0;")
public class AtomicCounter {

    //#if CSHARP==false
    private AtomicInteger counter;
    //#endif

    @CSharpReplaceBody(code="counter = initialValue;")
    public AtomicCounter(int initialValue) {
        counter = new AtomicInteger(initialValue);
    }

    @CSharpReplaceBody(code = "return System.Threading.Interlocked.Increment(ref counter);")
    public int getAndIncrement() {
        return counter.getAndIncrement();
    }

}

