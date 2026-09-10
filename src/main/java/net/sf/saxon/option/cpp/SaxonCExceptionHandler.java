package net.sf.saxon.option.cpp;

import org.graalvm.nativeimage.ObjectHandle;
import com.oracle.svm.core.annotate.*;
import org.graalvm.nativeimage.c.function.CEntryPoint;

public class SaxonCExceptionHandler implements CEntryPoint.ExceptionHandler {

    @Uninterruptible(reason = "exception handler")
    public static long handle(Throwable e) {
        ObjectHandle handle = SaxonCAPI.objectHandles.create(new SaxonCException(e));
        if(SaxonCAPI.debug) {
            System.err.println("DEBUG: Exception thrown message: "+e.getMessage());
        }
        return handle.rawValue();
   	}
}
