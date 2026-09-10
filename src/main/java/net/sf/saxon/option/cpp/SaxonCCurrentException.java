package net.sf.saxon.option.cpp;

import com.oracle.svm.core.Uninterruptible;

public class SaxonCCurrentException {
    private Throwable throwable = null;
    public static SaxonCCurrentException current = new SaxonCCurrentException();

    public SaxonCCurrentException() {}

    @Uninterruptible(reason = "exception handler")
    public void setThrowable(Throwable throwable) {
        this.throwable = throwable;
    }

    public Throwable getThrowable() {
        return this.throwable;
    }

    public static Throwable get(){
        return current.throwable;
    }

    public static void clear(){
        current.throwable = null;
    }

    @Uninterruptible(reason = "exception handler")
    public static SaxonCCurrentException getInstance(){
        return current;
    }


}
