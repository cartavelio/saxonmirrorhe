package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.ObjectHandle;
import com.oracle.svm.core.annotate.*;
import org.graalvm.nativeimage.c.function.CEntryPoint;

public class SaxonCExceptionHandler implements CEntryPoint.ExceptionHandler {

    @Uninterruptible(reason = "exception handler")
    public static long handle(Throwable e) {
        SaxonCException ex = new SaxonCException(e);
        return -2;
   	}
}
