package net.sf.saxon.om;

import net.sf.saxon.transpile.CSharpInjectMembers;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.concurrent.atomic.AtomicLong;

/**
 * An integer that can be incremented atomically with thread safety
 */

@CSharpInjectMembers(code="private long counter = 0;")
public class AtomicCounter {

    //#if CSHARP==false
    private AtomicLong counter;
    //#endif


    public AtomicCounter(int initialValue) {
        init(initialValue);
    }

    @CSharpReplaceBody(code = "counter = initialValue;")
    private void init(int initialValue) {
        counter = new AtomicLong(initialValue);
    }

    @CSharpReplaceBody(code = "return System.Threading.Interlocked.Increment(ref counter);")
    public long getAndIncrement() {
        return counter.getAndIncrement();
    }

    public long incrementAndGet() {
        return getAndIncrement() + 1L;
    }

    @CSharpReplaceBody(code="return counter;")
    public long get() {
        return counter.get();
    }

}

