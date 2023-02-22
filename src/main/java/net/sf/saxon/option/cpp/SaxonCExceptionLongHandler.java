package net.sf.saxon.option.cpp;


import com.oracle.svm.core.annotate.*;
import org.graalvm.nativeimage.c.function.CEntryPoint;

public class SaxonCExceptionLongHandler implements CEntryPoint.ExceptionHandler {

    @Uninterruptible(reason = "exception handler")
    public static long handle(Throwable e) {
        SaxonCException.throwable = e;
        return -2;
   	}
}
